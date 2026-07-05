using System.Text.Json.Nodes;

namespace ShipperApp.Services.Commerce.Naver;

public sealed record NaverCommerceApiResult(
    bool IsSuccess,
    int StatusCode,
    JsonNode? Body,
    string? ErrorMessage);
