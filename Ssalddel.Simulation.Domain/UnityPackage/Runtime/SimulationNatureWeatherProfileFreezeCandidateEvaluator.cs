using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "위험 수면 날씨 입력과 승인 관측 계보를 읽어 하루 날씨 Profile 동결 준비도를 판정한다.",
        StepKey = "domain.nature-weather-profile-freeze-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.nature-risky-sleep-outcome-candidate",
            "domain.nature-risky-sleep-outcome-candidate",
            "contract.nature-weather-profile-freeze-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SharedPublicData,
        FlowOrder = 22,
        Boundary = "동결 후보 준비도만 판정하며 외부 호출·날씨 상태·Save·Sky 표현을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q024 품질 승인 관측의 새 세계·하루 경계 Profile 동결과 플레이 중 불변 경계를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "후보 판정이며 실제 Provider·Save/Replay·Sky·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureWeatherProfileFreezeCandidateEvaluator
    {
        public SimulationNatureWeatherProfileFreezeCandidateSnapshot Evaluate(
            SimulationNatureWeatherProfileFreezeCandidateRequest request)
        {
            var missing = new List<string>();
            if (request?.RiskySleepOutcomeCandidate?.ReadinessCode !=
                SimulationNatureRiskySleepOutcomeCandidateCodes.Ready)
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .RiskySleepOutcomeCandidateRequired);
            if (!IsFreezeBoundary(request?.FreezeBoundaryCode))
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .FreezeBoundaryRequired);
            if (string.IsNullOrWhiteSpace(request?.SourceTypeCode))
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .SourceTypeRequired);
            if (string.IsNullOrWhiteSpace(request?.SourceSnapshotHashSha256))
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .SourceSnapshotHashRequired);
            if (request?.ObservationQualityApproved != true)
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .ObservationQualityApprovalRequired);
            if (string.IsNullOrWhiteSpace(
                request?.GeneralizationRuleRevision))
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .GeneralizationRuleRevisionRequired);
            if (string.IsNullOrWhiteSpace(request?.WeatherProfileCode))
                missing.Add(SimulationNatureWeatherProfileFreezeCandidateCodes
                    .WeatherProfileCodeRequired);

            return new SimulationNatureWeatherProfileFreezeCandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? SimulationNatureWeatherProfileFreezeCandidateCodes.Ready
                    : SimulationNatureWeatherProfileFreezeCandidateCodes.Gap,
                FreezeBoundaryCode = request?.FreezeBoundaryCode?.Trim()
                    ?? string.Empty,
                GameDayStableId = request?.GameDayStableId?.Trim()
                    ?? string.Empty,
                SourceTypeCode = request?.SourceTypeCode?.Trim()
                    ?? string.Empty,
                SourceSnapshotHashSha256 =
                    request?.SourceSnapshotHashSha256?.Trim() ?? string.Empty,
                GeneralizationRuleRevision =
                    request?.GeneralizationRuleRevision?.Trim() ?? string.Empty,
                WeatherProfileCode = request?.WeatherProfileCode?.Trim()
                    ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                FrozenForGameDay = missing.Count == 0,
                AllowsMidDayExternalMutation = false,
                RequiresSourceLineageInSave = true,
                AppliesWeatherProfile = false,
                ChangesWorldState = false,
            };
        }

        private static bool IsFreezeBoundary(string? code)
            => string.Equals(code,
                   SimulationNatureWeatherProfileFreezeCandidateCodes
                       .NewWorldBoundary, StringComparison.Ordinal)
               || string.Equals(code,
                   SimulationNatureWeatherProfileFreezeCandidateCodes
                       .GameDayStartBoundary, StringComparison.Ordinal);
    }
}
