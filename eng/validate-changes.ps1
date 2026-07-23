[CmdletBinding()]
param(
    [ValidateSet("Fast", "Task", "Release")]
    [string] $Level = "Fast",

    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Debug",

    [string[]] $Paths,

    [string] $BaseRef,

    [string] $TestFilter,

    [switch] $NoRestore,

    [switch] $PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$originalLocation = Get-Location

function Add-ChangedFiles {
    param(
        [System.Collections.Generic.List[string]] $Destination,
        [string[]] $GitArguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $lines = @(& git -c core.quotepath=false @GitArguments 2>$null)
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($exitCode -ne 0) {
        throw "git $($GitArguments -join ' ') failed."
    }

    foreach ($line in $lines) {
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $Destination.Add(($line.Trim() -replace "\\", "/"))
        }
    }
}

function Get-ChangedFiles {
    if ($null -ne $Paths -and $Paths.Count -gt 0) {
        return @(
            $Paths |
                Where-Object {
                    -not [string]::IsNullOrWhiteSpace($_)
                } |
                ForEach-Object {
                    $_ -split ","
                } |
                ForEach-Object {
                    $_.Trim() -replace "\\", "/"
                } |
                Where-Object {
                    $_ -notmatch "^(artifacts|bin|obj|\.vs)/"
                } |
                Sort-Object -Unique
        )
    }

    $files = New-Object "System.Collections.Generic.List[string]"

    if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
        Add-ChangedFiles -Destination $files -GitArguments @(
            "diff",
            "--name-only",
            "--diff-filter=ACMRTUXB",
            "$BaseRef...HEAD"
        )
    }

    Add-ChangedFiles -Destination $files -GitArguments @(
        "diff",
        "--name-only",
        "--diff-filter=ACMRTUXB",
        "HEAD"
    )
    Add-ChangedFiles -Destination $files -GitArguments @(
        "ls-files",
        "--others",
        "--exclude-standard"
    )

    $result = @(
        $files |
            Where-Object {
                $_ -notmatch "^(artifacts|bin|obj|\.vs)/"
            } |
            Sort-Object -Unique
    )

    if ($result.Count -eq 0) {
        & git rev-parse --verify "HEAD^" *> $null
        if ($LASTEXITCODE -eq 0) {
            Add-ChangedFiles -Destination $files -GitArguments @(
                "diff",
                "--name-only",
                "--diff-filter=ACMRTUXB",
                "HEAD^",
                "HEAD"
            )
            $result = @(
                $files |
                    Where-Object {
                        $_ -notmatch "^(artifacts|bin|obj|\.vs)/"
                    } |
                    Sort-Object -Unique
            )
        }
    }

    return $result
}

function Test-IsGuidanceOnly {
    param([string[]] $Files)

    if ($Files.Count -eq 0) {
        return $true
    }

    $workFiles = @(
        $Files | Where-Object {
            $_ -notmatch "^(docs/|eng/|\.codex/)" -and
            $_ -notmatch "(^|/)AGENTS\.md$" -and
            $_ -notmatch "\.(md|txt)$" -and
            $_ -notin @(".gitignore", ".gitattributes")
        }
    )

    return $workFiles.Count -eq 0
}

function Get-DirectBuildTargets {
    param([string[]] $Files)

    $targets = New-Object "System.Collections.Generic.HashSet[string]" (
        [System.StringComparer]::OrdinalIgnoreCase
    )

    foreach ($file in $Files) {
        $segments = $file -split "/"
        $topLevel = $segments[0]
        $candidate = Join-Path $repoRoot "$topLevel\$topLevel.csproj"

        if (Test-Path -LiteralPath $candidate) {
            [void] $targets.Add("$topLevel/$topLevel.csproj")
            continue
        }

        if ($file -match "\.slnx?$") {
            [void] $targets.Add($file)
            continue
        }

        if ($file -match "^(Directory\.(Build|Packages)\.(props|targets)|global\.json)$") {
            [void] $targets.Add("Ssalddel.v1.5.slnx")
        }
    }

    return @($targets | Sort-Object)
}

