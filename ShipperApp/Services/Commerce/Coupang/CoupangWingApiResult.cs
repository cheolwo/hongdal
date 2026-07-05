using System.Text.Json.Nodes;

namespace ShipperApp.Services.Commerce.Coupang;

public sealed record CoupangWingApiResult(
    bool IsSuccess,
    int StatusCode,
    JsonNode? Body,
    string? ErrorMessage);
