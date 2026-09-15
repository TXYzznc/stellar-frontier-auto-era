param([int]$IntervalSeconds=30,[switch]$Once)
$ErrorActionPreference='Stop'
$Root=Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$Lock=Join-Path $Root '.ai/dispatch/watchdog.lock'
$Python=(Get-Command python).Source
if(Test-Path $Lock){$age=(Get-Date)-(Get-Item $Lock).LastWriteTime;if($age.TotalMinutes -lt 10){exit 0};Remove-Item $Lock -Force}
New-Item $Lock -ItemType File -Force | Out-Null
try {
 do {
  & $Python (Join-Path $Root 'tools/agent_watchdog.py') --append | Out-Null
  & $Python (Join-Path $Root 'tools/agent_watchdog_wake.py') | Out-Null
  if($Once){break}; Start-Sleep -Seconds $IntervalSeconds
 } while($true)
} finally {Remove-Item $Lock -Force -ErrorAction SilentlyContinue}
