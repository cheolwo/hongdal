using System.Text.Json.Nodes;

namespace SsalddelApp.Services.Commerce.Coupang;

public sealed record CoupangWingApiResult(
    bool IsSuccess,
    int StatusCode,
    JsonNode? Body,
    string? ErrorMessage);
