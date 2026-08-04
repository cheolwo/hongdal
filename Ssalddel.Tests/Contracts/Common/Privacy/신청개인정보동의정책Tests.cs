using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Tests.Contracts.Common.Privacy;

public sealed class 신청개인정보동의정책Tests
{
    [Theory]
    [InlineData(신청개인정보업무Codes.물류대행, "물류대행 신청", "창고")]
    [InlineData(신청개인정보업무Codes.운송대행, "운송대행 신청", "상차·하차 주소")]
    [InlineData(신청개인정보업무Codes.개별주문, "개별 주문 신청", "수령 주소")]
    public void 신청별로_목적항목기간과거부권을_구체적으로고지한다(
        string code,
        string name,
        string expectedItem)
    {
        var notice = 신청개인정보동의정책.For(code);

        Assert.Equal(name, notice.업무명);
        Assert.False(string.IsNullOrWhiteSpace(notice.수집이용목적));
        Assert.Contains(notice.수집항목, item => item.Contains(expectedItem, StringComparison.Ordinal));
        Assert.Contains("목적 달성", notice.보유이용기간, StringComparison.Ordinal);
        Assert.Contains("거부할 권리", notice.동의거부안내, StringComparison.Ordinal);
        Assert.Contains("지도와 공개정보 조회", notice.동의거부안내, StringComparison.Ordinal);
    }

    [Fact]
    public void 제3자제공과국외이전은_상대가정해진뒤별도절차로분리한다()
    {
        var notice = 신청개인정보동의정책.For(신청개인정보업무Codes.운송대행);

        Assert.Contains("이 단계에서는", notice.제3자제공안내, StringComparison.Ordinal);
        Assert.Contains("별도 동의", notice.제3자제공안내, StringComparison.Ordinal);
        Assert.Contains("이 단계에서는", notice.국외이전안내, StringComparison.Ordinal);
        Assert.Contains("이전 국가", notice.국외이전안내, StringComparison.Ordinal);
        Assert.Contains("draft", 신청개인정보동의정책.현재버전, StringComparison.Ordinal);
    }

    [Fact]
    public void 알수없는신청업무는_포괄동의로대체하지않는다()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            신청개인정보동의정책.For("unknown"));

    [Theory]
    [InlineData(신청개인정보업무Codes.물류대행, "Logistics assistance request", "warehouse")]
    [InlineData(신청개인정보업무Codes.운송대행, "Transportation assistance request", "Pickup and delivery")]
    [InlineData(신청개인정보업무Codes.개별주문, "Individual order request", "Delivery address")]
    public void 영문안내도_신청별목적항목과공유경계를_구체적으로고지한다(
        string code,
        string name,
        string expectedItem)
    {
        var notice = 신청개인정보동의정책.ForEnglish(code);

        Assert.Equal(name, notice.WorkName);
        Assert.False(string.IsNullOrWhiteSpace(notice.CollectionUsePurpose));
        Assert.Contains(notice.CollectionItems, item => item.Contains(expectedItem, StringComparison.Ordinal));
        Assert.Contains("retain", notice.RetentionPeriod, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("decline", notice.RefusalNotice, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sell or share", notice.ThirdPartyDisclosureNotice, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cross-border", notice.CrossBorderTransferNotice, StringComparison.OrdinalIgnoreCase);
    }
}
