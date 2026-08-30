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
        "수면 안전 후보와 위협·날씨 노출을 읽어 강제 각성 인계와 기상 누적 결과를 결정적 후보로 분리한다.",
        StepKey = "domain.nature-risky-sleep-outcome-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.nature-sleep-safety-candidate",
            "domain.nature-sleep-safety-candidate-readiness",
            "contract.nature-risky-sleep-outcome-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 21,
        Boundary = "결과 후보만 판정하며 수면 Task·전투·피로·체온·질병·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q023 위협 접근의 수면 중단과 환경 노출의 기상 결과 후보를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14", "WI-NATURE-11" },
        Boundary = "후보 판정이며 실제 강제 각성·전투/후퇴·신체 상태·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureRiskySleepOutcomeCandidateEvaluator
    {
        public SimulationNatureRiskySleepOutcomeCandidateSnapshot Evaluate(
            SimulationNatureRiskySleepOutcomeCandidateRequest request)
        {
            var missing = new List<string>();
            if (request?.SleepSafetyCandidate?.SchemaVersion !=
                SimulationNatureSleepSafetyCandidateCodes.SchemaVersion)
                missing.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .SleepSafetyCandidateRequired);
            if (string.IsNullOrWhiteSpace(request?.WeatherInputRevision))
                missing.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .WeatherInputRevisionRequired);

            var threatCodes = new List<string>();
            if (request?.AnimalApproachDetected == true)
                threatCodes.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .AnimalApproach);
            if (request?.MonsterApproachDetected == true)
                threatCodes.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .MonsterApproach);
            var orderedThreatCodes =
                SimulationNatureRiskySleepOutcomeCandidateCodes
                    .OrderedThreatApproachCodes()
                    .Where(threatCodes.Contains).ToArray();

            var wakeOutcomes = new List<string>();
            if (request?.ColdExposureDetected == true)
            {
                wakeOutcomes.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .TemperatureWakeOutcome);
                wakeOutcomes.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .FatigueWakeOutcome);
            }
            if (request?.PrecipitationExposureDetected == true)
                wakeOutcomes.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .TemperatureWakeOutcome);
            if (request?.DiseaseRiskAccumulated == true)
                wakeOutcomes.Add(SimulationNatureRiskySleepOutcomeCandidateCodes
                    .DiseaseRiskWakeOutcome);
            var distinctWakeOutcomes = wakeOutcomes
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

            return new SimulationNatureRiskySleepOutcomeCandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? SimulationNatureRiskySleepOutcomeCandidateCodes.Ready
                    : SimulationNatureRiskySleepOutcomeCandidateCodes.Gap,
                WeatherInputRevision =
                    request?.WeatherInputRevision?.Trim() ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                ThreatApproachCodes = orderedThreatCodes,
                AccumulatedWakeOutcomeCodes = distinctWakeOutcomes,
                InterruptsSleepForThreatApproach = orderedThreatCodes.Length > 0,
                ReturnsCombatOrRetreatChoice = orderedThreatCodes.Length > 0,
                DefersEnvironmentalOutcomeUntilWake =
                    distinctWakeOutcomes.Length > 0,
                AppliesSleepInterruption = false,
                AppliesWakeOutcome = false,
                ChangesWorldState = false,
            };
        }
    }
}
