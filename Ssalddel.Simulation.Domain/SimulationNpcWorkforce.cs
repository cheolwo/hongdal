using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationNpcOrganizationSnapshot> npcOrganizations =
            new Dictionary<string, SimulationNpcOrganizationSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNpcActorSnapshot> npcActors =
            new Dictionary<string, SimulationNpcActorSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNpcCapabilityGrantSnapshot> npcCapabilityGrants =
            new Dictionary<string, SimulationNpcCapabilityGrantSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNpcWorkPolicySnapshot> npcWorkPolicies =
            new Dictionary<string, SimulationNpcWorkPolicySnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNpcTaskAssignmentSnapshot> npcTaskAssignments =
            new Dictionary<string, SimulationNpcTaskAssignmentSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNpcWorkRecordSnapshot> npcWorkRecords =
            new Dictionary<string, SimulationNpcWorkRecordSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNpcFacilityInventorySnapshot> npcFacilityInventories =
            new Dictionary<string, SimulationNpcFacilityInventorySnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된NpcPolicyCommand> appliedNpcPolicyCommands =
            new Dictionary<string, 적용된NpcPolicyCommand>(StringComparer.Ordinal);
        private SimulationNpcWorkforceInitialStateRequest? npcWorkforceCreationState;

        public 경영SimulationSessionSnapshot UpdateNpcPolicy(SimulationNpcPolicyChangeRequest request)
        {
            ValidateNpcPolicyChangeRequest(request);
            lock (gate)
            {
                if (appliedCommands.ContainsKey(request.CommandId)
                    || (HasAppliedDecisionCommand(request.CommandId)
                        && !HasAppliedNpcPolicyCommand(request.CommandId)))
                    throw new SimulationConflictException("SimulationCommandKindConflict");

                var payloadKey = BuildNpcPolicyPayloadKey(request);
                if (appliedNpcPolicyCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                if (!npcWorkPolicies.TryGetValue(request.PolicyStableId.Trim(), out var policy))
                    throw new SimulationNotFoundException("SimulationNpcWorkPolicyNotFound");

                var preferredActorStableId = request.PreferredActorStableId.Trim();
                if (preferredActorStableId.Length > 0)
                {
                    if (!npcActors.TryGetValue(preferredActorStableId, out var actor)
                        || !string.Equals(actor.OrganizationStableId, policy.OrganizationStableId, StringComparison.Ordinal)
                        || !string.Equals(actor.HomeFacilityStableId, policy.FacilityStableId, StringComparison.Ordinal))
                    {
                        throw new SimulationContractException("SimulationNpcPreferredActorScopeInvalid");
                    }
                }

                policy.AutomationEnabled = request.AutomationEnabled;
                policy.Priority = request.Priority;
                policy.PreferredActorStableId = preferredActorStableId;
                policy.AutoDelegationEnabled = request.AutoDelegationEnabled;
                policy.Revision++;
                Revision++;
                AppendNpcPolicyCommand(request);
                var snapshot = CreateSnapshot();
                appliedNpcPolicyCommands.Add(
                    request.CommandId,
                    new 적용된NpcPolicyCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private void InitializeNpcWorkforce(SimulationNpcWorkforceInitialStateRequest? request)
        {
            ValidateNpcWorkforceInitialState(request);
            npcWorkforceCreationState = CloneNpcWorkforceInitialState(request);
            if (request == null) return;

            foreach (var organization in request.Organizations)
            {
                npcOrganizations.Add(organization.OrganizationStableId.Trim(), new SimulationNpcOrganizationSnapshot
                {
                    OrganizationStableId = organization.OrganizationStableId.Trim(),
                    DisplayName = organization.DisplayName.Trim(),
                    FacilityStableIds = NormalizeIds(organization.FacilityStableIds),
                    AllowedCapabilityCodes = NormalizeIds(organization.AllowedCapabilityCodes),
                    SourceStableIds = NormalizeIds(organization.SourceStableIds),
                });
            }

            foreach (var actor in request.Actors)
            {
                npcActors.Add(actor.ActorStableId.Trim(), new SimulationNpcActorSnapshot
                {
                    ActorStableId = actor.ActorStableId.Trim(),
                    OrganizationStableId = actor.OrganizationStableId.Trim(),
                    DisplayName = actor.DisplayName.Trim(),
                    HomeFacilityStableId = actor.HomeFacilityStableId.Trim(),
                    ReferenceRoleCode = actor.ReferenceRoleCode.Trim(),
                    MaximumConcurrentTasks = actor.MaximumConcurrentTasks,
                    AssignableCapabilityCodes = NormalizeIds(actor.AssignableCapabilityCodes),
                    Skills = actor.Skills
                        .OrderBy(value => value.CapabilityCode, StringComparer.Ordinal)
                        .Select(value => new SimulationNpcSkillSnapshot
                        {
                            CapabilityCode = value.CapabilityCode.Trim(),
                            Score = value.Score,
                        }).ToArray(),
                    SourceStableIds = NormalizeIds(actor.SourceStableIds),
                });
            }

            foreach (var grant in request.CapabilityGrants)
            {
                npcCapabilityGrants.Add(grant.GrantStableId.Trim(), new SimulationNpcCapabilityGrantSnapshot
                {
                    GrantStableId = grant.GrantStableId.Trim(),
                    OrganizationStableId = grant.OrganizationStableId.Trim(),
                    ActorStableId = grant.ActorStableId.Trim(),
                    FacilityStableId = grant.FacilityStableId.Trim(),
                    CapabilityCode = grant.CapabilityCode.Trim(),
                    GrantedByActorStableId = grant.GrantedByActorStableId.Trim(),
                    GrantKindCode = SimulationNpcGrantKindCodes.Initial,
                    CanDelegate = grant.CanDelegate,
                    Active = true,
                    GrantedTick = 0,
                    Revision = 1,
                    SourceStableIds = NormalizeIds(grant.SourceStableIds),
                });
            }

            foreach (var policy in request.Policies)
            {
                npcWorkPolicies.Add(policy.PolicyStableId.Trim(), new SimulationNpcWorkPolicySnapshot
                {
                    PolicyStableId = policy.PolicyStableId.Trim(),
                    OrganizationStableId = policy.OrganizationStableId.Trim(),
                    FacilityStableId = policy.FacilityStableId.Trim(),
                    ActionCode = policy.ActionCode.Trim(),
                    RequiredCapabilityCode = policy.RequiredCapabilityCode.Trim(),
                    AutomationEnabled = policy.AutomationEnabled,
                    Priority = policy.Priority,
                    PreferredActorStableId = policy.PreferredActorStableId.Trim(),
                    AutoDelegationEnabled = policy.AutoDelegationEnabled,
                    AutoDelegationBacklogThreshold = policy.AutoDelegationBacklogThreshold,
                    TravelDurationTicks = policy.TravelDurationTicks,
                    WorkDurationTicks = policy.WorkDurationTicks,
                    InteractionPointKey = policy.InteractionPointKey.Trim(),
                    ActionVisualKey = policy.ActionVisualKey.Trim(),
                    Revision = 1,
                    SourceStableIds = NormalizeIds(policy.SourceStableIds),
                });
            }
        }

        private void ResolveNpcPreviewAssignment(SimulationTaskPlanSnapshot taskPlan)
        {
            if (taskPlan == null || string.IsNullOrWhiteSpace(taskPlan.ActionCode)) return;
            var policy = FindNpcPolicy(taskPlan.FacilityStableId, taskPlan.ActionCode);
            if (policy == null || !policy.AutomationEnabled) return;
            taskPlan.DurationTicks = policy.TravelDurationTicks + policy.WorkDurationTicks;

            var explicitActorStableId = taskPlan.AssignedActorStableId.Trim();
            var candidate = explicitActorStableId.Length > 0
                ? FindEligibleActor(policy, explicitActorStableId, false)
                : SelectEligibleActor(policy);
            taskPlan.AssignedActorStableId = candidate?.ActorStableId ?? string.Empty;
        }

        private void RegisterNpcTaskAssignment(SimulationTaskSnapshot task)
        {
            if (string.IsNullOrWhiteSpace(task.ActionCode)) return;
            var policy = FindNpcPolicy(task.FacilityStableId, task.ActionCode);
            if (policy == null) return;

            var assignment = new SimulationNpcTaskAssignmentSnapshot
            {
                AssignmentStableId = "npc-assignment:" + task.TaskStableId,
                TaskStableId = task.TaskStableId,
                PolicyStableId = policy.PolicyStableId,
                OrganizationStableId = policy.OrganizationStableId,
                FacilityStableId = policy.FacilityStableId,
                ActionCode = policy.ActionCode,
                RequiredCapabilityCode = policy.RequiredCapabilityCode,
                PhaseCode = SimulationNpcActionPhaseCodes.Blocked,
                AssignedTick = CurrentTick,
                PhaseStartedTick = CurrentTick,
                TravelDurationTicks = policy.TravelDurationTicks,
                WorkDurationTicks = policy.WorkDurationTicks,
                Revision = 1,
            };
            npcTaskAssignments.Add(assignment.AssignmentStableId, assignment);

            if (!policy.AutomationEnabled)
            {
                BlockNpcTask(task, assignment, "SimulationNpcAutomationDisabled");
                return;
            }

            if (ShouldAutoDelegate(policy, false))
                TryAutoDelegate(policy, CurrentTick);

            var explicitActorStableId = task.AssignedActorStableId.Trim();
            var actor = explicitActorStableId.Length > 0
                ? FindEligibleActor(policy, explicitActorStableId, true)
                : SelectEligibleActor(policy);
            if (actor == null)
            {
                BlockNpcTask(task, assignment, "SimulationNpcEligibleActorMissing");
                return;
            }

            AssignNpcTask(task, assignment, actor, CurrentTick + 1);
            RegisterPendingInspectionInventory(task);
        }

        private void AdvanceNpcWorkforce(int currentTick)
        {
            foreach (var assignment in npcTaskAssignments.Values
                .Where(value => value.PhaseCode == SimulationNpcActionPhaseCodes.Blocked)
                .OrderBy(value => value.AssignedTick)
                .ThenBy(value => value.AssignmentStableId, StringComparer.Ordinal))
            {
                if (!tasks.TryGetValue(assignment.TaskStableId, out var task)
                    || !npcWorkPolicies.TryGetValue(assignment.PolicyStableId, out var policy)
                    || !policy.AutomationEnabled)
                    continue;

                if (ShouldAutoDelegate(policy, currentTick - assignment.AssignedTick >= 1))
                    TryAutoDelegate(policy, currentTick);
                var actor = SelectEligibleActor(policy);
                if (actor == null) continue;

                var durationTicks = assignment.TravelDurationTicks + assignment.WorkDurationTicks;
                if (currentTick + durationTicks - 1 > DurationTicks)
                {
                    assignment.BlockReasonCodes = new[] { "SimulationNpcTaskDurationExceeded" };
                    assignment.Revision++;
                    continue;
                }

                AssignNpcTask(task, assignment, actor, currentTick);
                RegisterPendingInspectionInventory(task);
            }
        }

        private bool CanAdvanceNpcTask(SimulationTaskSnapshot task, int currentTick)
        {
            var assignment = FindNpcAssignment(task.TaskStableId);
            if (assignment == null) return true;
            if (assignment.PhaseCode == SimulationNpcActionPhaseCodes.Blocked) return false;
            if (assignment.PhaseCode == SimulationNpcActionPhaseCodes.Completed) return true;

            var workStartsAt = task.ScheduledStartTick + assignment.TravelDurationTicks;
            var phase = currentTick < task.ScheduledStartTick
                ? SimulationNpcActionPhaseCodes.Scheduled
                : currentTick < workStartsAt
                    ? SimulationNpcActionPhaseCodes.Navigating
                    : SimulationNpcActionPhaseCodes.Working;
            if (!string.Equals(assignment.PhaseCode, phase, StringComparison.Ordinal))
            {
                assignment.PhaseCode = phase;
                assignment.PhaseStartedTick = currentTick;
                assignment.Revision++;
            }
            return true;
        }

        private void CompleteNpcTaskAssignment(SimulationTaskSnapshot task, int completedTick)
        {
            var assignment = FindNpcAssignment(task.TaskStableId);
            if (assignment == null)
            {
                CompleteInspectionInventory(task, completedTick);
                CompletePutAwayInventory(task, completedTick);
                return;
            }
            if (assignment.PhaseCode == SimulationNpcActionPhaseCodes.Completed) return;

            assignment.PhaseCode = SimulationNpcActionPhaseCodes.Completed;
            assignment.PhaseStartedTick = completedTick;
            assignment.CompletedTick = completedTick;
            assignment.BlockReasonCodes = Array.Empty<string>();
            assignment.Revision++;

            var resultCodes = string.Equals(
                    assignment.ActionCode,
                    SimulationNpcActionCodes.WarehouseInboundInspection,
                    StringComparison.Ordinal)
                ? new[]
                {
                    IsMarketFacility(task.FacilityStableId)
                        ? SimulationNpcInventoryStateCodes.MarketReceived
                        : SimulationNpcInventoryStateCodes.StorageEligible,
                }
                : string.Equals(
                    assignment.ActionCode,
                    SimulationNpcActionCodes.WarehouseStorageMove,
                    StringComparison.Ordinal)
                    ? new[] { SimulationNpcInventoryStateCodes.PutAwayCompleted }
                    : new[] { "Completed" };
            var record = new SimulationNpcWorkRecordSnapshot
            {
                WorkRecordStableId = "npc-work-record:" + task.TaskStableId,
                AssignmentStableId = assignment.AssignmentStableId,
                TaskStableId = task.TaskStableId,
                ActorStableId = assignment.ActorStableId,
                ActionCode = assignment.ActionCode,
                FacilityStableId = assignment.FacilityStableId,
                StartedTick = task.ScheduledStartTick,
                CompletedTick = completedTick,
                ResultCodes = resultCodes,
                SourceStableIds = task.SourceStableIds.ToArray(),
            };
            npcWorkRecords[record.WorkRecordStableId] = record;
            CompleteInspectionInventory(task, completedTick);
            CompletePutAwayInventory(task, completedTick);
        }

        private void AssignNpcTask(
            SimulationTaskSnapshot task,
            SimulationNpcTaskAssignmentSnapshot assignment,
            SimulationNpcActorSnapshot actor,
            int scheduledStartTick)
        {
            assignment.ActorStableId = actor.ActorStableId;
            assignment.PhaseCode = SimulationNpcActionPhaseCodes.Scheduled;
            assignment.PhaseStartedTick = CurrentTick;
            assignment.BlockReasonCodes = Array.Empty<string>();
            assignment.Revision++;
            task.AssignedActorStableId = actor.ActorStableId;
            task.StateCode = SimulationTaskStateCodes.Scheduled;
            task.ScheduledStartTick = scheduledStartTick;
            task.ExpectedEndTick = scheduledStartTick
                + assignment.TravelDurationTicks
                + assignment.WorkDurationTicks - 1;
            task.BlockReasonCodes = Array.Empty<string>();
            task.Revision++;
        }

        private static void BlockNpcTask(
            SimulationTaskSnapshot task,
            SimulationNpcTaskAssignmentSnapshot assignment,
            string blockReasonCode)
        {
            task.StateCode = SimulationTaskStateCodes.Blocked;
            task.AssignedActorStableId = string.Empty;
            task.BlockReasonCodes = new[] { blockReasonCode };
            assignment.PhaseCode = SimulationNpcActionPhaseCodes.Blocked;
            assignment.ActorStableId = string.Empty;
            assignment.BlockReasonCodes = new[] { blockReasonCode };
        }

        private SimulationNpcWorkPolicySnapshot? FindNpcPolicy(string facilityStableId, string actionCode)
            => npcWorkPolicies.Values
                .Where(value => string.Equals(value.FacilityStableId, facilityStableId, StringComparison.Ordinal)
                    && string.Equals(value.ActionCode, actionCode, StringComparison.Ordinal))
                .OrderByDescending(value => value.Priority)
                .ThenBy(value => value.PolicyStableId, StringComparer.Ordinal)
                .FirstOrDefault();

        private SimulationNpcActorSnapshot? SelectEligibleActor(SimulationNpcWorkPolicySnapshot policy)
            => npcActors.Values
                .Where(actor => IsEligibleActor(policy, actor, true))
                .Select(actor => new
                {
                    Actor = actor,
                    IsPreferred = string.Equals(
                        actor.ActorStableId,
                        policy.PreferredActorStableId,
                        StringComparison.Ordinal),
                    Workload = ActiveWorkload(actor.ActorStableId),
                    Skill = SkillScore(actor, policy.RequiredCapabilityCode),
                })
                .Where(value => value.Workload < value.Actor.MaximumConcurrentTasks)
                .OrderByDescending(value => value.IsPreferred)
                .ThenBy(value => value.Workload)
                .ThenByDescending(value => value.Skill)
                .ThenBy(value => value.Actor.ActorStableId, StringComparer.Ordinal)
                .Select(value => value.Actor)
                .FirstOrDefault();

        private SimulationNpcActorSnapshot? FindEligibleActor(
            SimulationNpcWorkPolicySnapshot policy,
            string actorStableId,
            bool requireCapacity)
        {
            if (!npcActors.TryGetValue(actorStableId, out var actor)
                || !IsEligibleActor(policy, actor, requireCapacity))
                return null;
            return !requireCapacity || ActiveWorkload(actorStableId) < actor.MaximumConcurrentTasks
                ? actor
                : null;
        }

        private bool IsEligibleActor(
            SimulationNpcWorkPolicySnapshot policy,
            SimulationNpcActorSnapshot actor,
            bool requireCapacity)
            => string.Equals(actor.OrganizationStableId, policy.OrganizationStableId, StringComparison.Ordinal)
                && string.Equals(actor.HomeFacilityStableId, policy.FacilityStableId, StringComparison.Ordinal)
                && (!requireCapacity || ActiveWorkload(actor.ActorStableId) < actor.MaximumConcurrentTasks)
                && npcCapabilityGrants.Values.Any(grant => grant.Active
                    && string.Equals(grant.OrganizationStableId, policy.OrganizationStableId, StringComparison.Ordinal)
                    && string.Equals(grant.FacilityStableId, policy.FacilityStableId, StringComparison.Ordinal)
                    && string.Equals(grant.ActorStableId, actor.ActorStableId, StringComparison.Ordinal)
                    && string.Equals(grant.CapabilityCode, policy.RequiredCapabilityCode, StringComparison.Ordinal));

        private int ActiveWorkload(string actorStableId)
            => npcTaskAssignments.Values.Count(value =>
                string.Equals(value.ActorStableId, actorStableId, StringComparison.Ordinal)
                && value.PhaseCode != SimulationNpcActionPhaseCodes.Completed
                && value.PhaseCode != SimulationNpcActionPhaseCodes.Blocked);

        private static int SkillScore(SimulationNpcActorSnapshot actor, string capabilityCode)
            => actor.Skills.FirstOrDefault(value =>
                string.Equals(value.CapabilityCode, capabilityCode, StringComparison.Ordinal))?.Score ?? 0;

        private bool ShouldAutoDelegate(SimulationNpcWorkPolicySnapshot policy, bool force)
        {
            if (!policy.AutoDelegationEnabled) return false;
            var backlog = npcTaskAssignments.Values.Count(value =>
                string.Equals(value.PolicyStableId, policy.PolicyStableId, StringComparison.Ordinal)
                && value.PhaseCode != SimulationNpcActionPhaseCodes.Completed);
            return force || backlog >= policy.AutoDelegationBacklogThreshold;
        }

        private bool TryAutoDelegate(SimulationNpcWorkPolicySnapshot policy, int grantedTick)
        {
            var managerGrant = npcCapabilityGrants.Values
                .Where(value => value.Active && value.CanDelegate
                    && value.GrantKindCode == SimulationNpcGrantKindCodes.Initial
                    && value.OrganizationStableId == policy.OrganizationStableId
                    && value.FacilityStableId == policy.FacilityStableId
                    && value.CapabilityCode == SimulationNpcCapabilityCodes.WorkforceDelegate)
                .OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (managerGrant == null) return false;

            var actor = npcActors.Values
                .Where(value => value.OrganizationStableId == policy.OrganizationStableId
                    && value.HomeFacilityStableId == policy.FacilityStableId
                    && value.AssignableCapabilityCodes.Contains(
                        policy.RequiredCapabilityCode,
                        StringComparer.Ordinal)
                    && !npcCapabilityGrants.Values.Any(grant => grant.Active
                        && grant.ActorStableId == value.ActorStableId
                        && grant.FacilityStableId == policy.FacilityStableId
                        && grant.CapabilityCode == policy.RequiredCapabilityCode))
                .OrderByDescending(value => SkillScore(value, policy.RequiredCapabilityCode))
                .ThenBy(value => value.ActorStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (actor == null) return false;

            var grantStableId = "grant:sim:auto:" + policy.PolicyStableId + ":" + actor.ActorStableId;
            npcCapabilityGrants.Add(grantStableId, new SimulationNpcCapabilityGrantSnapshot
            {
                GrantStableId = grantStableId,
                OrganizationStableId = policy.OrganizationStableId,
                ActorStableId = actor.ActorStableId,
                FacilityStableId = policy.FacilityStableId,
                CapabilityCode = policy.RequiredCapabilityCode,
                GrantedByActorStableId = managerGrant.ActorStableId,
                GrantKindCode = SimulationNpcGrantKindCodes.Delegated,
                CanDelegate = false,
                Active = true,
                GrantedTick = grantedTick,
                Revision = 1,
                SourceStableIds = new[] { policy.PolicyStableId, managerGrant.GrantStableId },
            });
            return true;
        }

        private SimulationNpcTaskAssignmentSnapshot? FindNpcAssignment(string taskStableId)
            => npcTaskAssignments.Values.FirstOrDefault(value =>
                string.Equals(value.TaskStableId, taskStableId, StringComparison.Ordinal));

        private void RegisterPendingInspectionInventory(SimulationTaskSnapshot task)
        {
            if (!string.Equals(task.ActionCode, SimulationNpcActionCodes.WarehouseInboundInspection, StringComparison.Ordinal))
                return;
            foreach (var lotStableId in task.InputLotStableIds)
            {
                var inventoryStableId = "npc-inventory:" + task.FacilityStableId + ":" + lotStableId;
                if (npcFacilityInventories.ContainsKey(inventoryStableId)) continue;
                npcFacilityInventories.Add(inventoryStableId, new SimulationNpcFacilityInventorySnapshot
                {
                    InventoryStableId = inventoryStableId,
                    LotStableId = lotStableId,
                    FacilityStableId = task.FacilityStableId,
                    ProductStableId = logisticsMovements.TryGetValue(lotStableId,
                        out var movement) ? movement.ProductStableId : string.Empty,
                    StateCode = SimulationNpcInventoryStateCodes.PendingInspection,
                    Quantity = task.AssignedCapacity,
                    UnitCode = task.AssignedCapacityUnitCode,
                    SourceTaskStableId = task.TaskStableId,
                    UpdatedTick = CurrentTick,
                    Revision = 1,
                    SourceStableIds = task.SourceStableIds.ToArray(),
                });
            }
        }

        private void CompleteInspectionInventory(SimulationTaskSnapshot task, int completedTick)
        {
            if (!string.Equals(task.ActionCode, SimulationNpcActionCodes.WarehouseInboundInspection, StringComparison.Ordinal))
                return;
            foreach (var inventory in npcFacilityInventories.Values.Where(value =>
                string.Equals(value.SourceTaskStableId, task.TaskStableId, StringComparison.Ordinal)))
            {
                inventory.StateCode = IsMarketFacility(task.FacilityStableId)
                    ? SimulationNpcInventoryStateCodes.MarketReceived
                    : SimulationNpcInventoryStateCodes.StorageEligible;
                inventory.UpdatedTick = completedTick;
                inventory.Revision++;
                if (IsMarketFacility(task.FacilityStableId))
                    RegisterTownMarketContaminationIncident(
                        inventory.InventoryStableId,
                        task.FacilityStableId,
                        completedTick);
            }
        }

        private void CompletePutAwayInventory(SimulationTaskSnapshot task, int completedTick)
        {
            if (!string.Equals(task.ActionCode, SimulationNpcActionCodes.WarehouseStorageMove, StringComparison.Ordinal))
                return;
            if (!decisions.TryGetValue(task.CausedByDecisionStableId, out var decision))
                throw new SimulationNotFoundException("SimulationWarehousePutAwayDecisionNotFound");

            var inventoryStableId = decision.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("npc-inventory:", StringComparison.Ordinal));
            if (inventoryStableId == null
                || !npcFacilityInventories.TryGetValue(inventoryStableId, out var inventory))
            {
                throw new SimulationNotFoundException("SimulationWarehouseInventoryNotFound");
            }
            if (!string.Equals(
                    inventory.StateCode,
                    SimulationNpcInventoryStateCodes.StorageEligible,
                    StringComparison.Ordinal))
            {
                throw new SimulationConflictException("SimulationWarehouseInventoryNotPutAwayPending");
            }

            var transition = 업무상태전이Policy.판정(
                업무흐름코드.창고입고,
                창고입고상태코드.적재대기,
                창고입고상태코드.적재완료);
            if (!transition.허용여부)
                throw new SimulationConflictException("SimulationWarehousePutAwayTransitionNotAllowed");

            inventory.StateCode = SimulationNpcInventoryStateCodes.PutAwayCompleted;
            inventory.UpdatedTick = completedTick;
            inventory.Revision++;
            inventory.SourceStableIds = MergeSources(inventory.SourceStableIds, task.SourceStableIds);
        }

        private SimulationNpcOrganizationSnapshot[] CreateNpcOrganizationSnapshots()
            => npcOrganizations.Values.OrderBy(value => value.OrganizationStableId, StringComparer.Ordinal)
                .Select(CloneNpcOrganization).ToArray();

        private SimulationNpcActorSnapshot[] CreateNpcActorSnapshots()
            => npcActors.Values.OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                .Select(CloneNpcActor).ToArray();

        private SimulationNpcCapabilityGrantSnapshot[] CreateNpcCapabilityGrantSnapshots()
            => npcCapabilityGrants.Values.OrderBy(value => value.GrantStableId, StringComparer.Ordinal)
                .Select(CloneNpcCapabilityGrant).ToArray();

        private SimulationNpcWorkPolicySnapshot[] CreateNpcWorkPolicySnapshots()
            => npcWorkPolicies.Values.OrderByDescending(value => value.Priority)
                .ThenBy(value => value.PolicyStableId, StringComparer.Ordinal)
                .Select(CloneNpcWorkPolicy).ToArray();

        private SimulationNpcTaskAssignmentSnapshot[] CreateNpcTaskAssignmentSnapshots()
            => npcTaskAssignments.Values.OrderBy(value => value.AssignedTick)
                .ThenBy(value => value.AssignmentStableId, StringComparer.Ordinal)
                .Select(CloneNpcTaskAssignment).ToArray();

        private SimulationNpcWorkRecordSnapshot[] CreateNpcWorkRecordSnapshots()
            => npcWorkRecords.Values.OrderBy(value => value.CompletedTick)
                .ThenBy(value => value.WorkRecordStableId, StringComparer.Ordinal)
                .Select(CloneNpcWorkRecord).ToArray();

        private SimulationNpcFacilityInventorySnapshot[] CreateNpcFacilityInventorySnapshots()
            => npcFacilityInventories.Values.OrderBy(value => value.InventoryStableId, StringComparer.Ordinal)
                .Select(CloneNpcFacilityInventory).ToArray();

        private SimulationNpcActionProjection[] CreateNpcActionProjections()
            => npcTaskAssignments.Values.OrderBy(value => value.AssignmentStableId, StringComparer.Ordinal)
                .Select(assignment =>
                {
                    var policy = npcWorkPolicies[assignment.PolicyStableId];
                    var progress = CalculateNpcActionProgress(assignment);
                    return new SimulationNpcActionProjection
                    {
                        ProjectionStableId = "npc-action-projection:" + assignment.AssignmentStableId,
                        ActorStableId = assignment.ActorStableId,
                        TaskStableId = assignment.TaskStableId,
                        FacilityStableId = assignment.FacilityStableId,
                        InteractionPointKey = policy.InteractionPointKey,
                        ActionVisualKey = policy.ActionVisualKey,
                        PhaseCode = assignment.PhaseCode,
                        ProgressRate = progress,
                        BlockReasonCodes = assignment.BlockReasonCodes.ToArray(),
                        Revision = assignment.Revision,
                        WorldTick = CurrentTick,
                        PresentationOnly = true,
                    };
                }).ToArray();

        private decimal CalculateNpcActionProgress(SimulationNpcTaskAssignmentSnapshot assignment)
        {
            if (assignment.PhaseCode == SimulationNpcActionPhaseCodes.Completed) return 1m;
            if (assignment.PhaseCode != SimulationNpcActionPhaseCodes.Working
                || !tasks.TryGetValue(assignment.TaskStableId, out var task)) return 0m;
            var workStartsAt = task.ScheduledStartTick + assignment.TravelDurationTicks;
            var progressed = Math.Max(0, CurrentTick - workStartsAt + 1);
            return Math.Min(0.999m, (decimal)progressed / assignment.WorkDurationTicks);
        }

        private static SimulationNpcOrganizationSnapshot CloneNpcOrganization(SimulationNpcOrganizationSnapshot source)
            => new SimulationNpcOrganizationSnapshot
            {
                OrganizationStableId = source.OrganizationStableId,
                DisplayName = source.DisplayName,
                FacilityStableIds = source.FacilityStableIds.ToArray(),
                AllowedCapabilityCodes = source.AllowedCapabilityCodes.ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static SimulationNpcActorSnapshot CloneNpcActor(SimulationNpcActorSnapshot source)
            => new SimulationNpcActorSnapshot
            {
                ActorStableId = source.ActorStableId,
                OrganizationStableId = source.OrganizationStableId,
                DisplayName = source.DisplayName,
                HomeFacilityStableId = source.HomeFacilityStableId,
                ReferenceRoleCode = source.ReferenceRoleCode,
                MaximumConcurrentTasks = source.MaximumConcurrentTasks,
                AssignableCapabilityCodes = source.AssignableCapabilityCodes.ToArray(),
                Skills = source.Skills.Select(value => new SimulationNpcSkillSnapshot
                {
                    CapabilityCode = value.CapabilityCode,
                    Score = value.Score,
                }).ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static SimulationNpcCapabilityGrantSnapshot CloneNpcCapabilityGrant(SimulationNpcCapabilityGrantSnapshot source)
            => new SimulationNpcCapabilityGrantSnapshot
            {
                GrantStableId = source.GrantStableId,
                OrganizationStableId = source.OrganizationStableId,
                ActorStableId = source.ActorStableId,
                FacilityStableId = source.FacilityStableId,
                CapabilityCode = source.CapabilityCode,
                GrantedByActorStableId = source.GrantedByActorStableId,
                GrantKindCode = source.GrantKindCode,
                CanDelegate = source.CanDelegate,
                Active = source.Active,
                GrantedTick = source.GrantedTick,
                Revision = source.Revision,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static SimulationNpcWorkPolicySnapshot CloneNpcWorkPolicy(SimulationNpcWorkPolicySnapshot source)
            => new SimulationNpcWorkPolicySnapshot
            {
                PolicyStableId = source.PolicyStableId,
                OrganizationStableId = source.OrganizationStableId,
                FacilityStableId = source.FacilityStableId,
                ActionCode = source.ActionCode,
                RequiredCapabilityCode = source.RequiredCapabilityCode,
                AutomationEnabled = source.AutomationEnabled,
                Priority = source.Priority,
                PreferredActorStableId = source.PreferredActorStableId,
                AutoDelegationEnabled = source.AutoDelegationEnabled,
                AutoDelegationBacklogThreshold = source.AutoDelegationBacklogThreshold,
                TravelDurationTicks = source.TravelDurationTicks,
                WorkDurationTicks = source.WorkDurationTicks,
                InteractionPointKey = source.InteractionPointKey,
                ActionVisualKey = source.ActionVisualKey,
                Revision = source.Revision,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static SimulationNpcTaskAssignmentSnapshot CloneNpcTaskAssignment(SimulationNpcTaskAssignmentSnapshot source)
            => new SimulationNpcTaskAssignmentSnapshot
            {
                AssignmentStableId = source.AssignmentStableId,
                TaskStableId = source.TaskStableId,
                PolicyStableId = source.PolicyStableId,
                OrganizationStableId = source.OrganizationStableId,
                FacilityStableId = source.FacilityStableId,
                ActorStableId = source.ActorStableId,
                ActionCode = source.ActionCode,
                RequiredCapabilityCode = source.RequiredCapabilityCode,
                PhaseCode = source.PhaseCode,
                AssignedTick = source.AssignedTick,
                PhaseStartedTick = source.PhaseStartedTick,
                TravelDurationTicks = source.TravelDurationTicks,
                WorkDurationTicks = source.WorkDurationTicks,
                CompletedTick = source.CompletedTick,
                BlockReasonCodes = source.BlockReasonCodes.ToArray(),
                Revision = source.Revision,
            };

        private static SimulationNpcWorkRecordSnapshot CloneNpcWorkRecord(SimulationNpcWorkRecordSnapshot source)
            => new SimulationNpcWorkRecordSnapshot
            {
                WorkRecordStableId = source.WorkRecordStableId,
                AssignmentStableId = source.AssignmentStableId,
                TaskStableId = source.TaskStableId,
                ActorStableId = source.ActorStableId,
                ActionCode = source.ActionCode,
                FacilityStableId = source.FacilityStableId,
                StartedTick = source.StartedTick,
                CompletedTick = source.CompletedTick,
                ResultCodes = source.ResultCodes.ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static SimulationNpcFacilityInventorySnapshot CloneNpcFacilityInventory(SimulationNpcFacilityInventorySnapshot source)
            => new SimulationNpcFacilityInventorySnapshot
            {
                InventoryStableId = source.InventoryStableId,
                LotStableId = source.LotStableId,
                FacilityStableId = source.FacilityStableId,
                ProductStableId = source.ProductStableId,
                StateCode = source.StateCode,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                SourceTaskStableId = source.SourceTaskStableId,
                UpdatedTick = source.UpdatedTick,
                Revision = source.Revision,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private static SimulationNpcActionProjection CloneNpcActionProjection(SimulationNpcActionProjection source)
            => new SimulationNpcActionProjection
            {
                ProjectionStableId = source.ProjectionStableId,
                ActorStableId = source.ActorStableId,
                TaskStableId = source.TaskStableId,
                FacilityStableId = source.FacilityStableId,
                InteractionPointKey = source.InteractionPointKey,
                ActionVisualKey = source.ActionVisualKey,
                PhaseCode = source.PhaseCode,
                ProgressRate = source.ProgressRate,
                BlockReasonCodes = source.BlockReasonCodes.ToArray(),
                Revision = source.Revision,
                WorldTick = source.WorldTick,
                PresentationOnly = source.PresentationOnly,
            };

        internal static SimulationNpcWorkforceInitialStateRequest? CloneNpcWorkforceInitialState(
            SimulationNpcWorkforceInitialStateRequest? source)
            => source == null ? null : new SimulationNpcWorkforceInitialStateRequest
            {
                Organizations = source.Organizations.Select(value => new SimulationNpcOrganizationInitialRequest
                {
                    OrganizationStableId = value.OrganizationStableId,
                    DisplayName = value.DisplayName,
                    FacilityStableIds = value.FacilityStableIds.ToArray(),
                    AllowedCapabilityCodes = value.AllowedCapabilityCodes.ToArray(),
                    SourceStableIds = value.SourceStableIds.ToArray(),
                }).ToArray(),
                Actors = source.Actors.Select(value => new SimulationNpcActorInitialRequest
                {
                    ActorStableId = value.ActorStableId,
                    OrganizationStableId = value.OrganizationStableId,
                    DisplayName = value.DisplayName,
                    HomeFacilityStableId = value.HomeFacilityStableId,
                    ReferenceRoleCode = value.ReferenceRoleCode,
                    MaximumConcurrentTasks = value.MaximumConcurrentTasks,
                    AssignableCapabilityCodes = value.AssignableCapabilityCodes.ToArray(),
                    Skills = value.Skills.Select(skill => new SimulationNpcSkillInitialRequest
                    {
                        CapabilityCode = skill.CapabilityCode,
                        Score = skill.Score,
                    }).ToArray(),
                    SourceStableIds = value.SourceStableIds.ToArray(),
                }).ToArray(),
                CapabilityGrants = source.CapabilityGrants.Select(value => new SimulationNpcCapabilityGrantInitialRequest
                {
                    GrantStableId = value.GrantStableId,
                    OrganizationStableId = value.OrganizationStableId,
                    ActorStableId = value.ActorStableId,
                    FacilityStableId = value.FacilityStableId,
                    CapabilityCode = value.CapabilityCode,
                    GrantedByActorStableId = value.GrantedByActorStableId,
                    CanDelegate = value.CanDelegate,
                    SourceStableIds = value.SourceStableIds.ToArray(),
                }).ToArray(),
                Policies = source.Policies.Select(value => new SimulationNpcWorkPolicyInitialRequest
                {
                    PolicyStableId = value.PolicyStableId,
                    OrganizationStableId = value.OrganizationStableId,
                    FacilityStableId = value.FacilityStableId,
                    ActionCode = value.ActionCode,
                    RequiredCapabilityCode = value.RequiredCapabilityCode,
                    AutomationEnabled = value.AutomationEnabled,
                    Priority = value.Priority,
                    PreferredActorStableId = value.PreferredActorStableId,
                    AutoDelegationEnabled = value.AutoDelegationEnabled,
                    AutoDelegationBacklogThreshold = value.AutoDelegationBacklogThreshold,
                    TravelDurationTicks = value.TravelDurationTicks,
                    WorkDurationTicks = value.WorkDurationTicks,
                    InteractionPointKey = value.InteractionPointKey,
                    ActionVisualKey = value.ActionVisualKey,
                    SourceStableIds = value.SourceStableIds.ToArray(),
                }).ToArray(),
            };

        internal static string BuildNpcWorkforcePayloadKey(SimulationNpcWorkforceInitialStateRequest? request)
        {
            if (request == null) return "none";
            var normalized = CloneNpcWorkforceInitialState(request)!;
            return string.Join("\u001e", new[]
            {
                string.Join("\u001f", normalized.Organizations.OrderBy(value => value.OrganizationStableId, StringComparer.Ordinal)
                    .Select(value => value.OrganizationStableId + "|" + value.DisplayName + "|"
                        + string.Join(",", value.FacilityStableIds.OrderBy(item => item, StringComparer.Ordinal)) + "|"
                        + string.Join(",", value.AllowedCapabilityCodes.OrderBy(item => item, StringComparer.Ordinal)))),
                string.Join("\u001f", normalized.Actors.OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                    .Select(value => value.ActorStableId + "|" + value.OrganizationStableId + "|"
                        + value.HomeFacilityStableId + "|" + value.ReferenceRoleCode + "|"
                        + value.MaximumConcurrentTasks.ToString(CultureInfo.InvariantCulture) + "|"
                        + string.Join(",", value.AssignableCapabilityCodes.OrderBy(item => item, StringComparer.Ordinal)) + "|"
                        + string.Join(",", value.Skills.OrderBy(item => item.CapabilityCode, StringComparer.Ordinal)
                            .Select(item => item.CapabilityCode + ":" + item.Score.ToString(CultureInfo.InvariantCulture))))),
                string.Join("\u001f", normalized.CapabilityGrants.OrderBy(value => value.GrantStableId, StringComparer.Ordinal)
                    .Select(value => value.GrantStableId + "|" + value.OrganizationStableId + "|"
                        + value.ActorStableId + "|" + value.FacilityStableId + "|"
                        + value.CapabilityCode + "|" + value.GrantedByActorStableId + "|" + value.CanDelegate)),
                string.Join("\u001f", normalized.Policies.OrderBy(value => value.PolicyStableId, StringComparer.Ordinal)
                    .Select(value => value.PolicyStableId + "|" + value.OrganizationStableId + "|"
                        + value.FacilityStableId + "|" + value.ActionCode + "|" + value.RequiredCapabilityCode + "|"
                        + value.AutomationEnabled + "|" + value.Priority.ToString(CultureInfo.InvariantCulture) + "|"
                        + value.PreferredActorStableId + "|" + value.AutoDelegationEnabled + "|"
                        + value.AutoDelegationBacklogThreshold.ToString(CultureInfo.InvariantCulture) + "|"
                        + value.TravelDurationTicks.ToString(CultureInfo.InvariantCulture) + "|"
                        + value.WorkDurationTicks.ToString(CultureInfo.InvariantCulture) + "|"
                        + value.InteractionPointKey + "|" + value.ActionVisualKey)),
            });
        }

        internal static string BuildNpcPolicyPayloadKey(SimulationNpcPolicyChangeRequest request)
            => string.Join("\u001f", new[]
            {
                request.PolicyStableId.Trim(),
                request.AutomationEnabled.ToString(),
                request.Priority.ToString(CultureInfo.InvariantCulture),
                request.PreferredActorStableId.Trim(),
                request.AutoDelegationEnabled.ToString(),
            });

        internal static void ValidateNpcPolicyChangeRequest(SimulationNpcPolicyChangeRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            RequireStableId(request.PolicyStableId, "SimulationNpcPolicyStableIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Priority < 0 || request.Priority > 1000)
                throw new SimulationContractException("SimulationNpcPolicyPriorityInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredActorStableId))
                RequireStableId(request.PreferredActorStableId, "SimulationNpcPreferredActorStableIdInvalid");
        }

        internal static void ValidateNpcWorkforceInitialState(SimulationNpcWorkforceInitialStateRequest? request)
        {
            if (request == null) return;
            if (request.Organizations == null || request.Actors == null
                || request.CapabilityGrants == null || request.Policies == null)
                throw new SimulationContractException("SimulationNpcWorkforceInvalid");

            var organizationIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var organization in request.Organizations)
            {
                if (organization == null) throw new SimulationContractException("SimulationNpcOrganizationInvalid");
                RequireStableId(organization.OrganizationStableId, "SimulationNpcOrganizationStableIdInvalid");
                RequireText(organization.DisplayName, "SimulationNpcOrganizationDisplayNameMissing");
                ValidateIds(organization.FacilityStableIds, true, "SimulationNpcOrganizationFacilityIdsInvalid");
                ValidateIds(organization.AllowedCapabilityCodes, true, "SimulationNpcOrganizationCapabilitiesInvalid");
                ValidateIds(organization.SourceStableIds, true, "SimulationNpcOrganizationSourcesInvalid");
                if (!organizationIds.Add(organization.OrganizationStableId.Trim()))
                    throw new SimulationContractException("SimulationNpcOrganizationDuplicate");
            }

            var actorIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var actor in request.Actors)
            {
                if (actor == null) throw new SimulationContractException("SimulationNpcActorInvalid");
                RequireStableId(actor.ActorStableId, "SimulationNpcActorStableIdInvalid");
                RequireStableId(actor.OrganizationStableId, "SimulationNpcActorOrganizationInvalid");
                RequireStableId(actor.HomeFacilityStableId, "SimulationNpcActorFacilityInvalid");
                RequireText(actor.DisplayName, "SimulationNpcActorDisplayNameMissing");
                RequireText(actor.ReferenceRoleCode, "SimulationNpcActorReferenceRoleMissing");
                if (actor.MaximumConcurrentTasks <= 0 || actor.MaximumConcurrentTasks > 8)
                    throw new SimulationContractException("SimulationNpcActorCapacityInvalid");
                ValidateIds(actor.AssignableCapabilityCodes, false, "SimulationNpcAssignableCapabilitiesInvalid");
                ValidateIds(actor.SourceStableIds, true, "SimulationNpcActorSourcesInvalid");
                if (actor.Skills == null || actor.Skills.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.CapabilityCode)
                    || value.Score < 0 || value.Score > 100))
                    throw new SimulationContractException("SimulationNpcSkillsInvalid");
                if (!actorIds.Add(actor.ActorStableId.Trim()))
                    throw new SimulationContractException("SimulationNpcActorDuplicate");
            }

            var grantIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var grant in request.CapabilityGrants)
            {
                if (grant == null) throw new SimulationContractException("SimulationNpcGrantInvalid");
                RequireStableId(grant.GrantStableId, "SimulationNpcGrantStableIdInvalid");
                RequireStableId(grant.OrganizationStableId, "SimulationNpcGrantOrganizationInvalid");
                RequireStableId(grant.ActorStableId, "SimulationNpcGrantActorInvalid");
                RequireStableId(grant.FacilityStableId, "SimulationNpcGrantFacilityInvalid");
                RequireStableId(grant.CapabilityCode, "SimulationNpcGrantCapabilityInvalid");
                RequireStableId(grant.GrantedByActorStableId, "SimulationNpcGrantManagerInvalid");
                ValidateIds(grant.SourceStableIds, true, "SimulationNpcGrantSourcesInvalid");
                if (!grantIds.Add(grant.GrantStableId.Trim()))
                    throw new SimulationContractException("SimulationNpcGrantDuplicate");
            }

            var policyIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var policy in request.Policies)
            {
                if (policy == null) throw new SimulationContractException("SimulationNpcPolicyInvalid");
                RequireStableId(policy.PolicyStableId, "SimulationNpcPolicyStableIdInvalid");
                RequireStableId(policy.OrganizationStableId, "SimulationNpcPolicyOrganizationInvalid");
                RequireStableId(policy.FacilityStableId, "SimulationNpcPolicyFacilityInvalid");
                RequireStableId(policy.ActionCode, "SimulationNpcPolicyActionInvalid");
                RequireStableId(policy.RequiredCapabilityCode, "SimulationNpcPolicyCapabilityInvalid");
                RequireStableId(policy.InteractionPointKey, "SimulationNpcInteractionPointInvalid");
                RequireStableId(policy.ActionVisualKey, "SimulationNpcActionVisualKeyInvalid");
                ValidateIds(policy.SourceStableIds, true, "SimulationNpcPolicySourcesInvalid");
                if (policy.Priority < 0 || policy.Priority > 1000
                    || policy.AutoDelegationBacklogThreshold <= 0
                    || policy.TravelDurationTicks < 0
                    || policy.WorkDurationTicks <= 0)
                    throw new SimulationContractException("SimulationNpcPolicyTimingInvalid");
                if (!string.IsNullOrWhiteSpace(policy.PreferredActorStableId))
                    RequireStableId(policy.PreferredActorStableId, "SimulationNpcPreferredActorStableIdInvalid");
                if (!policyIds.Add(policy.PolicyStableId.Trim()))
                    throw new SimulationContractException("SimulationNpcPolicyDuplicate");
            }

            foreach (var actor in request.Actors)
            {
                var organization = request.Organizations.SingleOrDefault(value =>
                    value.OrganizationStableId.Trim() == actor.OrganizationStableId.Trim())
                    ?? throw new SimulationContractException("SimulationNpcActorOrganizationNotFound");
                if (!organization.FacilityStableIds.Contains(actor.HomeFacilityStableId.Trim(), StringComparer.Ordinal))
                    throw new SimulationContractException("SimulationNpcActorFacilityScopeInvalid");
            }

            foreach (var grant in request.CapabilityGrants)
            {
                var organization = request.Organizations.SingleOrDefault(value =>
                    value.OrganizationStableId.Trim() == grant.OrganizationStableId.Trim())
                    ?? throw new SimulationContractException("SimulationNpcGrantOrganizationNotFound");
                var actor = request.Actors.SingleOrDefault(value =>
                    value.ActorStableId.Trim() == grant.ActorStableId.Trim())
                    ?? throw new SimulationContractException("SimulationNpcGrantActorNotFound");
                if (actor.OrganizationStableId.Trim() != grant.OrganizationStableId.Trim()
                    || actor.HomeFacilityStableId.Trim() != grant.FacilityStableId.Trim()
                    || !organization.FacilityStableIds.Contains(grant.FacilityStableId.Trim(), StringComparer.Ordinal)
                    || !organization.AllowedCapabilityCodes.Contains(grant.CapabilityCode.Trim(), StringComparer.Ordinal))
                    throw new SimulationContractException("SimulationNpcGrantScopeInvalid");
                if (!actorIds.Contains(grant.GrantedByActorStableId.Trim()))
                    throw new SimulationContractException("SimulationNpcGrantManagerNotFound");
                if (grant.CanDelegate
                    && grant.CapabilityCode.Trim() != SimulationNpcCapabilityCodes.WorkforceDelegate)
                    throw new SimulationContractException("SimulationNpcGrantDelegationInvalid");
            }

            foreach (var policy in request.Policies)
            {
                var organization = request.Organizations.SingleOrDefault(value =>
                    value.OrganizationStableId.Trim() == policy.OrganizationStableId.Trim())
                    ?? throw new SimulationContractException("SimulationNpcPolicyOrganizationNotFound");
                if (!organization.FacilityStableIds.Contains(policy.FacilityStableId.Trim(), StringComparer.Ordinal)
                    || !organization.AllowedCapabilityCodes.Contains(policy.RequiredCapabilityCode.Trim(), StringComparer.Ordinal))
                    throw new SimulationContractException("SimulationNpcPolicyScopeInvalid");
            }
        }

        private bool HasAppliedNpcPolicyCommand(string commandId)
            => appliedNpcPolicyCommands.ContainsKey(commandId);

        private sealed class 적용된NpcPolicyCommand
        {
            public 적용된NpcPolicyCommand(string payloadKey, 경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
