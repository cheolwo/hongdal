$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$unityRoot = 'C:/Users/user/ssalddel'
$script:cases = 0

function Assert-FenceE5([bool] $condition, [string] $code) {
    if (-not $condition) { throw "HansFarmFenceRestorationE5Invalid:$code" }
    $script:cases++
}

function Read-Json([string] $path) {
    Get-Content -LiteralPath (Join-Path $root $path) -Raw -Encoding UTF8 |
        ConvertFrom-Json
}

$graph = Read-Json 'eng/world-seedbeds/graph-maps/hans-farm-fence-restoration.v1.json'
$placement = Read-Json 'eng/world-seedbeds/placement-map-profiles/hans-farm-first-fence-restoration-e5.v1.json'
$stages = Read-Json 'eng/execution-ledgers/evidence-stages.json'
$protocol = Read-Json 'eng/execution-ledgers/e7-vertical-implementation-protocol.json'
$modules = Read-Json 'eng/execution-ledgers/playable-loop-presentation-validation-modules.json'
$loops = Read-Json 'eng/execution-ledgers/playable-loops.json'
$wi = Read-Json 'eng/execution-ledgers/world-interactions.json'
$order = Read-Json 'eng/execution-ledgers/work-orders/hans-farm-fence-restoration.e7-work-order.json'
$delivery = Read-Json 'eng/execution-ledgers/world-interaction-delivery-priorities.json'
$goals = Read-Json 'eng/execution-ledgers/codex-playable-loop-goals.json'

$planningPath = Join-Path $root $graph.planningGate.documentRef
$planningHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $planningPath).Hash
Assert-FenceE5 ($planningHash -ceq $graph.planningGate.sha256) 'PlanningHash'
Assert-FenceE5 ($graph.planningGate.revision -ceq 'hans-farm-first-lumber-repair.r27') 'PlanningRevision'
Assert-FenceE5 ($graph.playableLoopStableId -ceq $placement.playableLoopStableId) 'GraphPlacementLoop'
$placementRef = $placement.profileStableId
Assert-FenceE5 ($placementRef -ceq
    'placement-map:hans-farm:first-fence-restoration-e5.v1') 'PlacementStableId'

$nodes = @($graph.level1.nodes)
$edges = @($graph.level1.edges)
Assert-FenceE5 ($nodes.Count -eq 7 -and $edges.Count -eq 5) 'FocusedGraphShape'
$broken = @($nodes | Where-Object roleCode -eq 'IncidentPickup')[0]
Assert-FenceE5 (-not $broken.equipmentAllowed -and $broken.stableRef -ceq 'pickup:hans-farm:broken-axe:first-incident') 'BrokenAxeSeparateFromEquipment'
$fence = @($nodes | Where-Object roleCode -eq 'AtomicRepairTarget')[0]
Assert-FenceE5 (@($fence.requiredSegmentIds).Count -eq 3 -and
    $fence.h2Ref -ceq 'h2-candidate:hans-farm:first-restoration' -and
    $fence.h1Ref -ceq 'h1-stock:farm-fence-edge') 'FenceHierarchyAndSegments'
$repairEdge = @($edges | Where-Object { $_.PSObject.Properties['worldInteractionId'] -and $_.worldInteractionId -eq 'WI-NATURE-20' })[0]
Assert-FenceE5 ($repairEdge.effect -ceq 'ConsumeTwoTimberAndRepairThreeSegmentsAtomically') 'AtomicRepairEdge'
Assert-FenceE5 ((@($edges | Where-Object { $_.PSObject.Properties['worldInteractionId'] } | ForEach-Object { $_.worldInteractionId }) -join ',') -ceq
    'WI-NATURE-19,WI-NATURE-06,WI-NATURE-18,WI-NATURE-20') 'WorldInteractionOrder'

Assert-FenceE5 (@($placement.instances | Where-Object placementRole -eq 'FenceSegment').Count -eq 3) 'ThreePlacementSegments'
Assert-FenceE5 ($placement.placementRootStableId -ceq 'placement-root:hans-farm:first-restoration') 'PlacementRoot'
Assert-FenceE5 ($placement.e5Checks -contains 'ActiveRenderer' -and
    $placement.e5Checks -contains 'ColliderPresent' -and
    $placement.e5Checks -contains 'NonZeroBounds') 'E5PhysicalReadabilityChecks'
Assert-FenceE5 ($placement.animationBoundary.fenceRepairAnimationRole -ceq 'NotApplicable' -and
    $placement.animationBoundary.woodcuttingActualAnimationStage -ceq 'E6') 'AnimationBoundary'

$e5 = @($stages.stages | Where-Object code -eq 'E5')[0]
$e6 = @($stages.stages | Where-Object code -eq 'E6')[0]
Assert-FenceE5 ($e5.completionGate.Contains('정적 자세') -and
    $e5.completionGate.Contains('실제 AnimationClip') -and
    $e5.completionGate.Contains('요구하지 않는다')) 'CommonE5MinimalWorldPolicy'
