$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
& (Join-Path $repositoryRoot "eng/execution-ledgers/manage-world-interaction-maturity.ps1") -Mode Check
