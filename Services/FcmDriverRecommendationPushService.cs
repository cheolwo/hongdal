using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Hongdal.Hubs;
using Microsoft.Extensions.Options;

namespace 홍달.Services
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
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.ServerKey))
            {
                _logger.LogDebug("PushNotifications:ServerKey is not configured. Skip FCM push for {DriverId}.", driverId);
                return false;
            }

            var token = await _tokenStore.GetAsync(driverId, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogDebug("No push token registered for {DriverId}.", driverId);
                return false;
            }

            var ids = recommendations.Select(x => x.의뢰Id).ToList();
            if (!await _pushStateStore.HasChangedAsync(driverId, ids, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            var top = recommendations[0];
            var body = recommendations.Count == 1
                ? $"{top.화물종류} · {top.픽업지} -> {top.하차지}"
                : $"{top.화물종류} 외 {recommendations.Count - 1}건 · {top.픽업지} 인근";

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
                    topDistanceKm = top.픽업거리Km,
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
                _logger.LogWarning("FCM push failed for {DriverId}: {StatusCode} {Body}", driverId, response.StatusCode, errorBody);
                return false;
            }

            return true;
        }
    }
}
