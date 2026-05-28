@echo off
CHCP 1252
setlocal

:: -------------------------------------------------------
:: Fuehrt test.cmd in allen drei Sub-Repos in der
:: korrekten Abhaengigkeits-Reihenfolge aus.
:: -------------------------------------------------------

set "ROOT=%~dp0.."

call :run_test "josyn-foundation-result-pattern"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_test "josyn-foundation-property-bag"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_test "josyn-foundation-jip"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [OK] Alle Tests erfolgreich.
exit /b 0

:run_test
echo.
echo ======================================================
echo  %~1
echo ======================================================
call "%ROOT%\%~1\.local-build\test.cmd"
exit /b %ERRORLEVEL%
