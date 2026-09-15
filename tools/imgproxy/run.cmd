@echo off
rem AutoEra image pre-processor: foreground launcher (shows the log window).
setlocal
set "HERE=%~dp0"
set "PY=%HERE%..\..\.venv\Scripts\python.exe"
if not exist "%PY%" set "PY=py -3"
set PYTHONIOENCODING=utf-8
"%PY%" "%HERE%img_proxy.py" %*
