[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PublishRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string[]]$AllowedOrigins,

    [ValidatePattern('^[A-Z][A-Z0-9_]*$')]
    [string]$EnvironmentVariableName = 'SSALDDEL_GOOGLE_MAPS_BROWSER_API_KEY',

    [switch]$AllowLoopback,

    [string]$UserSecretsProject,

    [string]$UserSecretName = 'GoogleMaps:BrowserApiKey'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$sourceRuntimeConfigPath = [System.IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot 'Ssalddel.WebApp\wwwroot\runtime-config.js'))

function Get-BrowserApiKey {
    $environmentValue = [Environment]::GetEnvironmentVariable(
        $EnvironmentVariableName,
        [EnvironmentVariableTarget]::Process)
    if (-not [string]::IsNullOrWhiteSpace($environmentValue)) {
        return $environmentValue.Trim()
    }

    if ([string]::IsNullOrWhiteSpace($UserSecretsProject)) {
        throw "$EnvironmentVariableName is not set. Configure the dedicated browser key in the deployment secret store."
    }

    $secretLines = @(& dotnet user-secrets list --project $UserSecretsProject 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw 'The configured user-secrets project could not be read.'
    }

    $prefix = "$UserSecretName = "
    foreach ($line in $secretLines) {
        if ($line.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            $value = $line.Substring($prefix.Length).Trim()
            if (-not [string]::IsNullOrWhiteSpace($value)) {
                return $value
            }
        }
    }

    throw "$UserSecretName was not found. Do not fall back to a unified or server API key."
}

function Get-NormalizedOrigin {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $uri = $null
    if (-not [System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$uri)) {
        throw "Allowed origin is not an absolute URI: $Value"
    }
    if ($uri.AbsolutePath -ne '/' -or -not [string]::IsNullOrEmpty($uri.Query) -or -not [string]::IsNullOrEmpty($uri.Fragment)) {
        throw "Allowed origin must not contain a path, query, or fragment: $Value"
    }
    if (-not [string]::IsNullOrEmpty($uri.UserInfo)) {
        throw "Allowed origin must not contain user information: $Value"
    }

    if ($uri.IsLoopback) {
        if (-not $AllowLoopback) {
            throw "Loopback origins require the explicit -AllowLoopback switch: $Value"
        }
        if ($uri.Scheme -ne [System.Uri]::UriSchemeHttp -and $uri.Scheme -ne [System.Uri]::UriSchemeHttps) {
            throw "Loopback origin must use HTTP or HTTPS: $Value"
        }
    }
    elseif ($uri.Scheme -ne [System.Uri]::UriSchemeHttps) {
        throw "Allowed origin must use HTTPS unless it is loopback development: $Value"
    }

    return $uri.GetLeftPart([System.UriPartial]::Authority).TrimEnd('/')
}

$publishRootPath = [System.IO.Path]::GetFullPath($PublishRoot)
$publishIndexPath = Join-Path $publishRootPath 'wwwroot\index.html'
if (-not (Test-Path -LiteralPath $publishIndexPath -PathType Leaf)) {
    throw "Published WebApp index was not found: $publishIndexPath"
}

$targetRuntimeConfigPath = [System.IO.Path]::GetFullPath(
    (Join-Path $publishRootPath 'wwwroot\runtime-config.js'))
if ([string]::Equals(
        $targetRuntimeConfigPath,
        $sourceRuntimeConfigPath,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'Refusing to inject a browser key into the tracked WebApp source directory.'
}

$apiKey = Get-BrowserApiKey
if ($apiKey -notmatch '^AIza[0-9A-Za-z_-]{35}$') {
    throw 'The configured Google Maps browser key has an unexpected format.'
}

$normalizedOrigins = @($AllowedOrigins | ForEach-Object { Get-NormalizedOrigin $_ } | Select-Object -Unique)
if ($normalizedOrigins.Count -eq 0) {
    throw 'At least one allowed origin is required.'
}

$hasLoopbackOrigin = @($normalizedOrigins | Where-Object { ([System.Uri]$_).IsLoopback }).Count -gt 0
$hasRemoteOrigin = @($normalizedOrigins | Where-Object { -not ([System.Uri]$_).IsLoopback }).Count -gt 0
if ($hasLoopbackOrigin -and $hasRemoteOrigin) {
    throw 'Do not mix loopback and remote origins on one browser key. Use separate development and deployment keys.'
}

$runtimeConfig = [ordered]@{
    googleMapsBrowserApiKey = $apiKey
    googleMapsAllowedOrigins = $normalizedOrigins
}
$json = $runtimeConfig | ConvertTo-Json -Compress
$content = "globalThis.ssalddelRuntimeConfig = $json;`n"
$temporaryPath = "$targetRuntimeConfigPath.tmp-$([System.Guid]::NewGuid().ToString('N'))"
$indexContent = [System.IO.File]::ReadAllText($publishIndexPath)
$runtimeConfigPattern = 'runtime-config\.js(?:\?v=[0-9a-f]{16})?'
$runtimeConfigMatches = [regex]::Matches($indexContent, $runtimeConfigPattern)
if ($runtimeConfigMatches.Count -ne 1) {
    throw 'Published WebApp index must contain exactly one runtime-config.js reference.'
}
$deploymentToken = [System.Guid]::NewGuid().ToString('N').Substring(0, 16)
$updatedIndexContent = [regex]::Replace(
    $indexContent,
    $runtimeConfigPattern,
    "runtime-config.js?v=$deploymentToken")
$temporaryIndexPath = "$publishIndexPath.tmp-$([System.Guid]::NewGuid().ToString('N'))"
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

try {
    [System.IO.File]::WriteAllText($temporaryPath, $content, $utf8WithoutBom)
    [System.IO.File]::WriteAllText($temporaryIndexPath, $updatedIndexContent, $utf8WithoutBom)
    Move-Item -LiteralPath $temporaryPath -Destination $targetRuntimeConfigPath -Force
    Move-Item -LiteralPath $temporaryIndexPath -Destination $publishIndexPath -Force
}
finally {
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Force
    }
    if (Test-Path -LiteralPath $temporaryIndexPath) {
        Remove-Item -LiteralPath $temporaryIndexPath -Force
    }
}

Write-Output "Injected Google Maps browser runtime config into the publish output. Allowed origins: $($normalizedOrigins.Count)."
