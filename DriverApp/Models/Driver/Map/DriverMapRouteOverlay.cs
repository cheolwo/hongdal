using System.Text.Json.Serialization;

namespace DriverApp.Models.Driver.Map;

public sealed record DriverMapRoutePoint(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("label")] string Label);

public sealed record DriverMapRouteOverlay(
    [property: JsonPropertyName("routeId")] string RouteId,
    [property: JsonPropertyName("caption")] string Caption,
    [property: JsonPropertyName("points")] IReadOnlyList<DriverMapRoutePoint> Points,
    [property: JsonPropertyName("strokeColor")] string StrokeColor = "#2563eb",
    [property: JsonPropertyName("outlineColor")] string OutlineColor = "#ffffff",
    [property: JsonPropertyName("width")] int Width = 9);
