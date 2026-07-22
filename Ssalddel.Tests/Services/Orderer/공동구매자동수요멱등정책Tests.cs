using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매자동수요멱등정책Tests
{
    [Fact]
    public void 같은키와같은입력은_이미처리된명령으로판단한다()
    {
        var command = 저장Command();
        var fingerprint = 공동구매자동수요멱등정책.저장요청지문(command);
        var history = new List<공동구매자동수요명령문서>();
        공동구매자동수요멱등정책.기록추가(
            history,
            command.요청멱등키,
            공동구매자동수요명령유형코드.저장,
            fingerprint,
            DateTime.UtcNow);

        var processed = 공동구매자동수요멱등정책.이미처리됨(
            history,
            command.요청멱등키,
            공동구매자동수요명령유형코드.저장,
            fingerprint);

        Assert.True(processed);
        Assert.Single(history);
    }

    [Fact]
    public void 같은키를다른수량에재사용하면_거부한다()
    {
        var first = 저장Command();
        var history = new List<공동구매자동수요명령문서>();
        공동구매자동수요멱등정책.기록추가(
            history,
            first.요청멱등키,
            공동구매자동수요명령유형코드.저장,
            공동구매자동수요멱등정책.저장요청지문(first),
            DateTime.UtcNow);
        var changed = 저장Command();
        changed.희망수량 = 20;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            공동구매자동수요멱등정책.이미처리됨(
                history,
                changed.요청멱등키,
                공동구매자동수요명령유형코드.저장,
                공동구매자동수요멱등정책.저장요청지문(changed)));

        Assert.Contains("멱등 키", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void 저장키를철회명령에재사용하면_거부한다()
    {
        var command = 저장Command();
        var history = new List<공동구매자동수요명령문서>();
        공동구매자동수요멱등정책.기록추가(
            history,
            command.요청멱등키,
            공동구매자동수요명령유형코드.저장,
            공동구매자동수요멱등정책.저장요청지문(command),
            DateTime.UtcNow);
        var withdrawal = new 공동구매자동수요철회Command
        {
            요청멱등키 = command.요청멱등키,
            수요출처키 = command.수요출처키,
            주문자키 = command.주문자키
        };

        Assert.Throws<InvalidOperationException>(() =>
            공동구매자동수요멱등정책.이미처리됨(
                history,
                withdrawal.요청멱등키,
                공동구매자동수요명령유형코드.철회,
                공동구매자동수요멱등정책.철회요청지문(withdrawal)));
    }

    private static 공동구매자동수요등록Command 저장Command()
        => new()
        {
            요청멱등키 = "save-demand-1",
            수요출처키 = "ingredient:garlic:seoul",
            상품키 = "garlic",
            상품명 = "마늘",
            주문자키 = "orderer-1",
            배송권키 = "seoul-mapogu",
            희망수량 = 3,
            수량단위 = "kg",
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제
        };
}
