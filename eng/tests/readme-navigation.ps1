[CmdletBinding()]
param(
    [string] $Revision,
    [string[]] $Paths = @('README.md', 'docs/AI/PLANNING.md',
        'docs/AI/Planning/스토리/PLAN-STORY-HEXAGRAM-SEQUENCE-001/README.md')
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Push-Location $root
try {
    $tree = @()
    if ($Revision) {
        $resolved = git rev-parse --verify "$Revision^{commit}"
        if ($LASTEXITCODE -ne 0) { throw "Invalid revision: $Revision" }
        $tree = @(git -c core.quotepath=false ls-tree -r --name-only $resolved)
    }
    $failures = [Collections.Generic.List[string]]::new()
    $checked = 0
    foreach ($path in $Paths) {
        if ($Revision) {
            $lines = @(git show "${resolved}:$path" 2>$null)
            if ($LASTEXITCODE -ne 0) { $failures.Add("Missing document: $path"); continue }
        } else { $lines = @(Get-Content -LiteralPath $path -Encoding utf8) }
        for ($i = 0; $i -lt $lines.Count; $i++) {
            foreach ($match in [regex]::Matches($lines[$i], '\]\(([^)]+)\)|(?:src|href)="([^"]+)"')) {
                $ref = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { $match.Groups[2].Value }
                if ($ref -match '^(https?:|mailto:|#)') { continue }
                $relative = [Uri]::UnescapeDataString(($ref -split '#')[0].Trim('<', '>'))
                if (-not $relative) { continue }
                $absolute = [IO.Path]::GetFullPath((Join-Path (Join-Path $root ([IO.Path]::GetDirectoryName($path))) $relative))
                if (-not $absolute.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
                    $failures.Add("Outside repository: ${path}:$($i+1) $ref"); continue
                }
                $target = [IO.Path]::GetRelativePath($root, $absolute).Replace('\', '/').TrimEnd('/')
                $exists = if ($Revision) {
                    ($tree -ccontains $target) -or @($tree | Where-Object { $_.StartsWith($target + '/', [StringComparison]::Ordinal) }).Count -gt 0
                } else { Test-Path -LiteralPath $absolute }
                $checked++
                if (-not $exists) { $failures.Add("${path}:$($i+1) -> $target") }
            }
        }
    }
    Write-Output "References=$checked Missing=$($failures.Count) Revision=$Revision"
    $failures | Write-Output
    if ($failures.Count) { exit 1 }
} finally { Pop-Location }
