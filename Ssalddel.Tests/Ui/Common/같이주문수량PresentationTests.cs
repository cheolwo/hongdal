using Ssalddel.Ui.Common.Areas.App.Models;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 같이주문수량PresentationTests
{
    [Theory]
    [InlineData(40, "25kg 상자", "40 25kg 상자")]
    [InlineData(100, "개", "100 개")]
    [InlineData(2.5, "망", "2.5 망")]
    public void 수량은_서버가준원거래단위를그대로보존한다(
        decimal quantity,
        string sourceUnit,
        string expected)
    {
        Assert.Equal(expected, 같이주문수량Presentation.수량(quantity, sourceUnit));
        Assert.Equal(
            $"{sourceUnit} · 원 거래단위 유지",
            같이주문수량Presentation.원거래단위(sourceUnit));
    }

    [Fact]
    public void 단위가없으면_kg로추정하지않고미확정으로표시한다()
    {
        Assert.Equal("12 단위 미확정", 같이주문수량Presentation.수량(12m, " "));
        Assert.Equal("공급자 원 거래단위 확인 전 미정", 같이주문수량Presentation.원거래단위(null));
    }

    [Fact]
    public void 추가필요수량이없으면_0으로추정하지않고미정으로표시한다()
    {
        Assert.Equal("미정", 같이주문수량Presentation.선택수량(null, "25kg 상자"));
    }
}
