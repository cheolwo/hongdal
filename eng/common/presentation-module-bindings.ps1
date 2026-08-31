# Presentation 연결 검증만 수행한다. Unity 실행·증거 생성·E 승격은 하지 않는다.
function Assert-PresentationBinding([bool] $Condition, [string] $Code) {
    if (-not $Condition) { throw "PresentationModuleBindingInvalid:$Code" }
}

function Resolve-PresentationModuleReference(
    [string] $Reference, [string] $RepositoryRoot, [string] $UnityProjectRoot,
    [bool] $RequireAvailable = $false) {
    Assert-PresentationBinding ($Reference -cmatch '^(repo|unity):[^:/\\].*$') "ReferenceInvalid:$Reference"
    $parts = $Reference.Split(@(':'), 2)
    $relative = $parts[1].Replace('\', '/')
    Assert-PresentationBinding ($relative -notmatch '(^|/)\.\.(/|$)|[:*?]') "ReferenceTraversal:$Reference"
    $root = if ($parts[0] -eq 'repo') { $RepositoryRoot } else { $UnityProjectRoot }
    if ([string]::IsNullOrWhiteSpace($root)) {
        Assert-PresentationBinding (-not $RequireAvailable) "WorkspaceUnavailable:$Reference"
        return $null
    }
    $base = [IO.Path]::GetFullPath($root).TrimEnd([char[]]@('/', '\'))
    $path = [IO.Path]::GetFullPath((Join-Path $base $relative))
    Assert-PresentationBinding ($path.StartsWith($base + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) "ReferenceOutsideRoot:$Reference"
    Assert-PresentationBinding (Test-Path -LiteralPath $path -PathType Leaf) "ReferenceMissing:$Reference"
    return $path
}

function Get-PresentationRequiredModules([object] $Catalog, [string] $LoopId) {
    $profiles = @($Catalog.loopProfiles | Where-Object loopStableId -eq $LoopId)
    $features = if ($profiles.Count -gt 0) { @($profiles[0].featureCodes) } else { @() }
    return @($Catalog.modules | Where-Object {
        $_.applicabilityCode -eq 'Common' -or
        @($_.requiredFeatureCodes | Where-Object { $features -notcontains $_ }).Count -eq 0
    })
}

function Test-PresentationModuleBindings(
    [object] $WorkOrder, [object] $Catalog, [string] $RepositoryRoot,
    [string] $UnityProjectRoot = '', [object] $EvidenceCatalog = $null) {
    $required = @(Get-PresentationRequiredModules $Catalog ([string] $WorkOrder.playableUnitStableId))
    $property = $WorkOrder.PSObject.Properties['presentationModuleBindings']
    # 과거 명세는 읽기 호환한다. 기존 증거·E를 자동 이관하거나 재판정하지 않는다.
    if ($null -eq $property) {
        return @($required | ForEach-Object { [pscustomobject]@{
            moduleCode = $_.moduleCode; evidenceStageCode = $_.evidenceStageCode
            statusCode = 'Unverified'; reason = '과거 명세: 모듈별 근거 미연결'
            implementationRefs = @(); testRefs = @(); evidenceRefs = @()
        } })
    }
    $byCode = @{}
    foreach ($binding in @($property.Value)) {
        $code = [string] $binding.moduleCode
        Assert-PresentationBinding (-not $byCode.ContainsKey($code)) "Duplicate:$code"
        Assert-PresentationBinding (@($required.moduleCode) -contains $code) "NotApplicableToProfile:$code"
        Assert-PresentationBinding (@('Unverified','Blocked','Passed','NotApplicable') -contains
            [string] $binding.statusCode) "StatusInvalid:$code"
        foreach ($field in @('implementationRefs','testRefs','evidenceRefs','reason','candidateRevisionOrFingerprint')) {
            Assert-PresentationBinding ($null -ne $binding.PSObject.Properties[$field]) "FieldMissing:${code}:$field"
        }
        if ($binding.statusCode -ne 'Passed') {
            Assert-PresentationBinding (-not [string]::IsNullOrWhiteSpace($binding.reason)) "ReasonMissing:$code"
        }
        $module = @($required | Where-Object moduleCode -eq $code)[0]
        if ($binding.statusCode -eq 'NotApplicable' -and $module.applicabilityCode -eq 'Common') {
            $preparation = $WorkOrder.PSObject.Properties['presentationE4Preparation']
            Assert-PresentationBinding ($code -in @('visual-source-bounds','player-scale-spacing') -and
                $null -ne $preparation -and $preparation.Value.applicabilityCode -eq 'NotApplicable') "CommonNotApplicable:$code"
        }
        $sourcePaths = @()
        foreach ($reference in @($binding.implementationRefs) + @($binding.testRefs)) {
            $path = Resolve-PresentationModuleReference ([string] $reference) $RepositoryRoot $UnityProjectRoot ($binding.statusCode -eq 'Passed')
            if ($null -ne $path) { $sourcePaths += $path }
        }
        if ($binding.statusCode -eq 'Passed') {
            if ([int] $module.evidenceStageCode.Substring(1) -ge 5) {
                Assert-PresentationBinding ([int] $WorkOrder.trackPlans.logic.currentEvidenceStage.Substring(1) -ge 5) "PresentationE5RequiresLogicE5:$code"
            }
            if ($module.evidenceStageCode -eq 'E2') {
                Assert-PresentationBinding (@($binding.implementationRefs).Count -gt 0) "ImplementationMissing:$code"
            }
            if ($module.evidenceStageCode -eq 'E3') {
                Assert-PresentationBinding (@($binding.testRefs).Count -gt 0) "TestsMissing:$code"
            }
            Assert-PresentationBinding (-not [string]::IsNullOrWhiteSpace($binding.candidateRevisionOrFingerprint)) "CandidateMissing:$code"
            Assert-PresentationBinding (@($binding.evidenceRefs).Count -gt 0) "EvidenceMissing:$code"
            if ($null -eq $EvidenceCatalog) {
                $EvidenceCatalog = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'eng/execution-ledgers/evidence-packages.json') -Raw -Encoding UTF8 | ConvertFrom-Json
            }
            $coveredPaths = @{}
            $kinds = @()
            foreach ($evidenceId in @($binding.evidenceRefs)) {
                $packages = @($EvidenceCatalog.packages | Where-Object evidenceId -eq $evidenceId)
                Assert-PresentationBinding ($packages.Count -eq 1) "EvidenceUnknown:${code}:$evidenceId"
                $package = $packages[0]
                Assert-PresentationBinding ($package.resultCode -eq 'Passed' -and $package.statusCode -eq 'Current' -and
                    $package.evidenceTrackCode -eq 'Presentation') "EvidenceNotCurrentPresentation:$code"
                # subjectRefs의 기존 폐루프 의미는 보존하고 세부 모듈/WI 범위는 별도 연결한다.
                $evidenceScope = $package.PSObject.Properties['presentationModuleScope']
                Assert-PresentationBinding ($null -ne $evidenceScope) "EvidenceModuleScopeMissing:$code"
                Assert-PresentationBinding (@($package.subjectRefs) -contains $WorkOrder.playableUnitStableId -and
                    @($evidenceScope.Value.moduleCodes) -contains $code -and
                    @($package.evidenceStageCodes) -contains $module.evidenceStageCode) "EvidenceScopeMismatch:$code"
                $scopeProperty = $WorkOrder.PSObject.Properties['presentationModuleScope']
                if ($null -ne $scopeProperty) {
                    foreach ($wi in @($scopeProperty.Value.worldInteractionIds)) {
                        Assert-PresentationBinding (@($evidenceScope.Value.worldInteractionIds) -contains $wi) "EvidenceWiMismatch:$code"
                    }
                }
                Assert-PresentationBinding ($package.sourceRevision -ceq $binding.candidateRevisionOrFingerprint) "EvidenceRevisionMismatch:$code"
                Assert-PresentationBinding (@($package.artifactReferences).Count -gt 0) "EvidenceArtifactsMissing:$code"
                $kinds += [string] $package.evidenceKindCode
                foreach ($artifact in @($package.artifactReferences)) {
                    Assert-PresentationBinding ([string] $artifact.sha256 -match '^[a-fA-F0-9]{64}$') "EvidenceHashInvalid:$code"
                    if ($artifact.locationKind -eq 'ExternalCheckout') {
                        Assert-PresentationBinding (-not [string]::IsNullOrWhiteSpace($UnityProjectRoot)) "EvidenceWorkspaceUnavailable:$code"
                        $base = [IO.Path]::GetFullPath($UnityProjectRoot).TrimEnd([char[]]@('/', '\'))
                        $path = [IO.Path]::GetFullPath([string] $artifact.locator)
                        Assert-PresentationBinding ($path.StartsWith($base + [IO.Path]::DirectorySeparatorChar,
                            [StringComparison]::OrdinalIgnoreCase)) "EvidenceOutsideWorkspace:$code"
                        Assert-PresentationBinding (Test-Path -LiteralPath $path -PathType Leaf) "EvidenceArtifactMissing:$code"
                    }
                    else {
                        Assert-PresentationBinding ($artifact.locationKind -in @('RepositoryPath','LocalArtifact')) "EvidenceLocationInvalid:$code"
                        $path = Resolve-PresentationModuleReference ('repo:' + $artifact.locator) $RepositoryRoot '' $true
                    }
                    Assert-PresentationBinding ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -eq $artifact.sha256) "EvidenceArtifactChanged:$code"
                    $coveredPaths[$path] = $true
                }
            }
            foreach ($path in $sourcePaths) {
                Assert-PresentationBinding ($coveredPaths.ContainsKey($path)) "ImplementationNotFingerprintBound:$code"
            }
            if ($module.evidenceStageCode -eq 'E3') {
                Assert-PresentationBinding (@($kinds | Where-Object { $_ -in @('AutomatedTest','UnityEditMode') }).Count -gt 0) "AutomatedEvidenceMissing:$code"
            }
            if ($module.evidenceStageCode -eq 'E7') {
                Assert-PresentationBinding ($kinds -contains 'UnityPlayMode' -and $kinds -contains 'GameView') "ActualPlayEvidenceMissing:$code"
            }
        }
        else {
            # 미연결 상태에 근거 ID를 넣더라도 Passed로 자동 변환하지 않는다.
            foreach ($evidenceId in @($binding.evidenceRefs)) {
                Assert-PresentationBinding (-not [string]::IsNullOrWhiteSpace([string] $evidenceId)) "EvidenceIdEmpty:$code"
            }
        }
        $byCode[$code] = [pscustomobject]@{
            moduleCode = $code; evidenceStageCode = $module.evidenceStageCode
            statusCode = $binding.statusCode; reason = $binding.reason
            implementationRefs = @($binding.implementationRefs); testRefs = @($binding.testRefs)
            evidenceRefs = @($binding.evidenceRefs)
        }
    }
    foreach ($module in $required) {
        Assert-PresentationBinding ($byCode.ContainsKey([string] $module.moduleCode)) "RequiredModuleMissing:$($module.moduleCode)"
    }
    return @($required | ForEach-Object { $byCode[[string] $_.moduleCode] })
}
