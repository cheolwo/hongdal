$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $root 'eng/world-seedbeds/GraphMapTooling.ps1') `
    -RepositoryRoot $root `
    -ErrorPrefix 'GraphMapToolingTest'

$checks = 0

function Assert([bool] $condition, [string] $code) {
    if (-not $condition) { throw "GraphMapToolingTestFailed:$code" }
    $script:checks++
}

function Reject([scriptblock] $action, [string] $expected) {
    try {
        & $action
        throw "GraphMapToolingTestFailed:ExpectedReject:$expected"
    }
    catch {
        Assert ($_.Exception.Message -like "*$expected*") "Reject:$expected"
    }
}

$catalogRef = 'eng/execution-ledgers/world-interactions.json'
$catalogPath = Resolve-RepoChild $catalogRef 'Catalog'
Assert ($catalogPath.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) `
    'RepositoryChild'

$catalog = Read-Json $catalogRef 'Catalog'
Assert (@($catalog.items).Count -gt 0) 'JsonRead'

$hash = File-Hash $catalogRef 'Catalog'
Assert ($hash -cmatch '^[0-9a-f]{64}$') 'LowercaseHash'
Assert ((Require-Hash $catalogRef $hash 'Catalog') -ceq $hash) 'HashMatch'

$normalized = Normalize-Text "first`r`nsecond`r`n`r`n"
Assert ($normalized -ceq "first`nsecond`n") 'TextNormalization'

$json = Stable-Json ([ordered]@{ value = 1 })
Assert ($json.EndsWith("`n", [StringComparison]::Ordinal)) 'JsonFinalNewline'
Assert (-not $json.Contains("`r", [StringComparison]::Ordinal)) 'JsonLfOnly'
Assert ((Escape-Cell "a|b`nc") -ceq 'a\|b<br>c') 'MarkdownEscape'

Require-Unique @('a', 'b') { param($value) $value } 'Unique'
$checks++
Reject { Require-Unique @('a', 'a') { param($value) $value } 'Unique' } `
    'GraphMapToolingTest:Unique:a'
Reject { Resolve-RepoChild '../outside.json' 'Unsafe' } `
    'GraphMapToolingTest:Unsafe:Traversal'
Reject { Resolve-RepoChild ([IO.Path]::GetFullPath($catalogPath)) 'Unsafe' } `
    'GraphMapToolingTest:Unsafe:Rooted'
Reject { Require $false 'PrefixPreserved' } `
    'GraphMapToolingTest:PrefixPreserved'

Write-Output "Graph Map tooling tests passed: $checks"
