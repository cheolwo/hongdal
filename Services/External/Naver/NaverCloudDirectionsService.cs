using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.Naver
{
    public interface INaverCloudDirectionsService
    {
        Task<NaverCloudDrivingRoute?> GetDrivingRouteAsync(
            decimal startLat,
            decimal startLng,
            decimal goalLat,
            decimal goalLng,
            string? option = null,
            CancellationToken cancellationToken = default);
    }

    public sealed class NaverCloudDirectionsService : INaverCloudDirectionsService
    {
        private readonly HttpClient _httpClient;
        private readonly NaverCloudDirectionsOptions _options;

        public NaverCloudDirectionsService(HttpClient httpClient, IOptions<NaverCloudDirectionsOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.ApiKeyId))
            {
                throw new InvalidOperationException("NaverCloudDirections:ApiKeyId configuration is required.");
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("NaverCloudDirections:ApiKey configuration is required.");
            }
        }

        public async Task<NaverCloudDrivingRoute?> GetDrivingRouteAsync(
            decimal startLat,
            decimal startLng,
            decimal goalLat,
            decimal goalLng,
            string? option = null,
            CancellationToken cancellationToken = default)
        {
            var start = FormattableString.Invariant($"{startLng},{startLat}");
            var goal = FormattableString.Invariant($"{goalLng},{goalLat}");
            var routeOption = string.IsNullOrWhiteSpace(option) ? _options.DefaultOption : option.Trim();

            var requestUrl = $"{_options.Path.TrimStart('/')}?start={Uri.EscapeDataString(start)}&goal={Uri.EscapeDataString(goal)}&option={Uri.EscapeDataString(routeOption)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.TryAddWithoutValidation("x-ncp-apigw-api-key-id", _options.ApiKeyId);
            request.Headers.TryAddWithoutValidation("x-ncp-apigw-api-key", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(content);

            if (!TryGetRouteSummary(document.RootElement, out var route))
            {
                return null;
            }

            return route;
        }

        private static bool TryGetRouteSummary(JsonElement root, out NaverCloudDrivingRoute route)
        {
            route = default!;

            if (!root.TryGetProperty("route", out var routeElement) || routeElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var routeType in new[] { "traoptimal", "trafast", "tracomfort" })
            {
                if (!routeElement.TryGetProperty(routeType, out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0)
                {
                    continue;
                }

                var firstCandidate = candidates[0];
                if (!firstCandidate.TryGetProperty("summary", out var summary))
                {
                    continue;
                }

                route = new NaverCloudDrivingRoute
                {
                    RouteType = routeType,
                    DistanceMeters = GetDecimal(summary, "distance"),
                    DurationMilliseconds = GetDecimal(summary, "duration"),
                    TollFare = GetDecimal(summary, "tollFare"),
                    FuelPrice = GetDecimal(summary, "fuelPrice"),
                    DepartureName = GetString(summary, "departureName"),
                    GoalName = GetString(summary, "goalName")
                };

                return true;
            }

            return false;
        }

        private static decimal? GetDecimal(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var valueElement))
            {
                return null;
            }

            return valueElement.ValueKind switch
            {
                JsonValueKind.Number when valueElement.TryGetDecimal(out var value) => value,
                JsonValueKind.String when decimal.TryParse(valueElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => null
            };
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var valueElement) ? valueElement.GetString() : null;
        }
    }

    public sealed class NaverCloudDrivingRoute
    {
        public string RouteType { get; set; } = string.Empty;
        public decimal? DistanceMeters { get; set; }
        public decimal? DurationMilliseconds { get; set; }
        public decimal? TollFare { get; set; }
        public decimal? FuelPrice { get; set; }
        public string? DepartureName { get; set; }
        public string? GoalName { get; set; }

        public decimal? DistanceKm => DistanceMeters.HasValue ? DistanceMeters.Value / 1000m : null;
        public TimeSpan? Duration => DurationMilliseconds.HasValue ? TimeSpan.FromMilliseconds((double)DurationMilliseconds.Value) : null;
    }
}


