using FluentResults;
using Microsoft.AspNetCore.Http;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Application.WorldProjection;

public sealed class 감자생산유통WorldTests
{
    [Fact]
    public void ProductOnly는_가격출처를보존하고_Farm화물창고마트를_추정하지않는다()
    {
        var result = new 감자생산유통WorldProjector().Project(new(
            FarmPerspective(),
            DomesticPrice(),
            null,
            감자생산유통SourceModeCodes.OperationalProjection,
            감자생산유통LinkageStatusCodes.ProductOnly,
            new DateTimeOffset(2026, 8, 10, 1, 0, 0, TimeSpan.Zero)));

        Assert.True(result.IsSuccess);
        Assert.Equal(감자생산유통LinkageStatusCodes.ProductOnly, result.Value.LinkageStatusCode);
        Assert.Equal("product:potato", result.Value.Product.ProductStableId);
        Assert.Equal("0701", result.Value.Product.HsPrefix);
        Assert.Equal(감자가격관측StatusCodes.Ready, result.Value.DomesticPrice.StatusCode);
        Assert.Equal(2450m, result.Value.DomesticPrice.Wholesale!.AverageKrwPerKg);
        Assert.Equal("KRW_PER_KG", result.Value.DomesticPrice.UnitCode);
        Assert.Null(result.Value.Farm);
        Assert.Null(result.Value.CargoJourney);
        Assert.Null(result.Value.Warehouse);
        Assert.Null(result.Value.Market);
        var priceLineage = Assert.Single(result.Value.SourceLineage);
        Assert.Equal("public-data:kamis-domestic-price", priceLineage.SourceKey);
        Assert.Equal(
            new DateTimeOffset(2026, 8, 9, 0, 0, 0, TimeSpan.Zero),
            priceLineage.ObservedAt);
    }

    [Fact]
    public void 선택재배는_stableId로만찾고_ProductStableId부재를_Unverified로표시한다()
    {
        var result = new 감자생산유통WorldProjector().Project(new(
            FarmPerspective(),
            DomesticPrice(),
            "cultivation:a.potato.2026",
            감자생산유통SourceModeCodes.OperationalProjection,
            감자생산유통LinkageStatusCodes.Unverified,
            DateTimeOffset.UtcNow));

        Assert.True(result.IsSuccess);
        Assert.Equal("farm:a", result.Value.Farm!.FarmStableId);
        Assert.Equal("farm-plot:a.1", result.Value.Farm.PlotStableId);
        Assert.Equal("crop-reference-category:fc01", result.Value.Farm.CropReferenceStableId);
        Assert.Equal(감자생산유통LinkageStatusCodes.Unverified,
            result.Value.Farm.ProductLinkageStatusCode);
        Assert.Contains(result.Value.Limitations,
            message => message.Contains("ProductStableId", StringComparison.Ordinal));
    }

    [Fact]
    public void SimulationLinked는_명시적재배선택없이는_생성되지않는다()
    {
        var result = new 감자생산유통WorldProjector().Project(new(
            FarmPerspective(),
            DomesticPrice(),
            null,
            감자생산유통SourceModeCodes.SimulationFixture,
            감자생산유통LinkageStatusCodes.SimulationLinked,
            DateTimeOffset.UtcNow));

        Assert.True(result.IsFailed);
        Assert.Equal("PotatoJourneyLinkedCultivationRequired", result.Errors[0].Message);
    }

    [Fact]
    public void OperationalSource는_SimulationLinked를_사칭할수없다()
    {
        var result = new 감자생산유통WorldProjector().Project(new(
            FarmPerspective(),
            DomesticPrice(),
            "cultivation:a.potato.2026",
            감자생산유통SourceModeCodes.OperationalProjection,
            감자생산유통LinkageStatusCodes.SimulationLinked,
            DateTimeOffset.UtcNow));

        Assert.True(result.IsFailed);
        Assert.Equal("PotatoJourneySimulationSourceRequired", result.Errors[0].Message);
    }

