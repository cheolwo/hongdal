$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot "eng/execution-ledgers/manage-e9-wi-h-module-bindings.ps1"
$result = & $manager
if ([string] $result -ne "E9WiHModuleBindingsValid:Bindings=15;Stages=9;Levels=H1-H4;SkeletonHeads=9;RuntimeWired=false") { throw "E9WiHModuleBindingsValidationFailed:$result" }
Write-Output "E9WiHModuleBindingsTestsPassed:Bindings=15;Stages=9;Levels=4;SkeletonHeads=9;RuntimeWired=false"
