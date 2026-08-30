using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q004 공통 수면 판정식과 모드별 위협 출몰 Profile 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14" },
    Boundary = "경계 시험이며 실제 Spawn 빈도·Save/Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationNatureDifficultyBoundaryTests
{
    [Fact]
    public void 노말과숙련자는_같은수면공식과_다른출몰Profile을쓴다()
    {
        var resolver = new SimulationNatureDifficultyBoundaryResolver();
        var normal = resolver.Resolve(
            SimulationNatureRiskySleepWarningCodes.Normal,
            "sleep-safety.r1", "spawn-standard.r1", "spawn-expert.r1");
        var expert = resolver.Resolve(
            SimulationNatureRiskySleepWarningCodes.Expert,
            "sleep-safety.r1", "spawn-standard.r1", "spawn-expert.r1");

        Assert.Equal(normal.SleepSafetyFormulaRevision,
            expert.SleepSafetyFormulaRevision);
        Assert.Equal("spawn-standard.r1",
            normal.SelectedSpawnProfileRevision);
        Assert.Equal("spawn-expert.r1",
            expert.SelectedSpawnProfileRevision);
        Assert.False(normal.IncreasedThreatExposure);
        Assert.True(expert.IncreasedThreatExposure);
        Assert.False(normal.ChangesCurrentSafetyForSameInputs);
        Assert.False(expert.ChangesCurrentSafetyForSameInputs);
    }

    [Fact]
    public void 숙련자는_출몰입력이강화되고_경고정보량은줄어든다()
    {
        var result = new SimulationNatureDifficultyBoundaryResolver()
            .Resolve(SimulationNatureRiskySleepWarningCodes.Expert,
                "sleep-safety.r1", "spawn-standard.r1", "spawn-expert.r1");

        Assert.True(result.UsesSharedSleepSafetyFormula);
        Assert.True(result.IncreasedThreatExposure);
        Assert.Equal(SimulationNatureDifficultyBoundaryCodes
            .ReducedWarningInformation,
            result.WarningInformationLevelCode);
    }
}
