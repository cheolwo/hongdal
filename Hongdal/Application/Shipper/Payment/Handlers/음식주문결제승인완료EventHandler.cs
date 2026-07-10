using Hongdal.Application.Shipper.Payment.Events;
using 홍달.도메인.결제;
using 홍달.도메인.설정;

namespace Hongdal.Application.Shipper.Payment.Handlers;

public sealed class 음식주문결제승인완료EventHandler : INotificationHandler<결제승인완료Event>
{
    private readonly HongdalContext _db;
    private readonly ILogger<음식주문결제승인완료EventHandler> _logger;

    public 음식주문결제승인완료EventHandler(HongdalContext db, ILogger<음식주문결제승인완료EventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(결제승인완료Event notification, CancellationToken cancellationToken)
    {
        if (notification.결제대상유형 != 결제공통정의.결제대상유형.음식주문)
        {
            return;
        }

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            결제Id = notification.결제Id,
            대상Id = notification.대상Id,
            금액 = notification.결제금액,
            통화 = notification.통화,
            승인시각Utc = notification.승인일시Utc,
            의도 = "FoodOrderPaymentApproved"
        });

        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = "결제승인완료EventHandler",
            EventName = nameof(결제승인완료Event),
            FeatureName = "FoodOrderPayment",
            Target = "HongdalFoodOrder",
            PayloadJson = payloadJson,
            Status = "Pending",
            TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "음식주문 결제승인 후처리 의도 적재 완료: 결제Id={결제Id}, 대상Id={대상Id}, 금액={금액}{통화}",
            notification.결제Id,
            notification.대상Id,
            notification.결제금액,
            notification.통화);
    }
}
