$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$chrome = "C:\Program Files\Google\Chrome\Application\chrome.exe"
if (-not (Test-Path $chrome)) {
    throw "Chrome executable was not found: $chrome"
}

$captureRoot = Join-Path $repoRoot "docs\ProjectOverview\assets\app-pages"

function Wait-HttpReady {
    param(
        [Parameter(Mandatory = $true)][int] $Port
    )

    $deadline = (Get-Date).AddSeconds(40)
    do {
        $client = $null
        try {
            $client = [System.Net.Sockets.TcpClient]::new()
            $task = $client.ConnectAsync("127.0.0.1", $Port)
            if ($task.Wait(1000) -and $client.Connected) {
                return
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
        finally {
            if ($client) {
                $client.Dispose()
            }
        }
    } while ((Get-Date) -lt $deadline)

    throw "Capture host on port $Port did not become ready."
}

function Invoke-PageCapture {
    param(
        [Parameter(Mandatory = $true)][string] $AppName,
        [Parameter(Mandatory = $true)][string] $PageId,
        [Parameter(Mandatory = $true)][string] $Url
    )

    $appDir = Join-Path $captureRoot $AppName
    New-Item -ItemType Directory -Force -Path $appDir | Out-Null
    $output = Join-Path $appDir "$PageId.png"

    & $chrome `
        --headless=new `
        --disable-gpu `
        --hide-scrollbars `
        --window-size=390,844 `
        --virtual-time-budget=8000 `
        "--screenshot=$output" `
        $Url | Out-Null

    if (-not (Test-Path $output)) {
        throw "Screenshot was not created: $output"
    }

    $item = Get-Item $output
    if ($item.Length -le 0) {
        throw "Screenshot is empty: $output"
    }

    [PSCustomObject]@{
        App = $AppName
        PageId = $PageId
        Url = $Url
        File = $output
        Bytes = $item.Length
    }
}

$hosts = @(
    @{
        App = "ShipperApp"
        Project = "artifacts\capture-hosts\ShipperSourceCaptureHost\ShipperSourceCaptureHost.csproj"
        Dll = "artifacts\capture-hosts\ShipperSourceCaptureHost\bin\Debug\net10.0\ShipperSourceCaptureHost.dll"
        Port = 5211
        Pages = @(
            @{ Id = "ShipperApp-P01"; Path = "/shipper" },
            @{ Id = "ShipperApp-P01-1"; Path = "/shipper/settings/profile" },
            @{ Id = "ShipperApp-P01-2"; Path = "/shipper/settings/views" },
            @{ Id = "ShipperApp-P01-3"; Path = "/shipper/public-cargo" },
            @{ Id = "ShipperApp-P01-4"; Path = "/shipper/exploration/inbox" },
            @{ Id = "ShipperApp-P02"; Path = "/shipper/request" },
            @{ Id = "ShipperApp-P02-1"; Path = "/shipper/request/bulk" },
            @{ Id = "ShipperApp-P02-2"; Path = "/dispatch/address-form" },
            @{ Id = "ShipperApp-P03"; Path = "/shipper/request/SHP-1001" },
            @{ Id = "ShipperApp-P04"; Path = "/shipper/inbound/dashboard" },
            @{ Id = "ShipperApp-P04-1"; Path = "/shipper/inbound/requests" },
            @{ Id = "ShipperApp-P05"; Path = "/shipper/warehouse/workspace" },
            @{ Id = "ShipperApp-P05-1"; Path = "/shipper/warehouse/inventory" },
            @{ Id = "ShipperApp-P05-2"; Path = "/shipper/warehouse/scan" },
            @{ Id = "ShipperApp-P05-3"; Path = "/shipper/warehouse/work/inbound" },
            @{ Id = "ShipperApp-P06"; Path = "/shipper/sales/channels" },
            @{ Id = "ShipperApp-P06-1"; Path = "/shipper/sales/listings" },
            @{ Id = "ShipperApp-P06-2"; Path = "/shipper/sales/orders" },
            @{ Id = "ShipperApp-P07"; Path = "/shipper/international/fcl-lcl" },
            @{ Id = "ShipperApp-P07-1"; Path = "/shipper/customs/hs-reviews" },
            @{ Id = "ShipperApp-P08"; Path = "/shipper/reconsignment/orders" },
            @{ Id = "ShipperApp-P90"; Path = "/weather" },
            @{ Id = "ShipperApp-P91"; Path = "/counter" },
            @{ Id = "ShipperApp-P99"; Path = "/not-found" }
        )
    },
    @{
        App = "DriverApp"
        Project = "artifacts\capture-hosts\DriverSourceCaptureHost\DriverSourceCaptureHost.csproj"
        Dll = "artifacts\capture-hosts\DriverSourceCaptureHost\bin\Debug\net10.0\DriverSourceCaptureHost.dll"
        Port = 5212
        Pages = @(
            @{ Id = "DriverApp-P00"; Path = "/" },
            @{ Id = "DriverApp-P01"; Path = "/login" },
            @{ Id = "DriverApp-P02"; Path = "/driver/menu" },
            @{ Id = "DriverApp-P02-1"; Path = "/driver/settings/views" },
            @{ Id = "DriverApp-P03"; Path = "/driver/reservations" },
            @{ Id = "DriverApp-P04"; Path = "/driver/exploration/campaigns" },
            @{ Id = "DriverApp-P05"; Path = "/driver/transports/history" },
            @{ Id = "DriverApp-P06"; Path = "/driver/work/start" },
            @{ Id = "DriverApp-P06-1"; Path = "/driver/work/settings" },
            @{ Id = "DriverApp-P07"; Path = "/driver/home" },
            @{ Id = "DriverApp-P07-1"; Path = "/driver/home/summary" },
            @{ Id = "DriverApp-P08"; Path = "/driver/recommendations" },
            @{ Id = "DriverApp-P09"; Path = "/driver/recommendations/1" },
            @{ Id = "DriverApp-P10"; Path = "/driver/recommendations/1/decision" },
            @{ Id = "DriverApp-P11"; Path = "/driver/transports/current" },
            @{ Id = "DriverApp-P12"; Path = "/driver/transports/1/pickup" },
            @{ Id = "DriverApp-P13"; Path = "/driver/transports/1/dropoff" },
            @{ Id = "DriverApp-P14"; Path = "/driver/settlements/current-month" },
            @{ Id = "DriverApp-P14-1"; Path = "/driver/settlements/info" },
            @{ Id = "DriverApp-P14-2"; Path = "/driver/account/bank" },
            @{ Id = "DriverApp-P15"; Path = "/driver/notifications" },
            @{ Id = "DriverApp-P15-1"; Path = "/driver/notifications/settings" },
            @{ Id = "DriverApp-P15-2"; Path = "/driver/notifications/push" }
        )
    },
    @{
        App = "WarehouseManagerApp"
        Project = "artifacts\capture-hosts\WarehouseSourceCaptureHost\WarehouseSourceCaptureHost.csproj"
        Dll = "artifacts\capture-hosts\WarehouseSourceCaptureHost\bin\Debug\net10.0\WarehouseSourceCaptureHost.dll"
        Port = 5213
        Pages = @(
            @{ Id = "WarehouseManagerApp-P01"; Path = "/" },
            @{ Id = "WarehouseManagerApp-P02"; Path = "/work-board" },
            @{ Id = "WarehouseManagerApp-P02-1"; Path = "/work/inbound" },
            @{ Id = "WarehouseManagerApp-P02-2"; Path = "/work/inbound/workbench" },
            @{ Id = "WarehouseManagerApp-P02-3"; Path = "/scan" },
            @{ Id = "WarehouseManagerApp-P03"; Path = "/work/inbound/inspection" },
            @{ Id = "WarehouseManagerApp-P03-1"; Path = "/work/inbound/products" },
            @{ Id = "WarehouseManagerApp-P04"; Path = "/work/picking-batch" },
            @{ Id = "WarehouseManagerApp-P05"; Path = "/mart" },
            @{ Id = "WarehouseManagerApp-P05-1"; Path = "/mart/work-board" },
            @{ Id = "WarehouseManagerApp-P05-2"; Path = "/mart/work/mart-picking" },
            @{ Id = "WarehouseManagerApp-P99"; Path = "/not-found" }
        )
    },
    @{
        App = "OrdererApp"
        Project = "artifacts\capture-hosts\OrdererSourceCaptureHost\OrdererSourceCaptureHost.csproj"
        Dll = "artifacts\capture-hosts\OrdererSourceCaptureHost\bin\Debug\net10.0\OrdererSourceCaptureHost.dll"
        Port = 5214
        Pages = @(
            @{ Id = "OrdererApp-P01"; Path = "/" },
            @{ Id = "OrdererApp-P02"; Path = "/group-purchase" },
            @{ Id = "OrdererApp-P03"; Path = "/cargo" },
            @{ Id = "OrdererApp-P04"; Path = "/food" },
            @{ Id = "OrdererApp-P04-1"; Path = "/food/restaurants" },
            @{ Id = "OrdererApp-P04-2"; Path = "/food/mart" },
            @{ Id = "OrdererApp-P05"; Path = "/orders" },
            @{ Id = "OrdererApp-P99"; Path = "/not-found" }
        )
    },
    @{
        App = "RestaurantDeskApp"
        Project = "artifacts\capture-hosts\RestaurantDeskSourceCaptureHost\RestaurantDeskSourceCaptureHost.csproj"
        Dll = "artifacts\capture-hosts\RestaurantDeskSourceCaptureHost\bin\Debug\net10.0\RestaurantDeskSourceCaptureHost.dll"
        Port = 5215
        Pages = @(
            @{ Id = "RestaurantDeskApp-P01"; Path = "/" },
            @{ Id = "RestaurantDeskApp-P02"; Path = "/restaurants/nearby" },
            @{ Id = "RestaurantDeskApp-P02-1"; Path = "/restaurants/popular" },
            @{ Id = "RestaurantDeskApp-P03"; Path = "/reviews/moderation" },
            @{ Id = "RestaurantDeskApp-P04"; Path = "/dispatch/address-form" }
        )
    },
    @{
        App = "HumanResourcesManagerApp"
        Project = "artifacts\capture-hosts\HumanResourcesSourceCaptureHost\HumanResourcesSourceCaptureHost.csproj"
        Dll = "artifacts\capture-hosts\HumanResourcesSourceCaptureHost\bin\Debug\net10.0\HumanResourcesSourceCaptureHost.dll"
        Port = 5216
        Pages = @(
            @{ Id = "HumanResourcesManagerApp-P01"; Path = "/" }
        )
    }
)

$results = New-Object System.Collections.Generic.List[object]
$failures = New-Object System.Collections.Generic.List[object]

foreach ($hostConfig in $hosts) {
    $project = Join-Path $repoRoot $hostConfig.Project
    dotnet build $project -v:minimal | Out-Host

    $dll = Join-Path $repoRoot $hostConfig.Dll
    $hostDirectory = Split-Path -Parent $project
    $port = [int]$hostConfig.Port
    $baseUrl = "http://127.0.0.1:$port"
    $process = $null

    try {
        $process = Start-Process -FilePath "dotnet" `
            -ArgumentList @($dll, "--urls", $baseUrl) `
            -WorkingDirectory $hostDirectory `
            -WindowStyle Hidden `
            -PassThru

        Wait-HttpReady -Port $port

        foreach ($page in $hostConfig.Pages) {
            $url = "$baseUrl$($page.Path)"
            try {
                $result = Invoke-PageCapture -AppName $hostConfig.App -PageId $page.Id -Url $url
                $results.Add($result)
                Write-Host "captured $($page.Id) -> $($result.File)"
            }
            catch {
                $failure = [PSCustomObject]@{
                    App = $hostConfig.App
                    PageId = $page.Id
                    Url = $url
                    Error = $_.Exception.Message
                }
                $failures.Add($failure)
                Write-Warning "failed $($page.Id): $($failure.Error)"
            }
        }
    }
    finally {
        if ($process -and -not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }
}

$summary = [PSCustomObject]@{
    Captured = $results.Count
    Failed = $failures.Count
    Results = $results
    Failures = $failures
}

$summaryPath = Join-Path $PSScriptRoot "capture-client-pages-result.json"
$summary | ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 $summaryPath
$summary
