using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q001 오두막의 안전한 수면 목적·보조 보관 경계와 구현 공백 판정을 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14" },
    Boundary = "계약 시험은 실제 체온·피로·질병 변화나 Play Mode 수면 증거가 아니다.")]
public sealed class SimulationNatureShelterPurposeReadinessTests
{
    [Fact]
    public void 오두막의_일차목적은_안전한수면이고_보관은_보조효용이다()
    {
        var result = new SimulationNatureShelterPurposeReadinessEvaluator()
            .Evaluate(new SimulationNatureCabinSnapshot
            {
                StateCode = SimulationNatureSurvivalCodes.Completed,
                RecoveryAvailable = true,
                StorageCapacity = 20,
            });

        Assert.Equal(SimulationNatureShelterPurposeCodes.SafeSleep,
            result.PrimaryPurposeCode);
        Assert.Equal(new[]
        {
            SimulationNatureShelterPurposeCodes.TemperatureStability,
            SimulationNatureShelterPurposeCodes.FatigueRecovery,
            SimulationNatureShelterPurposeCodes.DiseaseRiskReduction,
        }, result.RequiredCoreBenefitCodes);
        Assert.Equal(new[] { SimulationNatureShelterPurposeCodes.Storage },
            result.SecondaryBenefitCodes);
        Assert.DoesNotContain(SimulationNatureShelterPurposeCodes.Storage,
            result.RequiredCoreBenefitCodes);
        Assert.True(result.LegacyRecoverySignalAvailable);
        Assert.Equal(SimulationNatureShelterPurposeCodes.Gap,
            result.CoreBenefitReadinessCode);
        Assert.Empty(result.ImplementedCoreBenefitCodes);
        Assert.Equal(3, result.MissingCoreBenefitCodes.Length);
    }

    [Fact]
    public void 세가지핵심효용이_모두명시된_운영오두막만_Ready다()
    {
        var result = new SimulationNatureShelterPurposeReadinessEvaluator()
            .Evaluate(new SimulationNatureCabinSnapshot
            {
                StateCode = SimulationNatureSurvivalCodes.Completed,
                RecoveryAvailable = true,
            }, SimulationNatureShelterPurposeCodes.DiseaseRiskReduction,
                SimulationNatureShelterPurposeCodes.TemperatureStability,
                SimulationNatureShelterPurposeCodes.FatigueRecovery,
                SimulationNatureShelterPurposeCodes.Storage,
                SimulationNatureShelterPurposeCodes.FatigueRecovery);

        Assert.Equal(SimulationNatureShelterPurposeCodes.Ready,
            result.CoreBenefitReadinessCode);
        Assert.Equal(3, result.ImplementedCoreBenefitCodes.Length);
        Assert.Empty(result.MissingCoreBenefitCodes);
    }
}
