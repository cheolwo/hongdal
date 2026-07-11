using System.Text.Json;
using Hongdal.Contracts.Shipper.Request;
using 홍달.도메인.결제;
using 홍달.도메인.공통;
using 홍달.도메인.화주;

namespace Hongdal.Application.Driver.Transport;

public static class 운송완료입금요청정책
{
    public const string 알림FeatureName = "TransportSettlementDepositReminder";
    public const string 토스가상계좌결제수단 = "가상계좌";
    public const string 토스가상계좌흐름 = "TossPayments.VirtualAccount";

    private static readonly int[] ReminderDays = [1, 3, 7];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool 입금요청대상인가(화주운송의뢰 request)
        => 입금요청가능여부(request, 운송입금요청종류.운송완료후정산).가능;

    public static bool 조기입금요청대상인가(화주운송의뢰 request)
        => 입금요청가능여부(request, 운송입금요청종류.상차완료조기정산).가능;

    public static 운송입금요청판정 입금요청가능여부(화주운송의뢰 request, 운송입금요청종류 종류)
    {
        var 기본판정 = 입금요청기본가능여부(request);
        if (!기본판정.가능)
        {
            return 기본판정;
        }

        if (종류 == 운송입금요청종류.상차완료조기정산)
        {
            if (!string.Equals(request.배차상태, 상태값.배차상태.상차완료, StringComparison.Ordinal))
            {
                return 운송입금요청판정.불가("상차 완료 상태가 아닙니다.");
            }

            if (!화주승인된정산인가(request))
            {
                return 운송입금요청판정.불가("화주 승인된 정산 상태가 아닙니다.");
            }
        }

        return 운송입금요청판정.가능함();
    }

    private static 운송입금요청판정 입금요청기본가능여부(화주운송의뢰 request)
    {
        if (request.결제상태 == 상태값.결제상태.결제완료)
        {
            return 운송입금요청판정.불가("이미 결제완료된 의뢰입니다.");
        }

        if (string.Equals(request.정산상태, 운임정산상태.입금확인완료.ToString(), StringComparison.Ordinal)
            || string.Equals(request.정산상태, 운임정산상태.입금대기.ToString(), StringComparison.Ordinal)
            || string.Equals(request.정산상태, 운임정산상태.정산완료.ToString(), StringComparison.Ordinal)
            || string.Equals(request.정산상태, 운임정산상태.정산취소.ToString(), StringComparison.Ordinal))
        {
            return 운송입금요청판정.불가("이미 입금 요청 또는 정산 처리된 의뢰입니다.");
        }

        if (!string.Equals(request.정산시점, 정산시점.운송완료후정산.ToString(), StringComparison.Ordinal))
        {
            return 운송입금요청판정.불가("운송완료후정산 대상이 아닙니다.");
        }

        if (!string.Equals(request.수납주체, 수납주체.플랫폼.ToString(), StringComparison.Ordinal))
        {
            return 운송입금요청판정.불가("플랫폼 수납 대상이 아닙니다.");
        }

        return 운송입금요청판정.가능함();
    }

    private static bool 화주승인된정산인가(화주운송의뢰 request)
        => string.Equals(request.정산상태, 운임정산상태.후불승인완료.ToString(), StringComparison.Ordinal)
           || string.Equals(request.정산상태, 운임정산상태.청구대기.ToString(), StringComparison.Ordinal)
           || string.Equals(request.정산상태, 운임정산상태.인수증등록완료.ToString(), StringComparison.Ordinal);

    public static int 입금요청금액(화주운송의뢰 request)
    {
        if (request.결제예정금액 is > 0)
        {
            return request.결제예정금액.Value;
        }

        if (request.최종운임 is > 0)
        {
            return decimal.ToInt32(decimal.Round(request.최종운임.Value, 0, MidpointRounding.AwayFromZero));
        }

        return 0;
    }

    public static IReadOnlyList<운송완료입금알림예약> 알림예약목록(DateTime completedAtUtc)
        => ReminderDays
            .Select((days, index) => new 운송완료입금알림예약(index + 1, days, completedAtUtc.AddDays(days)))
            .ToArray();

    public static string 주문번호생성(string requestId, long transportId)
        => $"hongdal_va_{Sanitize(requestId)}_{transportId}";

    public static string 원본응답초안Json(string requestId, string paymentId, string orderId)
        => JsonSerializer.Serialize(
            new
            {
                PaymentFlow = 토스가상계좌흐름,
                Provider = "TossPayments",
                RequestId = requestId,
                PaymentId = paymentId,
                OrderId = orderId,
                Status = "PendingTossVirtualAccountIssue"
            },
            JsonOptions);

    private static string Sanitize(string value)
    {
        var chars = value.Where(char.IsLetterOrDigit).ToArray();
        return chars.Length == 0 ? "request" : new string(chars);
    }
}

public sealed record 운송완료입금알림예약(int 회차, int 경과일수, DateTime 예약시각Utc);

public enum 운송입금요청종류
{
    운송완료후정산,
    상차완료조기정산
}

public sealed record 운송입금요청판정(bool 가능, string 사유)
{
    public static 운송입금요청판정 가능함()
        => new(true, string.Empty);

    public static 운송입금요청판정 불가(string 사유)
        => new(false, 사유);
}

public sealed record 운송완료입금요청결과(
    bool 처리됨,
    string 사유,
    string? 의뢰Id = null,
    string? 결제Id = null,
    string? OrderId = null,
    int 알림예약건수 = 0);