Assert-FenceE5 ($e6.completionGate.Contains('실제 AnimationClip') -and
    $e6.completionGate.Contains('Rig')) 'CommonE6AnimationPolicy'
Assert-FenceE5 ($protocol.principles.minimalReadableActionCueSatisfiesE5 -and
    $protocol.principles.actualAnimationBindingRequiredAtE6WhenActorAction) 'ProtocolE5E6Policy'
$actorActionModule = @($modules.modules | Where-Object moduleCode -eq 'actor-action-animation-readability')[0]
Assert-FenceE5 ($actorActionModule.evidenceStageCode -ceq 'E6' -and
    $actorActionModule.requiredFeatureCodes -contains 'ActorAction') 'ActorActionModuleAtE6'

$loop = @($loops.items | Where-Object loopStableId -eq $graph.playableLoopStableId)[0]
Assert-FenceE5 ($loop.planningGate.designHashSha256 -ceq $planningHash) 'LoopPlanningHash'
Assert-FenceE5 (($loop.worldInteractionIds -join ',') -ceq
    'WI-NATURE-19,WI-NATURE-06,WI-NATURE-18,WI-NATURE-20') 'LoopWorldInteractions'
Assert-FenceE5 ($loop.maturityTracks.logic.currentStage -ceq 'E5' -and
    $loop.maturityTracks.presentation.currentStage -ceq 'E4') 'HonestDualMaturity'
Assert-FenceE5 ($order.trackPlans.logic.currentEvidenceStage -ceq 'E5' -and
    $order.trackPlans.presentation.currentEvidenceStage -ceq 'E4' -and
    -not $order.promotionEligible) 'WorkOrderDualMaturity'

foreach ($worldInteractionId in @('WI-NATURE-19','WI-NATURE-20')) {
    $worldInteraction = @($wi.items | Where-Object {
        $_.id -ceq $worldInteractionId
    })
    Assert-FenceE5 ($worldInteraction.Count -eq 1) "WorldInteraction:$worldInteractionId"
    Assert-FenceE5 ((@($worldInteraction[0].integration.e5PlacementRefs) -join ',') -ceq
        $placementRef) "WorldInteractionPlacement:$worldInteractionId"
}
Assert-FenceE5 (@($delivery.items | Where-Object {
        $_.worldInteractionId -in @('WI-NATURE-19','WI-NATURE-20') -and
        $_.playableLoopRefs -contains $graph.playableLoopStableId
    }).Count -eq 2) 'DeliveryPriorityCoverage'
$goal = @($goals.items | Where-Object loopStableId -eq $graph.playableLoopStableId)[0]
Assert-FenceE5 ($goal.goalStateCode -ceq 'Queued' -and
    $goal.nextWorldInteractionId -ceq 'WI-NATURE-19') 'QueuedGoalStartsWithBrokenAxePickup'

$contract = Get-Content -LiteralPath (Join-Path $root 'Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationNatureSurvivalContracts.cs') -Raw -Encoding UTF8
$domain = Get-Content -LiteralPath (Join-Path $root 'Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationHansFarmFenceRestoration.cs') -Raw -Encoding UTF8
$presenterPath = Join-Path $unityRoot 'Assets/Ssalddel/Presentation/World/Hans농장울타리E5Presenter.cs'
$presenterTestPath = Join-Path $unityRoot 'Assets/Ssalddel/Tests/EditMode/Hans농장울타리E5PresenterTests.cs'
Assert-FenceE5 (Test-Path -LiteralPath $presenterPath) 'UnityPresenterExists'
Assert-FenceE5 (Test-Path -LiteralPath $presenterTestPath) 'UnityPresenterTestExists'
$presenter = Get-Content -LiteralPath $presenterPath -Raw -Encoding UTF8
$presenterTest = Get-Content -LiteralPath $presenterTestPath -Raw -Encoding UTF8
Assert-FenceE5 ($contract.Contains('ProfileRevisionR6') -and
    $contract.Contains('HansFarmFenceRestorationPlayableLoopStableId') -and
    $contract.Contains('pickup:hans-farm:broken-axe:first-incident')) 'ContractsR6'
Assert-FenceE5 ($domain.Contains('HansFarmFenceRepairTimberCost') -and
    $domain.Contains('foreach (var segment in natureHansFarmFenceRestoration.Segments)')) 'DomainAtomicRepair'
Assert-FenceE5 ($presenter.Contains('SimulationNatureSurvivalStateSnapshot') -and
    $presenter.Contains('EnsureCollider') -and
    $presenter.Contains('ValidateSource(state, source)') -and
    $presenter.Contains('public const string AnimationRole = "NotApplicable"')) 'PresenterReadOnlyE5Shape'
Assert-FenceE5 ($presenterTest.Contains('GetComponentsInChildren<Animator>') -and
    $presenterTest.Contains('HasVisibleBoundsAndCollider') -and
    $presenterTest.Contains('기존World표현을지우기전에거부된다')) 'PresenterTestE5Checks'

Write-Output "HansFarmFenceRestorationE5TestsPassed:Cases=$script:cases;LogicE5=True;PresentationE5=False;UnityCompileBlocked=True;PlayMode=False;GameView=False"
