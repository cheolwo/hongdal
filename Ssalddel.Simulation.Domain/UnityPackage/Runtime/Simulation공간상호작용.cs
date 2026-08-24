using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 공간작업영역해제EffectCode = "SpatialWorkAreaReleased";
        private const string 공간보관용량사용EffectCode = "SpatialStorageCapacityConsumed";
        private const string 공간복원자재사용EffectCode = "SpatialRestorationMaterialConsumed";
        private const string 공간회복보급사용EffectCode = "SpatialRecoverySupplyConsumed";

        private readonly Dictionary<string, Simulation공간정의Snapshot> spatialDefinitions =
            new Dictionary<string, Simulation공간정의Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, Simulation공간실행상태Snapshot> spatialRuntimeStates =
            new Dictionary<string, Simulation공간실행상태Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, Simulation공간예약Snapshot> spatialReservations =
            new Dictionary<string, Simulation공간예약Snapshot>(StringComparer.Ordinal);
        private Simulation공간세계InitialStateRequest? spatialWorldCreationState;

        private void InitializeSimulationSpatialWorld(Simulation공간세계InitialStateRequest? request)
        {
            spatialWorldCreationState = CloneSimulationSpatialInitialState(request);
            if (request == null) return;

            foreach (var source in request.Definitions.OrderBy(
                value => value.SpatialStableId, StringComparer.Ordinal))
            {
                var definition = CloneSpatialDefinition(source);
                spatialDefinitions.Add(definition.SpatialStableId, definition);
                spatialRuntimeStates.Add(definition.SpatialStableId, new Simulation공간실행상태Snapshot
                {
                    SpatialStableId = definition.SpatialStableId,
                    AccessStateCode = definition.AccessStateCode,
                    OccupiedCapacities = definition.BaseCapacities.Select(value =>
                        Capacity(value.CapacityCode, 0m, value.UnitCode)).ToArray(),
                    ReservedCapacities = definition.BaseCapacities.Select(value =>
                        Capacity(value.CapacityCode, 0m, value.UnitCode)).ToArray(),
                    Revision = 1,
                });
            }
        }

        private void ResolveSimulationSpatialPreview(SimulationDecisionPreviewSnapshot preview)
        {
            if (preview.TaskPlan.ActionCode == "CargoRouteMovement"
                && (preview.TaskPlan.PreferredOriginSpatialStableId.Length > 0
                    || preview.TaskPlan.PreferredRouteSpatialStableId.Length > 0
                    || preview.TaskPlan.PreferredDestinationSpatialStableId.Length > 0))
            {
                ResolveLogisticsSpatialPreview(preview);
                return;
            }
            var rule = ResolveSpatialRule(preview.TaskPlan.ActionCode, preview.TaskPlan.AssignedCapacity,
                preview.TaskPlan.AssignedCapacityUnitCode);
            if (rule == null) return;

            var preferred = preview.TaskPlan.PreferredSpatialStableId;
            var facilityCandidates = spatialDefinitions.Values.Where(value => string.Equals(
                    value.FacilityStableId, preview.TaskPlan.FacilityStableId, StringComparison.Ordinal))
                .OrderBy(value => value.SpatialStableId, StringComparer.Ordinal).ToArray();
            var candidates = preferred.Length == 0
                ? facilityCandidates
                : facilityCandidates.Where(value => string.Equals(
                    value.SpatialStableId, preferred, StringComparison.Ordinal)).ToArray();
            var block = ResolveSpatialBlock(candidates, facilityCandidates, rule);
            var selected = block == null
                ? candidates.First(value => SpatialCandidateBlock(value, rule) == null)
                : null;

            if (block != null)
            {
                preview.Decision.BlockReasonCodes = MergeSources(
                    preview.Decision.BlockReasonCodes, new[] { block });
            }
            else if (selected != null)
            {
                preview.TaskPlan.SelectedSpatialStableId = selected.SpatialStableId;
                preview.TaskPlan.SpatialDefinitionRevision = selected.DefinitionRevision;
                preview.TaskPlan.SpatialDefinitionHashSha256 = selected.DefinitionHashSha256;
                preview.Decision.ExpectedEffects = preview.Decision.ExpectedEffects.Concat(
                    CreateSpatialExpectedEffects(selected, rule, preview.Decision.SourceStableIds))
                    .ToArray();
            }

            preview.SpatialInteraction = new Simulation공간상호작용PreviewSnapshot
            {
                PreferredSpatialStableId = preferred,
                SelectedSpatialStableId = selected?.SpatialStableId ?? string.Empty,
                RequiredCapabilityCodes = rule.RequiredCapabilities.ToArray(),
                RequiredCapacities = rule.Capacities.Select(CloneCapacity).ToArray(),
                DefinitionRevision = selected?.DefinitionRevision ?? string.Empty,
                DefinitionHashSha256 = selected?.DefinitionHashSha256 ?? string.Empty,
                EvidenceKindCode = selected?.EvidenceKindCode ?? string.Empty,
                BlockReasonCodes = block == null ? Array.Empty<string>() : new[] { block },
            };
        }

        private void ResolveLogisticsSpatialPreview(SimulationDecisionPreviewSnapshot preview)
        {
            var originRule = new Simulation공간Rule(
                new[]
                {
                    Simulation공간능력Codes.LoadingWorkArea,
                    Simulation공간능력Codes.VehicleAccessible,
                    Simulation공간능력Codes.CargoAccessible,
                    Simulation공간능력Codes.WorkerAccessible,
                },
                new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            var routeRule = new Simulation공간Rule(
                new[] { Simulation공간능력Codes.CargoRoute },
                Array.Empty<Simulation공간용량Snapshot>());
            var destinationRule = new Simulation공간Rule(
                new[]
                {
                    Simulation공간능력Codes.UnloadingWorkArea,
                    Simulation공간능력Codes.CargoAccessible,
                    Simulation공간능력Codes.WorkerAccessible,
                },
                new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });

            var bindings = new[]
            {
                ResolveSpatialRoleBinding(
                    Simulation공간역할Codes.OriginLoading,
                    preview.TaskPlan.PreferredOriginSpatialStableId,
                    spatialDefinitions.Values.Where(value => value.FacilityStableId ==
                        preview.TaskPlan.FacilityStableId).ToArray(), originRule),
                ResolveSpatialRoleBinding(
                    Simulation공간역할Codes.TransportRoute,
                    preview.TaskPlan.PreferredRouteSpatialStableId,
                    spatialDefinitions.Values.ToArray(), routeRule),
                ResolveSpatialRoleBinding(
                    Simulation공간역할Codes.DestinationUnloading,
                    preview.TaskPlan.PreferredDestinationSpatialStableId,
                    spatialDefinitions.Values.Where(value => value.FacilityStableId ==
                        preview.TaskPlan.DestinationFacilityStableId).ToArray(), destinationRule),
            };
            var blocks = bindings.SelectMany(value => value.BlockReasonCodes)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            preview.TaskPlan.SpatialRoleBindings = bindings.Select(CloneSpatialRoleBinding).ToArray();
            var primary = bindings[0];
            if (blocks.Length == 0)
            {
                preview.TaskPlan.SelectedSpatialStableId = primary.SelectedSpatialStableId;
                preview.TaskPlan.SpatialDefinitionRevision = primary.DefinitionRevision;
                preview.TaskPlan.SpatialDefinitionHashSha256 = primary.DefinitionHashSha256;
                foreach (var binding in bindings.Where(value => value.RequiredCapacities.Length > 0))
                {
                    var definition = spatialDefinitions[binding.SelectedSpatialStableId];
                    var rule = new Simulation공간Rule(binding.RequiredCapabilityCodes,
                        binding.RequiredCapacities);
                    preview.Decision.ExpectedEffects = preview.Decision.ExpectedEffects.Concat(
                        CreateSpatialExpectedEffects(definition, rule, preview.Decision.SourceStableIds))
                        .ToArray();
                }
            }
            else
            {
                preview.Decision.BlockReasonCodes = MergeSources(
                    preview.Decision.BlockReasonCodes, blocks);
            }
            preview.SpatialInteraction = new Simulation공간상호작용PreviewSnapshot
            {
                PreferredSpatialStableId = primary.PreferredSpatialStableId,
                SelectedSpatialStableId = blocks.Length == 0
                    ? primary.SelectedSpatialStableId : string.Empty,
                RequiredCapabilityCodes = bindings.SelectMany(value =>
                    value.RequiredCapabilityCodes).Distinct(StringComparer.Ordinal).ToArray(),
                RequiredCapacities = bindings.SelectMany(value => value.RequiredCapacities)
                    .Select(CloneCapacity).ToArray(),
                DefinitionRevision = blocks.Length == 0 ? primary.DefinitionRevision : string.Empty,
                DefinitionHashSha256 = blocks.Length == 0
                    ? primary.DefinitionHashSha256 : string.Empty,
                EvidenceKindCode = blocks.Length == 0 ? primary.EvidenceKindCode : string.Empty,
                BlockReasonCodes = blocks,
                RoleBindings = bindings.Select(CloneSpatialRoleBinding).ToArray(),
            };
        }

        private Simulation공간역할BindingSnapshot ResolveSpatialRoleBinding(
            string roleCode,
            string preferredSpatialStableId,
            Simulation공간정의Snapshot[] roleCandidates,
            Simulation공간Rule rule)
        {
            var orderedCandidates = roleCandidates.OrderBy(value => value.SpatialStableId,
                StringComparer.Ordinal).ToArray();
            var candidates = preferredSpatialStableId.Length == 0
                ? orderedCandidates
                : orderedCandidates.Where(value => value.SpatialStableId ==
                    preferredSpatialStableId).ToArray();
            var block = ResolveSpatialBlock(candidates, orderedCandidates, rule);
            var selected = block == null
                ? candidates.First(value => SpatialCandidateBlock(value, rule) == null)
                : null;
            return new Simulation공간역할BindingSnapshot
            {
                RoleCode = roleCode,
                PreferredSpatialStableId = preferredSpatialStableId,
                SelectedSpatialStableId = selected?.SpatialStableId ?? string.Empty,
                DefinitionRevision = selected?.DefinitionRevision ?? string.Empty,
                DefinitionHashSha256 = selected?.DefinitionHashSha256 ?? string.Empty,
                EvidenceKindCode = selected?.EvidenceKindCode ?? string.Empty,
                RequiredCapabilityCodes = rule.RequiredCapabilities.ToArray(),
                RequiredCapacities = rule.Capacities.Select(CloneCapacity).ToArray(),
                BlockReasonCodes = block == null ? Array.Empty<string>() : new[] { block },
            };
        }

        private Simulation공간상호작용PreviewSnapshot CreateFarmSpatialPreview(
            string facilityStableId,
            string actionCode,
            string preferredSpatialStableId,
            decimal assignedCapacity,
            string assignedCapacityUnitCode)
        {
            var preview = new SimulationDecisionPreviewSnapshot
            {
                Decision = new SimulationDecisionSnapshot
                {
                    SourceStableIds = new[] { farmSurvivalCreationState!.AreaStableId },
                },
                TaskPlan = new SimulationTaskPlanSnapshot
                {
                    FacilityStableId = facilityStableId,
                    ActionCode = actionCode,
                    PreferredSpatialStableId = preferredSpatialStableId,
                    AssignedCapacity = assignedCapacity,
                    AssignedCapacityUnitCode = assignedCapacityUnitCode,
                },
            };
            ResolveSimulationSpatialPreview(preview);
            return preview.SpatialInteraction
                ?? throw new SimulationContractException("SimulationSpatialRuleUnavailable");
        }

        private string? ResolveSpatialBlock(
            Simulation공간정의Snapshot[] candidates,
            Simulation공간정의Snapshot[] facilityCandidates,
            Simulation공간Rule rule)
        {
            if (facilityCandidates.Length == 0 || candidates.Length == 0)
                return Simulation공간차단Codes.DefinitionUnavailable;
            var capabilityCandidates = candidates.Where(value => rule.RequiredCapabilities.All(required =>
                    value.CapabilityCodes.Contains(required, StringComparer.Ordinal))).ToArray();
            if (capabilityCandidates.Length == 0)
                return Simulation공간차단Codes.CapabilityMissing;
            var accessibleCandidates = capabilityCandidates.Where(value => string.Equals(
                    spatialRuntimeStates[value.SpatialStableId].AccessStateCode,
                    Simulation공간접근상태Codes.Available, StringComparison.Ordinal)).ToArray();
            if (accessibleCandidates.Length == 0)
                return Simulation공간차단Codes.AccessUnavailable;
            var candidateBlocks = accessibleCandidates.Select(value => SpatialCandidateBlock(value, rule)).ToArray();
            if (candidateBlocks.Any(value => value == null)) return null;
            return candidateBlocks.Contains(Simulation공간차단Codes.CapacityInsufficient,
                    StringComparer.Ordinal)
                ? Simulation공간차단Codes.CapacityInsufficient
                : candidateBlocks.First();
        }

        private string? SpatialCandidateBlock(
            Simulation공간정의Snapshot definition,
            Simulation공간Rule rule)
        {
            if (!rule.RequiredCapabilities.All(required =>
                    definition.CapabilityCodes.Contains(required, StringComparer.Ordinal)))
                return Simulation공간차단Codes.CapabilityMissing;
            var runtime = spatialRuntimeStates[definition.SpatialStableId];
            if (!string.Equals(runtime.AccessStateCode, Simulation공간접근상태Codes.Available,
                    StringComparison.Ordinal))
                return Simulation공간차단Codes.AccessUnavailable;
            foreach (var required in rule.Capacities)
            {
                var baseValue = FindCapacity(definition.BaseCapacities, required);
                if (baseValue == null)
                    return Simulation공간차단Codes.CapacityInsufficient;
                var available = baseValue.Quantity
                    - CapacityQuantity(runtime.OccupiedCapacities, required)
                    - CapacityQuantity(runtime.ReservedCapacities, required);
                if (available < required.Quantity)
                {
                    return required.CapacityCode == Simulation공간용량Codes.WorkArea
                        ? Simulation공간차단Codes.ReservationConflict
                        : Simulation공간차단Codes.CapacityInsufficient;
                }
            }
            return null;
        }

        private Simulation공간예약Snapshot[] PrepareSimulationSpatialReservations(
            SimulationTaskSnapshot task)
        {
            if (task.SpatialRoleBindings.Length > 0)
            {
                var reservations = new List<Simulation공간예약Snapshot>();
                foreach (var binding in task.SpatialRoleBindings.Where(value =>
                    value.SelectedSpatialStableId.Length > 0))
                {
                    var roleDefinition = spatialDefinitions[binding.SelectedSpatialStableId];
                    var roleRule = new Simulation공간Rule(binding.RequiredCapabilityCodes,
                        binding.RequiredCapacities);
                    var roleBlock = SpatialCandidateBlock(roleDefinition, roleRule);
                    if (roleBlock != null) throw new SimulationConflictException(roleBlock);
                    reservations.AddRange(binding.RequiredCapacities.Select(required =>
                        new Simulation공간예약Snapshot
                        {
                            ReservationStableId = task.TaskStableId + ":spatial-reservation:"
                                + binding.RoleCode + ":" + required.CapacityCode,
                            SpatialStableId = binding.SelectedSpatialStableId,
                            TaskStableId = task.TaskStableId,
                            RoleCode = binding.RoleCode,
                            ReservationKindCode = required.CapacityCode,
                            Quantity = required.Quantity,
                            UnitCode = required.UnitCode,
                            StatusCode = Simulation공간예약상태Codes.Reserved,
                            ReservedAtTick = CurrentTick,
                            CreatedRevision = Revision + 1,
                        }));
                }
                return reservations.ToArray();
            }
            if (task.SelectedSpatialStableId.Length == 0) return Array.Empty<Simulation공간예약Snapshot>();
            if (!spatialDefinitions.TryGetValue(task.SelectedSpatialStableId, out var definition))
                throw new SimulationNotFoundException(Simulation공간차단Codes.DefinitionUnavailable);
            var rule = ResolveSpatialRule(task.ActionCode, task.AssignedCapacity,
                task.AssignedCapacityUnitCode)
                ?? throw new SimulationContractException("SimulationSpatialRuleUnavailable");
            var block = SpatialCandidateBlock(definition, rule);
            if (block != null) throw new SimulationConflictException(block);
            return rule.Capacities.Select(required => new Simulation공간예약Snapshot
            {
                ReservationStableId = task.TaskStableId + ":spatial-reservation:"
                    + required.CapacityCode,
                SpatialStableId = definition.SpatialStableId,
                TaskStableId = task.TaskStableId,
                RoleCode = string.Empty,
                ReservationKindCode = required.CapacityCode,
                Quantity = required.Quantity,
                UnitCode = required.UnitCode,
                StatusCode = Simulation공간예약상태Codes.Reserved,
                ReservedAtTick = CurrentTick,
                CreatedRevision = Revision + 1,
            }).ToArray();
        }

        private void ReleaseSimulationSpatialReservationsForTaskRole(
            SimulationTaskSnapshot task,
            string roleCode,
            int releasedTick)
        {
            var reservations = spatialReservations.Values.Where(value =>
                value.TaskStableId == task.TaskStableId
                && value.RoleCode == roleCode
                && value.StatusCode == Simulation공간예약상태Codes.Reserved).ToArray();
            foreach (var group in reservations.GroupBy(value => value.SpatialStableId,
                StringComparer.Ordinal))
            {
                var runtime = spatialRuntimeStates[group.Key];
                foreach (var reservation in group)
                {
                    AddCapacity(runtime.ReservedCapacities, reservation.ReservationKindCode,
                        -reservation.Quantity, reservation.UnitCode);
                    reservation.StatusCode = Simulation공간예약상태Codes.Released;
                    reservation.ReleasedAtTick = releasedTick;
                    reservation.FinalizedRevision = Revision + 1;
                }
                if (!spatialReservations.Values.Any(value => value.TaskStableId == task.TaskStableId
                    && value.SpatialStableId == group.Key
                    && value.StatusCode == Simulation공간예약상태Codes.Reserved))
                {
                    runtime.ActiveTaskStableIds = runtime.ActiveTaskStableIds.Where(value =>
                        value != task.TaskStableId).ToArray();
                }
                runtime.Revision++;
            }
        }

        private void RegisterSimulationSpatialReservations(
            SimulationTaskSnapshot task,
            Simulation공간예약Snapshot[] reservations)
        {
            if (reservations.Length == 0) return;
            foreach (var group in reservations.GroupBy(value => value.SpatialStableId,
                StringComparer.Ordinal))
            {
                var runtime = spatialRuntimeStates[group.Key];
                foreach (var reservation in group)
                {
                    spatialReservations.Add(reservation.ReservationStableId, reservation);
                    AddCapacity(runtime.ReservedCapacities, reservation.ReservationKindCode,
                        reservation.Quantity, reservation.UnitCode);
                }
                runtime.ActiveTaskStableIds = runtime.ActiveTaskStableIds.Append(task.TaskStableId)
                    .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                runtime.Revision++;
            }
        }

        private void CompleteSimulationSpatialReservationsForTask(
            SimulationTaskSnapshot task,
            int completedTick)
        {
            var reservations = spatialReservations.Values.Where(value =>
                    value.TaskStableId == task.TaskStableId
                    && value.StatusCode == Simulation공간예약상태Codes.Reserved)
                .OrderBy(value => value.ReservationStableId, StringComparer.Ordinal).ToArray();
            if (reservations.Length == 0) return;
            foreach (var group in reservations.GroupBy(value => value.SpatialStableId,
                StringComparer.Ordinal))
            {
                var runtime = spatialRuntimeStates[group.Key];
                foreach (var reservation in group)
                {
                    AddCapacity(runtime.ReservedCapacities, reservation.ReservationKindCode,
                        -reservation.Quantity, reservation.UnitCode);
                    if (IsConsumableCapacity(reservation.ReservationKindCode))
                    {
                        AddCapacity(runtime.OccupiedCapacities, reservation.ReservationKindCode,
                            reservation.Quantity, reservation.UnitCode);
                        reservation.StatusCode = Simulation공간예약상태Codes.Consumed;
                        reservation.ConsumedAtTick = completedTick;
                    }
                    else
                    {
                        reservation.StatusCode = Simulation공간예약상태Codes.Released;
                        reservation.ReleasedAtTick = completedTick;
                    }
                    reservation.FinalizedRevision = Revision + 1;
                }
                runtime.ActiveTaskStableIds = runtime.ActiveTaskStableIds.Where(value =>
                    value != task.TaskStableId).ToArray();
                runtime.Revision++;
            }
        }

        private Simulation공간정의Snapshot[] CreateSimulationSpatialDefinitionSnapshots()
            => spatialDefinitions.Values.OrderBy(value => value.SpatialStableId, StringComparer.Ordinal)
                .Select(CloneSpatialDefinition).ToArray();

        private Simulation공간실행상태Snapshot[] CreateSimulationSpatialRuntimeSnapshots()
            => spatialRuntimeStates.Values.OrderBy(value => value.SpatialStableId, StringComparer.Ordinal)
                .Select(CloneSpatialRuntime).ToArray();

        private Simulation공간예약Snapshot[] CreateSimulationSpatialReservationSnapshots()
            => spatialReservations.Values.OrderBy(value => value.ReservationStableId, StringComparer.Ordinal)
                .Select(CloneSpatialReservation).ToArray();

        private static void ValidateSimulationSpatialInitialState(
            Simulation공간세계InitialStateRequest? request)
        {
            if (request == null) return;
            if (request.Definitions == null)
                throw new SimulationContractException("SimulationSpatialDefinitionsInvalid");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in request.Definitions)
            {
                if (definition == null)
                    throw new SimulationContractException("SimulationSpatialDefinitionInvalid");
                RequireStableId(definition.SpatialStableId, "SimulationSpatialStableIdInvalid");
                RequireStableId(definition.FacilityStableId, "SimulationSpatialFacilityStableIdInvalid");
                RequireStableId(definition.EvidenceKindCode, "SimulationSpatialEvidenceKindInvalid");
                RequireStableId(definition.AccessStateCode, "SimulationSpatialAccessStateInvalid");
                RequireText(definition.DefinitionRevision, "SimulationSpatialDefinitionRevisionMissing");
                RequireText(definition.DefinitionHashSha256, "SimulationSpatialDefinitionHashMissing");
                ValidateIds(definition.CapabilityCodes, true, "SimulationSpatialCapabilitiesInvalid");
                ValidateIds(definition.SourceStableIds, true, "SimulationSpatialSourcesInvalid");
                if (!ids.Add(definition.SpatialStableId.Trim()))
                    throw new SimulationContractException("SimulationSpatialDefinitionDuplicate");
                if (definition.BaseCapacities == null)
                    throw new SimulationContractException("SimulationSpatialCapacitiesInvalid");
                var capacityIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var capacity in definition.BaseCapacities)
                {
                    RequireStableId(capacity.CapacityCode, "SimulationSpatialCapacityCodeInvalid");
                    RequireStableId(capacity.UnitCode, "SimulationSpatialCapacityUnitInvalid");
                    if (capacity.Quantity <= 0m || !capacityIds.Add(capacity.CapacityCode.Trim()))
                        throw new SimulationContractException("SimulationSpatialCapacityInvalid");
                }
            }
        }

        internal static string BuildSimulationSpatialPayloadKey(
            Simulation공간세계InitialStateRequest? request)
        {
            if (request == null) return string.Empty;
            return string.Join("\u001e", request.Definitions.OrderBy(
                value => value.SpatialStableId, StringComparer.Ordinal).Select(definition =>
                string.Join("\u001f", new[]
                {
                    definition.SpatialStableId.Trim(), definition.FacilityStableId.Trim(),
                    definition.EvidenceKindCode.Trim(), definition.AccessStateCode.Trim(),
                    string.Join(",", definition.CapabilityCodes.OrderBy(value => value, StringComparer.Ordinal)),
                    string.Join(",", definition.BaseCapacities.OrderBy(value => value.CapacityCode,
                        StringComparer.Ordinal).Select(value => value.CapacityCode + ":"
                        + value.Quantity.ToString(CultureInfo.InvariantCulture) + ":" + value.UnitCode)),
                    definition.DefinitionRevision.Trim(), definition.DefinitionHashSha256.Trim(),
                })));
        }

        internal static Simulation공간세계InitialStateRequest? CloneSimulationSpatialInitialState(
            Simulation공간세계InitialStateRequest? source)
            => source == null ? null : new Simulation공간세계InitialStateRequest
            {
                Definitions = source.Definitions.Select(value => new Simulation공간정의InitialRequest
                {
                    SpatialStableId = value.SpatialStableId,
                    FacilityStableId = value.FacilityStableId,
                    AreaStableId = value.AreaStableId,
                    AreaSetStableId = value.AreaSetStableId,
                    LandscapeGraphStableId = value.LandscapeGraphStableId,
                    LandscapeNodeStableId = value.LandscapeNodeStableId,
                    EvidenceKindCode = value.EvidenceKindCode,
                    AccessStateCode = value.AccessStateCode,
                    CapabilityCodes = value.CapabilityCodes.ToArray(),
                    BaseCapacities = value.BaseCapacities.Select(CloneCapacity).ToArray(),
                    DefinitionRevision = value.DefinitionRevision,
                    DefinitionHashSha256 = value.DefinitionHashSha256,
                    SourceStableIds = value.SourceStableIds.ToArray(),
                }).ToArray(),
            };

        internal static Simulation공간정의Snapshot CloneSpatialDefinition(
            Simulation공간정의InitialRequest source)
            => new Simulation공간정의Snapshot
            {
                SpatialStableId = source.SpatialStableId.Trim(),
                FacilityStableId = source.FacilityStableId.Trim(),
                AreaStableId = source.AreaStableId.Trim(),
                AreaSetStableId = source.AreaSetStableId.Trim(),
                LandscapeGraphStableId = source.LandscapeGraphStableId.Trim(),
                LandscapeNodeStableId = source.LandscapeNodeStableId.Trim(),
                EvidenceKindCode = source.EvidenceKindCode.Trim(),
                AccessStateCode = source.AccessStateCode.Trim(),
                CapabilityCodes = source.CapabilityCodes.Select(value => value.Trim())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                BaseCapacities = source.BaseCapacities.Select(CloneCapacity).ToArray(),
                DefinitionRevision = source.DefinitionRevision.Trim(),
                DefinitionHashSha256 = source.DefinitionHashSha256.Trim(),
                SourceStableIds = source.SourceStableIds.Select(value => value.Trim())
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };

        internal static Simulation공간정의Snapshot CloneSpatialDefinition(
            Simulation공간정의Snapshot source)
            => new Simulation공간정의Snapshot
            {
                SpatialStableId = source.SpatialStableId,
                FacilityStableId = source.FacilityStableId,
                AreaStableId = source.AreaStableId,
                AreaSetStableId = source.AreaSetStableId,
                LandscapeGraphStableId = source.LandscapeGraphStableId,
                LandscapeNodeStableId = source.LandscapeNodeStableId,
                EvidenceKindCode = source.EvidenceKindCode,
                AccessStateCode = source.AccessStateCode,
                CapabilityCodes = source.CapabilityCodes.ToArray(),
                BaseCapacities = source.BaseCapacities.Select(CloneCapacity).ToArray(),
                DefinitionRevision = source.DefinitionRevision,
                DefinitionHashSha256 = source.DefinitionHashSha256,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        internal static Simulation공간실행상태Snapshot CloneSpatialRuntime(
            Simulation공간실행상태Snapshot source)
            => new Simulation공간실행상태Snapshot
            {
                SpatialStableId = source.SpatialStableId,
                AccessStateCode = source.AccessStateCode,
                OccupiedCapacities = source.OccupiedCapacities.Select(CloneCapacity).ToArray(),
                ReservedCapacities = source.ReservedCapacities.Select(CloneCapacity).ToArray(),
                ActiveTaskStableIds = source.ActiveTaskStableIds.ToArray(),
                Revision = source.Revision,
            };

        internal static Simulation공간예약Snapshot CloneSpatialReservation(
            Simulation공간예약Snapshot source)
            => new Simulation공간예약Snapshot
            {
                ReservationStableId = source.ReservationStableId,
                SpatialStableId = source.SpatialStableId,
                TaskStableId = source.TaskStableId,
                RoleCode = source.RoleCode,
                ReservationKindCode = source.ReservationKindCode,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                StatusCode = source.StatusCode,
                ReservedAtTick = source.ReservedAtTick,
                ConsumedAtTick = source.ConsumedAtTick,
                ReleasedAtTick = source.ReleasedAtTick,
                CreatedRevision = source.CreatedRevision,
                FinalizedRevision = source.FinalizedRevision,
            };

        internal static Simulation공간역할BindingSnapshot CloneSpatialRoleBinding(
            Simulation공간역할BindingSnapshot source)
            => new Simulation공간역할BindingSnapshot
            {
                RoleCode = source.RoleCode,
                PreferredSpatialStableId = source.PreferredSpatialStableId,
                SelectedSpatialStableId = source.SelectedSpatialStableId,
                DefinitionRevision = source.DefinitionRevision,
                DefinitionHashSha256 = source.DefinitionHashSha256,
                EvidenceKindCode = source.EvidenceKindCode,
                RequiredCapabilityCodes = source.RequiredCapabilityCodes.ToArray(),
                RequiredCapacities = source.RequiredCapacities.Select(CloneCapacity).ToArray(),
                BlockReasonCodes = source.BlockReasonCodes.ToArray(),
            };

        private static Simulation공간용량Snapshot CloneCapacity(Simulation공간용량Snapshot source)
            => Capacity(source.CapacityCode, source.Quantity, source.UnitCode);

        private static Simulation공간용량Snapshot Capacity(string code, decimal quantity, string unit)
            => new Simulation공간용량Snapshot
            {
                CapacityCode = code,
                Quantity = quantity,
                UnitCode = unit,
            };

        private static Simulation공간용량Snapshot? FindCapacity(
            Simulation공간용량Snapshot[] values,
            Simulation공간용량Snapshot required)
            => values.FirstOrDefault(value => value.CapacityCode == required.CapacityCode
                && value.UnitCode == required.UnitCode);

        private static decimal CapacityQuantity(
            Simulation공간용량Snapshot[] values,
            Simulation공간용량Snapshot required)
            => FindCapacity(values, required)?.Quantity ?? 0m;

        private static void AddCapacity(
            Simulation공간용량Snapshot[] values,
            string code,
            decimal delta,
            string unit)
        {
            var value = values.Single(item => item.CapacityCode == code && item.UnitCode == unit);
            value.Quantity += delta;
        }

        private SimulationValueProjection[] CreateSpatialExpectedEffects(
            Simulation공간정의Snapshot definition,
            Simulation공간Rule rule,
            string[] sources)
        {
            var runtime = spatialRuntimeStates[definition.SpatialStableId];
            return rule.Capacities.Select(required =>
            {
                var isStorage = required.CapacityCode == Simulation공간용량Codes.StorageCapacity;
                var isRestorationMaterial = required.CapacityCode ==
                    Simulation공간용량Codes.RestorationMaterial;
                var isRecoverySupply = required.CapacityCode ==
                    Simulation공간용량Codes.RecoverySupply;
                var isConsumable = isStorage || isRestorationMaterial || isRecoverySupply;
                var before = isConsumable ? CapacityQuantity(runtime.OccupiedCapacities, required) : 1m;
                var delta = isConsumable ? required.Quantity : -1m;
                return new SimulationValueProjection
                {
                    ValueTypeCode = isStorage
                        ? 공간보관용량사용EffectCode
                        : isRestorationMaterial
                            ? 공간복원자재사용EffectCode
                            : isRecoverySupply
                                ? 공간회복보급사용EffectCode
                            : 공간작업영역해제EffectCode,
                    TargetLedgerStableId = definition.SpatialStableId,
                    BeforeValue = before,
                    Delta = delta,
                    AfterValue = before + delta,
                    UnitCode = required.UnitCode,
                    SourceStableIds = MergeSources(sources, definition.SourceStableIds),
                };
            }).ToArray();
        }

        private static Simulation공간Rule? ResolveSpatialRule(
            string actionCode,
            decimal assignedCapacity,
            string assignedCapacityUnitCode)
        {
            if (actionCode == SimulationNpcActionCodes.WarehouseInboundInspection)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.InspectionWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationNpcActionCodes.WarehouseStorageMove)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.Storage,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.LoadingWorkArea,
                    },
                    new[]
                    {
                        Capacity(Simulation공간용량Codes.StorageCapacity, assignedCapacity,
                            assignedCapacityUnitCode),
                        Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot"),
                    });
            }
            if (actionCode == SimulationFarmSurvivalCodes.Harvesting)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CropProduction,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.HarvestWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationFarmSurvivalCodes.Tilling)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.TillingWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationFarmSurvivalCodes.Sowing)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CropProduction,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.SowingWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationFarmSurvivalCodes.CropCare)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CropProduction,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.WaterAccessible,
                        Simulation공간능력Codes.CropCareWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationFarmSurvivalCodes.HarvestCollection)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CollectionWorkArea,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationFarmSurvivalCodes.OutboundPacking)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.PackingWorkArea,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationFarmSurvivalCodes.FenceRepair)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.RepairWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationSupplyChainActionCodes.WarehouseOutboundFlow)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.Storage,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.PickingWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationSupplyChainActionCodes.MarketInspection)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.InspectionWorkArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationSupplyChainActionCodes.MarketBackroomPutAway)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.Storage,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.LoadingWorkArea,
                    },
                    new[]
                    {
                        Capacity(Simulation공간용량Codes.StorageCapacity,
                            assignedCapacity, assignedCapacityUnitCode),
                        Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot"),
                    });
            }
            if (actionCode == SimulationSupplyChainActionCodes.MarketDisplayReplenishment)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.DisplayArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == ResidentOrderPickupActionCode)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.CustomerAccessible,
                        Simulation공간능력Codes.PickupArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationNatureInteractionCodes.RegionalThreatObservation)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.Traversable,
                        Simulation공간능력Codes.ObservationArea,
                        Simulation공간능력Codes.ThreatMonitoringArea,
                    },
                    new[] { Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot") });
            }
            if (actionCode == SimulationNatureInteractionCodes.EmergencyRetreat)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.Traversable,
                        Simulation공간능력Codes.EmergencyAccess,
                        Simulation공간능력Codes.PlayerEscapeRoute,
                        Simulation공간능력Codes.SafeCore,
                    },
                    new[] { Capacity(Simulation공간용량Codes.EscapeRouteCapacity, 1m, "party") });
            }
            if (actionCode == SimulationNatureInteractionCodes.NatureRestoration)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.RestorationWorkArea,
                    },
                    new[]
                    {
                        Capacity(Simulation공간용량Codes.WorkArea, 1m, "slot"),
                        Capacity(Simulation공간용량Codes.RestorationMaterial, 1m, "material-lot"),
                    });
            }
            if (actionCode == SimulationNatureInteractionCodes.PartyRecovery)
            {
                return new Simulation공간Rule(
                    new[]
                    {
                        Simulation공간능력Codes.Traversable,
                        Simulation공간능력Codes.RestArea,
                        Simulation공간능력Codes.SafeCore,
                    },
                    new[]
                    {
                        Capacity(Simulation공간용량Codes.RestAreaParty, 1m, "party"),
                        Capacity(Simulation공간용량Codes.RecoverySupply, 1m, "supply-lot"),
                    });
            }
            return null;
        }

        private static bool IsConsumableCapacity(string capacityCode)
            => capacityCode == Simulation공간용량Codes.StorageCapacity
                || capacityCode == Simulation공간용량Codes.RestorationMaterial
                || capacityCode == Simulation공간용량Codes.RecoverySupply;

        private sealed class Simulation공간Rule
        {
            public Simulation공간Rule(
                string[] requiredCapabilities,
                Simulation공간용량Snapshot[] capacities)
            {
                RequiredCapabilities = requiredCapabilities;
                Capacities = capacities;
            }

            public string[] RequiredCapabilities { get; }
            public Simulation공간용량Snapshot[] Capacities { get; }
        }
    }
}
