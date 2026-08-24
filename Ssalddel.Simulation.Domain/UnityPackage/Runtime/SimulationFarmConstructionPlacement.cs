using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const int ConstructionPlacementFineStepCentimeters = 25;
        private readonly Dictionary<string, SimulationConstructionPlacementZoneRequest>
            integratedConstructionPlacementZones = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ConstructionPlacementProposal>
            integratedConstructionPlacementProposals = new(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedConstructionPlacementCommand>
            appliedConstructionPlacementCommands = new(StringComparer.Ordinal);

        private void InitializeConstructionPlacementZones(
            IEnumerable<SimulationConstructionPlacementZoneRequest> source)
        {
            foreach (var zone in (source ?? Array.Empty<SimulationConstructionPlacementZoneRequest>())
                         .OrderBy(value => value.PlacementZoneStableId, StringComparer.Ordinal))
            {
                RequireStableId(zone.PlacementZoneStableId,
                    "SimulationConstructionPlacementZoneStableIdInvalid");
                RequireStableId(zone.TargetH2StableId,
                    "SimulationConstructionPlacementTargetH2Invalid");
                RequireStableId(zone.ZoneTypeCode,
                    "SimulationConstructionPlacementZoneTypeInvalid");
                RequireStableId(zone.PlacementProfileRevision,
                    "SimulationConstructionPlacementProfileRevisionInvalid");
                if (zone.MinXCentimeters >= zone.MaxXCentimeters
                    || zone.MinZCentimeters >= zone.MaxZCentimeters
                    || zone.TerrainSlopeMilliDegrees < 0)
                    throw new SimulationContractException(
                        "SimulationConstructionPlacementZoneGeometryInvalid");
                if (!integratedConstructionPlacementZones.TryAdd(
                        zone.PlacementZoneStableId, ClonePlacementZone(zone)))
                    throw new SimulationContractException(
                        "SimulationConstructionPlacementZoneDuplicate");
            }
        }

        public SimulationFarmConstructionPlacementPreviewSnapshot
            PreviewFarmConstructionPlacement(
                SimulationFarmConstructionPlacementPreviewRequest request)
        {
            ValidatePlacementPreviewRequest(request);
            lock (gate)
            {
                var payload = NormalizePlacementPayload(request);
                var command = new SimulationIntegratedWorldCommandRequest
                {
                    ActionCode = SimulationIntegratedWorldActionCodes.ConstructionOrder,
                    CommandId = "placement-preview",
                    ExpectedRevision = request.ExpectedRevision,
                    Construction = payload,
                };
                var standard = BuildIntegratedWorldPreview(command);
                var blocks = standard.BlockingReasonCodes.ToList();
                if (request.ExpectedRevision != Revision)
                    blocks.Add("SimulationExpectedRevisionMismatch");

                var blueprint = integratedBlueprints.TryGetValue(payload.BlueprintStableId,
                    out var resolvedBlueprint) ? resolvedBlueprint : null;
                var proposalId = BuildPlacementProposalStableId(payload, Revision);
                payload.PlacementProposalStableId = proposalId;
                var result = new SimulationFarmConstructionPlacementPreviewSnapshot
                {
                    PlacementProposalStableId = proposalId,
                    SourceWorldRevision = Revision,
                    CanConfirm = blocks.Count == 0,
                    BlockingReasonCodes = blocks.Distinct(StringComparer.Ordinal).ToArray(),
                    BlueprintStableId = payload.BlueprintStableId,
                    PlacementKindCode = payload.PlacementKindCode,
                    PlacementZoneStableId = payload.PlacementZoneStableId,
                    TargetH2StableId = payload.TargetH2StableId,
                    LocalXCentimeters = payload.LocalXCentimeters,
                    LocalZCentimeters = payload.LocalZCentimeters,
                    RotationQuarterTurns = payload.RotationQuarterTurns,
                    FootprintWidthCentimeters = RotatedWidth(blueprint, payload),
                    FootprintDepthCentimeters = RotatedDepth(blueprint, payload),
                    AccessConnectorStableId = payload.AccessConnectorStableId,
                    FenceChainStableId = payload.FenceChainStableId,
                    PlacementProfileRevision = payload.PlacementProfileRevision,
                    DevelopmentOpportunityStableId =
                        payload.DevelopmentOpportunityStableId,
                    MaterialRequirements = blueprint == null
                        ? Array.Empty<SimulationIntegratedItemRequirement>()
                        : CloneRequirements(blueprint.Materials),
                    ReservedMaterialLotStableIds = standard.ReservedLotStableIds.ToArray(),
                    SelectedActorStableIds = standard.SelectedActorStableIds.ToArray(),
                    ConstructionTicks = blueprint?.ConstructionTicks ?? 0,
                };
                result.PreviewHashSha256 = BuildPlacementPreviewHash(result);
                payload.PlacementPreviewHashSha256 = result.PreviewHashSha256;
                integratedConstructionPlacementProposals[proposalId] =
                    new ConstructionPlacementProposal(CloneConstructionPayload(payload),
                        ClonePlacementPreview(result));
                return ClonePlacementPreview(result);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmFarmConstructionPlacement(
            SimulationFarmConstructionPlacementConfirmRequest request)
        {
            ValidatePlacementConfirmRequest(request);
            lock (gate)
            {
                if (appliedConstructionPlacementCommands.TryGetValue(request.CommandId,
                        out var applied))
                {
                    if (!string.Equals(applied.ProposalStableId,
                            request.PlacementProposalStableId, StringComparison.Ordinal)
                        || !string.Equals(applied.PreviewHashSha256,
                            request.PreviewHashSha256, StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                if (!integratedConstructionPlacementProposals.TryGetValue(
                        request.PlacementProposalStableId, out var proposal))
                    throw new SimulationConflictException(
                        "SimulationConstructionPlacementProposalUnavailable");
                if (!string.Equals(proposal.Preview.PreviewHashSha256,
                        request.PreviewHashSha256, StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationConstructionPlacementPreviewHashMismatch");
                if (proposal.Preview.SourceWorldRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationConstructionPlacementProposalStale");
                if (!proposal.Preview.CanConfirm)
                    throw new SimulationConflictException(
                        proposal.Preview.BlockingReasonCodes.FirstOrDefault()
                        ?? "SimulationConstructionPlacementBlocked");

                var command = new SimulationIntegratedWorldCommandRequest
                {
                    ActionCode = SimulationIntegratedWorldActionCodes.ConstructionOrder,
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Construction = CloneConstructionPayload(proposal.Payload),
                };
                return ConfirmIntegratedWorldCommand(command);
            }
        }

        private void AppendDynamicConstructionPlacementBlocks(
            SimulationConstructionOrderPayload payload,
            SimulationFacilityBlueprintRequest blueprint,
            ICollection<string> blocks)
        {
            if (string.IsNullOrWhiteSpace(payload.PlacementZoneStableId)) return;
            if (!integratedConstructionPlacementZones.TryGetValue(
                    payload.PlacementZoneStableId, out var zone))
            {
                blocks.Add("SimulationConstructionPlacementZoneNotFound");
                return;
            }
            if (!string.Equals(zone.TargetH2StableId, payload.TargetH2StableId,
                    StringComparison.Ordinal))
                blocks.Add("SimulationConstructionPlacementH2Mismatch");
            if (!string.Equals(zone.PlacementProfileRevision,
                    payload.PlacementProfileRevision, StringComparison.Ordinal))
                blocks.Add("SimulationConstructionPlacementProfileRevisionMismatch");
            if (blueprint.AllowedPlacementZoneTypeCodes.Length == 0
                || !blueprint.AllowedPlacementZoneTypeCodes.Contains(zone.ZoneTypeCode,
                    StringComparer.Ordinal))
                blocks.Add("SimulationConstructionPlacementZoneTypeBlocked");
            if (zone.TerrainSlopeMilliDegrees > blueprint.MaxSlopeMilliDegrees)
                blocks.Add("SimulationConstructionPlacementSlopeBlocked");

            var width = RotatedWidth(blueprint, payload) + blueprint.ClearanceCentimeters * 2;
            var depth = RotatedDepth(blueprint, payload) + blueprint.ClearanceCentimeters * 2;
            if (!WithinBounds(payload.LocalXCentimeters, payload.LocalZCentimeters,
                    width, depth, zone))
                blocks.Add("SimulationConstructionPlacementOutsideZone");
            if (OverlapsExistingPlacement(payload, width, depth))
                blocks.Add("SimulationConstructionPlacementOccupied");

            if (blueprint.RequiresRoadAccess
                && (string.IsNullOrWhiteSpace(payload.AccessConnectorStableId)
                    || !zone.RoadAccessConnectorStableIds.Contains(
                        payload.AccessConnectorStableId, StringComparer.Ordinal)))
                blocks.Add("SimulationConstructionPlacementRoadAccessRequired");

            if (IsFence(blueprint.PlacementKindCode))
            {
                if (!string.Equals(zone.FenceChainStableId, payload.FenceChainStableId,
                        StringComparison.Ordinal))
                    blocks.Add("SimulationConstructionFenceChainMismatch");
                if (blueprint.PlacementKindCode ==
                        SimulationConstructionPlacementKindCodes.FenceGate)
                {
                    if (zone.ZoneTypeCode !=
                        SimulationConstructionPlacementZoneTypeCodes.FarmEntrance)
                        blocks.Add("SimulationConstructionFenceGateEntranceRequired");
                }
                else if (zone.ZoneTypeCode !=
                         SimulationConstructionPlacementZoneTypeCodes.FarmFenceEdge)
                    blocks.Add("SimulationConstructionFenceEntranceMustRemainOpen");
                if (!FenceTouchesChain(payload, blueprint, zone))
                    blocks.Add("SimulationConstructionFenceNotContinuous");
            }
        }

        private SimulationConstructionOrderPayload NormalizePlacementPayload(
            SimulationFarmConstructionPlacementPreviewRequest request)
        {
            var blueprint = integratedBlueprints.TryGetValue(request.BlueprintStableId,
                out var value) ? value : null;
            var zone = integratedConstructionPlacementZones.TryGetValue(
                request.PlacementZoneStableId, out var resolvedZone) ? resolvedZone : null;
            var x = Snap(request.LocalXCentimeters);
            var z = Snap(request.LocalZCentimeters);
            var rotation = ((request.RotationQuarterTurns % 4) + 4) % 4;
            var canonical = string.Join("|", request.BlueprintStableId,
                request.PlacementZoneStableId, request.TargetH2StableId,
                x, z, rotation, request.AccessConnectorStableId,
                request.FenceChainStableId, Revision);
            if (!string.IsNullOrWhiteSpace(request.DevelopmentOpportunityStableId))
                canonical += "|" + request.DevelopmentOpportunityStableId.Trim();
            return new SimulationConstructionOrderPayload
            {
                BlueprintStableId = request.BlueprintStableId.Trim(),
                BuildSiteH1StableId = "h1:Farm:player-placement:"
                    + HashIntegrated(canonical)[..20],
                PlacementZoneStableId = request.PlacementZoneStableId.Trim(),
                TargetH2StableId = request.TargetH2StableId.Trim(),
                PlacementKindCode = blueprint?.PlacementKindCode ?? string.Empty,
                LocalXCentimeters = x,
                LocalZCentimeters = z,
                RotationQuarterTurns = rotation,
                AccessConnectorStableId = request.AccessConnectorStableId.Trim(),
                FenceChainStableId = request.FenceChainStableId.Trim(),
                PlacementProfileRevision = zone?.PlacementProfileRevision ?? string.Empty,
                DevelopmentOpportunityStableId =
                    request.DevelopmentOpportunityStableId.Trim(),
            };
        }

        private bool OverlapsExistingPlacement(SimulationConstructionOrderPayload payload,
            int width, int depth)
        {
            foreach (var facility in integratedFacilities.Values.Where(value =>
                         value.LifecycleCode != SimulationFacilityLifecycleCodes.Removed
                         && value.PlacementZoneStableId.Length > 0))
            {
                var project = integratedConstructionProjects.Values.FirstOrDefault(value =>
                    value.TargetFacilityStableId == facility.FacilityStableId);
                if (project == null || !integratedBlueprints.TryGetValue(
                        project.BlueprintStableId, out var blueprint)) continue;
                var otherWidth = RotatedWidth(blueprint, project.RotationQuarterTurns)
                    + blueprint.ClearanceCentimeters * 2;
                var otherDepth = RotatedDepth(blueprint, project.RotationQuarterTurns)
                    + blueprint.ClearanceCentimeters * 2;
                if (RectanglesOverlap(payload.LocalXCentimeters, payload.LocalZCentimeters,
                        width, depth, facility.LocalXCentimeters,
                        facility.LocalZCentimeters, otherWidth, otherDepth))
                    return true;
            }
            return false;
        }

        private bool FenceTouchesChain(SimulationConstructionOrderPayload payload,
            SimulationFacilityBlueprintRequest blueprint,
            SimulationConstructionPlacementZoneRequest zone)
        {
            var endpoints = FenceEndpoints(payload.LocalXCentimeters,
                payload.LocalZCentimeters, RotatedWidth(blueprint, payload),
                RotatedDepth(blueprint, payload));
            var existing = integratedConstructionProjects.Values.Where(project =>
                    project.FenceChainStableId == payload.FenceChainStableId
                    && project.StateCode != SimulationConstructionProjectStateCodes.Cancelled)
                .ToArray();
            if (existing.Length == 0)
            {
                if (!zone.FenceStartXCentimeters.HasValue
                    || !zone.FenceStartZCentimeters.HasValue) return false;
                return endpoints.Any(point => Near(point.X, point.Z,
                    zone.FenceStartXCentimeters.Value,
                    zone.FenceStartZCentimeters.Value));
            }
            foreach (var project in existing)
            {
                if (!integratedBlueprints.TryGetValue(project.BlueprintStableId,
                        out var existingBlueprint)) continue;
                var other = FenceEndpoints(project.LocalXCentimeters,
                    project.LocalZCentimeters,
                    RotatedWidth(existingBlueprint, project.RotationQuarterTurns),
                    RotatedDepth(existingBlueprint, project.RotationQuarterTurns));
                if (endpoints.Any(point => other.Any(candidate =>
                        Near(point.X, point.Z, candidate.X, candidate.Z))))
                    return true;
            }
            return false;
        }

        private void ApplyDynamicPlacement(SimulationRuntimeFacilitySnapshot facility,
            SimulationConstructionOrderPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.PlacementZoneStableId)) return;
            facility.PlacementZoneStableId = payload.PlacementZoneStableId;
            facility.TargetH2StableId = payload.TargetH2StableId;
            facility.PlacementKindCode = payload.PlacementKindCode;
            facility.LocalXCentimeters = payload.LocalXCentimeters;
            facility.LocalZCentimeters = payload.LocalZCentimeters;
            facility.RotationQuarterTurns = payload.RotationQuarterTurns;
            facility.PlacementProfileRevision = payload.PlacementProfileRevision;
            facility.FenceChainStableId = payload.FenceChainStableId;
            facility.AccessConnectorStableIds = string.IsNullOrWhiteSpace(
                payload.AccessConnectorStableId)
                ? Array.Empty<string>()
                : new[] { payload.AccessConnectorStableId };
        }

        private void RecordAppliedConstructionPlacement(
            SimulationIntegratedWorldCommandRequest request,
            경영SimulationSessionSnapshot snapshot)
        {
            var placement = request.Construction;
            if (placement == null
                || string.IsNullOrWhiteSpace(placement.PlacementProposalStableId)) return;
            appliedConstructionPlacementCommands[request.CommandId] =
                new AppliedConstructionPlacementCommand(
                    placement.PlacementProposalStableId,
                    placement.PlacementPreviewHashSha256,
                    Clone(snapshot));
        }

        private static void ValidatePlacementPreviewRequest(
            SimulationFarmConstructionPlacementPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.BlueprintStableId,
                "SimulationFacilityBlueprintStableIdInvalid");
            RequireStableId(request.PlacementZoneStableId,
                "SimulationConstructionPlacementZoneStableIdInvalid");
            RequireStableId(request.TargetH2StableId,
                "SimulationConstructionPlacementTargetH2Invalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            if (!string.IsNullOrWhiteSpace(request.DevelopmentOpportunityStableId))
                RequireStableId(request.DevelopmentOpportunityStableId,
                    "SimulationRegionalDevelopmentOpportunityStableIdInvalid");
        }

        private static void ValidatePlacementConfirmRequest(
            SimulationFarmConstructionPlacementConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            RequireStableId(request.PlacementProposalStableId,
                "SimulationConstructionPlacementProposalStableIdInvalid");
            RequireStableId(request.PreviewHashSha256,
                "SimulationConstructionPlacementPreviewHashInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
        }

        private static string BuildPlacementProposalStableId(
            SimulationConstructionOrderPayload payload, long revision)
        {
            var canonical = string.Join("|", revision,
                payload.BlueprintStableId, payload.PlacementZoneStableId,
                payload.TargetH2StableId, payload.LocalXCentimeters,
                payload.LocalZCentimeters, payload.RotationQuarterTurns,
                payload.AccessConnectorStableId, payload.FenceChainStableId,
                payload.PlacementProfileRevision);
            if (!string.IsNullOrWhiteSpace(payload.DevelopmentOpportunityStableId))
                canonical += "|" + payload.DevelopmentOpportunityStableId;
            return "placement-proposal:" + HashIntegrated(canonical)[..24];
        }

        private static string BuildPlacementPreviewHash(
            SimulationFarmConstructionPlacementPreviewSnapshot value)
        {
            var canonical = string.Join("|", value.PlacementProposalStableId,
                value.SourceWorldRevision, value.CanConfirm,
                string.Join(",", value.BlockingReasonCodes), value.BlueprintStableId,
                value.PlacementKindCode, value.PlacementZoneStableId,
                value.TargetH2StableId, value.LocalXCentimeters,
                value.LocalZCentimeters, value.RotationQuarterTurns,
                value.FootprintWidthCentimeters, value.FootprintDepthCentimeters,
                value.AccessConnectorStableId, value.FenceChainStableId,
                value.PlacementProfileRevision,
                string.Join(",", value.MaterialRequirements.Select(item =>
                    string.Join(":", item.ItemCode, item.Quantity, item.UnitCode))),
                string.Join(",", value.ReservedMaterialLotStableIds),
                string.Join(",", value.SelectedActorStableIds),
                value.ConstructionTicks);
            if (!string.IsNullOrWhiteSpace(value.DevelopmentOpportunityStableId))
                canonical += "|" + value.DevelopmentOpportunityStableId;
            return HashIntegrated(canonical);
        }

        private static SimulationConstructionOrderPayload CloneConstructionPayload(
            SimulationConstructionOrderPayload value) => new()
        {
            BlueprintStableId = value.BlueprintStableId,
            BuildSiteH1StableId = value.BuildSiteH1StableId,
            PlacementProposalStableId = value.PlacementProposalStableId,
            PlacementPreviewHashSha256 = value.PlacementPreviewHashSha256,
            PlacementZoneStableId = value.PlacementZoneStableId,
            TargetH2StableId = value.TargetH2StableId,
            PlacementKindCode = value.PlacementKindCode,
            LocalXCentimeters = value.LocalXCentimeters,
            LocalZCentimeters = value.LocalZCentimeters,
            RotationQuarterTurns = value.RotationQuarterTurns,
            AccessConnectorStableId = value.AccessConnectorStableId,
            FenceChainStableId = value.FenceChainStableId,
            PlacementProfileRevision = value.PlacementProfileRevision,
            DevelopmentOpportunityStableId = value.DevelopmentOpportunityStableId,
        };

        private static SimulationFarmConstructionPlacementPreviewSnapshot ClonePlacementPreview(
            SimulationFarmConstructionPlacementPreviewSnapshot value) => new()
        {
            PlacementProposalStableId = value.PlacementProposalStableId,
            SourceWorldRevision = value.SourceWorldRevision,
            CanConfirm = value.CanConfirm,
            BlockingReasonCodes = value.BlockingReasonCodes.ToArray(),
            BlueprintStableId = value.BlueprintStableId,
            PlacementKindCode = value.PlacementKindCode,
            PlacementZoneStableId = value.PlacementZoneStableId,
            TargetH2StableId = value.TargetH2StableId,
            LocalXCentimeters = value.LocalXCentimeters,
            LocalZCentimeters = value.LocalZCentimeters,
            RotationQuarterTurns = value.RotationQuarterTurns,
            FootprintWidthCentimeters = value.FootprintWidthCentimeters,
            FootprintDepthCentimeters = value.FootprintDepthCentimeters,
            AccessConnectorStableId = value.AccessConnectorStableId,
            FenceChainStableId = value.FenceChainStableId,
            PlacementProfileRevision = value.PlacementProfileRevision,
            DevelopmentOpportunityStableId = value.DevelopmentOpportunityStableId,
            MaterialRequirements = CloneRequirements(value.MaterialRequirements),
            ReservedMaterialLotStableIds = value.ReservedMaterialLotStableIds.ToArray(),
            SelectedActorStableIds = value.SelectedActorStableIds.ToArray(),
            ConstructionTicks = value.ConstructionTicks,
            PreviewHashSha256 = value.PreviewHashSha256,
        };

        private static SimulationConstructionPlacementZoneRequest ClonePlacementZone(
            SimulationConstructionPlacementZoneRequest value) => new()
        {
            PlacementZoneStableId = value.PlacementZoneStableId,
            TargetH2StableId = value.TargetH2StableId,
            ZoneTypeCode = value.ZoneTypeCode,
            PlacementProfileRevision = value.PlacementProfileRevision,
            MinXCentimeters = value.MinXCentimeters,
            MaxXCentimeters = value.MaxXCentimeters,
            MinZCentimeters = value.MinZCentimeters,
            MaxZCentimeters = value.MaxZCentimeters,
            TerrainSlopeMilliDegrees = value.TerrainSlopeMilliDegrees,
            RoadAccessConnectorStableIds = value.RoadAccessConnectorStableIds.ToArray(),
            FenceChainStableId = value.FenceChainStableId,
            FenceStartXCentimeters = value.FenceStartXCentimeters,
            FenceStartZCentimeters = value.FenceStartZCentimeters,
        };

        private static int Snap(int value)
            => (int)(Math.Round(value / (decimal)ConstructionPlacementFineStepCentimeters,
                MidpointRounding.AwayFromZero)
                * ConstructionPlacementFineStepCentimeters);

        private static int RotatedWidth(SimulationFacilityBlueprintRequest? blueprint,
            SimulationConstructionOrderPayload payload)
            => blueprint == null ? 0 : RotatedWidth(blueprint,
                payload.RotationQuarterTurns);

        private static int RotatedDepth(SimulationFacilityBlueprintRequest? blueprint,
            SimulationConstructionOrderPayload payload)
            => blueprint == null ? 0 : RotatedDepth(blueprint,
                payload.RotationQuarterTurns);

        private static int RotatedWidth(SimulationFacilityBlueprintRequest blueprint,
            int rotationQuarterTurns)
            => rotationQuarterTurns % 2 == 0
                ? blueprint.FootprintWidthCentimeters
                : blueprint.FootprintDepthCentimeters;

        private static int RotatedDepth(SimulationFacilityBlueprintRequest blueprint,
            int rotationQuarterTurns)
            => rotationQuarterTurns % 2 == 0
                ? blueprint.FootprintDepthCentimeters
                : blueprint.FootprintWidthCentimeters;

        private static bool WithinBounds(int x, int z, int width, int depth,
            SimulationConstructionPlacementZoneRequest zone)
            => x - width / 2 >= zone.MinXCentimeters
               && x + width / 2 <= zone.MaxXCentimeters
               && z - depth / 2 >= zone.MinZCentimeters
               && z + depth / 2 <= zone.MaxZCentimeters;

        private static bool RectanglesOverlap(int x1, int z1, int width1, int depth1,
            int x2, int z2, int width2, int depth2)
            => Math.Abs(x1 - x2) * 2 < width1 + width2
               && Math.Abs(z1 - z2) * 2 < depth1 + depth2;

        private static bool IsFence(string kind)
            => kind == SimulationConstructionPlacementKindCodes.FenceSegment
               || kind == SimulationConstructionPlacementKindCodes.FenceCorner
               || kind == SimulationConstructionPlacementKindCodes.FenceGate;

        private static (int X, int Z)[] FenceEndpoints(int x, int z, int width, int depth)
            => width >= depth
                ? new[] { (x - width / 2, z), (x + width / 2, z) }
                : new[] { (x, z - depth / 2), (x, z + depth / 2) };

        private static bool Near(int x1, int z1, int x2, int z2)
            => Math.Abs(x1 - x2) <= ConstructionPlacementFineStepCentimeters
               && Math.Abs(z1 - z2) <= ConstructionPlacementFineStepCentimeters;

        private sealed class ConstructionPlacementProposal
        {
            public ConstructionPlacementProposal(SimulationConstructionOrderPayload payload,
                SimulationFarmConstructionPlacementPreviewSnapshot preview)
            {
                Payload = payload;
                Preview = preview;
            }

            public SimulationConstructionOrderPayload Payload { get; }
            public SimulationFarmConstructionPlacementPreviewSnapshot Preview { get; }
        }

        private sealed class AppliedConstructionPlacementCommand
        {
            public AppliedConstructionPlacementCommand(string proposalStableId,
                string previewHashSha256, 경영SimulationSessionSnapshot snapshot)
            {
                ProposalStableId = proposalStableId;
                PreviewHashSha256 = previewHashSha256;
                Snapshot = snapshot;
            }

            public string ProposalStableId { get; }
            public string PreviewHashSha256 { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
