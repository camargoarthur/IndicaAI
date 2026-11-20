@echo off
setlocal
title IndicaAI - feche esta janela para encerrar
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NoProfile -ExecutionPolicy RemoteSigned -File "%~dp0Start-IndicaAI.ps1" %*
set "INDICAAI_LAUNCH_EXIT=%ERRORLEVEL%"
if not "%INDICAAI_LAUNCH_EXIT%"=="0" (
  echo.
  echo O IndicaAI foi encerrado ou nao conseguiu iniciar. Consulte a mensagem acima.
  pause
)
exit /b %INDICAAI_LAUNCH_EXIT%
