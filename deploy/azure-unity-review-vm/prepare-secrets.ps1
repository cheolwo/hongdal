param(
    [Parameter(Mandatory = $true)]
    [string]$SiteHost,
    [string]$OutputPath = "artifacts/local/azure-unity-review-vm/unity-review.env"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$projectPath = Join-Path $repositoryRoot "Ssalddel.UnityReview.Api\Ssalddel.UnityReview.Api.csproj"
$resolvedOutput = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputPath))
$allowedRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts\local"))
if (-not $resolvedOutput.StartsWith($allowedRoot + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputPath must stay below artifacts/local."
}
if ($SiteHost -notmatch '^[a-z0-9][a-z0-9.-]+[a-z0-9]$') {
    throw "SiteHost must be a DNS hostname without scheme or path."
}

function New-HexSecret([int]$ByteCount) {
    $bytes = New-Object byte[] $ByteCount
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }
    return ([BitConverter]::ToString($bytes) -replace '-', '').ToLowerInvariant()
}

function New-RandomBytes([int]$ByteCount) {
    $bytes = New-Object byte[] $ByteCount
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }
    return $bytes
}

$secretJson = & dotnet user-secrets list --project $projectPath --json 2>$null
$secretMap = @{}
$secretJsonBody = @($secretJson | Where-Object { $_ -notmatch '^\s*//' }) -join [Environment]::NewLine
if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($secretJsonBody)) {
    $secretObject = $secretJsonBody | ConvertFrom-Json
    foreach ($property in $secretObject.PSObject.Properties) {
        $secretMap[$property.Name] = [string]$property.Value
    }
}

function Get-OrCreateSecret([string]$Key, [scriptblock]$Factory) {
    if ($secretMap.ContainsKey($Key) -and -not [string]::IsNullOrWhiteSpace([string]$secretMap[$Key])) {
        return [string]$secretMap[$Key]
    }
    $value = & $Factory
    & dotnet user-secrets set $Key $value --project $projectPath | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to store Unity Review deployment secret: $Key"
    }
    $secretMap[$Key] = $value
    return $value
}

$adminUserName = Get-OrCreateSecret "UnityReviewDeployment:AdminUserName" { "unity-review-admin" }
$adminPassword = Get-OrCreateSecret "UnityReviewDeployment:AdminPassword" { New-HexSecret 12 }
$mysqlRootPassword = Get-OrCreateSecret "UnityReviewDeployment:MySqlRootPassword" { New-HexSecret 24 }
$mysqlPassword = Get-OrCreateSecret "UnityReviewDeployment:MySqlPassword" { New-HexSecret 24 }
$jwtSigningKeyBase64 = Get-OrCreateSecret "UnityReviewDeployment:JwtSigningKeyBase64" {
    [Convert]::ToBase64String((New-RandomBytes 48))
}

$iterations = 250000
$salt = New-RandomBytes 16
$passwordBytes = [Text.Encoding]::UTF8.GetBytes($adminPassword)
$derive = [Security.Cryptography.Rfc2898DeriveBytes]::new(
    $passwordBytes,
    $salt,
    $iterations,
    [Security.Cryptography.HashAlgorithmName]::SHA256)
try {
    $hash = $derive.GetBytes(32)
}
finally {
    $derive.Dispose()
}
$encodedPassword = "$iterations.$([Convert]::ToBase64String($salt)).$([Convert]::ToBase64String($hash))"

$lines = @(
    "UNITY_REVIEW_SITE_HOST=$SiteHost"
    "UNITY_REVIEW_ADMIN_USER_NAME=$adminUserName"
    "UNITY_REVIEW_ADMIN_PASSWORD_PBKDF2=$encodedPassword"
    "UNITY_REVIEW_JWT_SIGNING_KEY_BASE64=$jwtSigningKeyBase64"
    "UNITY_REVIEW_MYSQL_ROOT_PASSWORD=$mysqlRootPassword"
    "UNITY_REVIEW_MYSQL_PASSWORD=$mysqlPassword"
)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedOutput) -Force | Out-Null
[System.IO.File]::WriteAllLines($resolvedOutput, $lines, [System.Text.UTF8Encoding]::new($false))

[pscustomobject]@{
    EnvironmentFile = $resolvedOutput
    SiteHost = $SiteHost
    AdminUserName = $adminUserName
    AdminPasswordSecretKey = "UnityReviewDeployment:AdminPassword"
    SecretsProject = $projectPath
}
