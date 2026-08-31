#requires -Version 7.0
[CmdletBinding()]
param([ValidateSet('Preflight', 'Set')][string] $Mode = 'Preflight')

# Run Set only in the user's local terminal. Never pass a credential as an argument.
# This is development-only source separation, not encrypted secret storage.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$taskSetting = 'PublicData:MfdsDomesticPharmacopoeia:ServiceKey'
$taskUserSecretsId = '47766019-f542-4cfc-9d4a-14b2fbfeac0e'
$taskRepo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskProject = Join-Path $taskRepo 'Ssalddel/Ssalddel.csproj'
$taskDirectory = Join-Path ([Environment]::GetFolderPath('ApplicationData')) "Microsoft/UserSecrets/$taskUserSecretsId"
$taskFile = Join-Path $taskDirectory 'secrets.json'
$taskErrorCode = 'PreflightFailed'
$taskWriteStarted = $false
$taskSecure = $null
$taskPlain = $null
$taskJson = $null
$taskPointer = [IntPtr]::Zero
$taskProcess = $null

function Assert-NoReparse([string] $Path) {
    $cursor = [IO.Path]::GetFullPath($Path)
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            if (((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw 'ReparsePathRejected'
            }
        }
        $parent = [IO.Directory]::GetParent($cursor)
        if ($null -eq $parent) { break }
        $cursor = $parent.FullName
    }
}

function Assert-PrivateAcl([string] $Path) {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    $allowed = @($identity, 'S-1-5-18', 'S-1-5-32-544')
    $acl = Get-Acl -LiteralPath $Path
    $owner = $acl.GetOwner([Security.Principal.SecurityIdentifier]).Value
    if ($owner -notin $allowed) { throw 'UnreviewedOwner' }
    foreach ($rule in $acl.GetAccessRules($true, $true, [Security.Principal.SecurityIdentifier])) {
        if ($rule.AccessControlType -eq [Security.AccessControl.AccessControlType]::Allow -and
            $rule.IdentityReference.Value -notin $allowed) { throw 'UnreviewedAccessRule' }
    }
}

function Read-FlatSecretStore {
    $info = Get-Item -LiteralPath $taskFile
    if ($info.Length -gt 4MB) { throw 'SecretStoreTooLarge' }
    $values = [IO.File]::ReadAllText($taskFile) | ConvertFrom-Json -AsHashtable
    if ($values -isnot [System.Collections.IDictionary]) { throw 'UnsupportedSecretStoreShape' }
    foreach ($entry in $values.GetEnumerator()) {
        if ($entry.Value -isnot [string]) { throw 'UnsupportedSecretStoreShape' }
    }
    return ,$values
}

