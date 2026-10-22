@echo off
setlocal
if exist "%~dp0LumenDesktop.exe" (
  start "Lumen" "%~dp0LumenDesktop.exe"
  exit /b 0
)
start "Lumen server" /b "%~dp0Lumen.exe"
for /l %%i in (1,1,30) do (
  powershell.exe -NoProfile -Command "try { Invoke-WebRequest http://127.0.0.1:8765/api/status -TimeoutSec 1 | Out-Null; exit 0 } catch { exit 1 }"
  if not errorlevel 1 goto ready
  timeout /t 1 /nobreak >nul
)
:ready
where msedge.exe >nul 2>nul && start "Lumen" msedge.exe --app=http://localhost:8765 || start "Lumen" http://localhost:8765
