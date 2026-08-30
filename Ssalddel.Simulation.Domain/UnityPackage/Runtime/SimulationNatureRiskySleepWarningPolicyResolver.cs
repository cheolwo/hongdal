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
        "위험 수면의 모드 기본값과 사용자 설정으로 경고 가시성을 판정한다.",
        StepKey = "domain.nature-risky-sleep-warning-policy",
        DependsOnStepKeys = new[]
        {
            "contract.nature-risky-sleep-warning",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 21,
        Boundary = "경고 표시만 판정하고 수면 선택을 차단하거나 Simulation 위험·회복 수치를 바꾸지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q003 위험 수면 경고 표시 정책을 권위 판정과 분리해 읽기 전용으로 계산한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "Preview 표시 준비이며 Confirm·WorldRevision·수면 결과를 변경하지 않는다.")]
    public sealed class SimulationNatureRiskySleepWarningPolicyResolver
    {
        public SimulationNatureRiskySleepWarningSnapshot Resolve(
            string difficultyCode,
            string preferenceCode,
            IEnumerable<string> warningReasonCodes)
        {
            RequireDifficulty(difficultyCode);
            RequirePreference(preferenceCode);
            var reasons = (warningReasonCodes ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var riskDetected = reasons.Length > 0;
            var visible = riskDetected && ResolveVisibility(difficultyCode,
                preferenceCode);

            return new SimulationNatureRiskySleepWarningSnapshot
            {
                DifficultyCode = difficultyCode,
                PreferenceCode = preferenceCode,
                RiskDetected = riskDetected,
                WarningReasonCodes = reasons,
                WarningVisible = visible,
                SleepSelectionAllowed = true,
                ChangesAuthoritySafetyJudgement = false,
            };
        }

        private static bool ResolveVisibility(string difficultyCode,
            string preferenceCode)
        {
            if (string.Equals(preferenceCode,
                    SimulationNatureRiskySleepWarningCodes.AlwaysShow,
                    StringComparison.Ordinal)) return true;
            if (string.Equals(preferenceCode,
                    SimulationNatureRiskySleepWarningCodes.NeverShow,
                    StringComparison.Ordinal)) return false;
            return !string.Equals(difficultyCode,
                SimulationNatureRiskySleepWarningCodes.Expert,
                StringComparison.Ordinal);
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
            throw new ArgumentException("NatureSleepDifficultyCodeUnknown",
                nameof(value));
        }

        private static void RequirePreference(string value)
        {
            if (string.Equals(value,
                    SimulationNatureRiskySleepWarningCodes.UseModeDefault,
                    StringComparison.Ordinal)
                || string.Equals(value,
                    SimulationNatureRiskySleepWarningCodes.AlwaysShow,
                    StringComparison.Ordinal)
                || string.Equals(value,
                    SimulationNatureRiskySleepWarningCodes.NeverShow,
                    StringComparison.Ordinal)) return;
            throw new ArgumentException(
                "NatureSleepWarningPreferenceCodeUnknown", nameof(value));
        }
    }
}
