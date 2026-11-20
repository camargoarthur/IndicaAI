param([string]$LauncherRoot=(Join-Path $PSScriptRoot '../scripts'),[int]$Port=5276,[switch]$ThroughBat,[string]$BatchPath,[string]$LogDirectory)
$ErrorActionPreference='Stop'
$taskProject=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if(!$LogDirectory){$LogDirectory=Join-Path $taskProject ('.tools/logs/launcher-'+[Guid]::NewGuid().ToString('N'))}
$null=New-Item -ItemType Directory -Path $LogDirectory -Force
$taskRoot=Join-Path $taskProject ('.local/launcher test-'+[Guid]::NewGuid().ToString('N'))
$taskScript=[IO.Path]::GetFullPath((Join-Path $LauncherRoot 'Start-IndicaAI.ps1'))
$taskBat=[IO.Path]::GetFullPath((Join-Path $LauncherRoot 'Start-IndicaAI.bat'))
if($BatchPath){$taskBat=[IO.Path]::GetFullPath($BatchPath)}
$taskHost=Join-Path $env:SystemRoot 'System32/WindowsPowerShell/v1.0/powershell.exe'
$taskOwner=$null;$taskServer=$null;$taskEntry=$null
# Falhar antes de iniciar processos se o ambiente bloquear a identificação dos filhos.
$null=Get-CimInstance Win32_Process -Filter "ProcessId=$PID" -ErrorAction Stop
function Assert-That($condition,$message){if(!$condition){throw $message}}
function Start-LauncherHost($logPrefix){
 if(!$ThroughBat){return Start-Process -FilePath $taskHost -ArgumentList $taskArguments -WindowStyle Hidden -PassThru -RedirectStandardOutput ($logPrefix+'-stdout.log') -RedirectStandardError ($logPrefix+'-stderr.log')}
 $taskBatchArguments='/d /s /c ""'+$taskBat+'" -DataRoot "'+$taskRoot+'" -Port '+$Port+' -NoBrowser"'
 $script:taskEntry=Start-Process -FilePath $env:ComSpec -ArgumentList $taskBatchArguments -WindowStyle Hidden -PassThru -RedirectStandardOutput ($logPrefix+'-stdout.log') -RedirectStandardError ($logPrefix+'-stderr.log')
 for($taskTry=0;$taskTry -lt 40;$taskTry++){
  $taskChild=@(Get-CimInstance Win32_Process -Filter "ParentProcessId=$($script:taskEntry.Id) AND Name='powershell.exe'")
  if($taskChild.Count -eq 1){return Get-Process -Id $taskChild[0].ProcessId}
  Start-Sleep -Milliseconds 100
 }
 throw 'FAIL: .bat não iniciou o host Windows PowerShell.'
}
try {
 Assert-That (Test-Path -LiteralPath $taskBat) 'FAIL: iniciador .bat ausente.'
 $taskArguments='-NoLogo -NoProfile -ExecutionPolicy RemoteSigned -File "'+$taskScript+'" -DataRoot "'+$taskRoot+'" -Port '+$Port+' -NoBrowser'
 $taskOwner=Start-LauncherHost (Join-Path $LogDirectory 'initial')
 for($taskAttempt=0;$taskAttempt -lt 80;$taskAttempt++){
  try {$taskResponse=Invoke-WebRequest -Uri "http://127.0.0.1:$Port/Library" -UseBasicParsing -TimeoutSec 1;if($taskResponse.StatusCode -eq 200){break}}catch{}
  if($taskOwner.HasExited){throw 'FAIL: iniciador encerrou antes de disponibilizar a biblioteca.'}
  Start-Sleep -Milliseconds 100
 }
 Assert-That ($taskResponse.StatusCode -eq 200) 'FAIL: biblioteca não respondeu.'
 # CIM TCP inventory is inconsistent in this harness; identify only the direct, owned child.
 $taskChildren=@(Get-CimInstance Win32_Process -Filter "ParentProcessId=$($taskOwner.Id) AND Name='IndicaAI.Web.exe'")
 Assert-That ($taskChildren.Count -eq 1) 'FAIL: esperado exatamente um servidor filho do iniciador.'
 $taskServer=Get-Process -Id $taskChildren[0].ProcessId
 # Terminate the launcher host without finally/normal shutdown: stronger than a normal console close.
 Stop-Process -Id $taskOwner.Id
 Assert-That ($taskServer.WaitForExit(5000)) 'FAIL: servidor órfão após encerrar o iniciador.'
 $taskStillResponds=$false
 try {$taskStillResponds=(Invoke-WebRequest -Uri "http://127.0.0.1:$Port/Library" -UseBasicParsing -TimeoutSec 1).StatusCode -eq 200}catch{}
 Assert-That (!$taskStillResponds) 'FAIL: biblioteca continua respondendo após encerramento.'
 if($taskEntry -and !$taskEntry.HasExited){Stop-Process -Id $taskEntry.Id}
 # Starting again against exactly the same data proves the lock was released and SQLite can reopen.
 $taskOwner=Start-LauncherHost (Join-Path $LogDirectory 'restart')
 $taskReopened=$false
 for($taskAttempt=0;$taskAttempt -lt 80;$taskAttempt++){
  try {$taskResponse=Invoke-WebRequest -Uri "http://127.0.0.1:$Port/Library" -UseBasicParsing -TimeoutSec 1;if($taskResponse.StatusCode -eq 200){$taskReopened=$true;break}}catch{}
  if($taskOwner.HasExited){break};Start-Sleep -Milliseconds 100
 }
 Assert-That $taskReopened 'FAIL: dados/lock não permitem reiniciar após fechamento.'
 $taskChildren=@(Get-CimInstance Win32_Process -Filter "ParentProcessId=$($taskOwner.Id) AND Name='IndicaAI.Web.exe'")
 Assert-That ($taskChildren.Count -eq 1) 'FAIL: servidor reiniciado não pertence ao iniciador.'
 $taskServer=Get-Process -Id $taskChildren[0].ProcessId
 Stop-Process -Id $taskOwner.Id
 Assert-That ($taskServer.WaitForExit(5000)) 'FAIL: servidor órfão após reinício.'
 Write-Output "PASS: Windows PowerShell 5.1 (bat=$ThroughBat): HTTP 200, owner killed => server exited and HTTP stopped, same data reopened, no orphan. DataRoot=$taskRoot; Logs=$LogDirectory"
} finally {
 if($taskOwner -and !$taskOwner.HasExited){Stop-Process -Id $taskOwner.Id -ErrorAction SilentlyContinue}
 if($taskServer -and !$taskServer.HasExited){Stop-Process -Id $taskServer.Id -ErrorAction SilentlyContinue}
 if($taskEntry -and !$taskEntry.HasExited){Stop-Process -Id $taskEntry.Id -ErrorAction SilentlyContinue}
}
