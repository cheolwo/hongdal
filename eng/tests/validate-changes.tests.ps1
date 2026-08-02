[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\validate-changes.ps1'))
. $scriptPath -ImportOnly

$script:Passed = 0
$script:Failed = 0

function Assert-True {
    param(
        [Parameter(Mandatory)]
        [bool] $Condition,

        [Parameter(Mandatory)]
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Assert-ThrowsLike {
    param(
        [Parameter(Mandatory)]
        [scriptblock] $Action,

        [Parameter(Mandatory)]
        [string] $Pattern
    )

    try {
        & $Action
    }
    catch {
        Assert-True -Condition ($_.Exception.Message -like $Pattern) -Message "Expected error like '$Pattern', got '$($_.Exception.Message)'."
        return
    }

    throw "Expected an error like '$Pattern', but no error was raised."
}

function Invoke-TestCase {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [scriptblock] $Test
    )

    try {
        & $Test
        $script:Passed++
        Write-Host "PASS $Name"
    }
    catch {
        $script:Failed++
        Write-Host "FAIL $Name"
        Write-Host $_.Exception.Message
    }
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) "hongdal-validation-tests-$PID"
try {
    New-Item -ItemType Directory -Path $temporaryRoot -Force | Out-Null
    $appDirectory = Join-Path $temporaryRoot 'App'
    $libraryDirectory = Join-Path $temporaryRoot 'Library'
    New-Item -ItemType Directory -Path $appDirectory, $libraryDirectory -Force | Out-Null

    $appProject = Join-Path $appDirectory 'App.csproj'
    $libraryProject = Join-Path $libraryDirectory 'Library.csproj'
    Set-Content -LiteralPath $appProject -Encoding UTF8 -Value @'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\Library\Library.csproj" />
  </ItemGroup>
</Project>
'@
    Set-Content -LiteralPath $libraryProject -Encoding UTF8 -Value '<Project Sdk="Microsoft.NET.Sdk" />'

    Invoke-TestCase 'project closure includes transitive project references' {
        $closure = @(Get-ProjectClosure -ProjectPath $appProject)
        Assert-True -Condition ($closure.Count -eq 2) -Message "Expected 2 projects, got $($closure.Count)."
        Assert-True -Condition ($closure -contains [IO.Path]::GetFullPath($libraryProject)) -Message 'Library project was not included in the closure.'
    }

    Invoke-TestCase 'default plan restores before an isolated no-restore validation' {
        $plan = New-ValidationPlan -RepositoryRoot $temporaryRoot -Action Test -Target $appProject -Configuration Debug -RunId 'stable-test'

        Assert-True -Condition ($plan.RestoreArguments[0] -eq 'restore') -Message 'Restore must be the first phase.'
        Assert-True -Condition ($plan.ValidationArguments[0] -eq 'test') -Message 'Test must be the validation action.'
        Assert-True -Condition ($plan.ValidationArguments -contains '--no-restore') -Message 'Validation must not perform an implicit second restore.'
        Assert-True -Condition ($plan.RestoreArguments -contains $plan.ArtifactsPath) -Message 'Restore did not receive the isolated artifacts path.'
        Assert-True -Condition ($plan.ValidationArguments -contains $plan.ArtifactsPath) -Message 'Validation did not receive the isolated artifacts path.'
        Assert-True -Condition ($plan.ValidationArguments -contains '--logger') -Message 'Test validation must write a machine-readable result.'
        Assert-True -Condition ($plan.TestResultsPath.EndsWith('.trx')) -Message 'Test result path must point to a TRX file.'
    }

    Invoke-TestCase 'no-restore preflight reports every missing project asset' {
        $artifactsPath = Join-Path $temporaryRoot 'artifacts\validation\assets-test'
        New-Item -ItemType Directory -Path (Join-Path $artifactsPath 'obj\App') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $artifactsPath 'obj\App\project.assets.json') -Value '{}'
        $projects = @($appProject, $libraryProject)

        $missing = @(Get-MissingRestoreAssets -ProjectPaths $projects -ArtifactsPath $artifactsPath)
        Assert-True -Condition ($missing.Count -eq 1) -Message "Expected one missing asset, got $($missing.Count)."
        Assert-True -Condition ($missing[0] -eq 'Library') -Message 'Missing dependency name was not reported.'
    }

    Invoke-TestCase 'no-restore preflight accepts a complete isolated restore' {
        $artifactsPath = Join-Path $temporaryRoot 'artifacts\validation\complete-assets-test'
        foreach ($projectName in @('App', 'Library')) {
            $projectObjectDirectory = Join-Path $artifactsPath "obj\$projectName"
            New-Item -ItemType Directory -Path $projectObjectDirectory -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $projectObjectDirectory 'project.assets.json') -Value '{}'
        }

        $missing = @(Get-MissingRestoreAssets -ProjectPaths @($appProject, $libraryProject) -ArtifactsPath $artifactsPath)
        Assert-True -Condition ($missing.Count -eq 0) -Message "Expected no missing assets, got $($missing.Count)."
    }

    Invoke-TestCase 'targets outside the repository are rejected' {
        $insideRoot = Join-Path $temporaryRoot 'inside'
        New-Item -ItemType Directory -Path $insideRoot -Force | Out-Null
        Assert-ThrowsLike -Pattern '*Target must stay inside the repository*' -Action {
            New-ValidationPlan -RepositoryRoot $insideRoot -Action Build -Target $appProject -Configuration Debug -RunId 'outside-test'
        }
    }

    Invoke-TestCase 'filters are rejected for build actions' {
        Assert-ThrowsLike -Pattern '*Filter is supported only*' -Action {
            New-ValidationPlan -RepositoryRoot $temporaryRoot -Action Build -Target $appProject -Configuration Debug -RunId 'filter-test' -Filter 'Category=Fast'
        }
    }

    Invoke-TestCase 'TRX counters expose zero-test false positives' {
        $resultsPath = Join-Path $temporaryRoot 'zero-tests.trx'
        Set-Content -LiteralPath $resultsPath -Encoding UTF8 -Value @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary>
    <Counters total="0" executed="0" passed="0" failed="0" />
  </ResultSummary>
</TestRun>
'@
        Assert-ThrowsLike -Pattern '*executed 0 tests*' -Action {
            Assert-TestRunExecuted -ResultsPath $resultsPath
        }
    }

    Invoke-TestCase 'TRX counters report executed tests' {
        $resultsPath = Join-Path $temporaryRoot 'executed-tests.trx'
        Set-Content -LiteralPath $resultsPath -Encoding UTF8 -Value @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary>
    <Counters total="12" executed="12" passed="12" failed="0" />
  </ResultSummary>
</TestRun>
'@
        $executed = Assert-TestRunExecuted -ResultsPath $resultsPath
        Assert-True -Condition ($executed -eq 12) -Message "Expected 12 executed tests, got $executed."
    }
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        $resolvedTemporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
        $systemTemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        $expectedLeaf = "hongdal-validation-tests-$PID"
        if (-not $resolvedTemporaryRoot.StartsWith($systemTemporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
            [IO.Path]::GetFileName($resolvedTemporaryRoot) -ne $expectedLeaf) {
            throw "Refusing to remove unexpected test directory: $resolvedTemporaryRoot"
        }

        Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force
    }
}

Write-Host "PowerShell regression tests: $script:Passed passed, $script:Failed failed."
if ($script:Failed -gt 0) {
    exit 1
}
