param(
    [string]$ResourceGroup = "rg-ssalddel-unity-review-krc",
    [string]$Location = "koreacentral",
    [string]$VmName = "vm-ssalddel-unity-review",
    [string]$VmSize = "Standard_B2ats_v2",
    [string]$DnsLabel = "ssalddel-unity-review",
    [string]$AdminUserName = "ssalddelreview",
    [string]$SshPublicKeyPath = "artifacts/local/azure-unity-review-vm/ssh/id_ed25519.pub"
)

$ErrorActionPreference = "Stop"
if ($VmSize -notin @("Standard_B1s", "Standard_B2ats_v2", "Standard_B2pts_v2")) {
    throw "VmSize must remain within the Azure free-account eligible VM set."
}
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$resolvedPublicKeyPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $SshPublicKeyPath))
$allowedKeyRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts\local"))
if (-not $resolvedPublicKeyPath.StartsWith($allowedKeyRoot + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "SshPublicKeyPath must stay below artifacts/local."
}
if (-not (Test-Path -LiteralPath $resolvedPublicKeyPath -PathType Leaf)) {
    $privateKeyPath = [System.IO.Path]::ChangeExtension($resolvedPublicKeyPath, $null)
    New-Item -ItemType Directory -Path (Split-Path -Parent $privateKeyPath) -Force | Out-Null
    $keygen = Start-Process `
        -FilePath "ssh-keygen" `
        -ArgumentList "-q -t ed25519 -N `"`" -C `"ssalddel-unity-review`" -f `"$privateKeyPath`"" `
        -WindowStyle Hidden `
        -Wait `
        -PassThru
    if ($keygen.ExitCode -ne 0) { throw "Unity Review SSH key generation failed." }
}

$subscriptionId = (az account show --query id -o tsv).Trim()
$subscriptionUri = "https://management.azure.com/subscriptions/$subscriptionId`?api-version=2022-12-01"
$subscription = az rest --method get --url $subscriptionUri `
    --query "{state:state,quotaId:subscriptionPolicies.quotaId,spendingLimit:subscriptionPolicies.spendingLimit}" `
    -o json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $subscription.state -ne "Enabled") {
    throw "The selected Azure subscription is not writable. State=$($subscription.state), QuotaId=$($subscription.quotaId), SpendingLimit=$($subscription.spendingLimit)"
}
$existing = az vm show --resource-group $ResourceGroup --name $VmName --query id -o tsv 2>$null
if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace(($existing -join ""))) {
    throw "The target VM already exists. Provisioning will not overwrite it."
}

$sku = az vm list-sizes `
    --location $Location `
    --query "[?name=='$VmSize'] | [0].{name:name}" `
    -o json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $null -eq $sku -or $sku.name -ne $VmSize) {
    throw "$VmSize is not available in $Location."
}

$renderRoot = Join-Path $repositoryRoot "artifacts\local\azure-unity-review-vm"
New-Item -ItemType Directory -Path $renderRoot -Force | Out-Null
$cloudInitPath = Join-Path $renderRoot "cloud-init.rendered.yaml"
$cloudInit = [System.IO.File]::ReadAllText(
    (Join-Path $PSScriptRoot "cloud-init.yaml"),
    [System.Text.Encoding]::UTF8).Replace("__ADMIN_USERNAME__", $AdminUserName)
[System.IO.File]::WriteAllText($cloudInitPath, $cloudInit, [System.Text.UTF8Encoding]::new($false))

az group create --name $ResourceGroup --location $Location --output none
if ($LASTEXITCODE -ne 0) { throw "Azure resource group creation failed." }
az vm create `
    --resource-group $ResourceGroup `
    --name $VmName `
    --location $Location `
    --image Canonical:ubuntu-24_04-lts:server:latest `
    --size $VmSize `
    --admin-username $AdminUserName `
    --ssh-key-values $resolvedPublicKeyPath `
    --custom-data $cloudInitPath `
    --os-disk-size-gb 30 `
    --storage-sku Standard_LRS `
    --public-ip-sku Standard `
    --public-ip-address-allocation static `
    --public-ip-address-dns-name $DnsLabel `
    --nsg-rule SSH `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Azure Unity Review VM creation failed." }
az vm open-port --resource-group $ResourceGroup --name $VmName --port 80 --priority 1010 --output none
if ($LASTEXITCODE -ne 0) { throw "Opening HTTP port failed." }
az vm open-port --resource-group $ResourceGroup --name $VmName --port 443 --priority 1020 --output none
if ($LASTEXITCODE -ne 0) { throw "Opening HTTPS port failed." }

az vm show --resource-group $ResourceGroup --name $VmName -d `
    --query "{name:name,size:hardwareProfile.vmSize,powerState:powerState,fqdn:fqdns,publicIp:publicIps,resourceGroup:resourceGroup}" `
    -o json
