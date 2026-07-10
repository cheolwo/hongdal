using System.Globalization;
using Hongdal.Hubs;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.Notifications
{
    public sealed class FcmDriverRecommendationPushService : IDriverRecommendationPushService
    {
        private readonly IDriverPushTokenStore _tokenStore;
        private readonly IDriverRecommendationPushStateStore _pushStateStore;
        private readonly IFcmPushService _fcmPushService;
        private readonly PushNotificationsOptions _options;
        private readonly ILogger<FcmDriverRecommendationPushService> _logger;

        public FcmDriverRecommendationPushService(
            IDriverPushTokenStore tokenStore,
            IDriverRecommendationPushStateStore pushStateStore,
            IFcmPushService fcmPushService,
            IOptions<PushNotificationsOptions> options,
            ILogger<FcmDriverRecommendationPushService> logger)
        {
            _tokenStore = tokenStore;
            _pushStateStore = pushStateStore;
            _fcmPushService = fcmPushService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(
            string driverId,
            IReadOnlyList<DispatchRecommendationDto> recommendations,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(driverId) || recommendations.Count == 0)
            {
                _logger.LogDebug(
                    "Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
                    "NotificationSkipped",
                    driverId,
                    "Skipped",
                    "Empty driverId or recommendations",
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                    DateTime.UtcNow);
                return false;
            }

            var token = await _tokenStore.GetAsync(driverId, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogDebug(
                    "Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
                    "NotificationSkipped",
                    driverId,
                    "Skipped",
                    "No push token registered",
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                    DateTime.UtcNow);
                return false;
            }

            var ids = recommendations.Select(x => x.의뢰Id).ToList();
            if (!await _pushStateStore.HasChangedAsync(driverId, ids, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogDebug(
                    "Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}",
                    "NotificationSkipped",
                    driverId,
                    "Skipped",
                    "Recommendation set unchanged",
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                    DateTime.UtcNow);
                return false;
            }

            var top = recommendations[0];
            var body = recommendations.Count == 1
                ? $"{top.화물종류} · {top.픽업지} -> {top.하차지}"
                : $"{top.화물종류} 외 {recommendations.Count - 1}건 · {top.픽업지} 등";

            var sent = await _fcmPushService.SendToTokenAsync(
                token,
                _options.DefaultTitle,
                body,
                new Dictionary<string, string>
                {
                    ["type"] = "DriverDispatchRecommendation",
                    ["driverId"] = driverId,
                    ["recommendationCount"] = recommendations.Count.ToString(CultureInfo.InvariantCulture),
                    ["topRequestId"] = top.의뢰Id,
                    ["topPickup"] = top.픽업지,
                    ["topDropoff"] = top.하차지,
                    ["topDistanceKm"] = top.직선거리Km.HasValue
                        ? top.직선거리Km.Value.ToString(CultureInfo.InvariantCulture)
                        : string.Empty,
                    ["requestIds"] = string.Join(",", ids)
                },
                cancellationToken).ConfigureAwait(false);

            if (!sent)
            {
                _logger.LogWarning(
                    "Action={Action} DriverId={DriverId} Result={Result} RecommendationCount={RecommendationCount} TraceId={TraceId} OccurredAt={OccurredAt}",
                    "NotificationSent",
                    driverId,
                    "Failed",
                    recommendations.Count,
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                    DateTime.UtcNow);
                return false;
            }

            _logger.LogInformation(
                "Action={Action} DriverId={DriverId} Result={Result} RecommendationCount={RecommendationCount} TraceId={TraceId} OccurredAt={OccurredAt}",
                "NotificationSent",
                driverId,
                "Success",
                recommendations.Count,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                DateTime.UtcNow);

            return true;
        }
    }
}
