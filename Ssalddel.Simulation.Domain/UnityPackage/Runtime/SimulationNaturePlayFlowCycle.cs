using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// Nature 현장 획득(발산)과 거점 정책·위임(수렴)을 현장 보급 꾸러미로
    /// 다시 연결한다. NPC 루틴은 플레이어의 직접 제작 WI를 대체하지 않는다.
    /// </summary>
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string NatureFieldSupplyResultCode =
            "NatureFieldSupplyPackAdded";

        private Simulation플레이어기회Snapshot
            CreateNatureFieldSupplyDelegatedOpportunity()
        {
            var projection = CreateNatureFieldSupplyRoutineProjection();
            var 세계상호작용Id = SimulationNatureSurvivalCodes
                .PrepareFieldSupplyDelegatedWorldInteractionId;
            return new Simulation플레이어기회Snapshot
            {
                OpportunityStableId =
                    "opportunity:nature:wi-nature-17:npc-policy",
                PlayerActivityTrackCode =
                    Simulation플레이어활동경로Codes.AreaOperation,
                PlayerFlowCode = Simulation플레이흐름Codes.수렴,
                NextPlayerFlowCode = Simulation플레이흐름Codes.발산,
                CycleHandoffCode =
                    Simulation플레이흐름인계Codes.수렴에서발산,
                WorldInteractionId = SimulationNatureSurvivalCodes
                    .PrepareFieldSupplyDelegatedWorldInteractionId,
                WorldInteractionName = Simulation세계상호작용이름Catalog
                    .한국어기능명(SimulationNatureSurvivalCodes
                        .PrepareFieldSupplyDelegatedWorldInteractionId),
                WorldInteractionDisplayName = Simulation세계상호작용이름Catalog
                    .한국어표시명(SimulationNatureSurvivalCodes
                        .PrepareFieldSupplyDelegatedWorldInteractionId),
                ResponsibilityKindCode = Simulation세계상호작용이름Catalog
                    .책임종류코드(세계상호작용Id),
                PrimaryOutcomeCode = Simulation세계상호작용이름Catalog
                    .주요결과코드(세계상호작용Id),
                SingleResponsibilityAssessmentCode =
                    Simulation세계상호작용이름Catalog
                        .단일책임판정코드(세계상호작용Id),
                ActionCode = SimulationNatureSurvivalCodes
                    .PrepareFieldSupplyDelegated,
                TargetStableId = PyeongchangSimulationNpcStableIds
                    .Nature현장보급정책,
                Available = projection.BlockReasonCodes.Length == 0,
                BlockReasonCodes = projection.BlockReasonCodes.ToArray(),
            };
        }

        private SimulationNpcRoutineWorkProjection
            CreateNatureFieldSupplyRoutineProjection()
        {
            var policy = FindNpcPolicy(
                Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                SimulationNpcActionCodes.NatureFieldSupplyPreparation);
            var assignment = LatestNatureFieldSupplyAssignment();
            var blocks = NatureNpcFieldSupplyBlockReasons(
                includeActiveWorkBlock: false);
            var isActive = IsNatureNpcFieldSupplyWork;
            var phase = assignment?.PhaseCode
                ?? (blocks.Length > 0
                    ? SimulationNpcActionPhaseCodes.Blocked
                    : SimulationNpcActionPhaseCodes.Candidate);
            var progress = assignment == null ? 0m
                : assignment.PhaseCode == SimulationNpcActionPhaseCodes.Completed
                    ? 1m
                    : isActive && natureActiveWork != null
                        ? Math.Min(.999m,
                            (decimal)natureActiveWork.CompletedWorkSeconds
                            / Math.Max(1, natureActiveWork.RequiredWorkSeconds))
                        : 0m;
            var interventions = new List<string>
            {
                SimulationNpcRoutinePlayerInterventionCodes.ChangePolicy,
            };
            if (isActive)
                interventions.Add(SimulationNpcRoutinePlayerInterventionCodes
                    .CancelBeforeCompletion);

            var execution = assignment == null ? null
                : npcRoutineExecutions.Values.FirstOrDefault(value =>
                    string.Equals(value.TaskStableId,
                        assignment.TaskStableId, StringComparison.Ordinal));
            var 세계상호작용Id = SimulationNatureSurvivalCodes
                .PrepareFieldSupplyDelegatedWorldInteractionId;
            return new SimulationNpcRoutineWorkProjection
            {
                ProjectionStableId =
                    "npc-routine-work:nature:field-supply",
                AreaCode = "Nature",
                WorldInteractionId = SimulationNatureSurvivalCodes
                    .PrepareFieldSupplyDelegatedWorldInteractionId,
                WorldInteractionName = Simulation세계상호작용이름Catalog
                    .한국어기능명(SimulationNatureSurvivalCodes
                        .PrepareFieldSupplyDelegatedWorldInteractionId),
                WorldInteractionDisplayName = Simulation세계상호작용이름Catalog
                    .한국어표시명(SimulationNatureSurvivalCodes
                        .PrepareFieldSupplyDelegatedWorldInteractionId),
                ResponsibilityKindCode = Simulation세계상호작용이름Catalog
                    .책임종류코드(세계상호작용Id),
                PrimaryOutcomeCode = Simulation세계상호작용이름Catalog
                    .주요결과코드(세계상호작용Id),
                SingleResponsibilityAssessmentCode =
                    Simulation세계상호작용이름Catalog
                        .단일책임판정코드(세계상호작용Id),
                PolicyStableId = policy?.PolicyStableId ?? string.Empty,
                TaskStableId = assignment?.TaskStableId ?? string.Empty,
                NpcActorStableId = assignment?.ActorStableId ?? string.Empty,
                TargetInventoryStableId = natureSurvivalCreationState == null
                    ? string.Empty
                    : "player-inventory:"
                      + natureSurvivalCreationState.PlayerStableId,
                PhaseCode = phase,
                ProgressRate = progress,
                OriginCode =
                    SimulationWorldInteractionOriginCodes.SimulationNative,
                ControlPolicyCode =
                    SimulationWorldInteractionControlPolicyCodes.NpcRoutine,
                TriggerSourceCode = execution?.TriggerSourceCode
                    ?? SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
                음양주체분류 = CloneWI음양주체분류(
                    execution?.음양주체분류
                    ?? SimulationWI음양주체사분면Rules.Resolve(
                        세계상호작용Id,
                        assignment == null
                            ? SimulationWI수행주체Codes.NotApplicable
                            : SimulationWI수행주체Codes.NpcActor,
                        execution?.TriggerSourceCode
                        ?? SimulationWorldInteractionTriggerSourceCodes.NpcDriven)),
                BlockReasonCodes = blocks,
                AllowedPlayerInterventionCodes = interventions.ToArray(),
                WorldTick = CurrentTick,
                WorldRevision = Revision,
            };
        }

        private string[] NatureNpcFieldSupplyBlockReasons(
            bool includeActiveWorkBlock)
        {
            var reasons = new List<string>();
            if (natureSurvivalCreationState == null || !IsNatureR4)
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            if (!IsNpcRoutineNatureEnabled)
                reasons.Add(SimulationNatureSurvivalCodes
                    .NpcRoutineNatureRevisionRequired);

            var policy = FindNpcPolicy(
                Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                SimulationNpcActionCodes.NatureFieldSupplyPreparation);
            if (policy == null)
                reasons.Add("SimulationNpcRoutinePolicyMissing");
            else
            {
                if (!policy.AutomationEnabled)
                    reasons.Add("SimulationNpcAutomationDisabled");
                if (!policy.AutoDelegationEnabled)
                    reasons.Add("SimulationNpcAutoDelegationDisabled");
            }

            if (!NatureWorkbenchOperational())
                reasons.Add(SimulationNatureSurvivalCodes.WorkbenchRequired);
            if (includeActiveWorkBlock && natureActiveWork != null)
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
            if (natureSurvivalCreationState != null
                && (NaturePlayerItemQuantity(SimulationNatureSurvivalCodes
                        .NatureFieldSupplyPackItemCode) > 0
                    || natureExpeditionPrepared))
                reasons.Add(SimulationNatureSurvivalCodes
                    .FieldSupplyAlreadyAvailable);
            if (natureSurvivalCreationState != null
                && NatureAvailableTimberQuantity() <
                SimulationNatureSurvivalCodes.FieldSupplyTimberCost)
                reasons.Add(SimulationNatureSurvivalCodes
                    .FieldSupplyTimberInsufficient);
            if (natureSurvivalCreationState != null
                && NaturePlayerItemQuantity(SimulationNatureSurvivalCodes
                    .RebuildPartItemCode) <
                SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost)
                reasons.Add(SimulationNatureSurvivalCodes
                    .FieldSupplyRebuildPartInsufficient);
            if (policy != null && policy.AutomationEnabled
                && policy.AutoDelegationEnabled
                && SelectEligibleActor(policy) == null)
                reasons.Add("SimulationNpcEligibleActorMissing");
            return reasons.Distinct(StringComparer.Ordinal).ToArray();
        }

        private void TryBeginNatureNpcFieldSupplyWork()
        {
            if (natureActiveWork != null
                || NatureNpcFieldSupplyBlockReasons(
                    includeActiveWorkBlock: true).Length > 0)
                return;

            var policy = FindNpcPolicy(
                Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                SimulationNpcActionCodes.NatureFieldSupplyPreparation)!;
            var actor = SelectEligibleActor(policy)!;
            var taskStableId = "task:npc-routine:nature-field-supply:r"
                               + (Revision + 1L);
            var assignmentStableId = "npc-assignment:" + taskStableId;

            ConsumeNatureBuildingTimber(
                SimulationNatureSurvivalCodes.FieldSupplyTimberCost);
            ConsumeNaturePlayerItem(
                SimulationNatureSurvivalCodes.RebuildPartItemCode,
                SimulationNatureSurvivalCodes.FieldSupplyRebuildPartCost);
            natureActiveWork = new SimulationNatureActiveWorkSnapshot
            {
                WorkKindCode =
                    SimulationNatureSurvivalCodes.FieldSupplyNpcCraft,
                TargetStableId = Simulation영역건물발전Codes
                    .NatureWorkbenchBlueprint,
                RequiredWorkSeconds =
                    SimulationNatureSurvivalCodes.FieldSupplyCraftSeconds,
                ReservedTimberQuantity =
                    SimulationNatureSurvivalCodes.FieldSupplyTimberCost,
                ReservedRebuildPartQuantity = SimulationNatureSurvivalCodes
                    .FieldSupplyRebuildPartCost,
            };

            npcTaskAssignments.Add(assignmentStableId,
                new SimulationNpcTaskAssignmentSnapshot
                {
                    AssignmentStableId = assignmentStableId,
                    TaskStableId = taskStableId,
                    PolicyStableId = policy.PolicyStableId,
                    OrganizationStableId = policy.OrganizationStableId,
                    FacilityStableId = policy.FacilityStableId,
                    ActorStableId = actor.ActorStableId,
                    ActionCode = policy.ActionCode,
                    RequiredCapabilityCode = policy.RequiredCapabilityCode,
                    PhaseCode = SimulationNpcActionPhaseCodes.Working,
                    AssignedTick = CurrentTick,
                    PhaseStartedTick = CurrentTick,
                    TravelDurationTicks = 0,
                    WorkDurationTicks =
                        SimulationNatureSurvivalCodes.FieldSupplyCraftSeconds,
                    Revision = 1,
                });

            var executionStableId = "npc-routine-execution:wi-nature-17:"
                                    + taskStableId;
            npcRoutineExecutions.Add(executionStableId,
                new SimulationNpcRoutineExecutionSnapshot
                {
                    ExecutionStableId = executionStableId,
                    AreaCode = "Nature",
                    WorldInteractionId = SimulationNatureSurvivalCodes
                        .PrepareFieldSupplyDelegatedWorldInteractionId,
                    PolicyStableId = policy.PolicyStableId,
                    TaskStableId = taskStableId,
                    InventoryStableId = "player-inventory:"
                        + natureSurvivalCreationState!.PlayerStableId,
                    OriginCode = SimulationWorldInteractionOriginCodes
                        .SimulationNative,
                    ControlPolicyCode = SimulationWorldInteractionControlPolicyCodes
                        .NpcRoutine,
                    TriggerSourceCode = SimulationWorldInteractionTriggerSourceCodes
                        .NpcDriven,
                    ActorStableId = actor.ActorStableId,
                    음양주체분류 = SimulationWI음양주체사분면Rules.Resolve(
                        SimulationNatureSurvivalCodes
                            .PrepareFieldSupplyDelegatedWorldInteractionId,
                        SimulationWI수행주체Codes.NpcActor,
                        SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
                        "playable-loop:nature-field-supply-return.v1"),
                    RecordedWorldTick = CurrentTick,
                    Revision = 1,
                });
        }

        private void CompleteNatureNpcFieldSupplyWork()
        {
            var assignment = LatestNatureFieldSupplyAssignment();
            if (assignment == null) return;
            assignment.PhaseCode = SimulationNpcActionPhaseCodes.Completed;
            assignment.CompletedTick = CurrentTick;
            assignment.Revision++;

            var workRecordStableId = "npc-work-record:"
                                     + assignment.TaskStableId;
            if (!npcWorkRecords.ContainsKey(workRecordStableId))
                npcWorkRecords.Add(workRecordStableId,
                    new SimulationNpcWorkRecordSnapshot
                    {
                        WorkRecordStableId = workRecordStableId,
                        AssignmentStableId = assignment.AssignmentStableId,
                        TaskStableId = assignment.TaskStableId,
                        ActorStableId = assignment.ActorStableId,
                        ActionCode = assignment.ActionCode,
                        FacilityStableId = assignment.FacilityStableId,
                        StartedTick = assignment.PhaseStartedTick,
                        CompletedTick = CurrentTick,
                        ResultCodes = new[] { NatureFieldSupplyResultCode },
                        SourceStableIds = new[]
                        {
                            assignment.PolicyStableId,
                            "wi:" + SimulationNatureSurvivalCodes
                                .PrepareFieldSupplyDelegatedWorldInteractionId,
                            "rule:npc-routine-control.r3",
                        },
                    });
        }

        private void CancelNatureNpcFieldSupplyWork()
        {
            var assignment = LatestNatureFieldSupplyAssignment();
            if (assignment == null
                || assignment.PhaseCode == SimulationNpcActionPhaseCodes.Completed
                || assignment.PhaseCode == SimulationNpcActionPhaseCodes.Cancelled)
                return;
            assignment.PhaseCode = SimulationNpcActionPhaseCodes.Cancelled;
            assignment.CompletedTick = CurrentTick;
            assignment.Revision++;
        }

        private SimulationNpcTaskAssignmentSnapshot?
            LatestNatureFieldSupplyAssignment()
            => npcTaskAssignments.Values
                .Where(value => string.Equals(value.ActionCode,
                    SimulationNpcActionCodes.NatureFieldSupplyPreparation,
                    StringComparison.Ordinal))
                .OrderByDescending(value => value.AssignedTick)
                .ThenByDescending(value => value.AssignmentStableId,
                    StringComparer.Ordinal)
                .FirstOrDefault();

        private bool IsNatureNpcFieldSupplyWork
            => natureActiveWork != null
               && string.Equals(natureActiveWork.WorkKindCode,
                   SimulationNatureSurvivalCodes.FieldSupplyNpcCraft,
                   StringComparison.Ordinal);
    }
}