try {
    $taskErrorCode = 'WindowsRequired'
    if (-not $IsWindows) { throw 'UnsupportedPlatform' }
    $taskErrorCode = 'ProjectIdentityMismatch'
    Assert-NoReparse $taskProject
    [xml]$taskProjectXml = [IO.File]::ReadAllText($taskProject)
    if (@($taskProjectXml.SelectNodes('//UserSecretsId')).Count -ne 1 -or
        $taskProjectXml.SelectSingleNode('//UserSecretsId').InnerText -cne $taskUserSecretsId) {
        throw 'ProjectIdentityMismatch'
    }
    $taskErrorCode = 'ExistingSecretStoreRequired'
    if (-not (Test-Path -LiteralPath $taskDirectory -PathType Container) -or
        -not (Test-Path -LiteralPath $taskFile -PathType Leaf)) { throw 'MissingSecretStore' }
    $taskErrorCode = 'SecretPathOrAccessRejected'
    Assert-NoReparse $taskFile
    Assert-PrivateAcl $taskDirectory
    Assert-PrivateAcl $taskFile
    $taskErrorCode = 'DotnetUnavailable'
    $taskDotnet = (Get-Command dotnet -CommandType Application -ErrorAction Stop).Source

    if ($Mode -eq 'Preflight') {
        # No secret contents are read in Preflight, including presence of this key.
        [pscustomobject]@{ Status='AccessPreflightPassed'; Project='Ssalddel/Ssalddel.csproj';
            Setting=$taskSetting; ExistingStore=$true; AccessRulesReviewed=$true;
            CredentialPresenceChecked=$false; Stored=$false; ApiCalled=$false } | ConvertTo-Json -Compress
        exit 0
    }

    $taskErrorCode = 'InteractiveTerminalRequired'
    if ([Console]::IsInputRedirected) { throw 'InteractiveTerminalRequired' }
    $taskErrorCode = 'ExistingSecretStoreShapeRejected'
    $taskBeforeHash = (Get-FileHash -LiteralPath $taskFile -Algorithm SHA256).Hash
    $taskBefore = Read-FlatSecretStore
    if ($taskBefore.Keys.Where({ [string]::Equals($_, $taskSetting, [StringComparison]::OrdinalIgnoreCase) }).Count -gt 0) {
        $taskErrorCode = 'DedicatedSecretAlreadyPresent'
        throw 'DoNotOverwriteExistingCredential'
    }
    Write-Host '개발용 User Secrets는 암호화되지 않습니다. 다른 설정 편집을 멈추고 로컬 터미널에서만 입력하세요.'
    Write-Host '키를 채팅/명령 인수에 붙이지 마세요. 이번 동작은 키 저장만 하며 API/DB를 호출하지 않습니다.'
    $taskSecure = Read-Host '식약처 일반(Decoding) 키 — 화면에 표시하지 않음' -AsSecureString
    $taskPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($taskSecure)
    $taskPlain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($taskPointer)
    $taskErrorCode = 'CredentialFormatRejected'
    if ($taskPlain -cnotmatch '^[A-Za-z0-9+/=]{16,512}$') { throw 'UsePlainDecodingKeyWithoutWhitespace' }

    $taskErrorCode = 'SecretStoreChangedDuringInput'
    Assert-NoReparse $taskFile
    Assert-PrivateAcl $taskDirectory
    Assert-PrivateAcl $taskFile
    if ((Get-FileHash -LiteralPath $taskFile -Algorithm SHA256).Hash -cne $taskBeforeHash) { throw 'ConcurrentChange' }
    $taskJson = @{ $taskSetting = $taskPlain } | ConvertTo-Json -Compress
    $taskStart = [Diagnostics.ProcessStartInfo]::new()
    $taskStart.FileName = $taskDotnet
    $taskStart.UseShellExecute = $false
    $taskStart.CreateNoWindow = $true
    $taskStart.RedirectStandardInput = $true
    $taskStart.RedirectStandardOutput = $true
    $taskStart.RedirectStandardError = $true
    $taskStart.StandardInputEncoding = [Text.UTF8Encoding]::new($false)
    foreach ($argument in @('user-secrets', 'set', '--project', $taskProject)) {
        [void]$taskStart.ArgumentList.Add($argument)
    }
    $taskProcess = [Diagnostics.Process]::new()
    $taskProcess.StartInfo = $taskStart
    $taskErrorCode = 'SecretWriteOutcomeUnconfirmed'
    [void]$taskProcess.Start()
    $taskWriteStarted = $true
    # Capture, but never relay CLI stdout/stderr: error output may include input.
    $taskStdout = $taskProcess.StandardOutput.ReadToEndAsync()
    $taskStderr = $taskProcess.StandardError.ReadToEndAsync()
    $taskProcess.StandardInput.WriteLine($taskJson)
    $taskProcess.StandardInput.Close()
    if (-not $taskProcess.WaitForExit(30000)) { throw 'ChildOutcomeUnconfirmedDoNotRetry' }
    [void]$taskStdout.GetAwaiter().GetResult()
    [void]$taskStderr.GetAwaiter().GetResult()
    if ($taskProcess.ExitCode -ne 0) { throw 'ChildFailedDoNotRetry' }

    Assert-NoReparse $taskFile
    Assert-PrivateAcl $taskDirectory
    Assert-PrivateAcl $taskFile
    $taskAfter = Read-FlatSecretStore
    if ($taskAfter.Count -ne $taskBefore.Count + 1 -or -not $taskAfter.Contains($taskSetting) -or
        -not [string]::Equals($taskAfter[$taskSetting], $taskPlain, [StringComparison]::Ordinal)) {
        throw 'StoredValueNotConfirmed'
    }
    foreach ($entry in $taskBefore.GetEnumerator()) {
        if (-not $taskAfter.Contains($entry.Key) -or
            -not [string]::Equals($taskAfter[$entry.Key], $entry.Value, [StringComparison]::Ordinal)) {
            throw 'OtherSettingsChangedDoNotAutoRestore'
        }
    }
    [pscustomobject]@{ Status='StoredAndReadBack'; Setting=$taskSetting; Stored=$true;
        OtherSettingsUnchanged=$true; EncryptedStore=$false; ApiCalled=$false;
        Limit='Concurrent writers outside this script and memory erasure are not guaranteed.' } | ConvertTo-Json -Compress
}
catch {
    # Do not expose $_, exception messages, inner exceptions, input, URLs, or CLI output.
    [pscustomobject]@{ Status='BlockedOrUnconfirmed'; Code=$taskErrorCode;
        WriteStarted=$taskWriteStarted; StoredConfirmed=$false; ApiCalled=$false;
        AutomaticRetry=$false; AutomaticRestore=$false } | ConvertTo-Json -Compress
    exit 1
}
finally {
    if ($taskPointer -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($taskPointer) }
    if ($null -ne $taskSecure) { $taskSecure.Dispose() }
    if ($null -ne $taskProcess) { $taskProcess.Dispose() }
    $taskPlain = $null; $taskJson = $null; $taskBefore = $null; $taskAfter = $null
    $taskStdout = $null; $taskStderr = $null
    # Managed strings/CLI process memory are not guaranteed to be securely erased.
}