function Get-TaskSlice {
    param([string[]] $Files)

    $requiresLaterSlice = @(
        $Files | Where-Object {
            $_ -match "^(OrdererApp|SsalddelAdmin|SsalddelAdminApp|Ssalddel\.BackOffice\.Client)/" -or
            $_ -match "(GroupPurchase|Orderer|Trade|Customs|Import|Export)"
        }
    )

    $sharedChange = @(
        $Files | Where-Object {
            $_ -match "^(Ssalddel\.Contracts|Ssalddel\.Domain|Ssalddel\.Infrastructure|Ssalddel\.Ui\.Common)/"
        }
    )

    if ($requiresLaterSlice.Count -gt 0 -or $sharedChange.Count -gt 0) {
        return "Ssalddel.v1.5.slnx"
    }

    return "Ssalddel.v0.0.slnx"
}

function Get-RelatedTestFilter {
    param([string[]] $Files)

    if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
        return $TestFilter
    }

    $testClasses = New-Object "System.Collections.Generic.HashSet[string]" (
        [System.StringComparer]::OrdinalIgnoreCase
    )

    foreach ($file in $Files) {
        if ($file -match "^Ssalddel\.Tests/.+Tests\.cs$") {
            [void] $testClasses.Add([System.IO.Path]::GetFileNameWithoutExtension($file))
        }
    }

    $rg = Get-Command rg -ErrorAction SilentlyContinue
    if ($null -ne $rg) {
        $terms = @(
            $Files |
                Where-Object {
                    $_ -match "\.(cs|razor)$" -and
                    $_ -notmatch "^Ssalddel\.Tests/"
                } |
                ForEach-Object {
                    [System.IO.Path]::GetFileNameWithoutExtension($_)
                } |
                Where-Object {
                    $_.Length -ge 5
                } |
                Sort-Object -Unique |
                Select-Object -First 16
        )

        foreach ($term in $terms) {
            $matches = @(
                & rg -l --fixed-strings --glob "*Tests.cs" --glob "!bin/**" --glob "!obj/**" -- $term "Ssalddel.Tests" 2>$null
            )
            foreach ($match in $matches) {
                [void] $testClasses.Add([System.IO.Path]::GetFileNameWithoutExtension($match))
            }
        }
    }

    $selected = @($testClasses | Sort-Object | Select-Object -First 24)
    if ($selected.Count -eq 0) {
        return $null
    }

    return (($selected | ForEach-Object { "FullyQualifiedName~$_" }) -join "|")
}

function Show-FailureSummary {
    param([string] $LogPath)

    $matches = @(
        Select-String -LiteralPath $LogPath -Pattern (
            "error [A-Z]{2,}[0-9]+",
            "Build FAILED",
            "Failed!",
            "실패",
            "Exception"
        ) -Encoding UTF8 |
            Select-Object -First 30 |
            ForEach-Object { $_.Line.Trim() }
    )

    if ($matches.Count -gt 0) {
        $matches | ForEach-Object { Write-Host "  $_" }
        return
    }

    Get-Content -LiteralPath $LogPath -Encoding UTF8 -Tail 40 |
        ForEach-Object { Write-Host "  $_" }
}

function Invoke-LoggedCommand {
    param(
        [string] $Name,
        [string] $Executable,
        [string[]] $Arguments,
        [string] $LogPath
    )

    Write-Host "[run] $Name"
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & $Executable @Arguments *> $LogPath
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($exitCode -ne 0) {
        Write-Host "[fail] $Name (exit $exitCode)"
        Show-FailureSummary -LogPath $LogPath
        throw "$Name failed. Full log: $LogPath"
    }

    Write-Host "[pass] $Name"
}

