using System.Text.Json.Nodes;

namespace SsalddelApp.Services.Commerce.Naver;

public sealed record NaverCommerceApiResult(
    bool IsSuccess,
    int StatusCode,
    JsonNode? Body,
    string? ErrorMessage);
