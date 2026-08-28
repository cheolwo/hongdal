using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public sealed class SimulationAreaSetHandoverPlanRequest
    {
        public string RequestEpoch { get; set; } = string.Empty;
        public string FocusL3CellKey { get; set; } = string.Empty;
        public string MovementDirectionCode { get; set; } = SimulationLhWorldCodes.None;
        public string CurrentAreaSetStableId { get; set; } = string.Empty;
        public SimulationPlayerAreaAccessStateSnapshot? AreaAccess { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
        Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
    public interface ISimulationAreaSetHandoverPlanner
    {
        SimulationAreaSetHandoverPlanResponse Plan(
            SimulationAreaSetHandoverPlanRequest request);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Application,
        "H5 물리 회랑과 이동 방향을 이용해 다음 AreaSet의 준비 깊이를 결정한다.",
        StepKey = "application.area-set-handover-plan",
        DependsOnStepKeys = new[] { "contract.area-set-handover-plan", "contract.world-layout-definition" },
        ExecutionStage = SsalddelCodeExecutionStage.Preview,
        ReadsFrom = SsalddelCodeDataScope.SimulationState | SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 30,
        Boundary = "준비 우선순위만 계산하며 자료 상주 상태, 현재 AreaSet, 접근 권한과 WorldTick을 확정하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
        Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
    public sealed class SimulationAreaSetHandoverPlanner :
        ISimulationAreaSetHandoverPlanner
    {
        private readonly SimulationWorldLayoutCatalog catalog;

        public SimulationAreaSetHandoverPlanner(
            ISimulationWorldLayoutCatalogReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (!reader.TryRead(out catalog, out var errorCode))
                throw new InvalidOperationException(errorCode);
        }

        public SimulationAreaSetHandoverPlanResponse Plan(
            SimulationAreaSetHandoverPlanRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.RequestEpoch)
                || !SimulationLhWorldService.TryParseL3CellKey(
                    request.FocusL3CellKey, out var cellX, out var cellY))
                throw new ArgumentException("AreaSetHandoverRequestInvalid", nameof(request));

            var definition = catalog.Definition;
            var focusX = (cellX - SimulationLhWorldService.CenterL3X)
                         * SimulationLhWorldService.L3CellSizeMeters;
            var focusZ = (cellY - SimulationLhWorldService.CenterL3Y)
                         * SimulationLhWorldService.L3CellSizeMeters;
            var current = ResolveCurrentAreaSet(
                request.CurrentAreaSetStableId, focusX, focusZ, definition);
            var direction = Direction(request.MovementDirectionCode);
            var accessByArea = (request.AreaAccess?.AccessEntries
                                ?? Array.Empty<SimulationPlayerAreaAccessSnapshot>())
                .GroupBy(value => value.AreaSetStableId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(),
                    StringComparer.Ordinal);

            var physicalCandidates = definition.CorridorInstances
                .Where(value => value.FromAreaSetInstanceStableId == current
                                || value.ToAreaSetInstanceStableId == current)
                .Select(corridor => CreateCandidate(
                    corridor, current, focusX, focusZ, direction,
                    request.MovementDirectionCode, definition, accessByArea));
            var reservedCandidates = definition.ReservedConnections
                .Where(value => value.FromAreaSetInstanceStableId == current
                                || value.ToAreaSetInstanceStableId == current)
                .Select(connection => CreateReservedCandidate(
                    connection, current, focusX, focusZ, direction,
                    request.MovementDirectionCode, definition, accessByArea));
            var candidates = physicalCandidates
                .Concat(reservedCandidates)
                .OrderByDescending(value => value.HeadingAlignment01)
                .ThenBy(value => value.DistanceToTransitionMeters)
                .ThenBy(value => value.TargetAreaSetStableId, StringComparer.Ordinal)
                .ToArray();
            for (var index = 0; index < candidates.Length; index++)
                candidates[index].Priority = index;

            var response = new SimulationAreaSetHandoverPlanResponse
            {
                RequestEpoch = request.RequestEpoch.Trim(),
                CurrentAreaSetStableId = current,
                FocusL3CellKey = request.FocusL3CellKey,
                MovementDirectionCode = request.MovementDirectionCode,
                WorldLayoutStableId = definition.WorldLayoutStableId,
                WorldLayoutRevision = definition.WorldLayoutRevision,
                WorldLayoutHashSha256 = definition.WorldLayoutHashSha256,
                WorldGroundingStateCode = catalog.GroundingBinding.WorldGroundingStateCode,
                AvailabilityCode = SimulationAreaSetHandoverCodes.H5Known,
                BlockingReasonCodes = Array.Empty<string>(),
                Candidates = candidates,
                ChangesCurrentAreaSet = false,
                RequiresExplicitTraversalConfirm = true,
                IsCandidateOnly = true,
                IsOperationalState = false,
            };
            response.PlanHashSha256 = Hash(string.Join("|", new[]
            {
                response.PlannerRevision,
                response.RequestEpoch,
                response.CurrentAreaSetStableId,
                response.FocusL3CellKey,
                response.MovementDirectionCode,
                response.WorldLayoutHashSha256,
                response.AvailabilityCode,
                string.Join(",", candidates.Select(value => value.CandidateHashSha256)),
            }));
            return response;
        }

        private static SimulationAreaSetHandoverCandidateResponse CreateReservedCandidate(
            SimulationWorldReservedConnectionResponse connection,
            string current,
            double focusX,
            double focusZ,
            (double X, double Z) direction,
            string directionCode,
            SimulationWorldLayoutDefinitionResponse definition,
            IReadOnlyDictionary<string, SimulationPlayerAreaAccessSnapshot> accessByArea)
        {
            var target = connection.FromAreaSetInstanceStableId == current
                ? connection.ToAreaSetInstanceStableId
                : connection.FromAreaSetInstanceStableId;
            var anchor = definition.AreaAnchors.Single(value =>
                value.AreaSetStableId == target);
            if (anchor.PlacementStateCode != SimulationWorldLayoutCodes.Reserved
                || anchor.CanTraverse || anchor.CanActivate)
                throw new InvalidOperationException("AreaSetHandoverReservedAnchorInvalid");

            var deltaX = anchor.FixedPlacementTransform.LocalXMeters - focusX;
            var deltaZ = anchor.FixedPlacementTransform.LocalZMeters - focusZ;
            var distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
            var alignment = directionCode == SimulationLhWorldCodes.None || distance < .000001d
                ? .5d
                : Math.Clamp((deltaX / distance * direction.X
                              + deltaZ / distance * direction.Z + 1d) / 2d, 0d, 1d);
            var access = accessByArea.TryGetValue(target, out var accessEntry)
                ? accessEntry.AccessStateCode
                : SimulationAreaSetHandoverCodes.AccessUnknown;
            var blockers = new[]
            {
                SimulationAreaSetHandoverCodes.AreaSetCorridorReserved,
                SimulationAreaSetHandoverCodes.AreaSetPlacementReserved,
                "AreaSetActualE5PackageMissing",
            };
            var response = new SimulationAreaSetHandoverCandidateResponse
            {
                TargetAreaSetStableId = target,
                RelationStableId = connection.RelationStableId,
                CorridorInstanceStableId = string.Empty,
                CorridorLandscapeGraphStableId = string.Empty,
                OverlapPolicyCode = SimulationWorldLayoutCodes.Disallow,
                SpatialRealizationCode = SimulationWorldLayoutCodes.ReservedCorridor,
                DistanceToTransitionMeters = Math.Round(distance, 3),
                HeadingAlignment01 = Math.Round(alignment, 6),
                PreparationTargetCode = SimulationAreaSetHandoverCodes.Known,
                SemanticDepthCode = "H4",
                ArtifactAvailabilityCode = SimulationAreaSetHandoverCodes.H5Reserved,
                ResidencyStateCode = SimulationAreaSetHandoverCodes.NotResident,
                SimulationAccessStateCode = access,
                ActivationAuthorityCode = SimulationAreaSetHandoverCodes.PreviewConfirmWorldTick,
                RequiredCapabilityCodes = Array.Empty<string>(),
                BlockingReasonCodes = blockers.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                RequiresActualE5Package = true,
                RequiresE6Grounding = false,
                CanRequestTraversal = false,
                CanActivate = false,
            };
            response.CandidateHashSha256 = Hash(string.Join("|", new[]
            {
                definition.WorldLayoutHashSha256,
                current,
                response.TargetAreaSetStableId,
                response.RelationStableId,
                response.SpatialRealizationCode,
                response.DistanceToTransitionMeters.ToString("0.000", CultureInfo.InvariantCulture),
                response.HeadingAlignment01.ToString("0.000000", CultureInfo.InvariantCulture),
                response.ArtifactAvailabilityCode,
                string.Join(",", response.BlockingReasonCodes),
            }));
            return response;
        }

        private SimulationAreaSetHandoverCandidateResponse CreateCandidate(
            SimulationWorldCorridorInstanceResponse corridor,
            string current,
            double focusX,
            double focusZ,
            (double X, double Z) direction,
            string directionCode,
            SimulationWorldLayoutDefinitionResponse definition,
            IReadOnlyDictionary<string, SimulationPlayerAreaAccessSnapshot> accessByArea)
        {
            var target = corridor.FromAreaSetInstanceStableId == current
                ? corridor.ToAreaSetInstanceStableId
                : corridor.FromAreaSetInstanceStableId;
            var deltaX = corridor.PlacementTransform.LocalXMeters - focusX;
            var deltaZ = corridor.PlacementTransform.LocalZMeters - focusZ;
            var distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
            var alignment = directionCode == SimulationLhWorldCodes.None || distance < .000001d
                ? .5d
                : Math.Clamp((deltaX / distance * direction.X
                              + deltaZ / distance * direction.Z + 1d) / 2d, 0d, 1d);
            var (targetCode, hCode) = PreparationTarget(distance);
            var access = accessByArea.TryGetValue(target, out var accessEntry)
                ? accessEntry.AccessStateCode
                : SimulationAreaSetHandoverCodes.AccessUnknown;
            var blockers = new List<string> { "AreaSetPackageReadinessUnverified" };
            if (targetCode != SimulationAreaSetHandoverCodes.H1TraversalPreparationRequested)
                blockers.Add("AreaSetTraversalPreparationIncomplete");
            if (access == SimulationAreaAccessCodes.Locked)
                blockers.Add("SimulationAreaAccessEvidenceMissing");
            else if (access == SimulationAreaSetHandoverCodes.AccessUnknown)
                blockers.Add("SimulationAreaAccessStateUnknown");

            var overlap = definition.OverlapRules.FirstOrDefault(value =>
                (value.FromInstanceStableId == current && value.ToInstanceStableId == target)
                || (value.FromInstanceStableId == target && value.ToInstanceStableId == current));
            var relation = definition.Relations.FirstOrDefault(value =>
                value.RelationStableId == corridor.RelationStableId);
            if (relation == null
                || relation.SpatialRealizationCode != SimulationWorldLayoutCodes.PhysicalCorridor
                || overlap == null
                || overlap.OverlapPolicyCode != SimulationWorldLayoutCodes.TransitionOverlap
                || overlap.CorridorInstanceStableId != corridor.CorridorInstanceStableId)
                throw new InvalidOperationException("AreaSetHandoverTopologyInvalid");
            var response = new SimulationAreaSetHandoverCandidateResponse
            {
                TargetAreaSetStableId = target,
                RelationStableId = corridor.RelationStableId,
                CorridorInstanceStableId = corridor.CorridorInstanceStableId,
                CorridorLandscapeGraphStableId = corridor.LandscapeGraphStableId,
                OverlapPolicyCode = overlap.OverlapPolicyCode,
                SpatialRealizationCode = relation.SpatialRealizationCode,
                DistanceToTransitionMeters = Math.Round(distance, 3),
                HeadingAlignment01 = Math.Round(alignment, 6),
                PreparationTargetCode = targetCode,
                SemanticDepthCode = hCode,
                ArtifactAvailabilityCode = SimulationAreaSetHandoverCodes.H5Known,
                ResidencyStateCode = SimulationAreaSetHandoverCodes.NotResident,
                SimulationAccessStateCode = access,
                ActivationAuthorityCode =
                    SimulationAreaSetHandoverCodes.PreviewConfirmWorldTick,
                RequiredCapabilityCodes = RequiredCapabilities(hCode),
                BlockingReasonCodes = blockers.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray(),
                RequiresActualE5Package = true,
                RequiresE6Grounding = definition.WorldGroundingPolicyCode
                                      == SimulationWorldLayoutCodes.Required,
                CanRequestTraversal = false,
                CanActivate = false,
            };
            response.CandidateHashSha256 = Hash(string.Join("|", new[]
            {
                definition.WorldLayoutHashSha256,
                current,
                response.TargetAreaSetStableId,
                response.RelationStableId,
                response.CorridorInstanceStableId,
                response.DistanceToTransitionMeters.ToString("0.000", CultureInfo.InvariantCulture),
                response.HeadingAlignment01.ToString("0.000000", CultureInfo.InvariantCulture),
                response.PreparationTargetCode,
                response.SemanticDepthCode,
                response.SimulationAccessStateCode,
                string.Join(",", response.BlockingReasonCodes),
            }));
            return response;
        }

        private static string ResolveCurrentAreaSet(
            string requested,
            double focusX,
            double focusZ,
            SimulationWorldLayoutDefinitionResponse definition)
        {
            if (!string.IsNullOrWhiteSpace(requested))
            {
                if (definition.AreaSetInstances.Any(value =>
                        value.AreaSetInstanceStableId == requested))
                    return requested;
                throw new InvalidOperationException(
                    "AreaSetHandoverCurrentAreaSetUnknown");
            }
            return definition.AreaSetInstances
                .OrderBy(value => DistanceSquared(value.PlacementTransform, focusX, focusZ))
                .ThenBy(value => value.AreaSetInstanceStableId, StringComparer.Ordinal)
                .First().AreaSetInstanceStableId;
        }

        private static double DistanceSquared(
            SimulationWorldPlacementTransformResponse pose, double x, double z)
            => (pose.LocalXMeters - x) * (pose.LocalXMeters - x)
               + (pose.LocalZMeters - z) * (pose.LocalZMeters - z);

        private static (string TargetCode, string HCode) PreparationTarget(double distance)
        {
            if (distance <= 125d)
                return (SimulationAreaSetHandoverCodes.H1TraversalPreparationRequested, "H1");
            if (distance <= 500d)
                return (SimulationAreaSetHandoverCodes.H2PrefetchRequested, "H2");
            if (distance <= 2000d)
                return (SimulationAreaSetHandoverCodes.H3PrepareRequested, "H3");
            return (SimulationAreaSetHandoverCodes.Known, "H4");
        }

        private static string[] RequiredCapabilities(string hCode) => hCode switch
        {
            "H1" => new[]
            {
                SimulationLhWorldCodes.TerrainVisual,
                SimulationLhWorldCodes.Collision,
                SimulationLhWorldCodes.Connector,
                SimulationLhWorldCodes.NpcNavigation,
                SimulationLhWorldCodes.H1Interaction,
            },
            "H2" => new[]
            {
                SimulationLhWorldCodes.TerrainVisual,
                SimulationLhWorldCodes.Connector,
            },
            "H3" => new[] { SimulationLhWorldCodes.TerrainVisual },
            _ => Array.Empty<string>(),
        };

        private static (double X, double Z) Direction(string code) => code switch
        {
            SimulationLhWorldCodes.North => (0d, 1d),
            SimulationLhWorldCodes.NorthEast => (.7071067811865476d, .7071067811865476d),
            SimulationLhWorldCodes.East => (1d, 0d),
            SimulationLhWorldCodes.SouthEast => (.7071067811865476d, -.7071067811865476d),
            SimulationLhWorldCodes.South => (0d, -1d),
            SimulationLhWorldCodes.SouthWest => (-.7071067811865476d, -.7071067811865476d),
            SimulationLhWorldCodes.West => (-1d, 0d),
            SimulationLhWorldCodes.NorthWest => (-.7071067811865476d, .7071067811865476d),
            SimulationLhWorldCodes.None => (0d, 0d),
            _ => throw new ArgumentException("AreaSetHandoverDirectionInvalid", nameof(code)),
        };

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(value))
                .Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
