using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class AgriculturalFisheriesInformationServiceTests
{
    [Fact]
    public void 개요는_정보제공단계와_주선기능비활성화를_명시한다()
    {
        var service = CreateService(
            new StubDomesticPriceLookupService(),
            new PublicDataOptions
            {
                AtFoodPrices = new AtFoodPricesOptions { ServiceKey = "configured" },
                UsdaNassQuickStats = new UsdaNassQuickStatsOptions { ApiKey = "configured" }
            });

        var result = service.GetOverview();

        Assert.Equal("InformationFoundation", result.StageCode);
        Assert.True(result.IsReadOnly);
        Assert.True(result.AllowsReadinessRecordWrites);
        Assert.False(result.AllowsTransactionExecution);
        Assert.False(result.IsBrokerageEnabled);
        Assert.Contains("US", result.SupportedMarketCodes);
        Assert.Contains("AU", result.SupportedMarketCodes);
        Assert.Contains("주선", result.BrokerageBoundaryNote, StringComparison.Ordinal);
        Assert.Contains(result.DataSources, source =>
            source.Key == "at-daily-wholesale-retail-food-price"
            && source.IsConfigured
            && source.StatusCode == "Ready");
        Assert.Contains(result.DataSources, source =>
            source.Key == 미국농수산가격출처Keys.UsdaNassQuickStats
            && source.IsConfigured
            && source.StatusCode == "Ready");
        Assert.Contains(result.DataSources, source =>
            source.Key == 호주농수산식품가격출처Keys.AbsConsumerPriceIndex
            && source.IsConfigured
            && source.StatusCode == "IntegratedApi");
        Assert.Contains(result.DataSources, source =>
            source.Key == 호주농수산식품가격출처Keys.AbaresFisheriesAquacultureStatistics
            && !source.IsConfigured
            && source.StatusCode == "DownloadAvailable");
        Assert.Contains(result.Capabilities, capability =>
            capability.Code == "MeatImportReadinessCollaboration" && capability.AvailableNow);
        Assert.Contains(result.Capabilities, capability =>
            capability.Code == "UnitedStatesOperatorInformationSources"
            && capability.AvailableNow
            && capability.Endpoint ==
                "GET /api/v1/agricultural-fisheries/us-operator-information-sources");
        Assert.Contains(result.Capabilities, capability =>
            capability.Code == "AustraliaFoodPriceIndexes"
            && capability.AvailableNow
            && capability.Endpoint ==
                "GET /api/v1/agricultural-fisheries/au-food-price-indexes");
        Assert.Contains(result.Capabilities, capability =>
            capability.Code == "FreightBrokerage" && !capability.AvailableNow);
        Assert.NotEmpty(result.BrokerageReadinessRequirements);
    }

    [Fact]
    public void 품목검색은_수산물분류와_품목명을_함께적용한다()
    {
        var service = CreateService(new StubDomesticPriceLookupService());

        var result = service.SearchItems("오징어", "600", 1, 20);

        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Equal("600", item.CategoryCode));
        Assert.All(result.Items, item => Assert.Equal("수산물", item.CategoryLabel));
        Assert.All(result.Items, item => Assert.Contains("오징어", item.ProductName, StringComparison.Ordinal));
        Assert.All(result.Items, item => Assert.True(item.InformationOnly));
    }

    [Fact]
    public void 시세정보원은_미국_생산자_도매_소매가격을_서로다른시장단계로등록한다()
    {
        var service = CreateService(new StubDomesticPriceLookupService());

        var result = service.GetMarketPriceSources("US", null);

        Assert.Contains(result.Sources, source =>
            source.SourceKey == 농수산시세정보원Keys.UsdaNass생산자수취가격
            && source.MarketStageCode == 농수산시세시장단계Codes.생산자수취
            && source.IntegrationStateCode == 농수산시세연동상태Codes.Archive연동됨);
        Assert.Contains(result.Sources, source =>
            source.SourceKey == 농수산시세정보원Keys.UsdaAms도매터미널가격
            && source.MarketStageCode == 농수산시세시장단계Codes.도매터미널
            && source.RequiresCredential
            && source.IntegrationStateCode == 농수산시세연동상태Codes.Archive연동됨);
        Assert.Contains(result.Sources, source =>
            source.SourceKey == 농수산시세정보원Keys.UsdaAms소매광고가격
            && source.MarketStageCode == 농수산시세시장단계Codes.소매광고
            && source.Limitations.Any(limit =>
                limit.Contains("광고·프로모션 가격", StringComparison.Ordinal)));
        Assert.Contains(result.Sources, source =>
            source.SourceKey == 농수산시세정보원Keys.Bls소비자평균소매가격
            && source.MarketStageCode == 농수산시세시장단계Codes.소비자평균소매
            && !source.RequiresCredential
            && source.IntegrationStateCode == 농수산시세연동상태Codes.Archive연동됨);
        Assert.All(result.Sources, source =>
            Assert.Equal(
                농수산시세발행정책Codes.검토후발행,
                source.PublicationPolicyCode));
    }

    [Fact]
    public void 생산자수취가격과_도매터미널가격은_차액계산없이_참고병렬표시한다()
    {
        var service = CreateService(new StubDomesticPriceLookupService());

        var result = service.AssessMarketPriceComparability(
            농수산시세정보원Keys.UsdaNass생산자수취가격,
            농수산시세정보원Keys.UsdaAms도매터미널가격);

        Assert.True(result.Success);
        Assert.Equal(농수산시세비교판정Codes.참고병렬표시, result.StatusCode);
        Assert.False(result.CanBecomeDirectlyComparable);
        Assert.False(result.AllowsDifferenceCalculation);
        Assert.Equal("SideBySideWithCaveat", result.DisplayModeCode);
        Assert.Contains(result.Notices, notice =>
            notice.Contains("시장 단계", StringComparison.Ordinal));
    }

    [Fact]
    public void 같은정보원도_품목규격지역기간단위검증전에는_차액계산을허용하지않는다()
    {
        var service = CreateService(new StubDomesticPriceLookupService());

        var result = service.AssessMarketPriceComparability(
            농수산시세정보원Keys.UsdaAms소매광고가격,
            농수산시세정보원Keys.UsdaAms소매광고가격);

        Assert.True(result.Success);
        Assert.Equal(농수산시세비교판정Codes.차원검증필요, result.StatusCode);
        Assert.True(result.CanBecomeDirectlyComparable);
        Assert.False(result.AllowsDifferenceCalculation);
        Assert.Contains("품종·등급·규격", result.RequiredDimensions);
    }

    [Fact]
    public async Task 국내가격조회는_HS품목을_aT요청으로변환하고_출처주의를제공한다()
    {
        var lookup = new StubDomesticPriceLookupService
        {
            Result = new AtDomesticFoodPriceLookupResult
            {
                Success = true,
                CategoryCode = "600",
                ItemCode = "611",
                ItemName = "고등어",
                StartDate = "20260701",
                EndDate = "20260714",
                Retail = new AtDomesticFoodPriceAggregate
                {
                    PriceTypeCode = "01",
                    PriceTypeLabel = "국내 소매가격",
                    LatestSurveyDate = "20260714",
                    AverageKrwPerKg = 9_000m,
                    MinimumKrwPerKg = 8_500m,
                    MaximumKrwPerKg = 9_500m,
                    SampleCount = 3
                }
            }
        };
        var service = CreateService(lookup);

        var result = await service.GetDomesticPriceAsync(
            new AgriculturalFisheriesDomesticPriceRequest
            {
                HsCode = "0303.54-0000",
                ReferenceDate = "20260714",
                LookbackDays = 14
            });

        Assert.True(result.Success);
        Assert.Equal("Complete", result.StatusCode);
        Assert.Equal("고등어", result.Item!.ProductName);
        Assert.Equal("600", lookup.LastRequest!.CategoryCode);
        Assert.Equal("611", lookup.LastRequest.ItemCode);
        Assert.Equal("20260701", lookup.LastRequest.StartDate);
        Assert.Equal("20260714", lookup.LastRequest.EndDate);
        Assert.Contains("05", lookup.LastRequest.VarietyCodes);
        Assert.Contains(result.Notices, notice => notice.Contains("주문·매입·운송 견적이 아닙니다", StringComparison.Ordinal));
    }

    [Fact]
    public async Task 연결되지않은_HS코드는_외부조회없이_검토필요를알린다()
    {
        var lookup = new StubDomesticPriceLookupService();
        var service = CreateService(lookup);

        var result = await service.GetDomesticPriceAsync(
            new AgriculturalFisheriesDomesticPriceRequest
            {
                HsCode = "2106909099",
                ReferenceDate = "20260714"
            });

        Assert.False(result.Success);
        Assert.Equal("MappingRequired", result.StatusCode);
        Assert.Null(lookup.LastRequest);
        Assert.Contains("연결", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static AgriculturalFisheriesInformationService CreateService(
        StubDomesticPriceLookupService lookup,
        PublicDataOptions? options = null)
        => new(
            new FoodPriceCrosswalkCatalog(),
            lookup,
            Options.Create(options ?? new PublicDataOptions()));

    private sealed class StubDomesticPriceLookupService : IAtDomesticFoodPriceLookupService
    {
        public AtDomesticFoodPriceLookupResult Result { get; init; } = new()
        {
            Success = false,
            ErrorMessage = "not configured"
        };

        public AtDomesticFoodPriceRequest? LastRequest { get; private set; }

        public Task<AtDomesticFoodPriceLookupResult> LookupAsync(
            AtDomesticFoodPriceRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(Result);
        }
    }
}