    [Fact]
    public void 선택재배가_승인된Farm관점에없으면_Projection을거부한다()
    {
        var result = new 감자생산유통WorldProjector().Project(new(
            FarmPerspective(),
            DomesticPrice(),
            "cultivation:other.potato.2026",
            감자생산유통SourceModeCodes.OperationalProjection,
            감자생산유통LinkageStatusCodes.Unverified,
            DateTimeOffset.UtcNow));

        Assert.True(result.IsFailed);
        Assert.Equal("PotatoJourneyCultivationNotFound", result.Errors[0].Message);
    }

    [Fact]
    public async Task 조회UseCase는_감자Hs와조회범위를_가격Source에전달한다()
    {
        var information = new FakeInformationService(DomesticPrice());
        var useCase = new 감자생산유통World조회UseCase(
            new FakeFarmPerspectiveUseCase(Result.Ok(FarmPerspective())),
            information,
            new 감자생산유통WorldProjector());

        var result = await useCase.조회Async(new 감자생산유통World조회요청
        {
            ReferenceDate = "2026-08-09",
            LookbackDays = 21,
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("0701", information.LastRequest!.HsCode);
        Assert.Equal("2026-08-09", information.LastRequest.ReferenceDate);
        Assert.Equal(21, information.LastRequest.LookbackDays);
        Assert.Equal(감자생산유통LinkageStatusCodes.ProductOnly, result.Value.LinkageStatusCode);
    }

    [Fact]
    public async Task 인증된Farm조회가실패하면_가격Source를호출하지않는다()
    {
        var unauthorized = Result.Fail<FarmProducerPerspectiveResponse>(
            new Error("로그인 사용자 인증 정보가 필요합니다.")
                .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));
        var information = new FakeInformationService(DomesticPrice());
        var useCase = new 감자생산유통World조회UseCase(
            new FakeFarmPerspectiveUseCase(unauthorized),
            information,
            new 감자생산유통WorldProjector());

        var result = await useCase.조회Async(new 감자생산유통World조회요청());

        Assert.True(result.IsFailed);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Errors[0].Metadata["StatusCode"]);
        Assert.Null(information.LastRequest);
    }

    [Fact]
    public async Task 권한범위밖재배선택은_404로반환한다()
    {
        var information = new FakeInformationService(DomesticPrice());
        var useCase = new 감자생산유통World조회UseCase(
            new FakeFarmPerspectiveUseCase(Result.Ok(FarmPerspective())),
            information,
            new 감자생산유통WorldProjector());

        var result = await useCase.조회Async(new 감자생산유통World조회요청
        {
            CultivationStableId = "cultivation:not-authorized",
        });

        Assert.True(result.IsFailed);
        Assert.Equal(StatusCodes.Status404NotFound, result.Errors[0].Metadata["StatusCode"]);
        Assert.Null(information.LastRequest);
    }

    [Fact]
    public void Contract에는_개인정보와_asset경로_실행필드가없다()
    {
        var names = typeof(감자생산유통WorldResponse).GetProperties()
            .Concat(typeof(감자재배WorldResponse).GetProperties())
            .Concat(typeof(감자상품WorldResponse).GetProperties())
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("OwnerUserId", names);
        Assert.DoesNotContain("Address", names);
        Assert.DoesNotContain("Latitude", names);
        Assert.DoesNotContain("Longitude", names);
        Assert.DoesNotContain("PrefabPath", names);
        Assert.DoesNotContain("AnimatorParameter", names);
        Assert.DoesNotContain("ConfirmCommand", names);
    }

