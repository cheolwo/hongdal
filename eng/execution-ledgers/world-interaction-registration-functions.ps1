function Read-WorldInteractionRegistration([string] $RepositoryRoot, [string] $RegistrationPath, [string] $CatalogPath) {
    $등록 = Get-Content -LiteralPath (Join-Path $RepositoryRoot $RegistrationPath) -Raw -Encoding UTF8 | ConvertFrom-Json
    $대장 = Get-Content -LiteralPath (Join-Path $RepositoryRoot $CatalogPath) -Raw -Encoding UTF8 | ConvertFrom-Json
    function Assert-Registration([bool] $조건, [string] $오류) { if (-not $조건) { throw "WiRegistrationInvalid:$오류" } }
    Assert-Registration ($등록.schemaVersion -eq 'world-interaction-registration-relations.v1') 'Schema'
    foreach ($원칙 in @('metadataFamiliesNeverExecute','specializationDoesNotExecuteParent','profilesDoNotCreateSecondConfirm','workflowOrderIsNotParentage','registrationDoesNotApproveImplementation','historicalCandidateIdsArePreserved','unresolvedBranchesRemainBlocked')) {
        Assert-Registration ($등록.principles.PSObject.Properties[$원칙].Value -eq $true) "Principle:$원칙"
    }
    $행동 = @{}; $분류 = @{}; $모듈 = @{}; $결정 = @{}
    foreach ($상위참조 in $등록.relatedUpperFamilyReferences) {
        Assert-Registration ($상위참조.newMemberBindingCode -eq 'NotApproved') 'UnapprovedGrowthBinding'
        Assert-Registration (Test-Path -LiteralPath (Join-Path $RepositoryRoot $상위참조.sourceReference)) 'UpperFamilySourceMissing'
    }
    foreach ($항목 in $대장.items) {
        Assert-Registration (-not $행동.ContainsKey([string] $항목.id)) "DuplicateAction:$($항목.id)"
        $행동[[string] $항목.id] = $항목
    }
    foreach ($항목 in $등록.families) {
        Assert-Registration (-not $분류.ContainsKey([string] $항목.id)) "DuplicateFamily:$($항목.id)"
        Assert-Registration ($항목.executionCode -eq 'MetadataOnly') "ExecutableFamily:$($항목.id)"
        Assert-Registration (-not $행동.ContainsKey([string] $항목.id)) "FamilyIsAction:$($항목.id)"
        Assert-Registration (@($항목.memberWorldInteractionIds | Select-Object -Unique).Count -eq @($항목.memberWorldInteractionIds).Count) 'DuplicateFamilyMember'
        foreach ($id in $항목.memberWorldInteractionIds) { Assert-Registration ($행동.ContainsKey([string] $id)) "UnknownMember:$id" }
        $분류[[string] $항목.id] = $항목
    }
    foreach ($항목 in $등록.families) {
        $방문 = @{}; $현재 = $항목
        while ($null -ne $현재) {
            Assert-Registration (-not $방문.ContainsKey([string] $현재.id)) 'FamilyCycle'
            $방문[[string] $현재.id] = $true
            if ([string]::IsNullOrEmpty([string] $현재.parentFamilyId)) { break }
            Assert-Registration ($분류.ContainsKey([string] $현재.parentFamilyId)) 'UnknownParentFamily'
            $현재 = $분류[[string] $현재.parentFamilyId]
        }
    }
    foreach ($항목 in $등록.modules) {
        Assert-Registration (-not $모듈.ContainsKey([string] $항목.id)) 'DuplicateModule'
        Assert-Registration ($항목.executionCode -eq 'ReadOnlyProjection' -and $행동.ContainsKey([string] $항목.ownerWorldInteractionId)) 'InvalidProjectionOwner'
        $모듈[[string] $항목.id] = $항목
    }
    $부모관계 = @{}
    foreach ($관계 in $등록.specializations) {
        $부모 = [string] $관계.parentWorldInteractionId; $자식 = [string] $관계.childWorldInteractionId
        Assert-Registration ($행동.ContainsKey($부모) -and $행동.ContainsKey($자식)) 'UnknownSpecialization'
        Assert-Registration (-not $부모관계.ContainsKey($자식)) 'DuplicateSpecialization'
        $부모관계[$자식] = $부모
    }
    foreach ($자식 in @($부모관계.Keys)) {
        $방문 = @{}; $현재 = $자식
        while ($부모관계.ContainsKey($현재)) {
            Assert-Registration (-not $방문.ContainsKey($현재)) 'SpecializationCycle'
            $방문[$현재] = $true; $현재 = $부모관계[$현재]
        }
    }
    Assert-Registration (@($등록.decisions).Count -eq 41) 'CandidateCoverage'
    foreach ($항목 in $등록.decisions) {
        $id = [string] $항목.candidateId; $대상 = [string] $항목.canonicalId
        Assert-Registration (-not $결정.ContainsKey($id)) "DuplicateCandidate:$id"
        Assert-Registration ($분류.ContainsKey([string] $항목.familyId)) "UnknownFamily:$id"
        Assert-Registration (-not [string]::IsNullOrWhiteSpace([string] $항목.reasonKo)) "ReasonMissing:$id"
        Assert-Registration (@($항목.questions).Count -gt 0 -and @($항목.sources).Count -gt 0) "ProvenanceMissing:$id"
        foreach ($출처 in $항목.sources) { Assert-Registration (Test-Path -LiteralPath (Join-Path $RepositoryRoot $출처)) "SourceMissing:$출처" }
        foreach ($질문 in $항목.questions) { Assert-Registration ($질문 -match '^Q-(\d{3})$' -and [int] $Matches[1] -le 339) 'UnapprovedQuestionRange' }
        switch ($항목.dispositionCode) {
            'RegisterAction' {
                Assert-Registration ($id -eq $대상 -and $행동.ContainsKey($대상)) "RegisteredActionMissing:$id"
                Assert-Registration (@($행동[$대상].effectCodes) -contains [string] $항목.primaryOutcomeCode) "OutcomeMismatch:$id"
            }
            'ReuseProfile' { Assert-Registration ($id -ne $대상 -and -not $행동.ContainsKey($id) -and $행동.ContainsKey($대상)) "DuplicateProfileAction:$id" }
            'MetadataFamily' { Assert-Registration ($분류.ContainsKey($대상) -and -not $행동.ContainsKey($id)) "FamilyRegisteredAsAction:$id" }
            'ResultProjection' { Assert-Registration ($모듈.ContainsKey($대상) -and -not $행동.ContainsKey($id)) "ProjectionRegisteredAsAction:$id" }
            default { throw "WiRegistrationInvalid:Disposition:$id" }
        }
        $결정[$id] = $항목
    }
    return $등록
}
