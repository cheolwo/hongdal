using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string NpcRoutinePlannerStableId =
            "actor:system:npc-routine-planner";
        private readonly Dictionary<string, SimulationNpcRoutineExecutionSnapshot>
            npcRoutineExecutions =
                new Dictionary<string, SimulationNpcRoutineExecutionSnapshot>(
                    StringComparer.Ordinal);

        private bool IsNpcRoutineControlEnabled
            => string.Equals(NpcRoutineControlRevision,
                   SimulationNpcRoutineControlRevisionCodes.R1,
                   StringComparison.Ordinal)
               || string.Equals(NpcRoutineControlRevision,
                   SimulationNpcRoutineControlRevisionCodes.R2,
                   StringComparison.Ordinal)
               || string.Equals(NpcRoutineControlRevision,
                   SimulationNpcRoutineControlRevisionCodes.R3,
                   StringComparison.Ordinal);

        private bool IsNpcRoutineFullWarehouseEnabled
            => string.Equals(NpcRoutineControlRevision,
                   SimulationNpcRoutineControlRevisionCodes.R2,
                   StringComparison.Ordinal)
               || string.Equals(NpcRoutineControlRevision,
                   SimulationNpcRoutineControlRevisionCodes.R3,
                   StringComparison.Ordinal);

        private bool IsNpcRoutineNatureEnabled
            => string.Equals(NpcRoutineControlRevision,
                SimulationNpcRoutineControlRevisionCodes.R3,
                StringComparison.Ordinal);

        public SimulationNpcRoutineWorkProjection[] GetNpcRoutineWork(string areaCode)
        {
            var normalizedAreaCode = (areaCode ?? string.Empty).Trim();
            if (!string.Equals(normalizedAreaCode, "Hub", StringComparison.Ordinal)
                && !string.Equals(normalizedAreaCode, "Nature",
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationNpcRoutineAreaCodeInvalid");

            lock (gate)
            {
                if (string.Equals(normalizedAreaCode, "Nature",
                        StringComparison.Ordinal))
                    return new[] { CreateNatureFieldSupplyRoutineProjection() };

                return npcFacilityInventories.Values
                    .Where(value => IsHubRoutineInventoryState(value.StateCode))
                    .OrderBy(value => value.UpdatedTick)
                    .ThenBy(value => value.InventoryStableId, StringComparer.Ordinal)
                    .Select(CreateHubRoutineProjection)
                    .ToArray();
            }
        }

        private void EvaluateNpcRoutineWork()
        {
            if (!IsNpcRoutineControlEnabled) return;

            var inventory = npcFacilityInventories.Values
                .Where(value => IsNpcRoutineCandidateState(value.StateCode)
                    && !HasActiveNpcRoutineTask(value.InventoryStableId))
                .OrderBy(value => NpcRoutineStageOrder(value.StateCode))
                .ThenBy(value => value.UpdatedTick)
                .ThenBy(value => value.InventoryStableId,
                    StringComparer.Ordinal)
                .FirstOrDefault();
            if (inventory == null) return;

            var actionCode = NpcActionCodeForInventory(inventory.StateCode);
            var policy = FindNpcPolicy(inventory.FacilityStableId, actionCode);
            if (policy == null || !policy.AutomationEnabled) return;

            var decisionRequest = CreateNpcRoutineDecisionRequest(policy,
                inventory, false);
            var preview = CreateDecisionPreview(decisionRequest);
            if (preview.Decision.BlockReasonCodes.Length > 0) return;

            ConfirmDecisionCore(new SimulationDecisionConfirmRequest
            {
                CommandId = BuildNpcRoutineCommandId(policy, inventory),
                ExpectedRevision = Revision,
                Preview = decisionRequest,
            }, false, _ => { });
        }

        private SimulationDecisionPreviewRequest CreateNpcRoutineDecisionRequest(
            SimulationNpcWorkPolicySnapshot policy,
            SimulationNpcFacilityInventorySnapshot inventory,
            bool includeValidationBlocks)
        {
            if (string.Equals(inventory.StateCode,
                    SimulationNpcInventoryStateCodes.PendingInspection,
                    StringComparison.Ordinal))
                return CreateNpcInboundInspectionDecision(policy, inventory,
                    includeValidationBlocks);
            if (string.Equals(inventory.StateCode,
                    SimulationNpcInventoryStateCodes.StorageEligible,
                    StringComparison.Ordinal))
                return CreateWarehousePutAwayDecisionRequest(
                    new SimulationWarehousePutAwayPreviewRequest
                    {
                        InventoryStableId = inventory.InventoryStableId,
                        InventoryRevision = inventory.Revision,
                        ActorStableId = NpcRoutinePlannerStableId,
                        PreferredSpatialStableId =
                            PyeongchangSimulation공간StableIds.진부Hub창고공간,
                        PutAwayDurationTicks = policy.TravelDurationTicks
                                               + policy.WorkDurationTicks,
                        SourceStableIds = new[]
                        {
                            policy.PolicyStableId,
                            inventory.InventoryStableId,
                            "wi:WI-002",
                            "trigger:npc-driven",
                            "rule:npc-routine-control.r2",
                        },
                    }, includeValidationBlocks);

            return CreateSupplyChainDecisionRequest(
                CreateNpcOutboundWork(policy, inventory),
                includeValidationBlocks);
        }

        private SimulationDecisionPreviewRequest
            CreateNpcInboundInspectionDecision(
                SimulationNpcWorkPolicySnapshot policy,
                SimulationNpcFacilityInventorySnapshot inventory,
                bool includeValidationBlocks)
        {
            var blocks = new List<string>();
            if (includeValidationBlocks)
            {
                if (!string.Equals(inventory.StateCode,
                        SimulationNpcInventoryStateCodes.PendingInspection,
                        StringComparison.Ordinal))
                    blocks.Add("SimulationWarehouseInventoryNotInspectionPending");
                if (HasActiveNpcRoutineTask(inventory.InventoryStableId))
                    blocks.Add("SimulationNpcRoutineWorkAlreadyScheduled");
            }
            var sources = MergeSources(inventory.SourceStableIds, new[]
            {
                policy.PolicyStableId,
                inventory.InventoryStableId,
                "wi:WI-001",
                "trigger:npc-driven",
                "rule:npc-routine-control.r2",
            });
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:npc-routine:hub-inspection:"
                                   + inventory.InventoryStableId,
                DecisionTypeCode =
                    SimulationNpcActionCodes.WarehouseInboundInspection,
                ActorStableId = NpcRoutinePlannerStableId,
                TargetStableIds = new[]
                {
                    inventory.InventoryStableId,
                    inventory.LotStableId,
                    "inventory-revision:" + inventory.Revision,
                },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode =
                            Simulation창고작업상태Codes.InspectionCompleted,
                        TargetLedgerStableId = inventory.InventoryStableId,
                        BeforeValue = 0m,
                        Delta = inventory.Quantity,
                        AfterValue = inventory.Quantity,
                        UnitCode = inventory.UnitCode,
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:npc-routine:hub-inspection:"
                                   + inventory.InventoryStableId,
                    TaskTypeCode = "WarehouseInboundInspection",
                    FacilityStableId = inventory.FacilityStableId,
                    ActionCode =
                        SimulationNpcActionCodes.WarehouseInboundInspection,
                    AssignedActorStableId = NpcRoutinePlannerStableId,
                    PreferredSpatialStableId =
                        PyeongchangSimulation공간StableIds.진부Hub검수공간,
                    AssignedCapacity = inventory.Quantity,
                    AssignedCapacityUnitCode = inventory.UnitCode,
                    DurationTicks = policy.TravelDurationTicks
                                    + policy.WorkDurationTicks,
                    InputLotStableIds = new[] { inventory.LotStableId },
                    OutputCandidateCodes = new[]
                    {
                        SimulationNpcInventoryStateCodes.StorageEligible,
                    },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationSupplyChainWorkPreviewRequest CreateNpcOutboundWork(
            SimulationNpcWorkPolicySnapshot policy,
            SimulationNpcFacilityInventorySnapshot inventory)
            => new SimulationSupplyChainWorkPreviewRequest
            {
                InventoryStableId = inventory.InventoryStableId,
                InventoryRevision = inventory.Revision,
                ActionCode = SimulationSupplyChainActionCodes.WarehouseOutboundFlow,
                ActorStableId = NpcRoutinePlannerStableId,
                PreferredSpatialStableId =
                    PyeongchangSimulation공간StableIds.진부Hub피킹공간,
                DurationTicks = policy.TravelDurationTicks + policy.WorkDurationTicks,
                SourceStableIds = new[]
                {
                    policy.PolicyStableId,
                    inventory.InventoryStableId,
                    "wi:WI-HUB-03",
                    "trigger:npc-driven",
                    "rule:npc-routine-control.r1",
                },
            };

        private static string BuildNpcRoutineCommandId(
            SimulationNpcWorkPolicySnapshot policy,
            SimulationNpcFacilityInventorySnapshot inventory)
            => "command:npc-routine:hub:"
                + policy.ActionCode.ToLowerInvariant() + ":"
                + policy.PolicyStableId + ":" + inventory.InventoryStableId
                + ":r" + inventory.Revision;

        private bool HasActiveNpcRoutineTask(string inventoryStableId)
            => tasks.Values.Any(task =>
                task.StateCode != SimulationTaskStateCodes.Completed
                && task.StateCode != SimulationTaskStateCodes.Cancelled
                && decisions.TryGetValue(task.CausedByDecisionStableId,
                    out var decision)
                && decision.TargetStableIds.Contains(inventoryStableId,
                    StringComparer.Ordinal));

        private bool IsNpcRoutineCandidateState(string stateCode)
            => string.Equals(stateCode,
                   SimulationNpcInventoryStateCodes.PutAwayCompleted,
                   StringComparison.Ordinal)
               || IsNpcRoutineFullWarehouseEnabled
               && (string.Equals(stateCode,
                       SimulationNpcInventoryStateCodes.PendingInspection,
                       StringComparison.Ordinal)
                   || string.Equals(stateCode,
                       SimulationNpcInventoryStateCodes.StorageEligible,
                       StringComparison.Ordinal));

        private static int NpcRoutineStageOrder(string stateCode)
            => string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.PendingInspection,
                    StringComparison.Ordinal)
                ? 0
                : string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.StorageEligible,
                    StringComparison.Ordinal)
                    ? 1 : 2;

        private static string NpcActionCodeForInventory(string stateCode)
            => string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.PendingInspection,
                    StringComparison.Ordinal)
                ? SimulationNpcActionCodes.WarehouseInboundInspection
                : string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.StorageEligible,
                    StringComparison.Ordinal)
                    ? SimulationNpcActionCodes.WarehouseStorageMove
                    : SimulationNpcActionCodes.WarehouseOutboundFlow;

        private void RegisterNpcRoutineExecutionForTask(SimulationTaskSnapshot task)
        {
            if (!IsNpcRoutineControlEnabled
                || !task.SourceStableIds.Contains("trigger:npc-driven",
                    StringComparer.Ordinal))
                return;

            var inventory = FindSupplyChainInventory(task);
            if (inventory == null)
                inventory = npcFacilityInventories.Values.FirstOrDefault(value =>
                    task.InputLotStableIds.Contains(value.LotStableId,
                        StringComparer.Ordinal)
                    && string.Equals(value.FacilityStableId,
                        task.FacilityStableId, StringComparison.Ordinal));
            var assignment = FindNpcAssignment(task.TaskStableId);
            if (inventory == null || assignment == null) return;
            var worldInteractionId = string.Equals(task.ActionCode,
                    SimulationNpcActionCodes.WarehouseInboundInspection,
                    StringComparison.Ordinal)
                ? "WI-001"
                : string.Equals(task.ActionCode,
                    SimulationNpcActionCodes.WarehouseStorageMove,
                    StringComparison.Ordinal)
                    ? "WI-002"
                    : string.Equals(task.ActionCode,
                        SimulationNpcActionCodes.WarehouseOutboundFlow,
                        StringComparison.Ordinal)
                        ? "WI-HUB-03" : string.Empty;
            if (worldInteractionId.Length == 0) return;
            var parentWorldInteractionId = worldInteractionId == "WI-002"
                ? "WI-001"
                : worldInteractionId == "WI-HUB-03"
                  && IsNpcRoutineFullWarehouseEnabled
                    ? "WI-002" : string.Empty;
            var parentExecutionStableId = parentWorldInteractionId.Length == 0
                ? string.Empty
                : BuildNpcRoutineExecutionId(parentWorldInteractionId,
                    inventory.InventoryStableId);
            RecordNpcRoutineExecution(worldInteractionId,
                parentExecutionStableId,
                assignment.PolicyStableId, task.TaskStableId,
                inventory.InventoryStableId,
                assignment.ActorStableId,
                SimulationWorldInteractionTriggerSourceCodes.NpcDriven);
        }

        private void ObserveNpcRoutineSupplyChainTransition(
            SimulationTaskSnapshot task,
            SimulationNpcFacilityInventorySnapshot inventory,
            string worldInteractionId)
        {
            if (!IsNpcRoutineControlEnabled) return;
            var assignment = FindNpcAssignment(task.TaskStableId);
            if (assignment == null) return;
            var parentWorldInteractionId = string.Equals(worldInteractionId,
                "WI-HUB-04", StringComparison.Ordinal)
                ? "WI-HUB-03" : "WI-HUB-04";
            var parentId = BuildNpcRoutineExecutionId(parentWorldInteractionId,
                inventory.InventoryStableId);
            RecordNpcRoutineExecution(worldInteractionId, parentId,
                assignment.PolicyStableId, task.TaskStableId,
                inventory.InventoryStableId,
                assignment.ActorStableId,
                SimulationWorldInteractionTriggerSourceCodes.WorldDerived);
        }

        private void RecordNpcRoutineExecution(
            string worldInteractionId,
            string parentExecutionStableId,
            string policyStableId,
            string taskStableId,
            string inventoryStableId,
            string actorStableId,
            string triggerSourceCode)
        {
            var executionId = BuildNpcRoutineExecutionId(worldInteractionId,
                inventoryStableId);
            if (npcRoutineExecutions.ContainsKey(executionId)) return;
            npcRoutineExecutions.Add(executionId,
                new SimulationNpcRoutineExecutionSnapshot
                {
                    ExecutionStableId = executionId,
                    AreaCode = "Hub",
                    WorldInteractionId = worldInteractionId,
                    ParentExecutionStableId = parentExecutionStableId,
                    PolicyStableId = policyStableId,
                    TaskStableId = taskStableId,
                    InventoryStableId = inventoryStableId,
                    OriginCode = SimulationWorldInteractionOriginCodes.OperationsDerived,
                    ControlPolicyCode =
                        SimulationWorldInteractionControlPolicyCodes.NpcRoutine,
                    TriggerSourceCode = triggerSourceCode,
                    ActorStableId = actorStableId,
                    음양주체분류 = SimulationWI음양주체사분면Rules.Resolve(
                        worldInteractionId,
                        Simulation세계상호작용이름Catalog.Find(
                                worldInteractionId)?.Actor전환필요 == true
                            && string.Equals(triggerSourceCode,
                                SimulationWorldInteractionTriggerSourceCodes
                                    .WorldDerived,
                                StringComparison.Ordinal)
                                ? SimulationWI수행주체Codes.NotApplicable
                                : SimulationWI수행주체Codes.NpcActor,
                        triggerSourceCode),
                    RecordedWorldTick = CurrentTick,
                    Revision = 1,
                });
        }

        private static string BuildNpcRoutineExecutionId(
            string worldInteractionId,
            string inventoryStableId)
            => "npc-routine-execution:" + worldInteractionId.ToLowerInvariant()
                + ":" + inventoryStableId;

        private SimulationNpcRoutineWorkProjection CreateHubRoutineProjection(
            SimulationNpcFacilityInventorySnapshot inventory)
        {
            var expectedAction = NpcActionCodeForInventory(inventory.StateCode);
            var task = tasks.Values.FirstOrDefault(value =>
                string.Equals(value.ActionCode, expectedAction,
                    StringComparison.Ordinal)
                && decisions.TryGetValue(value.CausedByDecisionStableId,
                    out var decision)
                && decision.TargetStableIds.Contains(inventory.InventoryStableId,
                    StringComparer.Ordinal));
            var assignment = task == null ? null : FindNpcAssignment(task.TaskStableId);
            var policy = assignment != null
                && npcWorkPolicies.TryGetValue(assignment.PolicyStableId, out var assignedPolicy)
                    ? assignedPolicy
                    : FindNpcPolicy(inventory.FacilityStableId,
                        expectedAction);
            var worldInteractionId = WorldInteractionIdForInventory(inventory.StateCode);
            var executionId = BuildNpcRoutineExecutionId(worldInteractionId,
                inventory.InventoryStableId);
            npcRoutineExecutions.TryGetValue(executionId, out var execution);

            var blocks = assignment?.BlockReasonCodes.ToArray()
                ?? CandidateBlockReasons(policy, inventory);
            var phase = assignment?.PhaseCode
                ?? (blocks.Length > 0 ? SimulationNpcActionPhaseCodes.Blocked
                    : SimulationNpcActionPhaseCodes.Candidate);
            var interventions = new List<string>
            {
                SimulationNpcRoutinePlayerInterventionCodes.ChangePolicy,
            };
            if (task != null
                && task.StateCode != SimulationTaskStateCodes.Completed
                && task.StateCode != SimulationTaskStateCodes.Cancelled)
                interventions.Add(
                    SimulationNpcRoutinePlayerInterventionCodes.CancelBeforeCompletion);
            if (blocks.Any(value => value.IndexOf("Capacity",
                    StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("Spatial", StringComparison.OrdinalIgnoreCase) >= 0))
                interventions.Add(
                    SimulationNpcRoutinePlayerInterventionCodes.ResolveFacilityCapacity);

            return new SimulationNpcRoutineWorkProjection
            {
                ProjectionStableId = "npc-routine-work:hub:"
                    + inventory.InventoryStableId,
                AreaCode = "Hub",
                WorldInteractionId = worldInteractionId,
                WorldInteractionName = Simulation세계상호작용이름Catalog
                    .한국어기능명(worldInteractionId),
                WorldInteractionDisplayName = Simulation세계상호작용이름Catalog
                    .한국어표시명(worldInteractionId),
                ResponsibilityKindCode = Simulation세계상호작용이름Catalog
                    .책임종류코드(worldInteractionId),
                PrimaryOutcomeCode = Simulation세계상호작용이름Catalog
                    .주요결과코드(worldInteractionId),
                SingleResponsibilityAssessmentCode =
                    Simulation세계상호작용이름Catalog
                        .단일책임판정코드(worldInteractionId),
                PolicyStableId = policy?.PolicyStableId ?? string.Empty,
                TaskStableId = task?.TaskStableId ?? string.Empty,
                NpcActorStableId = assignment?.ActorStableId ?? string.Empty,
                TargetInventoryStableId = inventory.InventoryStableId,
                PhaseCode = phase,
                ProgressRate = assignment == null ? 0m
                    : CalculateNpcActionProgress(assignment),
                OriginCode = SimulationWorldInteractionOriginCodes.OperationsDerived,
                ControlPolicyCode =
                    SimulationWorldInteractionControlPolicyCodes.NpcRoutine,
                TriggerSourceCode = execution?.TriggerSourceCode ?? string.Empty,
                음양주체분류 = CloneWI음양주체분류(
                    execution?.음양주체분류
                    ?? SimulationWI음양주체사분면Rules.Resolve(
                        worldInteractionId,
                        assignment == null
                            || Simulation세계상호작용이름Catalog.Find(
                                worldInteractionId)?.Actor전환필요 == true
                            ? SimulationWI수행주체Codes.NotApplicable
                            : SimulationWI수행주체Codes.NpcActor,
                        execution?.TriggerSourceCode ?? string.Empty)),
                BlockReasonCodes = blocks,
                AllowedPlayerInterventionCodes = interventions.ToArray(),
                WorldTick = CurrentTick,
                WorldRevision = Revision,
            };
        }

        private string[] CandidateBlockReasons(
            SimulationNpcWorkPolicySnapshot? policy,
            SimulationNpcFacilityInventorySnapshot inventory)
        {
            if (policy == null)
                return new[] { "SimulationNpcRoutinePolicyMissing" };
            if (!policy.AutomationEnabled)
                return new[] { "SimulationNpcAutomationDisabled" };
            var preview = CreateDecisionPreview(CreateNpcRoutineDecisionRequest(
                policy, inventory, false));
            return preview.Decision.BlockReasonCodes.ToArray();
        }

        private bool IsHubRoutineInventoryState(string stateCode)
            => IsNpcRoutineFullWarehouseEnabled
               && (string.Equals(stateCode,
                       SimulationNpcInventoryStateCodes.PendingInspection,
                       StringComparison.Ordinal)
                   || string.Equals(stateCode,
                       SimulationNpcInventoryStateCodes.StorageEligible,
                       StringComparison.Ordinal))
               || string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.PutAwayCompleted,
                    StringComparison.Ordinal)
                || string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.OutboundRequested,
                    StringComparison.Ordinal)
                || string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.Picked,
                    StringComparison.Ordinal)
                || string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.OutboundReady,
                    StringComparison.Ordinal);

        private static string WorldInteractionIdForInventory(string stateCode)
            => string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.PendingInspection,
                    StringComparison.Ordinal)
                ? "WI-001"
                : string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.StorageEligible,
                    StringComparison.Ordinal)
                    ? "WI-002"
                    : string.Equals(stateCode,
                        SimulationNpcInventoryStateCodes.Picked,
                    StringComparison.Ordinal)
                ? "WI-HUB-04"
                : string.Equals(stateCode,
                    SimulationNpcInventoryStateCodes.OutboundReady,
                    StringComparison.Ordinal)
                    ? "WI-HUB-05" : "WI-HUB-03";

        private void EnsureNpcRoutineDirectControlAllowed(
            SimulationDecisionPreviewRequest request)
        {
            if (!IsNpcRoutineControlEnabled
                || !(string.Equals(request.Task.ActionCode,
                        SimulationSupplyChainActionCodes.WarehouseOutboundFlow,
                        StringComparison.Ordinal)
                    || IsNpcRoutineFullWarehouseEnabled
                    && (string.Equals(request.Task.ActionCode,
                            SimulationNpcActionCodes.WarehouseInboundInspection,
                            StringComparison.Ordinal)
                        || string.Equals(request.Task.ActionCode,
                            SimulationNpcActionCodes.WarehouseStorageMove,
                            StringComparison.Ordinal))))
                return;
            throw new SimulationConflictException(
                "SimulationNpcRoutineDirectControlForbidden");
        }

        private SimulationNpcRoutineExecutionSnapshot[]
            CreateNpcRoutineExecutionSnapshots()
            => npcRoutineExecutions.Values
                .OrderBy(value => value.RecordedWorldTick)
                .ThenBy(value => value.ExecutionStableId, StringComparer.Ordinal)
                .Select(CloneNpcRoutineExecution).ToArray();

        internal static SimulationNpcRoutineExecutionSnapshot
            CloneNpcRoutineExecution(SimulationNpcRoutineExecutionSnapshot source)
            => new SimulationNpcRoutineExecutionSnapshot
            {
                ExecutionStableId = source.ExecutionStableId,
                AreaCode = source.AreaCode,
                WorldInteractionId = source.WorldInteractionId,
                ParentExecutionStableId = source.ParentExecutionStableId,
                PolicyStableId = source.PolicyStableId,
                TaskStableId = source.TaskStableId,
                InventoryStableId = source.InventoryStableId,
                OriginCode = source.OriginCode,
                ControlPolicyCode = source.ControlPolicyCode,
                TriggerSourceCode = source.TriggerSourceCode,
                ActorStableId = source.ActorStableId,
                음양주체분류 = CloneWI음양주체분류(source.음양주체분류),
                RecordedWorldTick = source.RecordedWorldTick,
                Revision = source.Revision,
            };
    }
}
