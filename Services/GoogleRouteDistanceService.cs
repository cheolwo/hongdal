using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace 홍달.Services;

public interface IRouteDistanceService
{
    Task<decimal?> GetDrivingDistanceKmAsync(decimal originLat, decimal originLng, decimal destinationLat, decimal destinationLng);
}

public sealed class GoogleRouteDistanceService : IRouteDistanceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleRouteDistanceService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleGeocodingApiKey"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("GoogleGeocodingApiKey configuration is required.");
        }
    }

    public async Task<decimal?> GetDrivingDistanceKmAsync(decimal originLat, decimal originLng, decimal destinationLat, decimal destinationLng)
    {
        var origin = string.Create(CultureInfo.InvariantCulture, $"{originLat},{originLng}");
        var destination = string.Create(CultureInfo.InvariantCulture, $"{destinationLat},{destinationLng}");
        var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={Uri.EscapeDataString(origin)}&destinations={Uri.EscapeDataString(destination)}&mode=driving&key={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (!root.TryGetProperty("status", out var statusElement) || !string.Equals(statusElement.GetString(), "OK", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!root.TryGetProperty("rows", out var rowsElement) || rowsElement.ValueKind != JsonValueKind.Array || rowsElement.GetArrayLength() == 0)
        {
            return null;
        }

        var firstRow = rowsElement[0];
        if (!firstRow.TryGetProperty("elements", out var elementsElement) || elementsElement.ValueKind != JsonValueKind.Array || elementsElement.GetArrayLength() == 0)
        {
            return null;
        }

        var firstElement = elementsElement[0];
        if (!firstElement.TryGetProperty("status", out var elementStatus) || !string.Equals(elementStatus.GetString(), "OK", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!firstElement.TryGetProperty("distance", out var distanceElement) || !distanceElement.TryGetProperty("value", out var valueElement))
        {
            return null;
        }

        return valueElement.GetDecimal() / 1000m;
    }
}