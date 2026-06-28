using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Hongdal.Hubs;
using Microsoft.Extensions.Options;

namespace 홍달.Services.Notifications
{
    public sealed class FcmDriverRecommendationPushService : IDriverRecommendationPushService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly IDriverPushTokenStore _tokenStore;
        private readonly IDriverRecommendationPushStateStore _pushStateStore;
        private readonly PushNotificationsOptions _options;
        private readonly ILogger<FcmDriverRecommendationPushService> _logger;

        public FcmDriverRecommendationPushService(
            HttpClient httpClient,
            IDriverPushTokenStore tokenStore,
            IDriverRecommendationPushStateStore pushStateStore,
            IOptions<PushNotificationsOptions> options,
            ILogger<FcmDriverRecommendationPushService> logger)
        {
            _httpClient = httpClient;
            _tokenStore = tokenStore;
            _pushStateStore = pushStateStore;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string driverId, IReadOnlyList<DispatchRecommendationDto> recommendations, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(driverId) || recommendations.Count == 0)
            {
                _logger.LogDebug("Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSkipped", driverId, "Skipped", "Empty driverId or recommendations", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.ServerKey))
            {
                _logger.LogDebug("Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSkipped", driverId, "Skipped", "PushNotifications:ServerKey is not configured", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
                return false;
            }

            var token = await _tokenStore.GetAsync(driverId, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogDebug("Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSkipped", driverId, "Skipped", "No push token registered", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
                return false;
            }

            var ids = recommendations.Select(x => x.의뢰Id).ToList();
            if (!await _pushStateStore.HasChangedAsync(driverId, ids, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogDebug("Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSkipped", driverId, "Skipped", "Recommendation set unchanged", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
                return false;
            }

            var top = recommendations[0];
            var body = recommendations.Count == 1
                ? $"{top.화물종류} · {top.픽업지} -> {top.하차지}"
                : $"{top.화물종류} 외 {recommendations.Count - 1}건 · {top.픽업지} 등";

            var payload = new
            {
                to = token,
                priority = "high",
                notification = new
                {
                    title = _options.DefaultTitle,
                    body = body
                },
                data = new
                {
                    driverId,
                    recommendationCount = recommendations.Count,
                    topRequestId = top.의뢰Id,
                    topPickup = top.픽업지,
                    topDropoff = top.하차지,
                    topDistanceKm = top.직선거리Km,
                    requestIds = ids
                }
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("key", _options.ServerKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Action={Action} DriverId={DriverId} Result={Result} Reason={Reason} StatusCode={StatusCode} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSent", driverId, "Failed", errorBody, response.StatusCode, System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
                return false;
            }

            _logger.LogInformation("Action={Action} DriverId={DriverId} Result={Result} RecommendationCount={RecommendationCount} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSent", driverId, "Success", recommendations.Count, System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);

            return true;
        }
    }
}

