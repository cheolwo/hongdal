using Ssalddel.Unity.Data;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class FarmEnvironmentalGrowthTurnTests
{
    private readonly 재배환경생육TurnCalculator calculator = new();
    private readonly 작물환경생육RuleSnapshot rule = 감자재배환경SimulationFixture.CreateRule();

    [Fact]
    public void FARM_ENV_0_정상환경은하루생육점수와발아단계를결정적으로만든다()
    {
        var state = 감자재배환경SimulationFixture.CreateState();
        var environment = 감자재배환경SimulationFixture.CreateEnvironment(state);

        var first = calculator.EvaluateDay(state, environment, rule);
        var replay = calculator.EvaluateDay(state, environment, rule);

        Assert.Equal(1m, first.DailyGrowthPoint);
        Assert.Equal(1m, first.State.AccumulatedGrowthPoint);
        Assert.Equal(재배생육단계Codes.Emerged, first.State.GrowthStageCode);
        Assert.Equal(재배환경제한요인Codes.None, first.LimitingFactorCode);
        Assert.Equal(first.State.SoilWaterMm, replay.State.SoilWaterMm);
        Assert.Equal(first.State.AccumulatedGrowthPoint, replay.State.AccumulatedGrowthPoint);
        Assert.Equal(state.Revision + 1, first.State.Revision);
        Assert.Equal(0m, state.AccumulatedGrowthPoint);
    }

    [Fact]
    public void FARM_ENV_0_연속건조는토양수분과생육을낮추고가뭄스트레스를누적한다()
    {
        var initial = 감자재배환경SimulationFixture.CreateState(30m);
        var first = calculator.EvaluateDay(initial,
            감자재배환경SimulationFixture.CreateEnvironment(
                initial, rainfallMm: 0m, evapotranspirationMm: 8m), rule);
        var second = calculator.EvaluateDay(first.State,
            감자재배환경SimulationFixture.CreateEnvironment(
                first.State, rainfallMm: 0m, evapotranspirationMm: 8m), rule);

        Assert.Equal(22m, first.State.SoilWaterMm);
        Assert.Equal(0.4m, first.WaterFactor);
        Assert.Equal(0.4m, first.DailyGrowthPoint);
        Assert.Equal(14m, second.State.SoilWaterMm);
        Assert.Equal(0.5333m, second.State.AccumulatedGrowthPoint);
        Assert.Equal(1.4667m, second.State.Stress.Drought);
        Assert.Equal(재배환경제한요인Codes.Water, second.LimitingFactorCode);
    }

    [Fact]
    public void FARM_ENV_0_집중호우는유출과배수뒤과습스트레스를남긴다()
    {
        var state = 감자재배환경SimulationFixture.CreateState(70m);
        var environment = 감자재배환경SimulationFixture.CreateEnvironment(
            state, rainfallMm: 100m, evapotranspirationMm: 5m);

        var result = calculator.EvaluateDay(state, environment, rule);

        Assert.Equal(80m, result.EffectiveRainfallMm);
        Assert.Equal(20m, result.RunoffMm);
        Assert.Equal(22.5m, result.DrainageMm);
        Assert.Equal(100m, result.State.SoilWaterMm);
        Assert.Equal(0m, result.WaterFactor);
        Assert.Equal(0m, result.DailyGrowthPoint);
        Assert.Equal(1m, result.State.Stress.Waterlogging);
        Assert.Equal(재배환경제한요인Codes.Water, result.LimitingFactorCode);
    }

    [Fact]
    public void FARM_ENV_0_저일사는물이충분해도생육을제한하고스트레스를남긴다()
    {
        var state = 감자재배환경SimulationFixture.CreateState();
        var environment = 감자재배환경SimulationFixture.CreateEnvironment(
            state, solarRadiation: 4m);

        var result = calculator.EvaluateDay(state, environment, rule);

        Assert.Equal(0.25m, result.SunlightFactor);
        Assert.Equal(1m, result.WaterFactor);
        Assert.Equal(0.25m, result.DailyGrowthPoint);
        Assert.Equal(0.75m, result.State.Stress.LowSunlight);
        Assert.Equal(재배환경제한요인Codes.Sunlight, result.LimitingFactorCode);
        Assert.Equal(재배생육단계Codes.Sown, result.State.GrowthStageCode);
    }

    [Fact]
    public void FARM_ENV_0_공식관측결측은Fixture로대체하지않고하루진행을차단한다()
    {
        var state = 감자재배환경SimulationFixture.CreateState();
        var environment = 감자재배환경SimulationFixture.CreateEnvironment(state);
        environment.ModeCode = 재배환경실행ModeCodes.PublicObservationReference;
        environment.Soil.SourceTypeCode = 데이터SourceTypes.PublicObservation;
        environment.Soil.QualityCode = 데이터품질Codes.Valid;
        environment.Climate.SourceTypeCode = 데이터SourceTypes.PublicObservation;
        environment.Climate.QualityCode = 데이터품질Codes.Missing;
        environment.Climate.SolarRadiationMjPerSquareMeter = null;

        Assert.Equal("FarmEnvironmentalObservationMissing",
            Assert.Throws<InvalidOperationException>(
                () => calculator.EvaluateDay(state, environment, rule)).Message);
        Assert.Equal(0, state.DaysAfterSowing);
        Assert.Equal(0m, state.AccumulatedGrowthPoint);
    }

    [Fact]
    public void FARM_ENV_0_Fixture와공식관측품질을한Snapshot에혼합하지않는다()
    {
        var state = 감자재배환경SimulationFixture.CreateState();
        var environment = 감자재배환경SimulationFixture.CreateEnvironment(state);
        environment.Climate.SourceTypeCode = 데이터SourceTypes.PublicObservation;
        environment.Climate.QualityCode = 데이터품질Codes.Valid;

        Assert.Equal("FarmEnvironmentalSourceQualityMismatch",
            Assert.Throws<InvalidOperationException>(
                () => calculator.EvaluateDay(state, environment, rule)).Message);
    }
}
