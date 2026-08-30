using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "Nature 난이도에서 공통 수면 판정식과 별도 위협 출몰 Profile을 선택한다.",
        StepKey = "domain.nature-difficulty-boundary",
        DependsOnStepKeys = new[]
        {
            "contract.nature-difficulty-boundary",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 22,
        Boundary = "Profile revision을 선택할 뿐 출몰을 생성하거나 수면 안전 결과·WorldRevision을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q004 같은 수면 공식·다른 출몰 Profile이라는 난이도 경계를 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "Profile 선택 준비이며 실제 Spawn·Save/Replay·수면 Preview 실행이 아니다.")]
    public sealed class SimulationNatureDifficultyBoundaryResolver
    {
        public SimulationNatureDifficultyBoundarySnapshot Resolve(
            string difficultyCode,
            string sleepSafetyFormulaRevision,
            string standardSpawnProfileRevision,
            string expertSpawnProfileRevision)
        {
            RequireDifficulty(difficultyCode);
            RequireRevision(sleepSafetyFormulaRevision,
                nameof(sleepSafetyFormulaRevision));
            RequireRevision(standardSpawnProfileRevision,
                nameof(standardSpawnProfileRevision));
            RequireRevision(expertSpawnProfileRevision,
                nameof(expertSpawnProfileRevision));
            var expert = string.Equals(difficultyCode,
                SimulationNatureRiskySleepWarningCodes.Expert,
                StringComparison.Ordinal);

            return new SimulationNatureDifficultyBoundarySnapshot
            {
                DifficultyCode = difficultyCode,
                SleepSafetyFormulaRevision = sleepSafetyFormulaRevision,
                UsesSharedSleepSafetyFormula = true,
                SelectedSpawnProfileRevision = expert
                    ? expertSpawnProfileRevision
                    : standardSpawnProfileRevision,
                IncreasedThreatExposure = expert,
                WarningInformationLevelCode = expert
                    ? SimulationNatureDifficultyBoundaryCodes
                        .ReducedWarningInformation
                    : SimulationNatureDifficultyBoundaryCodes
                        .StandardWarningInformation,
                ChangesCurrentSafetyForSameInputs = false,
            };
        }

        private static void RequireDifficulty(string value)
        {
            if (string.Equals(value,
                    SimulationNatureRiskySleepWarningCodes.Beginner,
                    StringComparison.Ordinal)
                || string.Equals(value,
                    SimulationNatureRiskySleepWarningCodes.Normal,
                    StringComparison.Ordinal)
                || string.Equals(value,
                    SimulationNatureRiskySleepWarningCodes.Expert,
                    StringComparison.Ordinal)) return;
            throw new ArgumentException("NatureDifficultyCodeUnknown",
                nameof(value));
        }

        private static void RequireRevision(string value, string parameterName)
        {
            if (!string.IsNullOrWhiteSpace(value)) return;
            throw new ArgumentException("NatureDifficultyRevisionRequired",
                parameterName);
        }
    }
}
