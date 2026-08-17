function ConvertTo-DeterministicText([string] $Content) {
    if ($null -eq $Content) { return "" }
    return $Content.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Write-DeterministicTextIfChanged(
    [string] $Path,
    [string] $Content) {
    $normalized = ConvertTo-DeterministicText $Content
    if (Test-Path -LiteralPath $Path) {
        $current = ConvertTo-DeterministicText ([IO.File]::ReadAllText($Path))
        if ($current -ceq $normalized) { return $false }
    }

    $directory = Split-Path -Parent $Path
    [IO.Directory]::CreateDirectory($directory) | Out-Null
    $temporaryPath = Join-Path $directory (".{0}.{1}.tmp" -f
        [IO.Path]::GetFileName($Path), [Guid]::NewGuid().ToString("N"))
    try {
        [IO.File]::WriteAllText($temporaryPath, $normalized, [Text.UTF8Encoding]::new($false))
        $lastError = $null
        for ($attempt = 0; $attempt -lt 5; $attempt++) {
            try {
                if (Test-Path -LiteralPath $Path) {
                    [IO.File]::Replace($temporaryPath, $Path, $null, $true)
                }
                else {
                    [IO.File]::Move($temporaryPath, $Path)
                }
                return $true
            }
            catch [IO.IOException] {
                $lastError = $_
                if ($attempt -lt 4) { Start-Sleep -Milliseconds 50 }
            }
        }
        throw $lastError
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
}
