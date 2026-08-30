using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q024 승인 기상 관측의 하루 Profile 동결·출처 계보·플레이 중 불변 경계를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorldInteractionIds = new[] { "WI-NATURE-14" },
    Boundary = "후보 판정 시험이며 실제 Provider 호출·Save/Replay·Sky·Play Mode 증거가 아니다.")]
public sealed class SimulationNatureWeatherProfileFreezeCandidateTests
{
    [Theory]
    [InlineData(SimulationNatureWeatherProfileFreezeCandidateCodes.NewWorldBoundary)]
    [InlineData(SimulationNatureWeatherProfileFreezeCandidateCodes.GameDayStartBoundary)]
    public void 승인관측은_새세계나하루시작에서만_날씨Profile로동결한다(
        string boundaryCode)
    {
        var result = new SimulationNatureWeatherProfileFreezeCandidateEvaluator()
            .Evaluate(ReadyRequest(boundaryCode));

        Assert.Equal(SimulationNatureWeatherProfileFreezeCandidateCodes.Ready,
            result.ReadinessCode);
        Assert.True(result.FrozenForGameDay);
        Assert.False(result.AllowsMidDayExternalMutation);
        Assert.True(result.RequiresSourceLineageInSave);
        Assert.False(result.AppliesWeatherProfile);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 품질승인이나출처Hash가없으면_동결후보를준비완료로보지않는다()
    {
        var request = ReadyRequest(
            SimulationNatureWeatherProfileFreezeCandidateCodes
                .GameDayStartBoundary);
        request.ObservationQualityApproved = false;
        request.SourceSnapshotHashSha256 = string.Empty;

        var result = new SimulationNatureWeatherProfileFreezeCandidateEvaluator()
            .Evaluate(request);

        Assert.Equal(SimulationNatureWeatherProfileFreezeCandidateCodes.Gap,
            result.ReadinessCode);
        Assert.False(result.FrozenForGameDay);
        Assert.Contains(SimulationNatureWeatherProfileFreezeCandidateCodes
            .ObservationQualityApprovalRequired,
            result.MissingRequirementCodes);
        Assert.Contains(SimulationNatureWeatherProfileFreezeCandidateCodes
            .SourceSnapshotHashRequired, result.MissingRequirementCodes);
    }

    private static SimulationNatureWeatherProfileFreezeCandidateRequest
        ReadyRequest(string boundaryCode)
    {
        return new SimulationNatureWeatherProfileFreezeCandidateRequest
        {
            RiskySleepOutcomeCandidate =
                new SimulationNatureRiskySleepOutcomeCandidateSnapshot
                {
                    ReadinessCode =
                        SimulationNatureRiskySleepOutcomeCandidateCodes.Ready,
                },
            FreezeBoundaryCode = boundaryCode,
            GameDayStableId = "game-day:1",
            SourceTypeCode =
                SimulationNatureWeatherProfileFreezeCandidateCodes
                    .PublicObservationSource,
            SourceSnapshotHashSha256 = new string('a', 64),
            ObservationQualityApproved = true,
            GeneralizationRuleRevision = "weather-generalization.r1",
            WeatherProfileCode = "weather-profile:cold-rain",
        };
    }
}
