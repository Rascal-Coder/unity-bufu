@echo off
setlocal EnableExtensions
title Open Bufu Unity Project
set "UNITY_EDITOR=E:\Unity\Editors\2022.3.62f3c1\Editor\Unity.exe"
for %%I in ("%~dp0.") do set "UNITY_PROJECT=%%~fI"
set "UNITY_LOG=%UNITY_PROJECT%\Logs\unity-editor-startup.log"

"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -Command "if(Get-Process -Name 'UnitySetup*' -ErrorAction SilentlyContinue){exit 2}" >nul 2>&1
if errorlevel 2 goto installing
if not exist "%UNITY_EDITOR%" goto missing
if not exist "E:\Unity\Editors\2022.3.62f3c1\Editor\Data\Resources\PackageManager\Server\UnityPackageManager.exe" goto incomplete
if not exist "%UNITY_PROJECT%\ProjectSettings\ProjectVersion.txt" goto projectmissing
if /I "%~1"=="--check" (
  echo Unity launcher checks passed.
  exit /b 0
)
if not exist "%UNITY_PROJECT%\Logs" mkdir "%UNITY_PROJECT%\Logs"
echo Opening the Unity project. The first import can take several minutes.
echo Editor: %UNITY_EDITOR%
echo Project: %UNITY_PROJECT%
echo Log: %UNITY_LOG%
echo Keep this window open while Unity is running.
start "" /wait "%UNITY_EDITOR%" -projectPath "%UNITY_PROJECT%" -logFile "%UNITY_LOG%"
set "UNITY_EXIT=%ERRORLEVEL%"
if "%UNITY_EXIT%"=="0" exit /b 0
echo.
echo Unity stopped with exit code %UNITY_EXIT%.
echo Startup details: %UNITY_LOG%
goto failed

:installing
echo Unity Setup is still running. Finish the installation, then try again.
goto failed
:missing
echo Unity was not found at: %UNITY_EDITOR%
goto failed
:incomplete
echo Unity installation is incomplete. Package Manager is missing.
echo Finish Unity Setup or repair this editor, then try again.
goto failed
:projectmissing
echo Unity project files were not found at: %UNITY_PROJECT%
:failed
echo.
if /I not "%~1"=="--check" pause
exit /b 1
