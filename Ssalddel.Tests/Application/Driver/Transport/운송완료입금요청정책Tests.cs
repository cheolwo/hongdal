using System.Text.Json;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Application.Shipper.Payment;
using Ssalddel.Contracts.Shipper.Request;
using 살뜰.도메인.공통;
using 살뜰.도메인.화주;

namespace Ssalddel.Tests.Application.Driver.Transport;

public class 운송완료입금요청정책Tests
{
    [Fact]
    public void 입금요청대상인가_운송완료후정산이고_플랫폼수납이면_true()
    {
        var request = new 화주운송의뢰
        {
            정산시점 = 정산시점.운송완료후정산.ToString(),
            수납주체 = 수납주체.플랫폼.ToString(),
            결제상태 = 상태값.결제상태.결제대기,
            정산상태 = 운임정산상태.청구대기.ToString()
        };

        Assert.True(운송완료입금요청정책.입금요청대상인가(request));
    }

    [Fact]
    public void 입금요청대상인가_이미입금대기이면_false()
    {
        var request = new 화주운송의뢰
        {
            정산시점 = 정산시점.운송완료후정산.ToString(),
            수납주체 = 수납주체.플랫폼.ToString(),
            결제상태 = 상태값.결제상태.결제대기,
            정산상태 = 운임정산상태.입금대기.ToString()
        };

        Assert.False(운송완료입금요청정책.입금요청대상인가(request));
    }

    [Fact]
    public void 조기입금요청대상인가_상차완료이고_화주승인된_플랫폼정산이면_true()
    {
        var request = new 화주운송의뢰
        {
            정산시점 = 정산시점.운송완료후정산.ToString(),
            수납주체 = 수납주체.플랫폼.ToString(),
            결제상태 = 상태값.결제상태.결제대기,
            정산상태 = 운임정산상태.후불승인완료.ToString(),
            배차상태 = 상태값.배차상태.상차완료
        };

        Assert.True(운송완료입금요청정책.조기입금요청대상인가(request));
    }

    [Theory]
    [InlineData("후불승인대기")]
    [InlineData("인수증대기")]
    public void 조기입금요청대상인가_화주승인전이면_false(string 정산상태값)
    {
        var request = new 화주운송의뢰
        {
            정산시점 = 정산시점.운송완료후정산.ToString(),
            수납주체 = 수납주체.플랫폼.ToString(),
            결제상태 = 상태값.결제상태.결제대기,
            정산상태 = 정산상태값,
            배차상태 = 상태값.배차상태.상차완료
        };

        Assert.False(운송완료입금요청정책.조기입금요청대상인가(request));
    }

    [Fact]
    public void 상차완료조기정산_스모크_입금대기후_중복요청을_막는다()
    {
        var request = new 화주운송의뢰
        {
            정산시점 = 정산시점.운송완료후정산.ToString(),
            수납주체 = 수납주체.플랫폼.ToString(),
            결제상태 = 상태값.결제상태.결제대기,
            정산상태 = 운임정산상태.후불승인완료.ToString(),
            배차상태 = 상태값.배차상태.상차완료,
            결제예정금액 = 120000
        };

        Assert.True(화주운송결제진행정책.상차완료이후인가(request.배차상태));
        var 조기판정 = 운송완료입금요청정책.입금요청가능여부(request, 운송입금요청종류.상차완료조기정산);
        Assert.True(조기판정.가능);
        Assert.Equal(120000, 운송완료입금요청정책.입금요청금액(request));

        request.정산상태 = 운임정산상태.입금대기.ToString();

        var 조기중복판정 = 운송완료입금요청정책.입금요청가능여부(request, 운송입금요청종류.상차완료조기정산);
        var 일반중복판정 = 운송완료입금요청정책.입금요청가능여부(request, 운송입금요청종류.운송완료후정산);
        Assert.False(조기중복판정.가능);
        Assert.False(일반중복판정.가능);
        Assert.Equal("이미 입금 요청 또는 정산 처리된 의뢰입니다.", 조기중복판정.사유);
    }

    [Theory]
    [InlineData("선결제", "플랫폼")]
    [InlineData("운송완료후정산", "기사")]
    public void 입금요청대상인가_정산시점이나_수납주체가_다르면_false(string 정산시점값, string 수납주체값)
    {
        var request = new 화주운송의뢰
        {
            정산시점 = 정산시점값,
            수납주체 = 수납주체값,
            결제상태 = 상태값.결제상태.결제대기,
            정산상태 = 운임정산상태.청구대기.ToString()
        };

        Assert.False(운송완료입금요청정책.입금요청대상인가(request));
    }

    [Fact]
    public void 알림예약목록_완료후_1일_3일_7일을_예약한다()
    {
        var completedAt = new DateTime(2026, 7, 9, 3, 0, 0, DateTimeKind.Utc);

        var schedules = 운송완료입금요청정책.알림예약목록(completedAt);

        Assert.Equal([1, 3, 7], schedules.Select(x => x.경과일수).ToArray());
        Assert.Equal(completedAt.AddDays(1), schedules[0].예약시각Utc);
        Assert.Equal(completedAt.AddDays(3), schedules[1].예약시각Utc);
        Assert.Equal(completedAt.AddDays(7), schedules[2].예약시각Utc);
    }

    [Fact]
    public void 주문번호생성_토스가상계좌용_orderId를_안정적으로_만든다()
    {
        var orderId = 운송완료입금요청정책.주문번호생성("HD-2026-001", 15);

        Assert.Equal("ssalddel_va_HD2026001_15", orderId);
    }

    [Fact]
    public void 원본응답초안Json_토스가상계좌_처리상태를_구조화한다()
    {
        var json = 운송완료입금요청정책.원본응답초안Json("HD-2026-001", "pay-1", "order-1");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(운송완료입금요청정책.토스가상계좌흐름, root.GetProperty("paymentFlow").GetString());
        Assert.Equal("TossPayments", root.GetProperty("provider").GetString());
        Assert.Equal("HD-2026-001", root.GetProperty("requestId").GetString());
        Assert.Equal("pay-1", root.GetProperty("paymentId").GetString());
        Assert.Equal("order-1", root.GetProperty("orderId").GetString());
        Assert.Equal("PendingTossVirtualAccountIssue", root.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("상차완료")]
    [InlineData("운송중")]
    [InlineData("하차지도착")]
    [InlineData("하차완료")]
    [InlineData("인수완료")]
    public void 화주운송결제진행정책_상차완료이후상태면_true(string 배차상태)
    {
        Assert.True(화주운송결제진행정책.상차완료이후인가(배차상태));
    }

    [Theory]
    [InlineData("미시작")]
    [InlineData("매칭중")]
    [InlineData("배차확정")]
    public void 화주운송결제진행정책_상차완료전상태면_false(string 배차상태)
    {
        Assert.False(화주운송결제진행정책.상차완료이후인가(배차상태));
    }
}
