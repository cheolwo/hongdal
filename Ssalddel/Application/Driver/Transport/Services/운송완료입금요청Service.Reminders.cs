using System.Text.Json;
using 살뜰.도메인.설정;

namespace Ssalddel.Application.Driver.Transport;

public sealed partial class 운송완료입금요청Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private async Task<int> 입금요청알림예약Async(
        살뜰.도메인.화주.화주운송의뢰 request,
        운송입금요청Context context,
        살뜰.도메인.결제.결제 payment,
        CancellationToken cancellationToken)
    {
        var existing = await _db.Command알림Outbox
            .Where(x => x.FeatureName == 운송완료입금요청정책.알림FeatureName
                        && x.Target == "Shipper")
            .Select(x => x.PayloadJson)
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var schedule in 운송완료입금요청정책.알림예약목록(context.발생시각Utc))
        {
            if (existing.Any(payload => IsSameReminder(payload, request.의뢰Id, schedule.경과일수)))
            {
                continue;
            }

            _db.Command알림Outbox.Add(new Command알림Outbox
            {
                CommandName = context.CommandName,
                EventName = context.EventName,
                FeatureName = 운송완료입금요청정책.알림FeatureName,
                Target = "Shipper",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    알림유형 = 운송완료입금요청정책.알림FeatureName,
                    TargetUserId = request.화주Id,
                    ShipperUserId = request.화주Id,
                    DriverId = context.기사Id,
                    RequestId = request.의뢰Id,
                    TransportId = context.운송Id,
                    PaymentId = payment.결제Id,
                    OrderId = payment.OrderId,
                    PaymentFlow = 운송완료입금요청정책.토스가상계좌흐름,
                    PaymentMethod = 운송완료입금요청정책.토스가상계좌결제수단,
                    SettlementTrigger = context.EventName,
                    SettlementMemo = context.정산메모,
                    Amount = payment.결제금액,
                    CargoType = request.화물종류,
                    PickupAddress = request.픽업_도로명주소,
                    DropoffAddress = request.하차_도로명주소,
                    RecipientPhone = request.픽업_연락처_전화번호,
                    ReminderDay = schedule.경과일수,
                    ReminderRound = schedule.회차,
                    ScheduledAtUtc = schedule.예약시각Utc,
                    CompletedAtUtc = context.발생시각Utc,
                    Title = $"{context.정산메모} 입금 요청 안내({schedule.경과일수}일차)",
                    Body = $"{request.화물종류} {context.정산메모} 금액 {payment.결제금액:N0}원을 토스페이먼츠 가상계좌 결제 흐름으로 입금해 주세요.",
                    Channels = new[] { "Push", "AlimTalk" }
                }, JsonOptions),
                Status = "Pending",
                TraceId = context.TraceId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            count++;
        }

        return count;
    }

    private static bool IsSameReminder(string payloadJson, string requestId, int reminderDay)
    {
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            var root = document.RootElement;
            return ReadString(root, "requestId") == requestId
                   && ReadInt(root, "reminderDay") == reminderDay;
        }
        catch
        {
            return false;
        }
    }

    private static string ReadString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int ReadInt(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var number)
            ? number
            : 0;
}
