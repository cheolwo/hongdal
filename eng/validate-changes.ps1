[CmdletBinding()]
param(
    [ValidateSet('Build', 'Test')]
    [string] $Action = 'Test',

    [string] $Target = 'Hongdal.Tests/Hongdal.Tests.csproj',

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $ArtifactsPath,

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string] $RunId,

    [string] $Filter,

    [switch] $NoRestore,

    [switch] $ImportOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-PathWithinRoot {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $Root
    )

    $normalizedRoot = [IO.Path]::GetFullPath($Root).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $normalizedPath = [IO.Path]::GetFullPath($Path)
    $rootWithSeparator = $normalizedRoot + [IO.Path]::DirectorySeparatorChar

    return $normalizedPath.Equals($normalizedRoot, [StringComparison]::OrdinalIgnoreCase) -or
        $normalizedPath.StartsWith($rootWithSeparator, [StringComparison]::OrdinalIgnoreCase)
}

function Get-ProjectClosure {
    param(
        [Parameter(Mandatory)]
        [string] $ProjectPath
    )

    $pending = [Collections.Generic.Queue[string]]::new()
    $visited = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $projects = [Collections.Generic.List[string]]::new()
    $pending.Enqueue([IO.Path]::GetFullPath($ProjectPath))

    while ($pending.Count -gt 0) {
        $current = $pending.Dequeue()
        if (-not $visited.Add($current)) {
            continue
        }

        if (-not (Test-Path -LiteralPath $current -PathType Leaf)) {
            throw "Project reference does not exist: $current"
        }

        $projects.Add($current)
        [xml] $projectXml = Get-Content -LiteralPath $current -Raw -Encoding UTF8
        foreach ($reference in @($projectXml.SelectNodes('//ProjectReference'))) {
            if ($null -eq $reference -or [string]::IsNullOrWhiteSpace($reference.Include)) {
                continue
            }

            $referencedPath = [IO.Path]::GetFullPath(
                [IO.Path]::Combine(
                    [IO.Path]::GetDirectoryName($current),
                    [string] $reference.Include))
            $pending.Enqueue($referencedPath)
        }
    }

    return $projects.ToArray()
}

function Get-MissingRestoreAssets {
    param(
        [Parameter(Mandatory)]
        [string[]] $ProjectPaths,

        [Parameter(Mandatory)]
        [string] $ArtifactsPath
    )

    $missing = foreach ($projectPath in $ProjectPaths) {
        $projectName = [IO.Path]::GetFileNameWithoutExtension($projectPath)
        $assetsPath = Join-Path $ArtifactsPath "obj/$projectName/project.assets.json"
        if (-not (Test-Path -LiteralPath $assetsPath -PathType Leaf)) {
            $projectName
        }
    }

    return @($missing)
}

function New-ValidationPlan {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [ValidateSet('Build', 'Test')]
        [string] $Action,

        [Parameter(Mandatory)]
        [string] $Target,

        [Parameter(Mandatory)]
        [ValidateSet('Debug', 'Release')]
        [string] $Configuration,

        [string] $ArtifactsPath,

        [string] $RunId,

        [string] $Filter,

        [switch] $NoRestore
    )

    $root = [IO.Path]::GetFullPath($RepositoryRoot)
    $targetPath = if ([IO.Path]::IsPathRooted($Target)) {
        [IO.Path]::GetFullPath($Target)
    }
    else {
        [IO.Path]::GetFullPath((Join-Path $root $Target))
    }

    if (-not (Test-PathWithinRoot -Path $targetPath -Root $root)) {
        throw "Target must stay inside the repository: $targetPath"
    }

    if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
        throw "Validation target does not exist: $targetPath"
    }

    if ([IO.Path]::GetExtension($targetPath) -ne '.csproj') {
        throw "Validation target must be a .csproj so restore assets can be checked explicitly: $targetPath"
    }

    if ([string]::IsNullOrWhiteSpace($RunId)) {
        $RunId = '{0:yyyyMMdd-HHmmss}-{1}' -f [DateTimeOffset]::Now, $PID
    }

    $resolvedArtifactsPath = if ([string]::IsNullOrWhiteSpace($ArtifactsPath)) {
        $targetName = [IO.Path]::GetFileNameWithoutExtension($targetPath)
        [IO.Path]::GetFullPath((Join-Path $root "artifacts/validation/$targetName/$RunId"))
    }
    elseif ([IO.Path]::IsPathRooted($ArtifactsPath)) {
        [IO.Path]::GetFullPath($ArtifactsPath)
    }
    else {
        [IO.Path]::GetFullPath((Join-Path $root $ArtifactsPath))
    }

    if (-not (Test-PathWithinRoot -Path $resolvedArtifactsPath -Root $root)) {
        throw "ArtifactsPath must stay inside the repository: $resolvedArtifactsPath"
    }

    $projectPaths = Get-ProjectClosure -ProjectPath $targetPath
    $restoreArguments = @(
        'restore',
        $targetPath,
        '--artifacts-path', $resolvedArtifactsPath,
        '--verbosity', 'minimal'
    )
    $validationArguments = @(
        $Action.ToLowerInvariant(),
        $targetPath,
        '--configuration', $Configuration,
        '--artifacts-path', $resolvedArtifactsPath,
        '--no-restore',
        '--verbosity', 'minimal'
    )
    $testResultsPath = $null

    if ($Action -eq 'Test') {
        $resultFileName = 'validation-{0:yyyyMMdd-HHmmss}-{1}.trx' -f [DateTimeOffset]::Now, $PID
        $resultsDirectory = Join-Path $resolvedArtifactsPath 'test-results'
        $testResultsPath = Join-Path $resultsDirectory $resultFileName
        $validationArguments += @(
            '--results-directory', $resultsDirectory,
            '--logger', "trx;LogFileName=$resultFileName"
        )

        if (-not [string]::IsNullOrWhiteSpace($Filter)) {
            $validationArguments += @('--filter', $Filter)
        }
    }
    elseif ($Action -eq 'Build' -and -not [string]::IsNullOrWhiteSpace($Filter)) {
        throw '-Filter is supported only when Action is Test.'
    }

    return [pscustomobject]@{
        RepositoryRoot = $root
        Target = $targetPath
        ProjectPaths = $projectPaths
        ArtifactsPath = $resolvedArtifactsPath
        RestoreArguments = $restoreArguments
        ValidationArguments = $validationArguments
        SkipRestore = [bool] $NoRestore
        TestResultsPath = $testResultsPath
    }
}

