@echo off
:: Batch script to elevate and run MySQL password reset
title Reset MySQL Password
echo ========================================================
echo   Requesting Administrator Privileges...
echo ========================================================

net session >nul 2>&1
if %errorLevel% == 0 (
    goto :run_as_admin
) else (
    echo Elevating to Administrator...
    powershell -Command "Start-Process cmd -ArgumentList '/c `\"%~f0`\"' -Verb RunAs"
    exit /b
)

:run_as_admin
cd /d "%~dp0"
cls
echo ========================================================
echo   RESETTING MYSQL ROOT PASSWORD TO: root
echo ========================================================
echo.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0reset_mysql_password.ps1"
echo.
echo Press any key to exit this window...
pause >nul
