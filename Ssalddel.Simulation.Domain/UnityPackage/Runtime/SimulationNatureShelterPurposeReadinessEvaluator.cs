using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "현재 Nature 오두막 상태가 안전한 수면의 핵심 효용을 실제로 구현했는지 판정한다.",
        StepKey = "domain.nature-shelter-purpose-readiness",
        DependsOnStepKeys = new[] { "contract.nature-shelter-purpose" },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 19,
        Boundary = "기존 RecoveryAvailable을 체온·피로·질병의 개별 구현으로 간주하지 않고 확인된 효용만 Ready로 판정한다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q001 오두막의 안전한 수면 핵심 효용 구현 공백을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "준비도 판정은 수면 Confirm·체온·피로·질병 상태를 변경하지 않는다.")]
    public sealed class SimulationNatureShelterPurposeReadinessEvaluator
    {
        public SimulationNatureShelterPurposeReadinessSnapshot Evaluate(
            SimulationNatureCabinSnapshot cabin,
            params string[] implementedCoreBenefitCodes)
        {
            if (cabin == null) throw new ArgumentNullException(nameof(cabin));
            var required = SimulationNatureShelterPurposeCodes
                .CoreBenefitCodes();
            var implemented = (implementedCoreBenefitCodes
                    ?? Array.Empty<string>())
                .Where(value => required.Contains(value,
                    StringComparer.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var missing = required.Except(implemented,
                    StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var operational = string.Equals(cabin.StateCode,
                SimulationNatureSurvivalCodes.Completed,
                StringComparison.Ordinal);

            return new SimulationNatureShelterPurposeReadinessSnapshot
            {
                CabinOperational = operational,
                LegacyRecoverySignalAvailable = operational
                    && cabin.RecoveryAvailable,
                CoreBenefitReadinessCode = operational && missing.Length == 0
                    ? SimulationNatureShelterPurposeCodes.Ready
                    : SimulationNatureShelterPurposeCodes.Gap,
                RequiredCoreBenefitCodes = required,
                ImplementedCoreBenefitCodes = implemented,
                MissingCoreBenefitCodes = missing,
                SecondaryBenefitCodes =
                    SimulationNatureShelterPurposeCodes
                        .SecondaryBenefitCodes(),
            };
        }
    }
}
