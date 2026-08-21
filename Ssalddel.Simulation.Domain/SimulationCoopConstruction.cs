using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationCoopConstructionProjectSnapshot>
            coopConstructionProjects = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationCoopContributionSnapshot>
            coopContributions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationCoopSourceLotStateSnapshot>
            coopSourceLots = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationWorldProtectionCheckpointSnapshot>
            coopProtectionCheckpoints = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationWorldRestoreEffectSnapshot>
            coopRestoreEffects = new(StringComparer.Ordinal);
        private readonly Dictionary<string, decimal> coopReservedQuantityByLot =
            new(StringComparer.Ordinal);

        private void InitializeCoopConstruction()
        {
            foreach (var lot in integratedLots.Values)
                coopSourceLots.Add(lot.LotStableId, new SimulationCoopSourceLotStateSnapshot
                {
                    LotStableId = lot.LotStableId,
                    Revision = 1,
                    RemainingQuantity = lot.Quantity,
                    UnitCode = lot.UnitCode,
                });
        }

        public SimulationCoopConstructionStateSnapshot GetCoopConstructionState()
        {
            lock (gate) return CreateCoopConstructionStateSnapshot();
        }

        public SimulationCoopConstructionPreviewSnapshot PreviewCoopContribution(
            SimulationCoopContributionPreviewRequest request)
        {
            ValidateCoopContributionRequest(request);
            lock (gate) return CreateCoopContributionPreview(request, true);
        }

        public 경영SimulationSessionSnapshot ConfirmCoopContribution(
            SimulationCoopContributionConfirmRequest request)
        {
            ValidateCoopContributionRequest(request);
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            lock (gate)
            {
                var replayed = ReplayAppliedCoopContribution(request);
                if (replayed != null) return replayed;
                var preview = CreateCoopContributionPreview(request, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CoopDecision(preview, request),
                });
            }
        }

        public SimulationCoopConstructionPreviewSnapshot PreviewCoopDemolition(
            SimulationCoopProtectedActionPreviewRequest request)
        {
            ValidateCoopProtectedRequest(request);
            lock (gate) return CreateCoopProtectedPreview(request,
                SimulationCoopConstructionCodes.Demolition, true);
        }

        public 경영SimulationSessionSnapshot ConfirmCoopDemolition(
            SimulationCoopProtectedActionConfirmRequest request)
        {
            ValidateCoopProtectedRequest(request);
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            lock (gate)
            {
                var replayed = ReplayAppliedCoopProtectedAction(request,
                    SimulationCoopConstructionCodes.Demolition);
                if (replayed != null) return replayed;
                var preview = CreateCoopProtectedPreview(request,
                    SimulationCoopConstructionCodes.Demolition, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CoopProtectedDecision(preview, request),
                });
            }
        }

        public SimulationCoopConstructionPreviewSnapshot PreviewCoopRestore(
            SimulationCoopProtectedActionPreviewRequest request)
        {
            ValidateCoopProtectedRequest(request);
            lock (gate) return CreateCoopProtectedPreview(request,
                SimulationCoopConstructionCodes.Restore, true);
        }

        public 경영SimulationSessionSnapshot ConfirmCoopRestore(
            SimulationCoopProtectedActionConfirmRequest request)
        {
            ValidateCoopProtectedRequest(request);
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            lock (gate)
            {
                var replayed = ReplayAppliedCoopProtectedAction(request,
                    SimulationCoopConstructionCodes.Restore);
                if (replayed != null) return replayed;
                var preview = CreateCoopProtectedPreview(request,
                    SimulationCoopConstructionCodes.Restore, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CoopProtectedDecision(preview, request),
                });
            }
        }

        private SimulationCoopConstructionPreviewSnapshot CreateCoopContributionPreview(
            SimulationCoopContributionPreviewRequest request, bool requireRevision)
        {
            if (requireRevision && request.ExpectedRevision != Revision)
                throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
            var blocks = new List<string>();
            if (hostedSessionModeCode != SimulationHostedWorldCodes.HostedMultiplayer)
                blocks.Add("SimulationHostedWorldRequired");
            var participant = hostedParticipants.Values.FirstOrDefault(value =>
                value.PlayerStableId == request.PlayerStableId);
            if (participant?.ParticipantStateCode != SimulationHostedWorldCodes.Active)
                blocks.Add("SimulationHostedParticipantNotActive");
            if (request.PlayerStableId != SimulationAreaAccessCodes.PlayerOwner)
            {
                var grant = FindHostedGrant(request.PlayerStableId,
                    SimulationAreaAccessCodes.FarmAreaSet,
                    SimulationHostedWorldCodes.PerformWork);
                if (grant?.GrantStateCode != SimulationHostedWorldCodes.Allow)
                    blocks.Add("SimulationHostedPermissionDenied");
            }
            if (!integratedBlueprints.TryGetValue(request.BlueprintStableId,
                    out var blueprint))
                blocks.Add("SimulationFacilityBlueprintNotFound");
            else if (blueprint.Materials.Length != 1)
                blocks.Add("SimulationCoopBlueprintMaterialShapeUnsupported");
            if (!integratedLots.TryGetValue(request.SourceLotStableId, out var lot))
                blocks.Add("SimulationCoopSourceLotNotFound");
            if (!coopSourceLots.TryGetValue(request.SourceLotStableId, out var lotState))
                blocks.Add("SimulationCoopSourceLotStateNotFound");
            else if (lotState.Revision != request.ExpectedSourceLotRevision)
                blocks.Add("SimulationCoopSourceLotRevisionMismatch");
            if (lot != null && lot.FacilityStableId != request.PlayerStableId)
                blocks.Add("SimulationCoopSourceLotCustodyMismatch");
            if (blueprint != null && lot != null
                && (lot.ItemCode != blueprint.Materials[0].ItemCode
                    || lot.UnitCode != blueprint.Materials[0].UnitCode))
                blocks.Add("SimulationCoopSourceLotMaterialMismatch");
            if (request.RequestedQuantity <= 0m)
                blocks.Add("SimulationCoopContributionQuantityInvalid");

            var required = blueprint?.Materials.Single().Quantity ?? 0m;
            var current = coopConstructionProjects.TryGetValue(request.ProjectStableId,
                out var project) ? project.ContributedMaterialQuantity : 0m;
            if (project != null && (project.BlueprintStableId != request.BlueprintStableId
                    || project.BuildSiteH1StableId != request.BuildSiteH1StableId))
                blocks.Add("SimulationCoopProjectIdentityMismatch");
            if (project?.StageCode == SimulationCoopConstructionCodes.Operational)
                blocks.Add("SimulationCoopProjectAlreadyOperational");
            var remaining = Math.Max(0m, required - current);
            var reserved = coopReservedQuantityByLot.TryGetValue(request.SourceLotStableId,
                out var reservedQuantity) ? reservedQuantity : 0m;
            var available = lot == null ? 0m : Math.Max(0m, lot.Quantity - reserved);
            var accepted = Math.Min(request.RequestedQuantity, Math.Min(remaining, available));
            if (accepted <= 0m) blocks.Add("SimulationCoopContributionUnavailable");
            var projected = required <= 0m ? 0m : (current + accepted) / required;
            var result = new SimulationCoopConstructionPreviewSnapshot
            {
                BaseRevision = Revision,
                ActionCode = SimulationCoopConstructionCodes.Contribution,
                PlayerStableId = request.PlayerStableId,
                ProjectStableId = request.ProjectStableId,
                SourceLotStableId = request.SourceLotStableId,
                SourceLotRevision = lotState?.Revision ?? 0,
                OfferedQuantity = request.RequestedQuantity,
                AcceptedQuantity = accepted,
                RemainingRequiredQuantity = remaining,
                UnitCode = blueprint?.Materials.Single().UnitCode ?? string.Empty,
                CurrentStageCode = project?.StageCode
                    ?? SimulationCoopConstructionCodes.Planned,
                ProjectedStageCode = StageFor(projected),
                DurationTicks = 1,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.Distinct(StringComparer.Ordinal).ToArray(),
            };
            result.PreviewHashSha256 = HashCoop(string.Join("|", result.ActionCode,
                result.BaseRevision, result.PlayerStableId, result.ProjectStableId,
                result.SourceLotStableId, result.SourceLotRevision,
                result.AcceptedQuantity, result.ProjectedStageCode,
                string.Join(",", result.BlockingReasonCodes)));
            return result;
        }

        private SimulationCoopConstructionPreviewSnapshot CreateCoopProtectedPreview(
            SimulationCoopProtectedActionPreviewRequest request, string action,
            bool requireRevision)
        {
            if (requireRevision && request.ExpectedRevision != Revision)
                throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
            var blocks = new List<string>();
            if (request.OwnerPlayerStableId != SimulationAreaAccessCodes.PlayerOwner)
                blocks.Add("SimulationWorldProtectionOwnerRequired");
            if (!coopConstructionProjects.TryGetValue(request.ProjectStableId,
                    out var project))
                blocks.Add("SimulationCoopProjectNotFound");
            else if (action == SimulationCoopConstructionCodes.Demolition
                     && project.StageCode != SimulationCoopConstructionCodes.Operational)
                blocks.Add("SimulationCoopFacilityNotOperational");
            else if (action == SimulationCoopConstructionCodes.Restore)
            {
                if (project.StageCode != SimulationCoopConstructionCodes.Removed)
                    blocks.Add("SimulationCoopFacilityNotRemoved");
                if (!coopProtectionCheckpoints.Values.Any(value =>
                        value.CheckpointKindCode
                            == SimulationCoopConstructionCodes.DestructiveActionCheckpoint
                        && value.TargetStableIds.Contains(project.TargetFacilityStableId,
                            StringComparer.Ordinal)))
                    blocks.Add("SimulationWorldProtectionCheckpointMissing");
                if (integratedFacilities.Values.Any(value =>
                        value.FacilityStableId != project.TargetFacilityStableId
                        && value.PlacementH1StableId == project.BuildSiteH1StableId
                        && value.LifecycleCode != SimulationFacilityLifecycleCodes.Removed))
                    blocks.Add("SimulationWorldRestoreDependencyConflict");
            }
            var result = new SimulationCoopConstructionPreviewSnapshot
            {
                BaseRevision = Revision,
                ActionCode = action,
                PlayerStableId = request.OwnerPlayerStableId,
                ProjectStableId = request.ProjectStableId,
                CurrentStageCode = project?.StageCode ?? string.Empty,
                ProjectedStageCode = action == SimulationCoopConstructionCodes.Restore
                    ? SimulationCoopConstructionCodes.Operational
                    : SimulationCoopConstructionCodes.Removed,
                DurationTicks = 1,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.ToArray(),
            };
            result.PreviewHashSha256 = HashCoop(string.Join("|", result.ActionCode,
                result.BaseRevision, result.PlayerStableId, result.ProjectStableId,
                result.CurrentStageCode, result.ProjectedStageCode,
                string.Join(",", result.BlockingReasonCodes)));
            return result;
        }

        private static SimulationDecisionPreviewRequest CoopDecision(
            SimulationCoopConstructionPreviewSnapshot preview,
            SimulationCoopContributionPreviewRequest request)
            => DecisionFor(preview, new[]
            {
                request.ProjectStableId, request.BlueprintStableId,
                request.BuildSiteH1StableId, request.SourceLotStableId,
                "coop-lot-revision:" + request.ExpectedSourceLotRevision.ToString(
                    CultureInfo.InvariantCulture),
                "coop-quantity:" + preview.AcceptedQuantity.ToString(
                    CultureInfo.InvariantCulture),
                "coop-offered-quantity:" + request.RequestedQuantity.ToString(
                    CultureInfo.InvariantCulture),
            });

        private static SimulationDecisionPreviewRequest CoopProtectedDecision(
            SimulationCoopConstructionPreviewSnapshot preview,
            SimulationCoopProtectedActionPreviewRequest request)
            => DecisionFor(preview, new[] { request.ProjectStableId });

        private 경영SimulationSessionSnapshot? ReplayAppliedCoopContribution(
            SimulationCoopContributionConfirmRequest request)
        {
            if (!appliedDecisionCommands.TryGetValue(request.CommandId, out var applied))
                return null;
            var stored = FindAppliedDecisionRequest(request.CommandId);
            var inputs = stored.Preview.Task.InputLotStableIds;
            var matches = stored.ExpectedRevision == request.ExpectedRevision
                && stored.Preview.DecisionTypeCode
                    == SimulationCoopConstructionCodes.Contribution
                && stored.Preview.ActorStableId == request.PlayerStableId
                && inputs.Contains(request.ProjectStableId, StringComparer.Ordinal)
                && inputs.Contains(request.BlueprintStableId, StringComparer.Ordinal)
                && inputs.Contains(request.BuildSiteH1StableId, StringComparer.Ordinal)
                && inputs.Contains(request.SourceLotStableId, StringComparer.Ordinal)
                && inputs.Contains("coop-lot-revision:"
                    + request.ExpectedSourceLotRevision.ToString(
                        CultureInfo.InvariantCulture), StringComparer.Ordinal)
                && inputs.Contains("coop-offered-quantity:"
                    + request.RequestedQuantity.ToString(
                        CultureInfo.InvariantCulture), StringComparer.Ordinal);
            if (!matches)
                throw new SimulationConflictException(
                    "SimulationCommandPayloadConflict");
            return Clone(applied.Snapshot);
        }

        private 경영SimulationSessionSnapshot? ReplayAppliedCoopProtectedAction(
            SimulationCoopProtectedActionConfirmRequest request, string actionCode)
        {
            if (!appliedDecisionCommands.TryGetValue(request.CommandId, out var applied))
                return null;
            var stored = FindAppliedDecisionRequest(request.CommandId);
            var matches = stored.ExpectedRevision == request.ExpectedRevision
                && stored.Preview.DecisionTypeCode == actionCode
                && stored.Preview.ActorStableId == request.OwnerPlayerStableId
                && stored.Preview.TargetStableIds.SequenceEqual(
                    new[] { request.ProjectStableId }, StringComparer.Ordinal);
            if (!matches)
                throw new SimulationConflictException(
                    "SimulationCommandPayloadConflict");
            return Clone(applied.Snapshot);
        }

        private SimulationDecisionConfirmRequest FindAppliedDecisionRequest(
            string commandId)
        {
            var stored = commandLog.Select(value => value.DecisionConfirmRequest)
                .SingleOrDefault(value => value?.CommandId == commandId);
            return stored ?? throw new SimulationConflictException(
                "SimulationCommandKindConflict");
        }

        private static SimulationDecisionPreviewRequest DecisionFor(
            SimulationCoopConstructionPreviewSnapshot preview, string[] inputs)
            => new()
            {
                DecisionStableId = "decision:coop:" + preview.ActionCode.ToLowerInvariant()
                    + ":" + preview.ProjectStableId + ":" + preview.PlayerStableId,
                DecisionTypeCode = preview.ActionCode,
                ActorStableId = preview.PlayerStableId,
                TargetStableIds = new[] { preview.ProjectStableId },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = preview.ActionCode,
                        TargetLedgerStableId = preview.ProjectStableId,
                        Delta = preview.AcceptedQuantity > 0m
                            ? preview.AcceptedQuantity : 1m,
                        AfterValue = preview.AcceptedQuantity > 0m
                            ? preview.AcceptedQuantity : 1m,
                        UnitCode = preview.UnitCode.Length > 0
                            ? preview.UnitCode : "state",
                        SourceStableIds = new[]
                        {
                            "source:" + SimulationCoopConstructionCodes.RuleRevision,
                        },
                    },
                },
                BlockReasonCodes = preview.BlockingReasonCodes,
                SourceStableIds = new[]
                {
                    "source:" + SimulationCoopConstructionCodes.RuleRevision,
                },
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:coop:" + preview.ActionCode.ToLowerInvariant()
                        + ":" + preview.ProjectStableId + ":" + preview.PlayerStableId,
                    TaskTypeCode = preview.ActionCode + "Task",
                    FacilityStableId = preview.ProjectStableId,
                    ActionCode = preview.ActionCode,
                    AssignedActorStableId = preview.PlayerStableId,
                    AssignedCapacity = preview.AcceptedQuantity > 0m
                        ? preview.AcceptedQuantity : 1m,
                    AssignedCapacityUnitCode = preview.UnitCode.Length > 0
                        ? preview.UnitCode : "state",
                    DurationTicks = preview.DurationTicks,
                    InputLotStableIds = inputs,
                    OutputCandidateCodes = new[] { preview.ProjectedStageCode },
                    SourceStableIds = new[]
                    {
                        "source:" + SimulationCoopConstructionCodes.RuleRevision,
                    },
                },
            };

        private CoopTaskReservation? PrepareCoopConstructionTask(
            SimulationTaskSnapshot task)
        {
            if (task.ActionCode == SimulationCoopConstructionCodes.Contribution)
            {
                var projectId = RequireInput(task, "coop-project:");
                var blueprintId = RequireInput(task, "blueprint:");
                var buildSite = RequireInput(task, "h1:");
                var lotId = task.InputLotStableIds.Single(value =>
                    value.StartsWith("lot:", StringComparison.Ordinal));
                var expectedLotRevision = ParseMarker(task, "coop-lot-revision:");
                var quantity = ParseDecimalMarker(task, "coop-quantity:");
                var replayRequest = new SimulationCoopContributionPreviewRequest
                {
                    ExpectedRevision = Revision,
                    PlayerStableId = task.AssignedActorStableId,
                    ProjectStableId = projectId,
                    BlueprintStableId = blueprintId,
                    BuildSiteH1StableId = buildSite,
                    SourceLotStableId = lotId,
                    ExpectedSourceLotRevision = expectedLotRevision,
                    RequestedQuantity = quantity,
                };
                var preview = CreateCoopContributionPreview(replayRequest, false);
                if (!preview.CanConfirm || preview.AcceptedQuantity != quantity)
                    throw new SimulationConflictException(
                        preview.BlockingReasonCodes.FirstOrDefault()
                        ?? "SimulationCoopContributionUnavailable");
                return new CoopTaskReservation(projectId, blueprintId, buildSite,
                    lotId, expectedLotRevision, quantity);
            }
            if (task.ActionCode == SimulationCoopConstructionCodes.Demolition)
            {
                var projectId = RequireInput(task, "coop-project:");
                var preview = CreateCoopProtectedPreview(
                    new SimulationCoopProtectedActionPreviewRequest
                    {
                        ExpectedRevision = Revision,
                        OwnerPlayerStableId = task.AssignedActorStableId,
                        ProjectStableId = projectId,
                    }, task.ActionCode, false);
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(preview.BlockingReasonCodes[0]);
                return new CoopTaskReservation(projectId, string.Empty, string.Empty,
                    string.Empty, 0, 0m);
            }
            if (task.ActionCode == SimulationCoopConstructionCodes.Restore)
            {
                var projectId = RequireInput(task, "coop-project:");
                var preview = CreateCoopProtectedPreview(
                    new SimulationCoopProtectedActionPreviewRequest
                    {
                        ExpectedRevision = Revision,
                        OwnerPlayerStableId = task.AssignedActorStableId,
                        ProjectStableId = projectId,
                    }, task.ActionCode, false);
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(preview.BlockingReasonCodes[0]);
                return new CoopTaskReservation(projectId, string.Empty, string.Empty,
                    string.Empty, 0, 0m);
            }
            return null;
        }

        private void ApplyCoopConstructionTaskReservation(SimulationTaskSnapshot task,
            CoopTaskReservation? reservation)
        {
            if (reservation == null) return;
            if (task.ActionCode == SimulationCoopConstructionCodes.Contribution)
            {
                if (!coopConstructionProjects.TryGetValue(reservation.ProjectStableId,
                        out _))
                {
                    var blueprint = integratedBlueprints[reservation.BlueprintStableId];
                    var required = blueprint.Materials.Single();
                    coopConstructionProjects.Add(reservation.ProjectStableId,
                        new SimulationCoopConstructionProjectSnapshot
                        {
                            ProjectStableId = reservation.ProjectStableId,
                            BlueprintStableId = reservation.BlueprintStableId,
                            BuildSiteH1StableId = reservation.BuildSiteH1StableId,
                            TargetFacilityStableId = "facility:coop:"
                                + reservation.ProjectStableId.Substring("coop-project:".Length),
                            StageCode = SimulationCoopConstructionCodes.Planned,
                            RequiredMaterialQuantity = required.Quantity,
                            UnitCode = required.UnitCode,
                            Revision = 1,
                        });
                }
                coopReservedQuantityByLot[reservation.LotStableId] =
                    coopReservedQuantityByLot.TryGetValue(reservation.LotStableId,
                        out var current) ? current + reservation.Quantity
                        : reservation.Quantity;
                var lotState = coopSourceLots[reservation.LotStableId];
                lotState.Revision++;
                lotState.ReservedQuantity += reservation.Quantity;
            }
            else if (task.ActionCode == SimulationCoopConstructionCodes.Demolition)
            {
                CreateDestructiveCheckpoint(task, reservation.ProjectStableId);
            }
        }

        private void ObserveCoopConstructionTaskCompletion(SimulationTaskSnapshot task,
            int completedWorldTick)
        {
            if (task.ActionCode == SimulationCoopConstructionCodes.Contribution)
                ApplyCoopContribution(task, completedWorldTick);
            else if (task.ActionCode == SimulationCoopConstructionCodes.Demolition)
                ApplyCoopDemolition(task);
            else if (task.ActionCode == SimulationCoopConstructionCodes.Restore)
                ApplyCoopRestore(task, completedWorldTick);
        }

        private void ApplyCoopContribution(SimulationTaskSnapshot task, int tick)
        {
            var projectId = RequireInput(task, "coop-project:");
            var lotId = task.InputLotStableIds.Single(value =>
                value.StartsWith("lot:", StringComparison.Ordinal));
            var quantity = ParseDecimalMarker(task, "coop-quantity:");
            var project = coopConstructionProjects[projectId];
            var lot = integratedLots[lotId];
            var lotState = coopSourceLots[lotId];
            lot.Quantity -= quantity;
            lotState.RemainingQuantity = lot.Quantity;
            lotState.ReservedQuantity -= quantity;
            coopReservedQuantityByLot[lotId] -= quantity;
            var contributionId = "coop-contribution:" + task.TaskStableId.Substring("task:".Length);
            var effectiveWork = quantity;
            var period = CreateNatureMindStateSnapshot().Periods.FirstOrDefault(value =>
                value.PlayerStableId == task.AssignedActorStableId);
            if (period?.PeriodStateCode == SimulationNaturePeriodCodes.GwangbokPeriod)
                effectiveWork += decimal.Round(quantity * .25m, 4);
            coopContributions.Add(contributionId, new SimulationCoopContributionSnapshot
            {
                ContributionStableId = contributionId,
                ProjectStableId = projectId,
                PlayerStableId = task.AssignedActorStableId,
                SourceLotStableId = lotId,
                SourceLotRevisionBefore = ParseMarker(task, "coop-lot-revision:"),
                MaterialQuantity = quantity,
                EffectiveWork = effectiveWork,
                UnitCode = project.UnitCode,
                StateCode = SimulationCoopConstructionCodes.Consumed,
                AppliedWorldTick = tick,
            });
            project.ContributedMaterialQuantity += quantity;
            project.ProgressValue = decimal.Round(
                project.ContributedMaterialQuantity / project.RequiredMaterialQuantity, 4);
            project.StageCode = StageFor(project.ProgressValue);
            project.Revision++;
            if (project.StageCode == SimulationCoopConstructionCodes.Operational)
            {
                project.CompletedWorldTick = tick;
                var blueprint = integratedBlueprints[project.BlueprintStableId];
                var definition = integratedFacilityDefinitions[
                    blueprint.FacilityDefinitionStableId];
                if (!integratedFacilities.ContainsKey(project.TargetFacilityStableId))
                    integratedFacilities.Add(project.TargetFacilityStableId,
                        CreateRuntimeFacility(project.TargetFacilityStableId, definition,
                            project.BuildSiteH1StableId, Array.Empty<string>(),
                            SimulationFacilityLifecycleCodes.Operational,
                            blueprint.SettlementFacilityTypeCode,
                            blueprint.SettlementDistrictStableId));
                project.OpenedCapabilityCodes = definition.CapabilityCodes.ToArray();
                project.OpenedWorldInteractionIds = new[] { "WI-FARM-STORE-01" };
            }
            RefreshProjectHash(project);
        }

        private void ApplyCoopDemolition(SimulationTaskSnapshot task)
        {
            var project = coopConstructionProjects[RequireInput(task, "coop-project:")];
            integratedFacilities[project.TargetFacilityStableId].LifecycleCode =
                SimulationFacilityLifecycleCodes.Removed;
            project.StageCode = SimulationCoopConstructionCodes.Removed;
            project.Revision++;
            RefreshProjectHash(project);
        }

        private void ApplyCoopRestore(SimulationTaskSnapshot task, int tick)
        {
            var project = coopConstructionProjects[RequireInput(task, "coop-project:")];
            var checkpoint = coopProtectionCheckpoints.Values.Where(value =>
                    value.CheckpointKindCode
                        == SimulationCoopConstructionCodes.DestructiveActionCheckpoint
                    && value.TargetStableIds.Contains(project.TargetFacilityStableId,
                        StringComparer.Ordinal))
                .OrderByDescending(value => value.BeforeWorldRevision).First();
            integratedFacilities[project.TargetFacilityStableId].LifecycleCode =
                SimulationFacilityLifecycleCodes.Operational;
            project.StageCode = SimulationCoopConstructionCodes.Operational;
            project.Revision++;
            RefreshProjectHash(project);
            var effectId = "restore-effect:" + task.TaskStableId;
            coopRestoreEffects.Add(effectId, new SimulationWorldRestoreEffectSnapshot
            {
                EffectStableId = effectId,
                CheckpointStableId = checkpoint.CheckpointStableId,
                TargetStableId = project.TargetFacilityStableId,
                EffectTypeCode = SimulationCoopConstructionCodes.CompensatingRestore,
                AppliedWorldTick = tick,
                DeletesHistoricalEffects = false,
                DuplicatesResources = false,
            });
        }

        private void EnsureHostedProtectionCheckpoint(string actionRequestId)
        {
            var checkpointId = "checkpoint:hosted-session:" + SessionStableId.Substring(
                "simulation-session:".Length);
            coopProtectionCheckpoints.TryAdd(checkpointId,
                new SimulationWorldProtectionCheckpointSnapshot
                {
                    CheckpointStableId = checkpointId,
                    CheckpointKindCode =
                        SimulationCoopConstructionCodes.HostedSessionProtection,
                    WorldStableId = SessionStableId,
                    TargetStableIds = new[] { "world:" + SessionStableId },
                    BeforeWorldRevision = Revision,
                    SpatialStateHashSha256 = HashCoop(SessionStableId + "|"
                        + SimulationAreaAccessCodes.FarmToHubSourceHHashSha256),
                    RelatedConnectorRefs = new[]
                    {
                        SimulationAreaAccessCodes.FarmToHubConnector,
                    },
                    CreatedByActionRequestId = actionRequestId,
                    HistoricalEffectsDeleted = false,
                });
        }

        private void CreateDestructiveCheckpoint(SimulationTaskSnapshot task,
            string projectId)
        {
            var project = coopConstructionProjects[projectId];
            var checkpointId = "checkpoint:destructive:" + task.TaskStableId;
            coopProtectionCheckpoints.Add(checkpointId,
                new SimulationWorldProtectionCheckpointSnapshot
                {
                    CheckpointStableId = checkpointId,
                    CheckpointKindCode =
                        SimulationCoopConstructionCodes.DestructiveActionCheckpoint,
                    WorldStableId = SessionStableId,
                    TargetStableIds = new[] { project.TargetFacilityStableId },
                    BeforeWorldRevision = Revision,
                    SpatialStateHashSha256 = HashCoop(project.BuildSiteH1StableId + "|"
                        + SimulationAreaAccessCodes.FarmToHubSourceHHashSha256),
                    RelatedResourceRefs = coopContributions.Values.Where(value =>
                            value.ProjectStableId == projectId)
                        .Select(value => value.SourceLotStableId)
                        .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray(),
                    RelatedConnectorRefs = new[]
                    {
                        SimulationAreaAccessCodes.FarmToHubConnector,
                    },
                    CreatedByActionRequestId = task.TaskStableId,
                    HistoricalEffectsDeleted = false,
                });
        }

        private SimulationCoopConstructionStateSnapshot CreateCoopConstructionStateSnapshot()
        {
            foreach (var project in coopConstructionProjects.Values)
                RefreshProjectHash(project);
            var result = new SimulationCoopConstructionStateSnapshot
            {
                WorldRevision = Revision,
                WorldTick = CurrentTick,
                Projects = coopConstructionProjects.Values.OrderBy(value =>
                    value.ProjectStableId, StringComparer.Ordinal).Select(CloneCoopProject).ToArray(),
                Contributions = coopContributions.Values.OrderBy(value =>
                    value.ContributionStableId, StringComparer.Ordinal)
                    .Select(CloneCoopContribution).ToArray(),
                SourceLots = coopSourceLots.Values.OrderBy(value => value.LotStableId,
                    StringComparer.Ordinal).Select(CloneCoopSourceLot).ToArray(),
                ProtectionCheckpoints = coopProtectionCheckpoints.Values.OrderBy(value =>
                    value.CheckpointStableId, StringComparer.Ordinal)
                    .Select(CloneProtectionCheckpoint).ToArray(),
                RestoreEffects = coopRestoreEffects.Values.OrderBy(value =>
                    value.EffectStableId, StringComparer.Ordinal)
                    .Select(CloneRestoreEffect).ToArray(),
                UsesCompensatingEffects = true,
                MutatesStaticHDefinitions = false,
            };
            result.StateHashSha256 = HashCoop(string.Join("|", result.RuleRevision,
                result.ProtectionRuleRevision, string.Join(",", result.Projects.Select(value =>
                    value.ProjectHashSha256)), string.Join(",", result.Contributions.Select(value =>
                    value.ContributionStableId)), string.Join(",", result.SourceLots.Select(value =>
                    value.LotStableId + "~" + value.Revision + "~" + value.RemainingQuantity)),
                string.Join(",", result.ProtectionCheckpoints.Select(value =>
                    value.CheckpointStableId)), string.Join(",", result.RestoreEffects.Select(value =>
                    value.EffectStableId))));
            return result;
        }

        internal static SimulationCoopConstructionStateSnapshot CloneCoopConstructionState(
            SimulationCoopConstructionStateSnapshot? value)
        {
            value ??= new SimulationCoopConstructionStateSnapshot();
            return new SimulationCoopConstructionStateSnapshot
            {
                RuleRevision = value.RuleRevision,
                ProtectionRuleRevision = value.ProtectionRuleRevision,
                WorldRevision = value.WorldRevision,
                WorldTick = value.WorldTick,
                Projects = value.Projects.Select(CloneCoopProject).ToArray(),
                Contributions = value.Contributions.Select(CloneCoopContribution).ToArray(),
                SourceLots = value.SourceLots.Select(CloneCoopSourceLot).ToArray(),
                ProtectionCheckpoints = value.ProtectionCheckpoints
                    .Select(CloneProtectionCheckpoint).ToArray(),
                RestoreEffects = value.RestoreEffects.Select(CloneRestoreEffect).ToArray(),
                UsesCompensatingEffects = value.UsesCompensatingEffects,
                MutatesStaticHDefinitions = value.MutatesStaticHDefinitions,
                StateHashSha256 = value.StateHashSha256,
                SimulationOnly = value.SimulationOnly,
                IsOperationalState = value.IsOperationalState,
            };
        }

        private static SimulationCoopConstructionProjectSnapshot CloneCoopProject(
            SimulationCoopConstructionProjectSnapshot value) => new()
        {
            ProjectStableId = value.ProjectStableId,
            BlueprintStableId = value.BlueprintStableId,
            BuildSiteH1StableId = value.BuildSiteH1StableId,
            TargetFacilityStableId = value.TargetFacilityStableId,
            StageCode = value.StageCode,
            RequiredMaterialQuantity = value.RequiredMaterialQuantity,
            ContributedMaterialQuantity = value.ContributedMaterialQuantity,
            ProgressValue = value.ProgressValue,
            UnitCode = value.UnitCode,
            Revision = value.Revision,
            CompletedWorldTick = value.CompletedWorldTick,
            OpenedCapabilityCodes = value.OpenedCapabilityCodes.ToArray(),
            OpenedWorldInteractionIds = value.OpenedWorldInteractionIds.ToArray(),
            ProjectHashSha256 = value.ProjectHashSha256,
        };
        private static SimulationCoopContributionSnapshot CloneCoopContribution(
            SimulationCoopContributionSnapshot value) => new()
        {
            ContributionStableId = value.ContributionStableId,
            ProjectStableId = value.ProjectStableId,
            PlayerStableId = value.PlayerStableId,
            SourceLotStableId = value.SourceLotStableId,
            SourceLotRevisionBefore = value.SourceLotRevisionBefore,
            MaterialQuantity = value.MaterialQuantity,
            EffectiveWork = value.EffectiveWork,
            UnitCode = value.UnitCode,
            StateCode = value.StateCode,
            AppliedWorldTick = value.AppliedWorldTick,
        };
        private static SimulationCoopSourceLotStateSnapshot CloneCoopSourceLot(
            SimulationCoopSourceLotStateSnapshot value) => new()
        {
            LotStableId = value.LotStableId,
            Revision = value.Revision,
            ReservedQuantity = value.ReservedQuantity,
            RemainingQuantity = value.RemainingQuantity,
            UnitCode = value.UnitCode,
        };
        private static SimulationWorldProtectionCheckpointSnapshot CloneProtectionCheckpoint(
            SimulationWorldProtectionCheckpointSnapshot value) => new()
        {
            CheckpointStableId = value.CheckpointStableId,
            CheckpointKindCode = value.CheckpointKindCode,
            WorldStableId = value.WorldStableId,
            TargetStableIds = value.TargetStableIds.ToArray(),
            BeforeWorldRevision = value.BeforeWorldRevision,
            SpatialStateHashSha256 = value.SpatialStateHashSha256,
            RelatedResourceRefs = value.RelatedResourceRefs.ToArray(),
            RelatedConnectorRefs = value.RelatedConnectorRefs.ToArray(),
            CreatedByActionRequestId = value.CreatedByActionRequestId,
            HistoricalEffectsDeleted = value.HistoricalEffectsDeleted,
        };
        private static SimulationWorldRestoreEffectSnapshot CloneRestoreEffect(
            SimulationWorldRestoreEffectSnapshot value) => new()
        {
            EffectStableId = value.EffectStableId,
            CheckpointStableId = value.CheckpointStableId,
            TargetStableId = value.TargetStableId,
            EffectTypeCode = value.EffectTypeCode,
            AppliedWorldTick = value.AppliedWorldTick,
            DeletesHistoricalEffects = value.DeletesHistoricalEffects,
            DuplicatesResources = value.DuplicatesResources,
        };

        private void RefreshProjectHash(SimulationCoopConstructionProjectSnapshot value)
            => value.ProjectHashSha256 = HashCoop(string.Join("|", value.ProjectStableId,
                value.BlueprintStableId, value.BuildSiteH1StableId,
                value.TargetFacilityStableId, value.StageCode,
                value.RequiredMaterialQuantity, value.ContributedMaterialQuantity,
                value.ProgressValue, value.UnitCode, value.Revision,
                value.CompletedWorldTick?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                string.Join(",", value.OpenedCapabilityCodes),
                string.Join(",", value.OpenedWorldInteractionIds)));

        private static string StageFor(decimal progress)
            => progress >= 1m ? SimulationCoopConstructionCodes.Operational
                : progress >= .75m ? SimulationCoopConstructionCodes.Finishing
                : progress >= .5m ? SimulationCoopConstructionCodes.Frame
                : progress > 0m ? SimulationCoopConstructionCodes.Foundation
                : SimulationCoopConstructionCodes.Planned;

        private static string RequireInput(SimulationTaskSnapshot task, string prefix)
            => task.InputLotStableIds.Single(value =>
                value.StartsWith(prefix, StringComparison.Ordinal));
        private static long ParseMarker(SimulationTaskSnapshot task, string prefix)
            => long.Parse(RequireInput(task, prefix).Substring(prefix.Length),
                CultureInfo.InvariantCulture);
        private static decimal ParseDecimalMarker(SimulationTaskSnapshot task,
            string prefix) => decimal.Parse(RequireInput(task, prefix).Substring(prefix.Length),
                CultureInfo.InvariantCulture);
        private static string HashCoop(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void ValidateCoopContributionRequest(
            SimulationCoopContributionPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0 || request.ExpectedSourceLotRevision <= 0)
                throw new SimulationContractException("SimulationCoopRevisionInvalid");
            RequireStableId(request.PlayerStableId, "SimulationCoopPlayerInvalid");
            RequireStableId(request.ProjectStableId, "SimulationCoopProjectInvalid");
            RequireStableId(request.BlueprintStableId, "SimulationCoopBlueprintInvalid");
            RequireStableId(request.BuildSiteH1StableId, "SimulationCoopBuildSiteInvalid");
            RequireStableId(request.SourceLotStableId, "SimulationCoopSourceLotInvalid");
        }

        private static void ValidateCoopProtectedRequest(
            SimulationCoopProtectedActionPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.OwnerPlayerStableId,
                "SimulationWorldProtectionOwnerInvalid");
            RequireStableId(request.ProjectStableId, "SimulationCoopProjectInvalid");
        }

        private sealed class CoopTaskReservation
        {
            public CoopTaskReservation(string projectStableId, string blueprintStableId,
                string buildSiteH1StableId, string lotStableId,
                long sourceLotRevision, decimal quantity)
            {
                ProjectStableId = projectStableId;
                BlueprintStableId = blueprintStableId;
                BuildSiteH1StableId = buildSiteH1StableId;
                LotStableId = lotStableId;
                SourceLotRevision = sourceLotRevision;
                Quantity = quantity;
            }

            public string ProjectStableId { get; }
            public string BlueprintStableId { get; }
            public string BuildSiteH1StableId { get; }
            public string LotStableId { get; }
            public long SourceLotRevision { get; }
            public decimal Quantity { get; }
        }
    }
}
