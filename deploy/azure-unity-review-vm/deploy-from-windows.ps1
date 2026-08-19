param(
    [string]$ResourceGroup = "rg-ssalddel-unity-review-krc",
    [string]$VmName = "vm-ssalddel-unity-review",
    [string]$AdminUserName = "ssalddelreview",
    [string]$PackageDirectory = "artifacts/local/azure-unity-review-vm",
    [string]$SshPrivateKeyPath = "artifacts/local/azure-unity-review-vm/ssh/id_ed25519"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$packageRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $PackageDirectory))
$resolvedPrivateKeyPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $SshPrivateKeyPath))
$archivePath = Join-Path $packageRoot "unity-review-vm.tar.gz"
$checksumPath = "$archivePath.sha256"
$environmentPath = Join-Path $packageRoot "unity-review.env"
foreach ($path in @($archivePath, $checksumPath, $environmentPath, $resolvedPrivateKeyPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required deployment file is missing: $path"
    }
}

$fqdn = (az vm show --resource-group $ResourceGroup --name $VmName -d --query fqdns -o tsv).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($fqdn)) {
    throw "Azure VM FQDN could not be resolved."
}
$checksum = ((Get-Content -LiteralPath $checksumPath -Encoding utf8 -Raw).Trim() -split '\s+')[0]
$remoteRoot = "/tmp/ssalddel-unity-review-deploy"
$sshTarget = "$AdminUserName@$fqdn"

& ssh -i $resolvedPrivateKeyPath $sshTarget "install -d -m 0700 $remoteRoot"
if ($LASTEXITCODE -ne 0) { throw "Remote staging directory creation failed." }
& scp -i $resolvedPrivateKeyPath `
    $archivePath `
    $environmentPath `
    (Join-Path $PSScriptRoot "deploy-release.sh") `
    "${sshTarget}:$remoteRoot/"
if ($LASTEXITCODE -ne 0) { throw "Unity Review release upload failed." }

& ssh -i $resolvedPrivateKeyPath $sshTarget `
    "chmod 700 $remoteRoot/deploy-release.sh && sudo $remoteRoot/deploy-release.sh $remoteRoot/unity-review-vm.tar.gz $checksum $remoteRoot/unity-review.env"
if ($LASTEXITCODE -ne 0) { throw "Unity Review remote deployment failed." }

[pscustomobject]@{
    Site = "https://$fqdn/"
    Health = "https://$fqdn/healthz"
    ResourceGroup = $ResourceGroup
    VmName = $VmName
}
