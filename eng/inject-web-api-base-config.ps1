[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PublishRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ApiBaseAddress,

    [switch]$AllowLoopback
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$sourceConfigPath = [System.IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot 'Ssalddel.WebApp\wwwroot\appsettings.Production.json'))
$publishRootPath = [System.IO.Path]::GetFullPath($PublishRoot)
$targetConfigPath = [System.IO.Path]::GetFullPath(
    (Join-Path $publishRootPath 'wwwroot\appsettings.Production.json'))

if (-not (Test-Path -LiteralPath $targetConfigPath -PathType Leaf)) {
    throw "Published WebApp production configuration was not found: $targetConfigPath"
}
if ([string]::Equals(
        $targetConfigPath,
        $sourceConfigPath,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'Refusing to inject an API address into the tracked WebApp source directory.'
}

$apiUri = $null
if (-not [System.Uri]::TryCreate($ApiBaseAddress.Trim(), [System.UriKind]::Absolute, [ref]$apiUri)) {
    throw 'ApiBaseAddress must be an absolute URI.'
}
if ($apiUri.Scheme -ne [System.Uri]::UriSchemeHttp -and $apiUri.Scheme -ne [System.Uri]::UriSchemeHttps) {
    throw 'ApiBaseAddress must use HTTP or HTTPS.'
}
if (-not [string]::IsNullOrEmpty($apiUri.Query) -or -not [string]::IsNullOrEmpty($apiUri.Fragment)) {
    throw 'ApiBaseAddress must not contain a query or fragment.'
}
if (-not [string]::IsNullOrEmpty($apiUri.UserInfo)) {
    throw 'ApiBaseAddress must not contain user information.'
}
if ($apiUri.IsLoopback -and -not $AllowLoopback) {
    throw 'Loopback API addresses require the explicit -AllowLoopback switch.'
}
if (-not $apiUri.IsLoopback -and $apiUri.Scheme -ne [System.Uri]::UriSchemeHttps) {
    throw 'Remote API addresses must use HTTPS.'
}

$normalizedApiBaseAddress = $apiUri.GetLeftPart([System.UriPartial]::Path)
if (-not $normalizedApiBaseAddress.EndsWith('/', [System.StringComparison]::Ordinal)) {
    $normalizedApiBaseAddress += '/'
}

$config = Get-Content -LiteralPath $targetConfigPath -Raw | ConvertFrom-Json
$config.SsalddelApiBaseAddress = $normalizedApiBaseAddress
$content = $config | ConvertTo-Json -Depth 20
$temporaryPath = "$targetConfigPath.tmp-$([System.Guid]::NewGuid().ToString('N'))"
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

try {
    [System.IO.File]::WriteAllText($temporaryPath, "$content`n", $utf8WithoutBom)
    Move-Item -LiteralPath $temporaryPath -Destination $targetConfigPath -Force

    foreach ($compression in @('br', 'gz')) {
        $compressedPath = "$targetConfigPath.$compression"
        if (Test-Path -LiteralPath $compressedPath -PathType Leaf) {
            Remove-Item -LiteralPath $compressedPath -Force
        }
    }
}
finally {
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Force
    }
}

Write-Output "Injected the WebApp API base address into the publish output: $normalizedApiBaseAddress"
