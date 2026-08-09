[CmdletBinding()]
param(
    [string]$ServerBaseUrl = "http://127.0.0.1:5104/",
    [string]$UnityProjectPath = "artifacts/local/urban-market-unity-verification",
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Unity.exe",
    [int]$ProxyPort = 5105
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$serverConfigPath = Join-Path $repoRoot "Ssalddel/appsettings.Local.json"
$unityProject = (Resolve-Path (Join-Path $repoRoot $UnityProjectPath)).Path
$resultsPath = Join-Path $unityProject "WarehouseWorldOperationalResults.xml"
$logPath = Join-Path $unityProject "WarehouseWorldOperationalTests.log"
$containerName = "hongdal-mysql-1"

function ConvertTo-Base64Url([byte[]]$bytes) {
    [Convert]::ToBase64String($bytes).TrimEnd("=").Replace("+", "-").Replace("/", "_")
}

function New-DiagnosticToken([string]$userId, $configuration, [string]$warehouseManagerRole) {
    $now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $headerJson = @{ alg = "HS256"; typ = "JWT" } | ConvertTo-Json -Compress
    $payloadJson = @{
        iss = [string]$configuration.Jwt.Issuer
        aud = [string]$configuration.Jwt.Audience
        sub = $userId
        jti = [Guid]::NewGuid().ToString("N")
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" = $userId
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" = "shipper1"
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" = $warehouseManagerRole
        nbf = $now
        iat = $now
        exp = $now + 600
    } | ConvertTo-Json -Compress
    $unsigned = (ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($headerJson))) + "." +
        (ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($payloadJson)))
    $hmac = [Security.Cryptography.HMACSHA256]::new(
        [Text.Encoding]::UTF8.GetBytes([string]$configuration.Jwt.SecretKey))
    try {
        $signature = ConvertTo-Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($unsigned)))
        return $unsigned + "." + $signature
    }
    finally {
        $hmac.Dispose()
    }
}

$previousErrorAction = $ErrorActionPreference
$ErrorActionPreference = "Continue"
$databaseQuery = 'MYSQL_PWD="$MYSQL_PASSWORD" mysql --user="$MYSQL_USER" --database="$MYSQL_DATABASE" --batch --skip-column-names --execute="SELECT Id FROM AspNetUsers WHERE UserName = ''shipper1'' LIMIT 1;"'
$encodedDatabaseQuery = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($databaseQuery))
$userId = (& docker exec $containerName sh -c "echo $encodedDatabaseQuery | base64 -d | sh" 2>$null).Trim()
$mysqlExitCode = $LASTEXITCODE
$ErrorActionPreference = $previousErrorAction
if ($mysqlExitCode -ne 0 -or [string]::IsNullOrWhiteSpace($userId)) {
    throw "WarehouseW1DiagnosticWarehouseManagerMissing"
}

$configuration = Get-Content -Raw -LiteralPath $serverConfigPath | ConvertFrom-Json
$warehouseManagerRole = -join ([char[]]@(0xCC3D, 0xACE0, 0xAD00, 0xB9AC, 0xC790))
$accessToken = New-DiagnosticToken $userId $configuration $warehouseManagerRole
$headers = @{ Authorization = "Bearer " + $accessToken; Accept = "application/json" }
$snapshotUri = $ServerBaseUrl.TrimEnd("/") + "/api/v1/warehouse-operations/world/zones/warehouse"
try {
    $snapshot = Invoke-RestMethod -Method Get -Uri $snapshotUri -Headers $headers
}
catch {
    $response = $_.Exception.Response
    if ($response) {
        $reader = [IO.StreamReader]::new($response.GetResponseStream())
        try { $problem = $reader.ReadToEnd() | ConvertFrom-Json }
        finally { $reader.Dispose() }
        throw "WarehouseW1SnapshotDenied:$([int]$response.StatusCode):$([string]$problem.reason):$([string]$problem.errorCode)"
    }
    throw
}
$warehouseStableId = @($snapshot.inventoryItems)[0].warehouseStableId
if ($warehouseStableId -notmatch "^warehouse:(\d+)$") {
    throw "WarehouseW1WarehouseIdMissing"
}
$warehouseId = $Matches[1]

