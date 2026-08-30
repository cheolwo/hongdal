using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "수면 보호 공간층의 역할·배치·형상·경계 Graph 근거를 읽어 중첩 공간 후보 준비도를 판정한다.",
        StepKey = "domain.nature-sleep-protection-spatial-layer-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.nature-weather-profile-freeze-candidate",
            "domain.nature-weather-profile-freeze-candidate",
            "contract.nature-sleep-protection-spatial-layer-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 23,
        Boundary = "공간층 근거 준비도만 판정하며 H 정의·배치·Collider·Graph·보호 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q025 다섯 수면 보호 공간층의 배치·형상·경계 Graph 후보를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "후보 판정이며 실제 H 조립·배치·물리 범위·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureSleepProtectionSpatialLayerCandidateEvaluator
    {
        public SimulationNatureSleepProtectionSpatialLayerCandidateSnapshot
            Evaluate(SimulationNatureSleepProtectionSpatialLayerCandidateRequest request)
        {
            var missing = new List<string>();
            if (request?.WeatherProfileCandidate?.ReadinessCode !=
                SimulationNatureWeatherProfileFreezeCandidateCodes.Ready)
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .WeatherProfileCandidateRequired);
            if (string.IsNullOrWhiteSpace(request?.SpatialPolicyRevision))
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .SpatialPolicyRevisionRequired);

            var layers = (request?.Layers ??
                    Array.Empty<SimulationNatureSleepProtectionSpatialLayerDefinition>())
                .OrderBy(value => value.LayerRoleCode, StringComparer.Ordinal)
                .ThenBy(value => value.LayerStableId, StringComparer.Ordinal)
                .ToArray();
            var requiredRoles =
                SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .RequiredLayerRoleCodes();
            var missingRoles = requiredRoles.Where(role => !layers.Any(layer =>
                string.Equals(layer.LayerRoleCode, role,
                    StringComparison.Ordinal))).ToArray();
            if (missingRoles.Length > 0)
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .RequiredLayerMissing);
            if (layers.Any(value => string.IsNullOrWhiteSpace(value.LayerStableId)))
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .LayerStableIdRequired);
            if (layers.Where(value => !string.IsNullOrWhiteSpace(value.LayerStableId))
                    .GroupBy(value => value.LayerStableId, StringComparer.Ordinal)
                    .Any(group => group.Count() > 1))
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .LayerStableIdDuplicated);
            if (layers.Any(value =>
                    string.IsNullOrWhiteSpace(value.PlacementStableId)))
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .PlacementStableIdRequired);
            if (layers.Any(value =>
                    string.IsNullOrWhiteSpace(value.GeometryRevision)))
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .GeometryRevisionRequired);
            if (layers.Any(value => IsBoundaryRole(value.LayerRoleCode) &&
                    string.IsNullOrWhiteSpace(value.BoundaryGraphRevision)))
                missing.Add(SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                    .BoundaryGraphRevisionRequired);

            return new SimulationNatureSleepProtectionSpatialLayerCandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? SimulationNatureSleepProtectionSpatialLayerCandidateCodes.Ready
                    : SimulationNatureSleepProtectionSpatialLayerCandidateCodes.Gap,
                SpatialPolicyRevision =
                    request?.SpatialPolicyRevision?.Trim() ?? string.Empty,
                Layers = layers,
                MissingRequirementCodes = missing.Distinct(StringComparer.Ordinal)
                    .ToArray(),
                MissingLayerRoleCodes = missingRoles,
                SupportsOverlappingLayers = true,
                UsesPlacementAndGeometryEvidence = true,
                UsesBoundaryGraphEvidence = true,
                AppliesSpatialProtection = false,
                ChangesWorldState = false,
            };
        }

        private static bool IsBoundaryRole(string role)
            => string.Equals(role,
                   SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                       .PhysicalPerimeterLayer, StringComparison.Ordinal)
               || string.Equals(role,
                   SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                       .HigherThreatBoundaryLayer, StringComparison.Ordinal);
    }
}
