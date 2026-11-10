@echo off
setlocal
title J-PDF's - Compilar instalador
cd /d "%~dp0"

where powershell.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: Windows PowerShell no esta disponible.
    pause
    exit /b 1
)

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-JPDFs-Installer.ps1" %*
set "BUILD_EXIT=%ERRORLEVEL%"

if not "%BUILD_EXIT%"=="0" (
    echo.
    echo La compilacion no pudo completarse. Revisa el error anterior.
) else (
    echo.
    echo El instalador esta disponible en la raiz del repositorio.
)

pause
exit /b %BUILD_EXIT%
