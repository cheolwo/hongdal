# 개발 작업의 개수가 아니라 승인, 의존성과 쓰기 소유 경계를 검사한다.
function Get-ParallelDevelopmentWorkItems {
    param([object] $Ledger)
    if ($Ledger.PSObject.Properties['workItems']) { return @($Ledger.workItems) }
    # v3 읽기 호환. 이 투영을 새 작업의 승인 자료로 사용하지 않는다.
    $focus = $Ledger.activeGoal
    if ($focus -and [string] $focus.goalStateCode -eq 'Active') {
        return [pscustomobject]@{
            workItemId = 'legacy-focus'; loopStableId = $focus.loopStableId
            worldInteractionId = $focus.activeWorldInteractionId
            trackCode = $focus.activeMaturityTrackCode; statusCode = 'Active'
            workOrderRef = $focus.workOrderRef
        }
    }
}

function Get-ParallelWorkField {
    param([object] $Object, [string] $Name, $Default = $null)
    if ($Object -is [Collections.IDictionary] -and $Object.Contains($Name)) { return $Object[$Name] }
    if ($null -ne $Object -and $Object.PSObject.Properties[$Name]) { return $Object.$Name }
    return $Default
}

function Resolve-ParallelWorkPath {
    param([string] $RepositoryRoot, [string] $Path)
    if ([string]::IsNullOrWhiteSpace($Path) -or [IO.Path]::IsPathRooted($Path) -or $Path -match '[*?]') {
        throw "ParallelDevelopmentInvalid:RepositoryRelativePathRequired:$Path"
    }
    $root = [IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\', '/')
    $resolved = [IO.Path]::GetFullPath((Join-Path $root $Path))
    if (-not $resolved.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "ParallelDevelopmentInvalid:PathOutsideRepository:$Path"
    }
    return $resolved.TrimEnd('\', '/')
}

function Test-ParallelDevelopmentWorkItems {
    param([object] $Ledger, [object] $Loops, [string] $RepositoryRoot)
    $work = @(Get-ParallelDevelopmentWorkItems -Ledger $Ledger)
    if ([string] $Ledger.schemaVersion -ne 'codex-playable-loop-goals.v4') { return }
    $byId = @{}; $paths = @{}; $visits = @{}
    $loopById = @{}; $goalById = @{}
    foreach ($loop in @($Loops.items)) { $loopById[[string] $loop.loopStableId] = $loop }
    foreach ($goal in @($Ledger.items)) { $goalById[[string] $goal.loopStableId] = $goal }
    foreach ($item in $work) {
        $id = [string] $item.workItemId
        foreach ($field in @('workItemId','loopStableId','worldInteractionId','trackCode','statusCode','ownerThreadId','worktreePath','workOrderRef','workOrderSha256','targetEvidenceStageCode')) {
            if ([string]::IsNullOrWhiteSpace([string](Get-ParallelWorkField $item $field))) { throw "ParallelDevelopmentInvalid:FieldMissing:${id}:$field" }
        }
        if ($byId.ContainsKey($id)) { throw "ParallelDevelopmentInvalid:DuplicateWorkItem:$id" }
        if ($item.statusCode -notin @('Active','Blocked','ReadyForIntegration','Integrated')) { throw "ParallelDevelopmentInvalid:StatusInvalid:$id" }
        if ($item.trackCode -notin @('Logic','Presentation')) { throw "ParallelDevelopmentInvalid:TrackInvalid:$id" }
        if ($item.targetEvidenceStageCode -notmatch '^E[1-7]$') { throw "ParallelDevelopmentInvalid:TargetInvalid:$id" }
        if (-not [IO.Path]::IsPathRooted([string] $item.worktreePath)) { throw "ParallelDevelopmentInvalid:WorktreePathRequired:$id" }
        foreach ($arrayName in @('writePaths','sharedContractKeys','dependsOnWorkItemIds','baselineFiles')) {
            if (-not $item.PSObject.Properties[$arrayName]) { throw "ParallelDevelopmentInvalid:FieldMissing:${id}:$arrayName" }
        }
        if (@($item.writePaths).Count -eq 0) { throw "ParallelDevelopmentInvalid:WriteOwnershipMissing:$id" }
        if (@($item.baselineFiles).Count -eq 0) { throw "ParallelDevelopmentInvalid:BaselineMissing:$id" }
        if (-not $loopById.ContainsKey([string] $item.loopStableId) -or -not $goalById.ContainsKey([string] $item.loopStableId)) { throw "ParallelDevelopmentInvalid:LoopUnknown:$id" }
        $loop = $loopById[[string] $item.loopStableId]
        if ([string] $loop.loopLevelCode -ne 'PlayableUnit' -or @($loop.worldInteractionIds) -notcontains [string] $item.worldInteractionId) { throw "ParallelDevelopmentInvalid:WorkOutsideLoop:$id" }
        $paths[$id] = @($item.writePaths | ForEach-Object { Resolve-ParallelWorkPath $RepositoryRoot ([string] $_) } | Sort-Object -Unique)
        $byId[$id] = $item
    }
    # 누락/순환은 대기 상태에도 허용하지 않는다. 대기 작업은 다른 작업을 막지 않는다.
    function Visit-ParallelDependency([string] $Id) {
        if ($visits[$Id] -eq 'Visiting') { throw "ParallelDevelopmentInvalid:DependencyCycle:$Id" }
        if ($visits[$Id] -eq 'Visited') { return }
        $visits[$Id] = 'Visiting'
        foreach ($dependency in @($byId[$Id].dependsOnWorkItemIds)) {
            if (-not $byId.ContainsKey([string] $dependency)) { throw "ParallelDevelopmentInvalid:DependencyUnknown:${Id}:$dependency" }
            Visit-ParallelDependency ([string] $dependency)
        }
        $visits[$Id] = 'Visited'
    }
    foreach ($id in @($byId.Keys)) { Visit-ParallelDependency $id }
    # 통합 상태 문자열만으로 선행 작업을 통과시키지 않는다. 과거 소스 대신
    # 고정된 승인 기록과 결과 근거를 확인하여 이후 소스 변경과 분리한다.
    $integratedValid = @{}
    foreach ($item in @($work | Where-Object statusCode -eq 'Integrated')) {
        $valid = $false
        try {
            $receiptRef = [string](Get-ParallelWorkField $item 'integrationReceiptRef')
            $receiptPath = Resolve-ParallelWorkPath $RepositoryRoot $receiptRef
            $receiptHash = [string](Get-ParallelWorkField $item 'integrationReceiptSha256')
            $receipt = Get-Content -LiteralPath $receiptPath -Raw -Encoding UTF8 | ConvertFrom-Json
            $owner = [string](Get-ParallelWorkField (Get-ParallelWorkField $Ledger 'policy') 'integrationOwnerThreadId')
            $valid = $receiptHash -match '^[a-fA-F0-9]{64}$' -and
                (Get-FileHash -LiteralPath $receiptPath -Algorithm SHA256).Hash -eq $receiptHash -and
                $receipt.statusCode -eq 'Accepted' -and $receipt.workItemId -eq $item.workItemId -and
                $receipt.worldInteractionId -eq $item.worldInteractionId -and $receipt.loopStableId -eq $item.loopStableId -and
                $receipt.workOrderSha256 -eq $item.workOrderSha256 -and $receipt.targetEvidenceStageCode -eq $item.targetEvidenceStageCode -and
                -not [string]::IsNullOrWhiteSpace($owner) -and $receipt.acceptedByThreadId -eq $owner -and
                -not [string]::IsNullOrWhiteSpace([string] $receipt.acceptedAt) -and @($receipt.artifactRefs).Count -gt 0
            foreach ($artifact in @($receipt.artifactRefs)) {
                $artifactPath = Resolve-ParallelWorkPath $RepositoryRoot ([string] $artifact.path)
                if ((Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256).Hash -ne [string] $artifact.sha256) { $valid = $false }
            }
        } catch { $valid = $false }
        $integratedValid[[string] $item.workItemId] = $valid
    }
    $executing = @($work | Where-Object { $_.statusCode -in @('Active','ReadyForIntegration') })
    for ($i = 0; $i -lt $executing.Count; $i++) {
        for ($j = $i + 1; $j -lt $executing.Count; $j++) {
            $left = $executing[$i]; $right = $executing[$j]
            foreach ($a in $paths[[string] $left.workItemId]) {
                foreach ($b in $paths[[string] $right.workItemId]) {
                    if ($a.Equals($b, [StringComparison]::OrdinalIgnoreCase) -or
                        $a.StartsWith($b + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
                        $b.StartsWith($a + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
                        throw "ParallelDevelopmentInvalid:WriteOwnershipConflict:$($left.workItemId):$($right.workItemId):$a"
                    }
                }
            }
            foreach ($key in @($left.sharedContractKeys)) {
                if ([string]::IsNullOrWhiteSpace([string] $key)) { throw 'ParallelDevelopmentInvalid:EmptyContractKey' }
                if (@($right.sharedContractKeys) -contains $key) { throw "ParallelDevelopmentInvalid:SharedContractConflict:$key" }
            }
        }
    }
    foreach ($item in $work) {
        $id = [string] $item.workItemId
        $blockers = [Collections.Generic.List[string]]::new()
        $order = $null
        if ($item.statusCode -eq 'Integrated' -and -not $integratedValid[$id]) { $blockers.Add('IntegrationReceiptInvalid') }
        if ($item.statusCode -ne 'Integrated') {
            $gate = Get-ParallelWorkField $item 'planningGate'
            if ((Get-ParallelWorkField $gate 'statusCode') -ne 'Approved' -or
                [string]::IsNullOrWhiteSpace([string](Get-ParallelWorkField $gate 'approvalEvidenceRef'))) { $blockers.Add('PlanningNotApproved') }
            foreach ($binding in @(@{path=$item.workOrderRef;sha256=$item.workOrderSha256}) + @($item.baselineFiles)) {
                $path = Resolve-ParallelWorkPath $RepositoryRoot ([string] $binding.path)
                if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { $blockers.Add("BaselineFileMissing:$($binding.path)") }
                elseif ([string] $binding.sha256 -notmatch '^[a-fA-F0-9]{64}$' -or
                    (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne [string] $binding.sha256) { $blockers.Add("BaselineHashMismatch:$($binding.path)") }
            }
            $designRef = [string](Get-ParallelWorkField $gate 'designDocumentRef')
            if ([string]::IsNullOrWhiteSpace($designRef)) { $blockers.Add('DesignDocumentMissing') }
            else {
                $designPath = Resolve-ParallelWorkPath $RepositoryRoot $designRef
                if (-not (Test-Path -LiteralPath $designPath -PathType Leaf)) { $blockers.Add('DesignDocumentMissing') }
                elseif ((Get-FileHash -LiteralPath $designPath -Algorithm SHA256).Hash -ne [string](Get-ParallelWorkField $gate 'designHashSha256')) { $blockers.Add('DesignHashMismatch') }
            }
            $orderPath = Resolve-ParallelWorkPath $RepositoryRoot ([string] $item.workOrderRef)
            if (Test-Path -LiteralPath $orderPath -PathType Leaf) {
                try {
                $order = Get-Content -LiteralPath $orderPath -Raw -Encoding UTF8 | ConvertFrom-Json
                if ($order.schemaVersion -ne 'simulation-e7-vertical-work-order.v2' -or $order.playableUnitStableId -ne $item.loopStableId -or $order.activeWorldInteractionId -ne $item.worldInteractionId) { $blockers.Add('WorkOrderBindingMismatch') }
                if ($order.integratedGate.candidateRevision -ne (Get-ParallelWorkField $gate 'designRevision')) { $blockers.Add('DesignRevisionMismatch') }
                $cap = [string] $order.deliveryCap.currentDispatchTargetStage
                if ($cap -notmatch '^E[1-7]$' -or [int] $item.targetEvidenceStageCode.Substring(1) -gt [int] $cap.Substring(1)) { $blockers.Add('DeliveryCapExceeded') }
                $track = Get-ParallelWorkField $order.trackPlans ([string] $item.trackCode)
                if ($null -eq $track) { $blockers.Add('TrackPlanMissing') }
                elseif (@($track.downwardPlan | Where-Object { $_.code -eq $item.targetEvidenceStageCode -and $_.disposition -ne 'Blocked' }).Count -ne 1) { $blockers.Add('TrackTargetNotApproved') }
                if ($item.trackCode -eq 'Presentation' -and [int] $item.targetEvidenceStageCode.Substring(1) -ge 5 -and [int] $order.trackPlans.Logic.currentEvidenceStage.Substring(1) -lt 5) { $blockers.Add('PresentationRequiresLogicE5') }
                $profileKey = "$($item.loopStableId)|$($item.worldInteractionId)"
                if ($order.pipelineValidation.profileKey -ne $profileKey) { $blockers.Add('PipelineBindingMismatch') }
                } catch {
                    $order = $null
                    $blockers.Add('WorkOrderInvalid')
                }
            }
            foreach ($dependency in @($item.dependsOnWorkItemIds)) {
                if ($byId[[string] $dependency].statusCode -ne 'Integrated' -or -not $integratedValid[[string] $dependency]) { $blockers.Add("DependencyNotIntegrated:$dependency") }
            }
            foreach ($loopDependency in @($goalById[[string] $item.loopStableId].activationPrerequisiteLoopStableIds)) {
                if (-not $goalById.ContainsKey([string] $loopDependency) -or $goalById[[string] $loopDependency].goalStateCode -ne 'Completed') { $blockers.Add("GoalDependencyNotCompleted:$loopDependency") }
            }
        }
        [pscustomobject]@{
            workItemId=$id; worldInteractionId=$item.worldInteractionId; loopStableId=$item.loopStableId
            statusCode=$item.statusCode; canExecute=($blockers.Count -eq 0 -and $item.statusCode -in @('Active','ReadyForIntegration'))
            blockerCodes=@($blockers.ToArray()); workOrder=$order
        }
    }
}
