$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$manager = Join-Path $repositoryRoot (
    "eng/execution-ledgers/manage-e9-refactor-module-catalog.ps1")
$result = & $manager

if ([string] $result -ne
    "E9RefactorModuleCatalogValid:Modules=9;Status=Named;Order=E9-E1-E9") {
    throw "E9RefactorModuleCatalogValidationFailed:$result"
}

Write-Output "E9RefactorModuleCatalogTestsPassed:Modules=9;Named=9"
