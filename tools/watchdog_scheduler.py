"""Windows Task Scheduler bridge for the AutoEra watchdog control panel."""

from __future__ import annotations

import base64
import json
import subprocess
from pathlib import Path


TASK_NAME = "AutoEra-AgentWatchdog"
MIN_INTERVAL_SECONDS = 60
MAX_INTERVAL_SECONDS = 600


def normalize_interval(value: int | str) -> int:
    """Clamp the UI value to the interval supported by this background task."""
    return max(MIN_INTERVAL_SECONDS, min(MAX_INTERVAL_SECONDS, int(value)))


def _powershell(script: str) -> subprocess.CompletedProcess[str]:
    encoded = base64.b64encode(script.encode("utf-16le")).decode("ascii")
    return subprocess.run(
        ["powershell.exe", "-NoProfile", "-NonInteractive", "-EncodedCommand", encoded],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
        check=False,
    )


def _quoted(value: Path | str) -> str:
    return "'" + str(value).replace("'", "''") + "'"


def status() -> dict[str, object]:
    """Return the actual Windows task state, without changing it."""
    script = f"""
$task = Get-ScheduledTask -TaskName {_quoted(TASK_NAME)} -ErrorAction SilentlyContinue
if ($null -eq $task) {{ [pscustomobject]@{{ exists = $false }} | ConvertTo-Json -Compress; exit 0 }}
$trigger = $task.Triggers | Select-Object -First 1
$seconds = if ($null -ne $trigger.Repetition.Interval) {{ [int][Math]::Round([System.Xml.XmlConvert]::ToTimeSpan([string]$trigger.Repetition.Interval).TotalSeconds) }} else {{ 0 }}
[pscustomobject]@{{ exists = $true; enabled = ($task.State -ne 'Disabled'); state = [string]$task.State; intervalSeconds = $seconds }} | ConvertTo-Json -Compress
"""
    result = _powershell(script)
    if result.returncode:
        raise RuntimeError(result.stderr.strip() or "无法读取 Windows 任务计划程序状态")
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError as exc:
        raise RuntimeError("任务计划程序未返回可识别状态") from exc


def configure(project_root: Path, enabled: bool, interval_seconds: int | str) -> dict[str, object]:
    """Create or update the persisted watchdog task from the front-end settings."""
    interval = normalize_interval(interval_seconds)
    launcher = project_root / "tools" / "Start-AgentWatchdog-hidden.vbs"
    if not launcher.is_file():
        raise RuntimeError(f"找不到守护启动器：{launcher}")

    script = f"""
$taskName = {_quoted(TASK_NAME)}
$launcher = {_quoted(launcher)}
$action = New-ScheduledTaskAction -Execute 'wscript.exe' -Argument ('"' + $launcher + '"')
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Seconds {interval}) -RepetitionDuration (New-TimeSpan -Days 3650)
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings -Force | Out-Null
if ({'$true' if enabled else '$false'}) {{ Enable-ScheduledTask -TaskName $taskName | Out-Null }} else {{ Disable-ScheduledTask -TaskName $taskName | Out-Null }}
"""
    result = _powershell(script)
    if result.returncode:
        raise RuntimeError(result.stderr.strip() or "无法更新 Windows 守护任务")
    return status()
