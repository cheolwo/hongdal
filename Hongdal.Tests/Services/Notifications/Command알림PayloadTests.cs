using System.Text.Json;
using 홍달.Services.Notifications;

namespace Hongdal.Tests.Services.Notifications;

public class Command알림PayloadTests
{
    [Fact]
    public void Parse_예약시각이_미래면_아직_발송대상으로_보지_않는다()
    {
        var now = new DateTime(2026, 7, 9, 3, 0, 0, DateTimeKind.Utc);
        var payloadJson = JsonSerializer.Serialize(new
        {
            targetUserId = "shipper-1",
            requestId = "request-1",
            scheduledAtUtc = now.AddMinutes(30),
            channels = new[] { "Push" }
        });

        var payload = Command알림Payload.Parse(payloadJson);

        Assert.True(payload.IsScheduledForFuture(now));
    }

    [Fact]
    public void Parse_입금요청_필드와_수신번호_기본값을_읽는다()
    {
        var payloadJson = JsonSerializer.Serialize(new
        {
            알림유형 = Command알림FeatureNames.운송완료입금요청,
            shipperUserId = "shipper-1",
            driverId = "driver-1",
            requestId = "request-1",
            paymentId = "payment-1",
            orderId = "order-1",
            paymentFlow = "TossPayments.VirtualAccount",
            amount = 123000,
            reminderDay = 3,
            pickupContactPhone = "010-1111-2222",
            channels = new[] { "Push", "AlimTalk" }
        });

        var payload = Command알림Payload.Parse(payloadJson);

        Assert.Equal(Command알림FeatureNames.운송완료입금요청, payload.NotificationType);
        Assert.Equal("shipper-1", payload.TargetUserId);
        Assert.Equal("payment-1", payload.PaymentId);
        Assert.Equal("order-1", payload.OrderId);
        Assert.Equal("TossPayments.VirtualAccount", payload.PaymentFlow);
        Assert.Equal(123000, payload.Amount);
        Assert.Equal("123,000원", payload.AmountText);
        Assert.Equal(3, payload.ReminderDay);
        Assert.Equal("010-1111-2222", payload.RecipientPhone);
        Assert.Contains("AlimTalk", payload.Channels);
    }
}
