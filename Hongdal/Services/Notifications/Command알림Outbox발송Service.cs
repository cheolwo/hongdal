using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;
using 홍달.Services.Storage.Local;

namespace 홍달.Services.Notifications;

public sealed class Command알림Outbox발송Service : ICommand알림Outbox발송Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string 상태_대기 = "Pending";
    private const string 상태_성공 = "Succeeded";
    private const string 상태_실패 = "Failed";

    private readonly HongdalContext _db;
    private readonly I사용자PushTokenStore _userPushTokenStore;
    private readonly IFcmPushService _fcmPushService;
    private readonly IKakaoAlimTalkService _kakaoAlimTalkService;
    private readonly KakaoAlimTalkOptions _kakaoOptions;
    private readonly ILogger<Command알림Outbox발송Service> _logger;

    public Command알림Outbox발송Service(
        HongdalContext db,
        I사용자PushTokenStore userPushTokenStore,
        IFcmPushService fcmPushService,
        IKakaoAlimTalkService kakaoAlimTalkService,
        IOptions<KakaoAlimTalkOptions> kakaoOptions,
        ILogger<Command알림Outbox발송Service> logger)
    {
        _db = db;
        _userPushTokenStore = userPushTokenStore;
        _fcmPushService = fcmPushService;
        _kakaoAlimTalkService = kakaoAlimTalkService;
        _kakaoOptions = kakaoOptions.Value;
        _logger = logger;
    }

    public async Task<int> 대기알림발송Async(int take = 100, CancellationToken cancellationToken = default)
    {
        var items = await _db.Command알림Outbox
            .Where(x => x.Status == 상태_대기
                        && (x.FeatureName == "DispatchAccepted"
                            || x.FeatureName == "DispatchPickupApproach")
                        && x.Target == "Shipper")
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return 0;
        }

        var processed = 0;
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processed++;
            item.RetryCount += 1;
            item.UpdatedAt = DateTime.UtcNow;

            try
            {
                var payload = Command알림Payload.Parse(item.PayloadJson);
                var pushRequested = payload.Channels.Contains("Push");
                var alimTalkRequested = payload.Channels.Contains("AlimTalk");

                var pushSent = !pushRequested || await Fcm발송Async(payload, cancellationToken);
                var alimTalkSent = !alimTalkRequested
                                   || !_kakaoOptions.Enabled
                                   || await 알림톡발송Async(item.FeatureName, payload, cancellationToken);

                item.Status = pushSent && alimTalkSent ? 상태_성공 : 상태_실패;
                if (item.Status == 상태_실패)
                {
                    _logger.LogWarning(
                        "Command 알림 발송 실패. OutboxId={OutboxId} FeatureName={FeatureName} TargetUserId={TargetUserId} PushSent={PushSent} AlimTalkSent={AlimTalkSent}",
                        item.Id,
                        item.FeatureName,
                        payload.TargetUserId,
                        pushSent,
                        alimTalkSent);
                }
            }
            catch (Exception ex)
            {
                item.Status = 상태_실패;
                _logger.LogWarning(ex, "Command 알림 발송 처리 중 예외가 발생했습니다. OutboxId={OutboxId}", item.Id);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task<bool> Fcm발송Async(Command알림Payload payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload.TargetUserId))
        {
            return false;
        }

        var token = await _userPushTokenStore.GetAsync(payload.TargetUserId, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return await _fcmPushService.SendToTokenAsync(
            token,
            payload.Title,
            payload.Body,
            new Dictionary<string, string>
            {
                ["type"] = payload.NotificationType,
                ["requestId"] = payload.RequestId,
                ["driverId"] = payload.DriverId
            },
            cancellationToken);
    }

    private Task<bool> 알림톡발송Async(string featureName, Command알림Payload payload, CancellationToken cancellationToken)
    {
        var variables = new Dictionary<string, string>
        {
            ["requestId"] = payload.RequestId,
            ["driverId"] = payload.DriverId,
            ["cargoType"] = payload.CargoType,
            ["pickupAddress"] = payload.PickupAddress,
            ["pickupWindow"] = payload.PickupWindowText
        };
        var templateCode = string.Equals(featureName, "DispatchPickupApproach", StringComparison.Ordinal)
            ? _kakaoOptions.DispatchPickupApproachTemplateCode
            : _kakaoOptions.DispatchAcceptedTemplateCode;

        return _kakaoAlimTalkService.SendAsync(
            new KakaoAlimTalkMessage(
                payload.PickupContactPhone,
                templateCode,
                payload.Title,
                payload.Body,
                variables),
            cancellationToken);
    }

    private sealed record Command알림Payload(
        string NotificationType,
        string TargetUserId,
        string DriverId,
        string RequestId,
        string CargoType,
        string PickupAddress,
        string PickupContactPhone,
        string PickupWindowText,
        string Title,
        string Body,
        IReadOnlySet<string> Channels)
    {
        public static Command알림Payload Parse(string payloadJson)
        {
            using var document = JsonDocument.Parse(payloadJson);
            var root = document.RootElement;
            var pickupWindowStart = ReadString(root, "pickupWindowStartUtc");
            var pickupWindowEnd = ReadString(root, "pickupWindowEndUtc");
            var pickupWindowText = string.IsNullOrWhiteSpace(pickupWindowStart) && string.IsNullOrWhiteSpace(pickupWindowEnd)
                ? "상차 시간 협의"
                : $"{pickupWindowStart} ~ {pickupWindowEnd}";

            return new Command알림Payload(
                ReadString(root, "알림유형", "DispatchAccepted"),
                ReadString(root, "targetUserId", ReadString(root, "shipperUserId")),
                ReadString(root, "driverId"),
                ReadString(root, "requestId"),
                ReadString(root, "cargoType"),
                ReadString(root, "pickupAddress"),
                ReadString(root, "pickupContactPhone"),
                pickupWindowText,
                ReadString(root, "title", "기사님이 운송 의뢰를 수락했습니다."),
                ReadString(root, "body", "기사님이 운송 의뢰를 수락했습니다. 상차 준비를 확인해 주세요."),
                ReadStringSet(root, "channels", new[] { "Push" }));
        }

        private static string ReadString(JsonElement root, string propertyName, string fallback = "")
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return fallback;
            }

            return value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? fallback
                : value.ToString();
        }

        private static IReadOnlySet<string> ReadStringSet(JsonElement root, string propertyName, IReadOnlyList<string> fallback)
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return new HashSet<string>(fallback, StringComparer.OrdinalIgnoreCase);
            }

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                {
                    result.Add(item.GetString()!);
                }
            }

            return result.Count == 0
                ? new HashSet<string>(fallback, StringComparer.OrdinalIgnoreCase)
                : result;
        }
    }
}