$proxyPrefix = "http://127.0.0.1:$ProxyPort/"
$proxyJob = Start-Job -ArgumentList $proxyPrefix, $ServerBaseUrl, $accessToken -ScriptBlock {
    param($prefix, $serverBaseUrl, $token)
    $listener = [Net.HttpListener]::new()
    $listener.Prefixes.Add($prefix)
    $listener.Start()
    try {
        for ($requestIndex = 0; $requestIndex -lt 2; $requestIndex++) {
            $context = $listener.GetContext()
            $target = $serverBaseUrl.TrimEnd("/") + $context.Request.RawUrl
            $upstreamRequest = [Net.HttpWebRequest]::CreateHttp($target)
            $upstreamRequest.Method = "GET"
            $upstreamRequest.Accept = "application/json"
            $upstreamRequest.Headers["Authorization"] = "Bearer " + $token
            try {
                $upstreamResponse = $upstreamRequest.GetResponse()
            }
            catch [Net.WebException] {
                $upstreamResponse = $_.Exception.Response
            }
            try {
                $context.Response.StatusCode = [int]$upstreamResponse.StatusCode
                $context.Response.ContentType = $upstreamResponse.ContentType
                $upstreamStream = $upstreamResponse.GetResponseStream()
                try { $upstreamStream.CopyTo($context.Response.OutputStream) }
                finally { $upstreamStream.Dispose() }
            }
            finally {
                $upstreamResponse.Dispose()
                $context.Response.OutputStream.Close()
            }
        }
    }
    finally {
        $listener.Stop()
        $listener.Close()
    }
}

try {
    $proxyDeadline = (Get-Date).AddSeconds(10)
    do {
        $proxyListener = Get-NetTCPConnection -State Listen -LocalPort $ProxyPort -ErrorAction SilentlyContinue |
            Select-Object -First 1
        if ($proxyListener) { break }
        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $proxyDeadline)
    if (-not $proxyListener) { throw "WarehouseW1ProxyStartTimeout" }

    $arguments = @(
        "-batchmode", "-nographics", "-runTests",
        "-projectPath", $unityProject,
        "-testPlatform", "EditMode",
        "-testFilter", "Ssalddel.Unity.Samples.WarehouseWorld.EditorTests.WarehouseWorldOperationalRefreshTests",
        "-testResults", $resultsPath,
        "-logFile", $logPath,
        "-warehouseW1BaseUrl", $proxyPrefix,
        "-warehouseW1AccessToken", "local-proxy-token",
        "-warehouseW1WarehouseId", $warehouseId)
    $unityProcess = Start-Process -FilePath $UnityExe -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $deadline = (Get-Date).AddSeconds(120)
    while (-not $unityProcess.HasExited -and (Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 500
        $unityProcess.Refresh()
    }
    if (-not $unityProcess.HasExited) {
        Stop-Process -Id $unityProcess.Id -Force
        throw "WarehouseW1UnityTestTimeout"
    }
    if (-not (Test-Path -LiteralPath $resultsPath)) { throw "WarehouseW1UnityTestResultsMissing" }
    $results = [xml](Get-Content -Raw -LiteralPath $resultsPath)
    [PSCustomObject]@{
        UnityExitCode = $unityProcess.ExitCode
        Total = [int]$results."test-run".total
        Passed = [int]$results."test-run".passed
        Failed = [int]$results."test-run".failed
        Skipped = [int]$results."test-run".skipped
    }
    if ($unityProcess.ExitCode -ne 0 -or [int]$results."test-run".failed -ne 0) {
        throw "WarehouseW1UnityOperationalTestsFailed"
    }
}
finally {
    if ($proxyJob) {
        Stop-Job -Job $proxyJob -ErrorAction SilentlyContinue
        Remove-Job -Job $proxyJob -Force -ErrorAction SilentlyContinue
    }
}
