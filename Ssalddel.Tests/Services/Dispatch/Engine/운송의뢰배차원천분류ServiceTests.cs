using 살뜰.도메인.공통;
using 살뜰.도메인.배차;
using 살뜰.Services.Dispatch.Engine;

namespace Ssalddel.Tests.Services.Dispatch.Engine;

public sealed class 운송의뢰배차원천분류ServiceTests
{
    private readonly 운송의뢰배차원천분류Service _service = new();

    [Theory]
    [InlineData(운송의뢰배차원천유형.화주운송의뢰)]
    [InlineData(운송의뢰배차원천유형.창고출고연계운송)]
    [InlineData(운송의뢰배차원천유형.판매채널출고)]
    [InlineData(운송의뢰배차원천유형.살뜰마트출고)]
    public void 창고출고성격의_화물운송은_출고예정대상으로_분류한다(string sourceType)
    {
        var result = _service.분류(CreateQueue(상태값.배차업무유형.용달운송, sourceType));

        Assert.Equal(상태값.배차업무유형.용달운송, result.배차업무유형);
        Assert.Equal("창고 출고 연계 운송", result.상위흐름);
        Assert.True(result.출고예정대상여부);
        Assert.True(result.창고선행작업필요);
    }

    [Theory]
    [InlineData(운송의뢰배차원천유형.수입화물운송)]
    [InlineData(운송의뢰배차원천유형.공동주문국내운송)]
    [InlineData(운송의뢰배차원천유형.Fcl연계운송)]
    [InlineData(운송의뢰배차원천유형.Lcl연계운송)]
    public void 수입통관성격의_화물운송은_반출가능화물로_분류한다(string sourceType)
    {
        var result = _service.분류(CreateQueue(상태값.배차업무유형.용달운송, sourceType));

        Assert.Equal("수입/통관 연계 운송", result.상위흐름);
        Assert.True(result.출고예정대상여부);
        Assert.False(result.창고선행작업필요);
    }

    [Theory]
    [InlineData(운송의뢰배차원천유형.음식점주문)]
    [InlineData(운송의뢰배차원천유형.음식주문)]
    public void 음식점주문은_음식점즉시배달로_분류한다(string sourceType)
    {
        var result = _service.분류(CreateQueue(상태값.배차업무유형.음식배달, sourceType));

        Assert.Equal(상태값.배차업무유형.음식배달, result.배차업무유형);
        Assert.Equal("음식점 즉시 배달", result.상위흐름);
        Assert.False(result.출고예정대상여부);
        Assert.False(result.창고선행작업필요);
    }

    [Theory]
    [InlineData(운송의뢰배차원천유형.살뜰마트주문)]
    [InlineData(운송의뢰배차원천유형.살뜰마트음식주문)]
    [InlineData(운송의뢰배차원천유형.살뜰마트포장완료주문)]
    public void 살뜰마트음식주문은_창고선행작업이_있는_즉시배송으로_분류한다(string sourceType)
    {
        var result = _service.분류(CreateQueue(상태값.배차업무유형.음식배달, sourceType));

        Assert.Equal("알뜰살뜰 마트 즉시배송", result.상위흐름);
        Assert.True(result.출고예정대상여부);
        Assert.True(result.창고선행작업필요);
    }

    [Theory]
    [InlineData(운송의뢰배차원천유형.살뜰마트주문)]
    [InlineData(운송의뢰배차원천유형.살뜰마트음식주문)]
    public void 살뜰마트주문은_포장완료전_음식배달배차를_시작하지_않는다(string sourceType)
    {
        var resolver = new 음식배달배차흐름Resolver();

        var result = resolver.Resolve(CreateQueue(상태값.배차업무유형.음식배달, sourceType));

        Assert.True(result.창고선행작업필요);
        Assert.False(result.배차시작가능);
        Assert.Contains("포장 완료 전", result.배차시작조건);
    }

    [Fact]
    public void 살뜰마트포장완료주문은_음식배달배차를_시작할_수_있다()
    {
        var resolver = new 음식배달배차흐름Resolver();

        var result = resolver.Resolve(CreateQueue(상태값.배차업무유형.음식배달, 운송의뢰배차원천유형.살뜰마트포장완료주문));

        Assert.True(result.창고선행작업필요);
        Assert.True(result.배차시작가능);
        Assert.Equal("알뜰살뜰 마트 포장 완료 배달", result.표시명);
    }

    private static 운송원장 CreateQueue(int businessType, string sourceType)
        => new()
        {
            의뢰Id = $"REQ-{sourceType}",
            배차업무유형 = businessType,
            원본의뢰유형 = sourceType
        };
}
