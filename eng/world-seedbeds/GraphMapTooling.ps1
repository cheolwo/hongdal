[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $RepositoryRoot,
    [Parameter(Mandatory = $true)]
    [string] $ErrorPrefix
)

# Graph Map 생성기와 인계 관리자가 공유하는 결정적 파일 경계다.
# 호출 스크립트가 고유 오류 접두사를 넘겨 기존 진단 계약을 보존한다.
$resolvedRepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
$script:GraphMapToolingRepositoryRoot = $resolvedRepositoryRoot.TrimEnd(
    [char[]] @([IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar))
$script:GraphMapToolingErrorPrefix = $ErrorPrefix

function Require([bool] $condition, [string] $code) {
    if (-not $condition) {
        throw "$script:GraphMapToolingErrorPrefix`:$code"
    }
}

function Require-Text([object] $value, [string] $code) {
    Require ($null -ne $value -and
        -not [string]::IsNullOrWhiteSpace([string] $value)) $code
}

function Resolve-RepoChild(
    [string] $relativePath,
    [string] $code = 'RepositoryPath') {
    Require-Text $relativePath "$code`:Empty"
    Require (-not [IO.Path]::IsPathRooted($relativePath)) "$code`:Rooted"
    Require (-not (@($relativePath -split '[/\\]') -contains '..')) "$code`:Traversal"

    $childPath = $relativePath -replace '/', [IO.Path]::DirectorySeparatorChar
    $combinedPath = Join-Path $script:GraphMapToolingRepositoryRoot $childPath
    $candidate = [IO.Path]::GetFullPath($combinedPath)
    $prefix = $script:GraphMapToolingRepositoryRoot +
        [IO.Path]::DirectorySeparatorChar
    Require ($candidate.StartsWith($prefix,
            [StringComparison]::OrdinalIgnoreCase)) "$code`:OutsideRoot"
    return $candidate
}

function Resolve-RepoPath([string] $relativePath) {
    return Resolve-RepoChild $relativePath 'RepositoryPath'
}

function Read-Json(
    [string] $relativePath,
    [string] $code = 'Json') {
    $path = Resolve-RepoChild $relativePath $code
    if ($script:GraphMapToolingErrorPrefix -eq 'GraphMapInvalid' -and
        $code -eq 'Json') {
        Require (Test-Path -LiteralPath $path -PathType Leaf) "JsonMissing:$relativePath"
    }
    else {
        Require (Test-Path -LiteralPath $path -PathType Leaf) "$code`:Missing:$relativePath"
    }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 |
        ConvertFrom-Json
}

function File-Hash(
    [string] $relativePath,
    [string] $code = 'File') {
    $path = Resolve-RepoChild $relativePath $code
    Require (Test-Path -LiteralPath $path -PathType Leaf) "$code`:Missing:$relativePath"
    return ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash).ToLowerInvariant()
}

function Require-Hash(
    [string] $relativePath,
    [object] $expected,
    [string] $code) {
    Require-Text $expected "$code`:ExpectedEmpty"
    Require ([string] $expected -match '^[0-9a-fA-F]{64}$') "$code`:ExpectedFormat"
    $actual = File-Hash $relativePath $code
    Require ($actual -eq ([string] $expected).ToLowerInvariant()) "$code`:HashMismatch"
    return $actual
}

function Require-Unique(
    [object[]] $values,
    [scriptblock] $selector,
    [string] $code) {
    $seen = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    foreach ($value in @($values)) {
        $key = [string] (& $selector $value)
        Require-Text $key "$code`:Empty"
        Require ($seen.Add($key)) "$code`:$key"
    }
}

function Stable-Json([object] $value) {
    return (($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n") + "`n"
}

function Normalize-Text([string] $value) {
    return (($value -replace "`r`n", "`n").TrimEnd()) + "`n"
}

function Escape-Cell([object] $value) {
    if ($null -eq $value) { return '' }
    return ([string] $value).Replace('|', '\|').Replace("`r", '').Replace("`n", '<br>')
}
