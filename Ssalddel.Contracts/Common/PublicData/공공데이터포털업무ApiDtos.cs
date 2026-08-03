using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.PublicData;

public static class 공공데이터포털업무ApiFeature
{
    public const string Key = "public-data-portal-business-api-client";
}

[SsalddelCodeMetadata(
    공공데이터포털업무ApiFeature.Key,
    SsalddelCodeLayer.Contract,
    "수협 유통 및 공동주택 공개 API의 허용된 오퍼레이션을 조회하는 공통 계약",
    FlowOrder = 1,
    Boundary = "인증키를 계약이나 응답에 포함하지 않고 공개 원문과 출처 및 조회시각만 전달")]
public sealed class 공공데이터포털업무ApiRequest
{
    public string ApiKey { get; init; } = string.Empty;

    public string? OperationPath { get; init; }

    public IReadOnlyDictionary<string, string?> Parameters { get; init; }
        = new Dictionary<string, string?>();
}

public sealed class 공공데이터포털업무ApiResponse
{
    public bool Success { get; init; }

    public string ApiKey { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string OperationPath { get; init; } = string.Empty;

    public int HttpStatusCode { get; init; }

    public string? ContentType { get; init; }

    public string Body { get; init; } = string.Empty;

    public DateTimeOffset ObservedAt { get; init; }
}

public sealed record 공공데이터포털업무ApiDefinition(
    string Key,
    string DisplayName,
    string DefaultOperationPath,
    IReadOnlyList<string> AllowedOperationPrefixes)
{
    public string ServiceKeyParameterName { get; init; } = "serviceKey";
}
