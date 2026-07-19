using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using 살뜰.도메인.설정;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed partial class 배차수락사후처리EventHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private async Task 화주수락알림의도적재Async(배차수락됨Event notification, CancellationToken cancellationToken)
    {
        var dispatchRequest = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == notification.의뢰Id, cancellationToken);
        if (dispatchRequest is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var payloadJson = JsonSerializer.Serialize(new
        {
            알림유형 = "배차수락",
            TargetUserId = notification.화주Id,
            ShipperUserId = notification.화주Id,
            DriverId = notification.기사Id,
            RequestId = notification.의뢰Id,
            CargoType = dispatchRequest.화물종류,
            PickupAddress = dispatchRequest.픽업_도로명주소,
            PickupAddressDetail = dispatchRequest.픽업_상세주소,
            PickupContactName = dispatchRequest.픽업_연락처_이름,
            PickupContactPhone = dispatchRequest.픽업_연락처_전화번호,
            PickupWindowStartUtc = dispatchRequest.픽업_시간창_시작일시,
            PickupWindowEndUtc = dispatchRequest.픽업_시간창_종료일시,
            AcceptedAtUtc = notification.발생시각Utc,
            Title = "기사님이 운송 의뢰를 수락했습니다.",
            Body = $"{dispatchRequest.화물종류} 운송 의뢰가 수락되었습니다. 상차 준비를 확인해 주세요.",
            Channels = new[] { "Push", "AlimTalk" }
        }, JsonOptions);

        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = nameof(배차수락Command),
            EventName = nameof(배차수락됨Event),
            FeatureName = "DispatchAccepted",
            Target = "Shipper",
            PayloadJson = payloadJson,
            Status = "Pending",
            TraceId = notification.TraceId,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
