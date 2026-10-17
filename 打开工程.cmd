@echo off
set "EDITOR=C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe"
if not exist "%EDITOR%" (
  echo 未找到 Unity 2022.3.62f3c1。请先安装 Unity Hub，并安装该版本。
  exit /b 1
)
start "" "%EDITOR%" -projectPath "%~dp0."
