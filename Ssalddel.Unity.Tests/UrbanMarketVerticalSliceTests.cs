using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

public sealed class UrbanMarketVerticalSliceTests
{
    [Fact]
    public async Task Simulated_도심마트는_진열대_3개와_출처를_제공한다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();

        Assert.Equal(도심마트SourceTypeCodes.SimulatedFixture, model.SourceTypeCode);
        Assert.Equal(3, model.상품목록.Length);
        Assert.All(model.상품목록, product => Assert.StartsWith("SIMULATED", product.SourceName));
        Assert.All(model.상품목록, product => Assert.NotEqual(default, product.EvidenceAsOf));
        Assert.Empty(new 도심마트ScreenModelValidator().Validate(model));
    }

    [Fact]
    public async Task 감자_진열대는_20kg_가격과_재고를_표시할_수_있다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();

        var potato = Assert.Single(model.상품목록, product => product.상품명 == "감자");
        Assert.Equal("20kg", potato.포장표시);
        Assert.Equal(35000m, potato.가격);
        Assert.Equal("KRW", potato.통화Code);
        Assert.Equal(12, potato.재고수량);
        Assert.Equal("상자", potato.재고단위);
        Assert.Equal(재고상태Codes.InStock, potato.재고상태Code);
    }

    [Fact]
    public async Task 중복_상품_StableId는_잘못된_ScreenModel로_거부한다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();
        model.상품목록[1].StableId = model.상품목록[0].StableId;

        var errors = new 도심마트ScreenModelValidator().Validate(model);

        Assert.Contains("DuplicateProductStableId:" + model.상품목록[0].StableId, errors);
    }

    [Fact]
    public async Task 상품_출처가_없으면_표현계약이_거부된다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();
        model.상품목록[0].SourceName = string.Empty;

        var errors = new 도심마트ScreenModelValidator().Validate(model);

        Assert.Contains("SourceNameMissing:" + model.상품목록[0].StableId, errors);
    }

    [Fact]
    public async Task 상품목록이_null이면_명시적_오류로_거부한다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();
        model.상품목록 = null!;

        var errors = new 도심마트ScreenModelValidator().Validate(model);

        Assert.Contains("ProductListMissing", errors);
    }

    [Fact]
    public async Task null_상품은_index가_있는_명시적_오류로_거부한다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();
        model.상품목록[1] = null!;

        var errors = new 도심마트ScreenModelValidator().Validate(model);

        Assert.Contains("ProductMissing:1", errors);
    }

    [Fact]
    public async Task 마트_생성시각이_없으면_표현계약이_거부된다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();
        model.GeneratedAt = default;

        var errors = new 도심마트ScreenModelValidator().Validate(model);

        Assert.Contains("MarketGeneratedAtMissing", errors);
    }

    [Fact]
    public async Task 상품_근거시각이_없으면_표현계약이_거부된다()
    {
        var model = await new Simulated도심마트조회UseCase().조회Async();
        model.상품목록[0].EvidenceAsOf = default;

        var errors = new 도심마트ScreenModelValidator().Validate(model);

        Assert.Contains("EvidenceAsOfMissing:" + model.상품목록[0].StableId, errors);
    }
}
