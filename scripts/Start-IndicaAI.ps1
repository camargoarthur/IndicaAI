param([string]$DataRoot = (Join-Path $env:LOCALAPPDATA 'IndicaAI'), [ValidateRange(1024,65535)][int]$Port = 5273, [switch]$NoBrowser)
$ErrorActionPreference = 'Stop'
$taskPackageRoot = if(Test-Path -LiteralPath (Join-Path $PSScriptRoot 'IndicaAI.Web.exe')){$PSScriptRoot}else{Join-Path $PSScriptRoot '../artifacts/IndicaAI-win-x64'}
$taskExe = [IO.Path]::GetFullPath((Join-Path $taskPackageRoot 'IndicaAI.Web.exe'))
if(!(Test-Path -LiteralPath $taskExe)){throw 'Pacote local ausente. Consulte docs/OPERATIONS.md para gerar o Release.'}
if(!('IndicaAI.Launcher.ServerProcess' -as [type])){Add-Type -Path (Join-Path $PSScriptRoot 'LauncherProcess.cs')}
$taskOldRoot=$env:INDICAAI_DATA_ROOT; $taskOldPort=$env:INDICAAI_PORT
$env:INDICAAI_DATA_ROOT=[IO.Path]::GetFullPath($DataRoot);$env:INDICAAI_PORT=$Port.ToString()
$taskUrl="http://127.0.0.1:$Port/Library"
$taskBrowserJob=$null
$taskServer=$null
Write-Host "IndicaAI: $taskUrl"
Write-Host "Dados: $env:INDICAAI_DATA_ROOT"
Write-Host 'Feche esta janela ou use Ctrl+C para encerrar o servidor local.'
try {
 $taskServer=[IndicaAI.Launcher.ServerProcess]::Start($taskExe,[IO.Path]::GetDirectoryName($taskExe))
 if(!$NoBrowser){$taskBrowserJob=Start-Job -ArgumentList $taskUrl -ScriptBlock {param($url) for($attempt=0;$attempt -lt 40;$attempt++){try{$response=Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 1;if($response.StatusCode -eq 200){Start-Process $url;break}}catch{};Start-Sleep -Milliseconds 250}}}
 while(!$taskServer.WaitForExit(200)){}
 if($taskServer.ExitCode -ne 0){throw "O servidor terminou com código $($taskServer.ExitCode). Os dados existentes foram preservados."}
} finally {if($taskServer){$taskServer.Dispose()};if($taskBrowserJob){Stop-Job $taskBrowserJob -ErrorAction SilentlyContinue;Remove-Job $taskBrowserJob -Force -ErrorAction SilentlyContinue};$env:INDICAAI_DATA_ROOT=$taskOldRoot;$env:INDICAAI_PORT=$taskOldPort}