try {
    Set-Location -LiteralPath $repoRoot

    if ($Level -eq "Release" -and -not $PSBoundParameters.ContainsKey("Configuration")) {
        $Configuration = "Release"
    }

    $changedFiles = @(Get-ChangedFiles)
    $guidanceOnly = Test-IsGuidanceOnly -Files $changedFiles
    $effectiveTestFilter = Get-RelatedTestFilter -Files $changedFiles
    $buildTargets = @()
    $runTests = $false
    $runFullTests = $false

    if (-not $guidanceOnly) {
        switch ($Level) {
            "Fast" {
                $buildTargets = @(Get-DirectBuildTargets -Files $changedFiles)
                if ($buildTargets.Count -gt 3) {
                    $buildTargets = @(Get-TaskSlice -Files $changedFiles)
                }
                $runTests = -not [string]::IsNullOrWhiteSpace($effectiveTestFilter)
            }
            "Task" {
                $buildTargets = @(Get-TaskSlice -Files $changedFiles)
                $runTests = $true
                $runFullTests = [string]::IsNullOrWhiteSpace($effectiveTestFilter)
            }
            "Release" {
                $buildTargets = @(
                    "Ssalddel.v0.0.slnx",
                    "Ssalddel.v1.0.slnx",
                    "Ssalddel.v1.5.slnx"
                )
                $runTests = $true
                $runFullTests = $true
            }
        }
    }

    Write-Host "Validation plan"
    Write-Host "  Level: $Level"
    Write-Host "  Configuration: $Configuration"
    Write-Host "  Changed paths: $($changedFiles.Count)"
    Write-Host "  Guidance/docs only: $guidanceOnly"
    if ($buildTargets.Count -eq 0) {
        Write-Host "  Build: skipped"
    }
    else {
        Write-Host "  Build: $($buildTargets -join ', ')"
    }
    if (-not $runTests) {
        Write-Host "  Tests: skipped"
    }
    elseif ($runFullTests) {
        Write-Host "  Tests: full suite"
    }
    else {
        $filterCount = @($effectiveTestFilter -split "\|").Count
        Write-Host "  Tests: targeted ($filterCount filters)"
    }

    if ($PlanOnly) {
        exit 0
    }

    $runId = Get-Date -Format "yyyyMMdd-HHmmss"
    $runDirectory = Join-Path $repoRoot "artifacts\local\validation\$runId"
    New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null

    Invoke-LoggedCommand `
        -Name "git diff --check" `
        -Executable "git" `
        -Arguments @("diff", "--check") `
        -LogPath (Join-Path $runDirectory "diff-check.log")

    $buildIndex = 0
    foreach ($target in $buildTargets) {
        $buildIndex++
        $arguments = @(
            "build",
            $target,
            "--configuration",
            $Configuration,
            "--nologo",
            "--verbosity",
            "quiet"
        )
        if ($NoRestore) {
            $arguments += "--no-restore"
        }

        Invoke-LoggedCommand `
            -Name "build $target" `
            -Executable "dotnet" `
            -Arguments $arguments `
            -LogPath (Join-Path $runDirectory ("build-{0:D2}.log" -f $buildIndex))
    }

    if ($runTests) {
        $testArguments = @(
            "test",
            "Ssalddel.Tests/Ssalddel.Tests.csproj",
            "--configuration",
            $Configuration,
            "--nologo",
            "--logger",
            "trx;LogFileName=ssalddel-tests.trx",
            "--results-directory",
            $runDirectory
        )
        if ($NoRestore) {
            $testArguments += "--no-restore"
        }
        $testsAlreadyBuilt = @(
            $buildTargets | Where-Object {
                $_ -match "\.slnx?$" -or
                $_ -eq "Ssalddel.Tests/Ssalddel.Tests.csproj"
            }
        ).Count -gt 0
        if ($Level -ne "Fast" -or $testsAlreadyBuilt) {
            $testArguments += "--no-build"
        }
        if (-not $runFullTests) {
            $testArguments += @("--filter", $effectiveTestFilter)
        }

        Invoke-LoggedCommand `
            -Name $(if ($runFullTests) { "full tests" } else { "targeted tests" }) `
            -Executable "dotnet" `
            -Arguments $testArguments `
            -LogPath (Join-Path $runDirectory "tests.log")
    }

    Write-Host "Validation complete"
    Write-Host "  Detailed logs: $runDirectory"
}
finally {
    Set-Location -LiteralPath $originalLocation
}