function Get-ExecutedTestCount {
    param(
        [Parameter(Mandatory)]
        [string] $ResultsPath
    )

    if (-not (Test-Path -LiteralPath $ResultsPath -PathType Leaf)) {
        throw "dotnet test exited successfully, but its TRX result was not created: $ResultsPath"
    }

    [xml] $results = Get-Content -LiteralPath $ResultsPath -Raw -Encoding UTF8
    $counters = $results.SelectSingleNode(
        "/*[local-name()='TestRun']/*[local-name()='ResultSummary']/*[local-name()='Counters']")
    if ($null -eq $counters -or -not $counters.HasAttribute('total')) {
        throw "The TRX result does not contain test counters: $ResultsPath"
    }

    return [int] $counters.GetAttribute('total')
}

function Assert-TestRunExecuted {
    param(
        [Parameter(Mandatory)]
        [string] $ResultsPath
    )

    $executedTestCount = Get-ExecutedTestCount -ResultsPath $ResultsPath
    if ($executedTestCount -eq 0) {
        throw 'dotnet test exited successfully but executed 0 tests. Check the test filter and target instead of treating this run as green.'
    }

    return $executedTestCount
}

function Invoke-DotNetPhase {
    param(
        [Parameter(Mandatory)]
        [string] $Phase,

        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    Write-Host "[$Phase] dotnet $($Arguments -join ' ')"
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        if ($Phase -eq 'restore') {
            throw "Restore phase failed with exit code $LASTEXITCODE. Build/test was not started; review the package source or network error above."
        }

        throw "Validation phase failed with exit code $LASTEXITCODE after restore succeeded. If MSB3021/MSB3027 is shown, retry with a new -RunId because a process is locking this run's isolated artifacts."
    }
}

function Invoke-Validation {
    param(
        [Parameter(Mandatory)]
        [pscustomobject] $Plan
    )

    Write-Host "[validation] target: $($Plan.Target)"
    Write-Host "[validation] artifacts: $($Plan.ArtifactsPath)"
    New-Item -ItemType Directory -Path $Plan.ArtifactsPath -Force | Out-Null

    if ($Plan.SkipRestore) {
        $missingAssets = @(Get-MissingRestoreAssets -ProjectPaths $Plan.ProjectPaths -ArtifactsPath $Plan.ArtifactsPath)
        if ($missingAssets.Count -gt 0) {
            $missingList = $missingAssets -join ', '
            throw "Restore was explicitly skipped, but isolated project.assets.json files are missing for: $missingList. Run again without -NoRestore and keep the same -ArtifactsPath for a later no-restore run."
        }

        Write-Host '[restore] skipped by explicit -NoRestore; isolated assets were found.'
    }
    else {
        Invoke-DotNetPhase -Phase 'restore' -Arguments $Plan.RestoreArguments
    }

    Invoke-DotNetPhase -Phase 'validation' -Arguments $Plan.ValidationArguments
    if (-not [string]::IsNullOrWhiteSpace($Plan.TestResultsPath)) {
        $executedTestCount = Assert-TestRunExecuted -ResultsPath $Plan.TestResultsPath
        Write-Host "[validation] executed tests: $executedTestCount"
    }

    Write-Host '[validation] completed successfully.'
}

if (-not $ImportOnly) {
    $repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    $planParameters = @{
        RepositoryRoot = $repositoryRoot
        Action = $Action
        Target = $Target
        Configuration = $Configuration
        ArtifactsPath = $ArtifactsPath
        RunId = $RunId
        Filter = $Filter
        NoRestore = $NoRestore
    }
    $plan = New-ValidationPlan @planParameters
    Invoke-Validation -Plan $plan
}
