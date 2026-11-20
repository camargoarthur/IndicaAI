@echo off
rem Usar somente apos restaurar a copia, conforme TM39 do roteiro manual.
if not exist "%~dp0.local\teste-manual-restaurado-01\data\indicaai.db" (
  echo Copia restaurada ausente. Conclua o restore do TM39 antes de abrir.
  pause
  exit /b 1
)
call "%~dp0Iniciar-IndicaAI.bat" -DataRoot "%~dp0.local\teste-manual-restaurado-01" -Port 5274
