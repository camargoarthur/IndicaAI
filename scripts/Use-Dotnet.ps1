$taskProjectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$env:DOTNET_ROOT = Join-Path $taskProjectRoot '.tools/dotnet'
$env:DOTNET_CLI_HOME = Join-Path $taskProjectRoot '.tools/cli-home'
$env:NUGET_PACKAGES = Join-Path $taskProjectRoot '.tools/nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
function dotnet { & (Join-Path $env:DOTNET_ROOT 'dotnet.exe') @args }
