param(
 [ValidateSet('Auto','Docs','Visual','Functional','Module','Full')][string]$Profile='Auto',
 [string[]]$ChangedPath=@(),
 [string[]]$Module=@(),
 [switch]$PlanOnly
)
$ErrorActionPreference='Stop'
$taskArguments=@((Join-Path $PSScriptRoot 'validate.cjs'),'--profile',$Profile)
foreach($taskPath in $ChangedPath){$taskArguments+=@('--changed',$taskPath)}
foreach($taskModule in $Module){$taskArguments+=@('--module',$taskModule)}
if($PlanOnly){$taskArguments+='--plan'}
& node @taskArguments
exit $LASTEXITCODE
