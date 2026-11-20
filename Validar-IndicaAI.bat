@echo off
setlocal
title IndicaAI - validacao automatizada
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NoProfile -ExecutionPolicy RemoteSigned -File "%~dp0scripts\Validate.ps1" -Profile Full
set "INDICAAI_VALIDATION_EXIT=%ERRORLEVEL%"
echo.
if "%INDICAAI_VALIDATION_EXIT%"=="0" (echo Validacao aprovada.) else (echo Validacao reprovada. Consulte o resumo indicado acima.)
pause
exit /b %INDICAAI_VALIDATION_EXIT%
