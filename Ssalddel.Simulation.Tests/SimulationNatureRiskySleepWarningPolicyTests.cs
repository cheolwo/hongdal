using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q003 위험 수면 허용과 난이도·사용자 설정별 경고 표시 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14" },
    Boundary = "정책 시험이며 실제 Preview UI·수면 결과·Play Mode 증거가 아니다.")]
public sealed class SimulationNatureRiskySleepWarningPolicyTests
{
    [Theory]
    [InlineData(SimulationNatureRiskySleepWarningCodes.Beginner, true)]
    [InlineData(SimulationNatureRiskySleepWarningCodes.Normal, true)]
    [InlineData(SimulationNatureRiskySleepWarningCodes.Expert, false)]
    public void 모드기본값은_입문자노말만_위험경고를_보인다(
        string difficultyCode, bool expectedVisible)
    {
        var result = new SimulationNatureRiskySleepWarningPolicyResolver()
            .Resolve(difficultyCode,
                SimulationNatureRiskySleepWarningCodes.UseModeDefault,
                new[] { "LowTemperature", "NearbyAnimal" });

        Assert.Equal(expectedVisible, result.WarningVisible);
        Assert.True(result.SleepSelectionAllowed);
        Assert.False(result.ChangesAuthoritySafetyJudgement);
        Assert.Equal(new[] { "LowTemperature", "NearbyAnimal" },
            result.WarningReasonCodes);
    }

    [Fact]
    public void 사용자는_모드와무관하게_위험경고를_켜거나끌수있다()
    {
        var resolver = new SimulationNatureRiskySleepWarningPolicyResolver();
        var expert = resolver.Resolve(
            SimulationNatureRiskySleepWarningCodes.Expert,
            SimulationNatureRiskySleepWarningCodes.AlwaysShow,
            new[] { "NearbyMonster" });
        var beginner = resolver.Resolve(
            SimulationNatureRiskySleepWarningCodes.Beginner,
            SimulationNatureRiskySleepWarningCodes.NeverShow,
            new[] { "NearbyMonster" });

        Assert.True(expert.WarningVisible);
        Assert.False(beginner.WarningVisible);
        Assert.True(expert.SleepSelectionAllowed);
        Assert.True(beginner.SleepSelectionAllowed);
    }

    [Fact]
    public void 위험이없으면_강제표시설정이어도_경고를만들지않는다()
    {
        var result = new SimulationNatureRiskySleepWarningPolicyResolver()
            .Resolve(SimulationNatureRiskySleepWarningCodes.Normal,
                SimulationNatureRiskySleepWarningCodes.AlwaysShow,
                System.Array.Empty<string>());

        Assert.False(result.RiskDetected);
        Assert.False(result.WarningVisible);
        Assert.True(result.SleepSelectionAllowed);
    }
}
