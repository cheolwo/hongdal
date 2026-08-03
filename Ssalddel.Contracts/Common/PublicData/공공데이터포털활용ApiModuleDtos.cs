using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.PublicData;

public static class 공공데이터포털활용ApiModuleFeature
{
    public const string Key = "public-data-portal-active-api-modules";
}

public static class 공공데이터포털활용ApiModuleCoverageCodes
{
    public const string Full = "Full";
    public const string Partial = "Partial";
    public const string CatalogOnly = "CatalogOnly";
}

[SsalddelCodeMetadata(
    공공데이터포털활용ApiModuleFeature.Key,
    SsalddelCodeLayer.Contract,
    "공공데이터포털 활용 중 API를 업무 모듈과 기존 client 연결 상태로 제공하는 계약",
    FlowOrder = 1,
    Boundary = "공개 데이터 ID와 구현 상태만 제공하고 활용계정 식별자, 인증키와 비밀값은 포함하지 않음")]
public sealed class 공공데이터포털활용ApiModuleResponse
{
    public string SourcePortal { get; init; } = "data.go.kr";

    public string SourceStatus { get; init; } = "활용 중";

    public DateOnly VerifiedOn { get; init; }

    public IReadOnlyList<공공데이터포털활용ApiModuleItem> Items { get; init; } = [];
}

public sealed record 공공데이터포털활용ApiModuleItem
{
    public string Key { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string ProductBoundary { get; init; } = string.Empty;

    public string CoverageCode { get; init; } = 공공데이터포털활용ApiModuleCoverageCodes.CatalogOnly;

    public IReadOnlyList<공공데이터포털활용ApiItem> Apis { get; init; } = [];
}

public sealed record 공공데이터포털활용ApiItem
{
    public string DataId { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string MetadataKey { get; init; } = string.Empty;

    public string ImplementationStatusCode { get; init; } = PublicDataApiImplementationStatusCodes.ReferenceOnly;

    public string ClientType { get; init; } = string.Empty;

    public bool IsServiceKeyConfigured { get; init; }
}

public sealed class 공공데이터포털활용ApiModuleQuery
{
    public string? ModuleKey { get; init; }

    public string? ImplementationStatusCode { get; init; }
}
