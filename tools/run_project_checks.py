#!/usr/bin/env python3
"""AutoEra 项目层健康检查入口（P0-013）。

把五类检查串成一套可重复执行的入口：

1) 编译检查（Unity 8092 编译 0 error）
2) 引用检查（悬空资源/GUID 引用）
3) AppConfigs 检查（数据表/配置/语言/流程引用齐全）
4) 资源表检查（衔接 GameData/AIData/Reports 校验报告）
5) 框架纯度检查（audit_framework_purity + audit_project_boundaries）

用法：

    python tools/run_project_checks.py [--port 8092]

退出码：0=全部通过；1=至少一项未通过。

说明：
- 本入口只读不写（除第 4 项触发 Unity 菜单“校验 AI 数据表”会新增一份
  校验报告到 GameData/AIData/Reports/ 之外，不修改产品代码、框架核心与资产）。
- 第 5 项框架纯度审计自 2026-09-18 起 5 项历史债务已清零（详见
  Docs/Development/CompilePurityCheck.md），当前应 5/5 PASS；若暴露新的发现路径，
  按 ProjectBaseline.md 的边界判断是否属于产品回归，不自行扩权处理。
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
import time
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
UNITY_SKILLS = ROOT / ".agents" / "skills" / "unity-skills" / "scripts" / "unity_skills.py"
APPCONFIGS = ROOT / "Assets" / "Game" / "ScriptableAssets" / "Core" / "AppConfigs.asset"
REPORTS_DIR = ROOT / "GameData" / "AIData" / "Reports"

VALIDATE_MENU = "Game Framework/GameTools/AI Data/Validate DataTables Json"


@dataclass(frozen=True)
class Check:
    name: str
    passed: bool
    detail: str


def run(cmd: list[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        [str(item) for item in cmd],
        cwd=str(ROOT),
        capture_output=True,
        text=True,
    )


def run_unity(skill: str, *params: str, port: int) -> subprocess.CompletedProcess[str]:
    return run([sys.executable, str(UNITY_SKILLS), skill, "--port", str(port), *params])


def parse_json(proc: subprocess.CompletedProcess[str]) -> object:
    try:
        return json.loads(proc.stdout)
    except json.JSONDecodeError:
        return {"raw_stdout": proc.stdout, "raw_stderr": proc.stderr}


def wait_not_compiling(port: int, tries: int = 30, delay: float = 2.0) -> bool:
    for _ in range(tries):
        proc = run_unity("debug_check_compilation", port=port)
        data = parse_json(proc)
        if isinstance(data, dict) and data.get("isCompiling") is False:
            return True
        time.sleep(delay)
    return False


def check_compile(port: int) -> Check:
    wait_not_compiling(port)
    proc = run_unity("debug_get_errors", port=port)
    data = parse_json(proc)
    if isinstance(data, dict) and "count" in data:
        count = data.get("count", -1)
        return Check("编译检查 (Unity 8092)", count == 0, f"compile errors = {count}")
    return Check("编译检查 (Unity 8092)", False, f"无法查询编译错误: {data}")


def check_references(port: int) -> Check:
    proc = run_unity("cleaner_find_missing_references", port=port)
    data = parse_json(proc)
    if isinstance(data, dict) and "issueCount" in data:
        issue = data.get("issueCount", -1)
        missing_scripts = data.get("missingScripts", -1)
        missing_refs = data.get("missingReferences", -1)
        detail = f"missing scripts = {missing_scripts}, missing references = {missing_refs}"
        return Check("引用检查 (悬空引用)", issue == 0, detail)
    return Check("引用检查 (悬空引用)", False, f"无法查询引用: {data}")


def _extract_list(text: str, key: str) -> list[str]:
    pattern = re.compile(
        rf"^\s*{re.escape(key)}:\s*\n(?P<items>(?:^\s*-\s+.*\n?)*)",
        re.MULTILINE,
    )
    match = pattern.search(text)
    if not match:
        return []
    items: list[str] = []
    for line in match.group("items").splitlines():
        stripped = line.strip()
        if stripped.startswith("- "):
            items.append(stripped[2:].strip())
    return items


def check_appconfigs() -> Check:
    if not APPCONFIGS.exists():
        return Check("AppConfigs 检查", False, f"{APPCONFIGS.relative_to(ROOT)} 缺失")
    text = APPCONFIGS.read_text(encoding="utf-8")
    datatables = _extract_list(text, "mDataTables")
    configs = _extract_list(text, "mConfigs")
    languages = _extract_list(text, "mLanguages")
    procedures = _extract_list(text, "mProcedures")

    problems: list[str] = []
    for entry in datatables:
        if not (ROOT / "Assets" / "Game" / "DataTable" / f"{entry}.txt").exists():
            problems.append(f"数据表缺失: {entry}")
    for entry in configs:
        if not (ROOT / "Assets" / "Game" / "Config" / f"{entry}.txt").exists():
            problems.append(f"配置缺失: {entry}")
    for entry in languages:
        if not (ROOT / "Assets" / "Game" / "Language" / f"{entry}.json").exists():
            problems.append(f"语言缺失: {entry}")
    if not procedures:
        problems.append("未注册任何流程 (mProcedures)")

    passed = not problems
    detail = (
        f"dataTables={len(datatables)}, configs={len(configs)}, "
        f"languages={len(languages)}, procedures={len(procedures)}"
    )
    if problems:
        detail += " | " + "; ".join(problems[:8])
    return Check("AppConfigs 检查", passed, detail)


def check_datatables(port: int) -> Check:
    before = {p.name for p in REPORTS_DIR.glob("validate-data-tables-json_*.json")}
    run_unity("editor_execute_menu", f"menuPath={VALIDATE_MENU}", port=port)
    wait_not_compiling(port)
    reports = sorted(
        REPORTS_DIR.glob("validate-data-tables-json_*.json"),
        key=lambda p: p.stat().st_mtime,
    )
    if not reports:
        return Check("资源表检查 (AIData 校验)", False, "未找到 validate-data-tables-json 报告")
    latest = reports[-1]
    is_new = latest.name not in before
    try:
        payload = json.loads(latest.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        return Check("资源表检查 (AIData 校验)", False, f"报告无法解析: {exc}")
    success = payload.get("successCount", -1)
    failure = payload.get("failureCount", -1)
    warnings = payload.get("warningCount", -1)
    passed = failure == 0 and success > 0
    freshness = "新生成" if is_new else "复用既有最新"
    detail = (
        f"{latest.name} [{freshness}]: success={success}, failure={failure}, warning={warnings}"
    )
    return Check("资源表检查 (AIData 校验)", passed, detail)


def check_purity() -> Check:
    steps = [
        (
            [sys.executable, "tools/audit_framework_purity.py",
             "--product-profile", "tools/audit_product_profile.json"],
            "audit_framework_purity",
        ),
        ([sys.executable, "tools/audit_project_boundaries.py"], "audit_project_boundaries"),
    ]
    parts: list[str] = []
    all_passed = True
    for cmd, label in steps:
        proc = run(cmd)
        ok = proc.returncode == 0
        all_passed = all_passed and ok
        lines = (proc.stdout or proc.stderr).strip().splitlines()
        if ok:
            parts.append(f"{label} = PASS")
            continue
        summary = lines[0].strip() if lines else "no output"
        rules = sorted(
            {
                line.split(":", 1)[0].lstrip("- ").strip()
                for line in lines
                if line.startswith("- ")
            }
        )
        parts.append(f"{label} = FAIL [{summary}] rules={rules}")
    return Check("框架纯度检查", all_passed, "; ".join(parts))


def main(argv: list[str] | None = None) -> int:
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8")
        except (AttributeError, OSError):
            pass
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", type=int, default=8092, help="Unity REST 端口（默认 8092）")
    args = parser.parse_args(argv)

    checks = [
        check_compile(port=args.port),
        check_references(port=args.port),
        check_appconfigs(),
        check_datatables(port=args.port),
        check_purity(),
    ]

    print("=== AutoEra 项目健康检查入口 (P0-013 编译与框架纯度检查) ===")
    for index, check in enumerate(checks, 1):
        marker = "PASS" if check.passed else "FAIL"
        print(f"[{index}/5] {check.name}: {marker} — {check.detail}")

    passed = sum(1 for check in checks if check.passed)
    print(f"=== 结果: {passed}/5 PASS ===")
    return 0 if passed == len(checks) else 1


if __name__ == "__main__":
    sys.exit(main())
