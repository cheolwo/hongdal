using Ssalddel.Unity.TraditionalMarkets;
using Xunit;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class TraditionalMarketHubVerticalSliceTests
{
    [Fact]
    public async Task simulation_fixture는_실제물류거점으로_표시되지_않는다()
    {
        var model = await new Simulated전통시장물류거점조회UseCase().조회Async();

        Assert.Equal(전통시장물류거점SourceTypeCodes.SimulatedFixture, model.SourceTypeCode);
        Assert.StartsWith("SIMULATED", model.SourceName);
        Assert.Empty(new 전통시장물류거점ScreenModelValidator().Validate(model));
    }

    [Fact]
    public async Task public_projection은_Pilot과_Active_상태만_허용한다()
    {
        var model = await new Simulated전통시장물류거점조회UseCase().조회Async();
        model.상태Code = "Candidate";

        var errors = new 전통시장물류거점ScreenModelValidator().Validate(model);

        Assert.Contains("HubPublicStatusInvalid", errors);
    }

    [Fact]
    public async Task 검증되지_않은_위치정밀도는_거부한다()
    {
        var model = await new Simulated전통시장물류거점조회UseCase().조회Async();
        model.LocationPrecisionCode = "ExactPrivateLocation";

        var errors = new 전통시장물류거점ScreenModelValidator().Validate(model);

        Assert.Contains("LocationPrecisionInvalid", errors);
    }

    [Fact]
    public async Task 위도와_경도_범위를_검증한다()
    {
        var model = await new Simulated전통시장물류거점조회UseCase().조회Async();
        model.Latitude = 91m;
        model.Longitude = -181m;

        var errors = new 전통시장물류거점ScreenModelValidator().Validate(model);

        Assert.Contains("LatitudeInvalid", errors);
        Assert.Contains("LongitudeInvalid", errors);
    }

    [Fact]
    public async Task 물류기능이_없으면_표현계약을_거부한다()
    {
        var model = await new Simulated전통시장물류거점조회UseCase().조회Async();
        model.물류기능 = null!;

        var errors = new 전통시장물류거점ScreenModelValidator().Validate(model);

        Assert.Contains("LogisticsCapabilitiesMissing", errors);
    }

    [Fact]
    public async Task 근거와_생성시각이_없으면_거부한다()
    {
        var model = await new Simulated전통시장물류거점조회UseCase().조회Async();
        model.EvidenceAsOf = default;
        model.GeneratedAt = default;

        var errors = new 전통시장물류거점ScreenModelValidator().Validate(model);

        Assert.Contains("EvidenceAsOfMissing", errors);
        Assert.Contains("GeneratedAtMissing", errors);
    }
}
