#!/usr/bin/env python3
"""Audit AutoEra project boundaries without coupling the framework purity audit."""

from __future__ import annotations

import argparse
import re
import sys
from dataclasses import dataclass
from pathlib import Path


PRODUCT_NAMESPACE = "AutoEra"
PRODUCT_ROOT = Path("Assets/Game/Scripts/AutoEra")
SCRIPTS_ROOT = Path("Assets/Game/Scripts")
BUILTIN_ROOT = Path("Assets/Game/ScriptsBuiltin")
BASELINE_PATH = Path("Docs/Development/ProjectBaseline.md")

ENTRY_LINKS = {
    Path("AGENTS.md"): "Docs/Development/ProjectBaseline.md",
    Path("README.md"): "Docs/Development/ProjectBaseline.md",
    Path(".claude/CLAUDE.md"): "../Docs/Development/ProjectBaseline.md",
}

RESOURCE_ROOTS = (
    Path("Assets/Game/Animations"),
    Path("Assets/Game/Audio"),
    Path("Assets/Game/Config"),
    Path("Assets/Game/DataTable"),
    Path("Assets/Game/Font"),
    Path("Assets/Game/Language"),
    Path("Assets/Game/Materials"),
    Path("Assets/Game/Models"),
    Path("Assets/Game/Prefabs"),
    Path("Assets/Game/Scene"),
    Path("Assets/Game/ScriptableAssets"),
    Path("Assets/Game/Shaders"),
    Path("Assets/Game/Sprites"),
    Path("Assets/Game/Textures"),
    Path("Assets/Game/Timeline"),
    Path("Assets/Game/VFX"),
    Path("Assets/Game/Video"),
    Path("GameData/Configs"),
    Path("GameData/DataTables"),
    Path("GameData/Languages"),
)

NAMESPACE_DECLARATION = re.compile(
    r"^\s*namespace\s+([A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)\s*(?:;|\{)",
    re.MULTILINE,
)
PRODUCT_TOKEN = re.compile(r"\bAutoEra(?:\b|\.)")
CSHARP_NON_CODE = re.compile(
    r"//[^\r\n]*|/\*.*?\*/|@\"(?:\"\"|[^\"])*\"|\"(?:\\.|[^\"\\])*\"|'(?:\\.|[^'\\])'",
    re.DOTALL | re.MULTILINE,
)


@dataclass(frozen=True)
class Finding:
    rule: str
    path: str
    detail: str


def relative(path: Path, root: Path) -> str:
    return path.relative_to(root).as_posix()


def read_csharp_code(path: Path) -> str:
    # Unity/Windows tooling may emit UTF-8 BOM. Decode it away so a namespace
    # declaration at byte zero still matches the anchored declaration pattern.
    text = path.read_text(encoding="utf-8-sig", errors="replace")
    return CSHARP_NON_CODE.sub(" ", text)


def is_product_namespace(name: str) -> bool:
    return name == PRODUCT_NAMESPACE or name.startswith(f"{PRODUCT_NAMESPACE}.")


def iter_csharp_files(path: Path):
    if path.is_dir():
        yield from sorted(item for item in path.rglob("*.cs") if item.is_file())


def audit(root: Path) -> list[Finding]:
    root = root.resolve()
    findings: list[Finding] = []
    product_root = root / PRODUCT_ROOT
    scripts_root = root / SCRIPTS_ROOT
    builtin_root = root / BUILTIN_ROOT

    if not product_root.is_dir():
        findings.append(
            Finding("product-root", PRODUCT_ROOT.as_posix(), "product code root is missing")
        )
    elif not (product_root / "README.md").is_file():
        findings.append(
            Finding(
                "product-root",
                (PRODUCT_ROOT / "README.md").as_posix(),
                "product root contract is missing",
            )
        )

    for path in iter_csharp_files(product_root):
        code = read_csharp_code(path)
        namespaces = NAMESPACE_DECLARATION.findall(code)
        if not namespaces:
            findings.append(
                Finding(
                    "product-namespace",
                    relative(path, root),
                    "product C# file must declare AutoEra or AutoEra.* namespace",
                )
            )
            continue
        invalid = sorted({name for name in namespaces if not is_product_namespace(name)})
        if invalid:
            findings.append(
                Finding(
                    "product-namespace",
                    relative(path, root),
                    "non-product namespace declaration(s): " + ", ".join(invalid),
                )
            )

    for path in iter_csharp_files(scripts_root):
        if product_root in path.parents:
            continue
        namespaces = NAMESPACE_DECLARATION.findall(read_csharp_code(path))
        product_namespaces = sorted({name for name in namespaces if is_product_namespace(name)})
        if product_namespaces:
            findings.append(
                Finding(
                    "product-path",
                    relative(path, root),
                    "AutoEra namespace declared outside product root: "
                    + ", ".join(product_namespaces),
                )
            )

    for path in iter_csharp_files(builtin_root):
        if PRODUCT_TOKEN.search(read_csharp_code(path)):
            findings.append(
                Finding(
                    "framework-dependency",
                    relative(path, root),
                    "framework core must not reference AutoEra",
                )
            )

    for resource_root_name in RESOURCE_ROOTS:
        resource_root = root / resource_root_name
        if not resource_root.is_dir():
            continue
        for path in sorted(item for item in resource_root.rglob("*") if item.is_dir()):
            if path.name.casefold() == PRODUCT_NAMESPACE.casefold():
                findings.append(
                    Finding(
                        "redundant-product-directory",
                        relative(path, root),
                        f"use a business category under {resource_root_name.as_posix()} instead",
                    )
                )

    baseline = root / BASELINE_PATH
    if not baseline.is_file():
        findings.append(
            Finding("baseline", BASELINE_PATH.as_posix(), "project baseline is missing")
        )

    for entry_path, expected_link in ENTRY_LINKS.items():
        entry = root / entry_path
        if not entry.is_file():
            findings.append(
                Finding("baseline-entry", entry_path.as_posix(), "entry file is missing")
            )
            continue
        text = entry.read_text(encoding="utf-8", errors="replace")
        if expected_link not in text:
            findings.append(
                Finding(
                    "baseline-entry",
                    entry_path.as_posix(),
                    f"missing project baseline link: {expected_link}",
                )
            )

    return sorted(findings, key=lambda item: (item.rule, item.path, item.detail))


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parent.parent)
    args = parser.parse_args(argv)
    root = args.root.resolve()

    findings = audit(root)
    if findings:
        print(f"[FAIL] project boundary audit found {len(findings)} issue(s)")
        for item in findings:
            print(f"- {item.rule}: {item.path}: {item.detail}")
        return 1

    print("[OK] project boundary audit passed")
    return 0


if __name__ == "__main__":
    sys.exit(main())
