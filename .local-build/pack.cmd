@echo off
CHCP 1252
setlocal

:: -------------------------------------------------------
:: Fuehrt pack.cmd in allen drei Sub-Repos in der
:: korrekten Abhaengigkeits-Reihenfolge aus.
:: -------------------------------------------------------

set "ROOT=%~dp0.."

call :run_pack "josyn-foundation-result-pattern"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-foundation-property-bag"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-foundation-jip"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [OK] Alle Pakete erfolgreich erstellt.
exit /b 0

:run_pack
echo.
echo ======================================================
echo  %~1
echo ======================================================
call "%ROOT%\%~1\.local-build\pack.cmd"
exit /b %ERRORLEVEL%
