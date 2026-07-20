using Ssalddel.Contracts.Mart;

namespace Ssalddel.Tests.Contracts.Common;

public sealed class 마트주문요청개인정보최소화Tests
{
    [Theory]
    [InlineData("UserId")]
    [InlineData("PhoneNumber")]
    [InlineData("Address")]
    [InlineData("Recipient")]
    [InlineData("PaymentMethod")]
    public void 등록계약은_사용자식별자와배송결제정보를받지않는다(string propertyName)
    {
        Assert.Null(typeof(마트주문요청등록요청).GetProperty(propertyName));
    }

    [Fact]
    public void 현재안내는_재고예약결제배송비실행과개인정보미수집을명시한다()
    {
        var notice = string.Join(" ", 마트주문요청안내.문구);

        Assert.Contains("재고", notice);
        Assert.Contains("결제", notice);
        Assert.Contains("배송", notice);
        Assert.Contains("주소", notice);
        Assert.Contains("수집하지 않습니다", notice);
    }
}
