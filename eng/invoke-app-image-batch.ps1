[CmdletBinding()]
param(
    [ValidateSet("Preview", "Submit", "Status")]
    [string]$Mode = "Preview",

    [string]$PromptPack,

    [string]$BatchManifest,

    # Build the Korean project directory from code points so Windows
    # PowerShell 5.1 can parse this UTF-8 script without path mojibake.
    [string]$HongikEnvPath = (Join-Path `
        ([Environment]::GetFolderPath("MyDocuments")) `
        ((-join @(
            [char]0xD559,
            [char]0xB2F9,
            [char]0xC601,
            [char]0xC0C1)) + "\.env")),

    [switch]$ConfirmBillable
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$serverProject = Join-Path $repositoryRoot "Ssalddel\Ssalddel.csproj"

function Get-DotEnvValue {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Local secret file was not found: $Path"
    }

    foreach ($line in Get-Content -LiteralPath $Path -Encoding UTF8) {
        if ($line -notmatch '^\s*(?:export\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') {
            continue
        }

        if (-not [string]::Equals(
                $Matches[1],
                $Name,
                [System.StringComparison]::Ordinal)) {
            continue
        }

        $value = $Matches[2].Trim()
        if (($value.StartsWith('"') -and $value.EndsWith('"')) -or
            ($value.StartsWith("'") -and $value.EndsWith("'"))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        if ([string]::IsNullOrWhiteSpace($value)) {
            throw "$Name is empty."
        }

        return $value
    }

    throw "$Name was not found in the local secret file."
}

if ($Mode -in @("Preview", "Submit") -and
    [string]::IsNullOrWhiteSpace($PromptPack)) {
    throw "Preview and Submit require -PromptPack."
}

if ($Mode -eq "Status" -and
    [string]::IsNullOrWhiteSpace($BatchManifest)) {
    throw "Status requires -BatchManifest."
}

if ($Mode -eq "Submit" -and -not $ConfirmBillable) {
    throw "Billable Batch submission requires -ConfirmBillable."
}

$commandArguments = @(
    "run",
    "--project",
    $serverProject,
    "--no-launch-profile",
    "--"
)

switch ($Mode) {
    "Preview" {
        $commandArguments += "--app-image-batch-preview"
        $commandArguments += "--prompt-pack=$PromptPack"
    }
    "Submit" {
        $commandArguments += "--app-image-batch-submit"
        $commandArguments += "--prompt-pack=$PromptPack"
        $commandArguments += "--confirm-billable=true"
    }
    "Status" {
        $commandArguments += "--app-image-batch-status"
        $commandArguments += "--batch-manifest=$BatchManifest"
    }
}

$needsApiKey = $Mode -in @("Submit", "Status")
$previousApiKey = $env:GeminiImageBatch__ApiKey
$previousEnabled = $env:GeminiImageBatch__Enabled
$previousRequireCertificate = $env:PersonalDataProtection__RequireCertificate

try {
    # This command does not start the HTTP server, so it does not need to
    # initialize the server's encrypted Data Protection key ring.
    $env:PersonalDataProtection__RequireCertificate = "false"
    if ($needsApiKey) {
        $env:GeminiImageBatch__ApiKey = Get-DotEnvValue `
            -Path $HongikEnvPath `
            -Name "GEMINI_API_KEY"
        $env:GeminiImageBatch__Enabled = "true"
    }

    & dotnet @commandArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Ssalddel app image Batch command failed with exit code $LASTEXITCODE."
    }
}
finally {
    $env:GeminiImageBatch__ApiKey = $previousApiKey
    $env:GeminiImageBatch__Enabled = $previousEnabled
    $env:PersonalDataProtection__RequireCertificate = $previousRequireCertificate
}
