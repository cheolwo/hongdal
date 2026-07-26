using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 같이주문용어Tests
{
    [Fact]
    public void 제품표시명은_같이주문이고_기존기술코드는_호환유지한다()
    {
        Assert.Equal("같이 주문", 같이주문용어.표시명);
        Assert.Equal("GroupPurchase", 같이주문용어.기술호환코드);
    }

    [Fact]
    public void 주문방식비교는_같이주문을_자동선택하지않고_별도동의를요구한다()
    {
        var response = new 주문방식비교응답();

        Assert.Equal("같이 주문", response.같이주문표시명);
        Assert.True(response.기본선택없음);
        Assert.True(response.자동같이주문금지);
        Assert.True(response.같이주문별도동의필수);
    }
}
