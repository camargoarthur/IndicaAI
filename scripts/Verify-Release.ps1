param([string]$PackageRoot=(Join-Path $PSScriptRoot '../artifacts/IndicaAI-win-x64'),[string]$ReportPath=(Join-Path $PSScriptRoot '../docs/evidence/RELEASE-ASSETS.json'))
$ErrorActionPreference='Stop'
$taskProjectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskPackageRoot=[IO.Path]::GetFullPath($PackageRoot)
foreach($taskRequired in @('IndicaAI.Web.exe','IndicaAI.Web.dll','IndicaAI.Web.runtimeconfig.json','pt-BR/IndicaAI.Web.resources.dll','en/IndicaAI.Web.resources.dll')){if(!(Test-Path -LiteralPath (Join-Path $taskPackageRoot $taskRequired))){throw "Arquivo obrigatório ausente: $taskRequired"}}
$taskSourceRoot=Join-Path $taskProjectRoot 'src/IndicaAI.Web/wwwroot'
$taskManifest=@()
foreach($taskFolder in @('covers','fonts','icons','css','js')){
 foreach($taskFile in Get-ChildItem -LiteralPath (Join-Path $taskSourceRoot $taskFolder) -File -Recurse){
  # Enumerated files are descendants of this root; Substring also works on Windows PowerShell 5.1.
  $taskRelative=$taskFile.FullName.Substring($taskSourceRoot.TrimEnd('\','/').Length+1).Replace('\','/')
  $taskPackaged=Join-Path $taskPackageRoot ('wwwroot/'+$taskRelative)
  if(!(Test-Path -LiteralPath $taskPackaged)){throw "Recurso ausente: $taskRelative"}
  $taskHash=(Get-FileHash -LiteralPath $taskFile.FullName -Algorithm SHA256).Hash
  if($taskHash -ne (Get-FileHash -LiteralPath $taskPackaged -Algorithm SHA256).Hash){throw "Hash diferente: $taskRelative"}
  $taskManifest += [pscustomobject]@{path='wwwroot/'+$taskRelative;sha256=$taskHash;bytes=$taskFile.Length}
 }
}
foreach($taskLauncherFile in @('Start-IndicaAI.bat','Start-IndicaAI.ps1','LauncherProcess.cs')){
 $taskSource=Join-Path $taskProjectRoot ('scripts/'+$taskLauncherFile)
 $taskPackaged=Join-Path $taskPackageRoot $taskLauncherFile
 if(!(Test-Path -LiteralPath $taskPackaged)){throw "Iniciador ausente: $taskLauncherFile"}
 $taskHash=(Get-FileHash -LiteralPath $taskSource -Algorithm SHA256).Hash
 if($taskHash -ne (Get-FileHash -LiteralPath $taskPackaged -Algorithm SHA256).Hash){throw "Iniciador divergente: $taskLauncherFile"}
 $taskManifest += [pscustomobject]@{path=$taskLauncherFile;sha256=$taskHash;bytes=(Get-Item -LiteralPath $taskSource).Length}
}
if((Get-ChildItem -LiteralPath $taskPackageRoot -Recurse -File|Where-Object {$_.Extension -in @('.db','.sqlite')}).Count -gt 0){throw 'O pacote contém um banco de dados; não distribuir dados pessoais.'}
$taskManifest|ConvertTo-Json -Depth 4|Set-Content -LiteralPath $ReportPath -Encoding utf8
Write-Host "Release verificado: $($taskManifest.Count) recursos idênticos; executável e idiomas presentes; nenhum banco incluído."
