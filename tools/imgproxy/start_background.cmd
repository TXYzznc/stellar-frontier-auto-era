@echo off
rem AutoEra image pre-processor: silent background launcher (used by the Startup folder).
rem - skips the launch when port 15722 is already serving
rem - waits (bounded) for the upstream cc-switch port 15721 so Codex never meets a dead proxy
setlocal
set "HERE=%~dp0"
set "PY=%HERE%..\..\.venv\Scripts\pythonw.exe"
if not exist "%PY%" set "PY=pythonw"
set PYTHONIOENCODING=utf-8

powershell -NoProfile -Command "if (Get-NetTCPConnection -State Listen -LocalPort 15722 -ErrorAction SilentlyContinue) { exit 0 } else { exit 1 }"
if errorlevel 1 goto start
exit /b 0

:start
for /l %%i in (1,1,60) do (
  powershell -NoProfile -Command "if (Get-NetTCPConnection -State Listen -LocalPort 15721 -ErrorAction SilentlyContinue) { exit 0 } else { exit 1 }"
  if not errorlevel 1 goto launch
  ping -n 2 127.0.0.1 >nul
)

:launch
start "" /b "%PY%" "%HERE%img_proxy.py" >> "%HERE%logs\launcher.log" 2>&1
