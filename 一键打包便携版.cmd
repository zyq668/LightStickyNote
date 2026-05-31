@echo off
setlocal

cd /d "%~dp0"
title LightStickyNote Portable Builder

echo.
echo [1/3] Preparing portable package...
powershell -NoProfile -ExecutionPolicy Bypass -File ".\tools\Publish-Portable.ps1"
if errorlevel 1 goto :fail

echo.
echo [2/3] Opening artifacts folder...
start "" explorer "%cd%\artifacts"

echo.
echo [3/3] Portable package created successfully.
echo.
echo ZIP output:
echo   %cd%\artifacts\LightStickyNote-win-x64-portable.zip
echo.
echo EXE folder:
echo   %cd%\artifacts\LightStickyNote-win-x64-portable
echo.
pause
exit /b 0

:fail
echo.
echo Portable packaging failed. Please review the output above.
echo.
pause
exit /b 1
