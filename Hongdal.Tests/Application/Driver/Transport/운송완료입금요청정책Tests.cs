using System.Text.Json;
using Hongdal.Application.Driver.Transport;
using Hongdal.Contracts.Shipper.Request;
using 홍달.도메인.공통;
using 홍달.도메인.화주;

namespace Hongdal.Tests.Application.Driver.Transport;

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

        Assert.Equal("hongdal_va_HD2026001_15", orderId);
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
}
