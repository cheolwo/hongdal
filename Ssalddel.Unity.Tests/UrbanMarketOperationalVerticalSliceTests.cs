using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

public sealed class UrbanMarketOperationalVerticalSliceTests
{
    [Fact]
    public async Task 공개마트Aggregate를_operational_ScreenModel로_투영한다()
    {
        var useCase = new Operational도심마트조회UseCase(
            new 도심마트ApiRepository(new Client(Response()), new 도심마트ApiMapper()));

        var result = await useCase.조회Async();

        Assert.Equal("market:urban-public", result.StableId);
        Assert.Equal(도심마트SourceTypeCodes.OperationalProjection, result.SourceTypeCode);
        Assert.Equal(2, result.상품목록.Length);
        Assert.Equal("mart-product:41", result.상품목록[0].StableId);
        Assert.Equal("Ssalddel 마트 공개 상품 API", result.상품목록[0].SourceName);
        Assert.Equal(재고상태Codes.OutOfStock, result.상품목록[1].재고상태Code);
    }

    [Fact]
    public void Mapper는_서버판매가능판정을_재고표현에_보존한다()
    {
        var response = Response();
        response.Items[0].판매가능여부 = false;

        var result = new 도심마트ApiMapper().Map(response);

        Assert.Equal(12, result.상품목록[0].재고수량);
        Assert.Equal(재고상태Codes.OutOfStock, result.상품목록[0].재고상태Code);
    }

    [Fact]
    public void Mapper는_상품목록누락과_잘못된Id를_거부한다()
    {
        Assert.Equal(
            "UrbanMarketProductListMissing",
            Assert.Throws<InvalidOperationException>(() =>
                new 도심마트ApiMapper().Map(new 도심마트목록ApiModel { Items = null! })).Message);

        var response = Response();
        response.Items[0].Id = 0;
        Assert.Equal(
            "UrbanMarketProductIdentityInvalid",
            Assert.Throws<InvalidOperationException>(() => new 도심마트ApiMapper().Map(response)).Message);
    }

    private static 도심마트목록ApiModel Response()
    {
        return new 도심마트목록ApiModel
        {
            TotalCount = 2,
            재고기준안내 = "판매 가능 수량 projection",
            Items =
            [
                Product(41, "감자", "20kg", 35_000m, 12, true, "2026-08-08T01:00:00Z"),
                Product(42, "양파", "10kg", 18_000m, 0, false, "2026-08-08T01:05:00Z"),
            ],
        };
    }

    private static 도심마트상품ApiModel Product(
        long id,
        string name,
        string unit,
        decimal price,
        int quantity,
        bool available,
        string asOf)
    {
        return new 도심마트상품ApiModel
        {
            Id = id,
            상품명 = name,
            판매단위 = unit,
            판매가 = price,
            판매가능수량 = quantity,
            판매가능여부 = available,
            재고기준시각 = DateTimeOffset.Parse(asOf),
            수정시각 = DateTimeOffset.Parse(asOf),
        };
    }

    private sealed class Client(도심마트목록ApiModel response) : I도심마트ApiClient
    {
        public Task<도심마트목록ApiModel> GetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }
}
