using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.Notifications
{
    public sealed class FirebaseFcmPushService : IFcmPushService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly PushNotificationsOptions _options;
        private readonly ILogger<FirebaseFcmPushService> _logger;

        public FirebaseFcmPushService(
            HttpClient httpClient,
            IOptions<PushNotificationsOptions> options,
            ILogger<FirebaseFcmPushService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendToTokenAsync(
            string token,
            string title,
            string body,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_options.ServerKey))
            {
                return false;
            }

            var payload = new
            {
                to = token,
                priority = "high",
                notification = new
                {
                    title,
                    body
                },
                data
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
                _logger.LogWarning("Action={Action} Result={Result} Reason={Reason} StatusCode={StatusCode} TraceId={TraceId} OccurredAt={OccurredAt}",
                    "FcmSend",
                    "Failed",
                    errorBody,
                    response.StatusCode,
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
                    DateTime.UtcNow);
                return false;
            }

            return true;
        }
    }
}
