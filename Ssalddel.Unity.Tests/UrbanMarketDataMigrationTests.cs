using Ssalddel.Unity.Data;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class UrbanMarketDataMigrationTests
{
    [Fact]
    public void 공개상품DataMapper는_주문자용판매가능Projection의의미를_보존한다()
    {
        var snapshot = new 도심마트공개상품DataMapper().Map(Response());
        var product = Assert.Single(snapshot.상품목록);

        Assert.Equal(도심마트ProjectionAudienceCodes.OrdererPublic, snapshot.ProjectionAudienceCode);
        Assert.Equal(DataScopeKind.Global, snapshot.ScopeKind);
        Assert.Equal(DataRuntimeMode.Operational, snapshot.Mode);
        Assert.Equal("판매 가능 수량은 내부 재고가 아님", snapshot.QuantityDisclosure);
        Assert.Equal(12, product.투영판매가능수량);
        Assert.True(product.서버판매가능여부);
        Assert.Equal(
            도심마트QuantityMeaningCodes.ProjectedSaleAvailability,
            product.QuantityMeaningCode);
        Assert.Empty(new 도심마트공개상품DataSnapshotValidator().Validate(snapshot));
    }

    [Fact]
    public void 공개상품Data계약은_물리진열과내부재고필드를_만들지않는다()
    {
        var properties = typeof(도심마트공개상품Data)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("보관재고수량", properties);
        Assert.DoesNotContain("진열재고수량", properties);
        Assert.DoesNotContain("예약수량", properties);
        Assert.Contains("투영판매가능수량", properties);
    }

    [Fact]
    public async Task OperationalDataRepository는_ApiModel다음에_DataMapper를실행한다()
    {
        var client = new Client(Response());
        var repository = new 도심마트공개상품ApiDataRepository(
            client,
            new 도심마트공개상품DataMapper());

        var snapshot = await repository.조회Async();

        Assert.Equal(1, client.CallCount);
        Assert.Equal("market:urban-public", snapshot.StableId);
        Assert.StartsWith("public-products:", snapshot.DataRevision);
        Assert.Equal("mart-product:41", snapshot.상품목록[0].StableId);
    }

    [Fact]
    public async Task SimulationFixture도_ScreenModel이아닌_DataSnapshot을제공한다()
    {
        var snapshot = await new Simulated도심마트공개상품DataQuery().조회Async();

        Assert.Equal(DataRuntimeMode.Simulation, snapshot.Mode);
        Assert.Equal(3, snapshot.상품목록.Length);
        Assert.StartsWith("SIMULATED", snapshot.QuantityDisclosure);
        Assert.All(snapshot.상품목록, product =>
            Assert.Equal(
                도심마트QuantityMeaningCodes.ProjectedSaleAvailability,
                product.QuantityMeaningCode));
        Assert.Empty(new 도심마트공개상품DataSnapshotValidator().Validate(snapshot));
    }

    [Fact]
    public void 기존ApiMapperFacade는_DataSnapshot을거쳐_ScreenModel호환을유지한다()
    {
        var mapper = new 도심마트ApiMapper();

        var data = mapper.MapData(Response());
        var screen = mapper.Map(Response());

        Assert.Equal(data.StableId, screen.StableId);
        Assert.Equal(data.LegacyRevision, screen.Revision);
        Assert.Equal(data.상품목록[0].투영판매가능수량, screen.상품목록[0].재고수량);
        Assert.Equal(재고상태Codes.InStock, screen.상품목록[0].재고상태Code);
    }

    [Fact]
    public void DataMapper는_음수판매가능수량과_수량의미안내누락을_거부한다()
    {
        var negative = Response();
        negative.Items[0].판매가능수량 = -1;
        Assert.Equal(
            "UrbanMarketProjectedAvailableQuantityInvalid",
            Assert.Throws<InvalidOperationException>(() =>
                new 도심마트공개상품DataMapper().Map(negative)).Message);

        var missingDisclosure = Response();
        missingDisclosure.재고기준안내 = string.Empty;
        Assert.Equal(
            "ProjectedQuantityDisclosureMissing",
            Assert.Throws<InvalidOperationException>(() =>
                new 도심마트공개상품DataMapper().Map(missingDisclosure)).Message);
    }

    private static 도심마트목록ApiModel Response()
        => new()
        {
            TotalCount = 1,
            재고기준안내 = "판매 가능 수량은 내부 재고가 아님",
            Items =
            [
                new 도심마트상품ApiModel
                {
                    Id = 41,
                    상품명 = "감자",
                    판매단위 = "20kg",
                    판매가 = 35_000m,
                    판매가능수량 = 12,
                    판매가능여부 = true,
                    재고기준시각 = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                    수정시각 = DateTimeOffset.Parse("2026-08-08T01:05:00Z"),
                },
            ],
        };

    private sealed class Client(도심마트목록ApiModel response) : I도심마트ApiClient
    {
        public int CallCount { get; private set; }

        public Task<도심마트목록ApiModel> GetAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(response);
        }
    }
}