    private static FarmProducerPerspectiveResponse FarmPerspective()
        => new(
            "role-perspective:farm.producer",
            8,
            "Producer",
            "farm",
            RolePerspectiveViewerScopeCodes.AuthorizedParty,
            RolePerspectiveSourceTypeCodes.OperationalProjection,
            "authorized-farm-producer:8.1",
            new DateTimeOffset(2026, 8, 10, 0, 0, 0, TimeSpan.Zero),
            [
                new FarmResponse(
                    "farm:a",
                    4,
                    "A 농장",
                    FarmProducerStatusCodes.Operating,
                    [
                        new FarmPlotResponse(
                            "farm-plot:a.1",
                            5,
                            "1번 밭",
                            "soil-profile:a",
                            [
                                new FarmCultivationResponse(
                                    "cultivation:a.potato.2026",
                                    6,
                                    "감자",
                                    "crop-reference-category:fc01",
                                    "nongsaro:crop-ebook",
                                    "Growing",
                                    new DateOnly(2026, 4, 1),
                                    new DateOnly(2026, 9, 1)),
                            ],
                            [
                                new FarmSensorResponse(
                                    "sensor:a.soil-moisture.1",
                                    7,
                                    "SoilMoisture",
                                    "Active",
                                    new FarmSensorObservationResponse(
                                        18.5m,
                                        "Percent",
                                        new DateTimeOffset(2026, 8, 10, 0, 0, 0, TimeSpan.Zero),
                                        "Fresh",
                                        FarmSensorConditionCodes.Dry,
                                        "soil-water-rule:3",
                                        "SOIL-WATER-001",
                                        "Medium",
                                        "토성과 생육 단계에 따라 해석 범위가 달라집니다.")),
                            ])
                    ])
            ],
            []);

    private static AgriculturalFisheriesDomesticPriceResponse DomesticPrice()
        => new()
        {
            Success = true,
            StatusCode = "Ready",
            HsCode = "0701",
            Item = new AgriculturalFisheriesItemResponse
            {
                HsPrefix = "0701",
                ProductName = "감자",
                MatchQualityCode = "ExactCommodity",
                MatchQualityLabel = "동일 품목",
                Note = "감자의 국내 유통가격을 사용합니다.",
            },
            Price = new AtDomesticFoodPriceLookupResult
            {
                Success = true,
                StartDate = "20260801",
                EndDate = "20260809",
                DataSource = "한국농수산식품유통공사(aT) 일별 도·소매 가격정보",
                Wholesale = new AtDomesticFoodPriceAggregate
                {
                    PriceTypeCode = "Wholesale",
                    PriceTypeLabel = "도매",
                    AverageKrwPerKg = 2450m,
                    MinimumKrwPerKg = 2200m,
                    MaximumKrwPerKg = 2700m,
                    SampleCount = 8,
                    LatestSurveyDate = "20260809",
                },
            },
            Notices = ["정보 제공용 가격입니다."],
            InformationOnly = true,
        };

    private sealed class FakeFarmPerspectiveUseCase(
        Result<FarmProducerPerspectiveResponse> response)
        : IFarmProducerPerspectiveUseCase
    {
        public Task<Result<FarmProducerPerspectiveResponse>> QueryAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class FakeInformationService(
        AgriculturalFisheriesDomesticPriceResponse response)
        : IAgriculturalFisheriesInformationService
    {
        public AgriculturalFisheriesDomesticPriceRequest? LastRequest { get; private set; }

        public AgriculturalFisheriesInformationOverviewResponse GetOverview() => new();

        public AgriculturalFisheriesItemSearchResponse SearchItems(
            string? query,
            string? categoryCode,
            int page,
            int pageSize) => new();

        public AgriculturalFisheriesItemResponse? FindItem(string? hsCode) => response.Item;

        public 농수산시세정보원목록응답 GetMarketPriceSources(
            string? countryCode,
            string? marketStageCode) => new(DateTime.UtcNow, [], string.Empty);

        public 농수산시세비교판정응답 AssessMarketPriceComparability(
            string? leftSourceKey,
            string? rightSourceKey) => new(
                false,
                "NotEvaluated",
                leftSourceKey ?? string.Empty,
                rightSourceKey ?? string.Empty,
                false,
                false,
                "SeparateCards",
                [],
                []);

        public Task<AgriculturalFisheriesDomesticPriceResponse> GetDomesticPriceAsync(
            AgriculturalFisheriesDomesticPriceRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(response);
        }
    }
}
