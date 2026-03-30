@echo off
setlocal
title UltraWide Desert - Build EXE
cd /d "%~dp0"

echo.
echo   Building UltraWideDesert.exe with Nuitka...
echo.

py -3 -m nuitka ^
    --mode=onefile ^
    --output-filename=UltraWideDesert.exe ^
    --output-dir=dist ^
    --include-module=camera_mod ^
    --include-module=camera_rules ^
    --include-module=paz_crypto ^
    --include-module=paz_parse ^
    --include-module=paz_repack ^
    --include-package=cryptography ^
    --include-package=lz4 ^
    --include-package=customtkinter ^
    --include-package=darkdetect ^
    --enable-plugin=tk-inter ^
    --windows-console-mode=disable ^
    --company-name=TheFitzy ^
    --product-name=UltraWideDesert ^
    --file-version=1.3.0.0 ^
    --product-version=1.3.0.0 ^
    --file-description="UltraWide Desert - Widescreen Camera Mod" ^
    --assume-yes-for-downloads ^
    src\main.py

if errorlevel 1 (
    echo.
    echo   BUILD FAILED
    echo.
    pause
    exit /b 1
)

echo.
echo   BUILD SUCCESS!
echo   Output: dist\UltraWideDesert.exe
echo.
pause
