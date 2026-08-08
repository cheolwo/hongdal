using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.WorldProjection;

public static class RolePerspectiveRoutes
{
    public const string DriverUrbanLogisticsCenter =
        "api/v1/driver/world/zones/urban-logistics-center/perspective";
}

public static class RolePerspectiveRoleCodes
{
    public const string Transporter = "Transporter";
}

public static class RolePerspectiveWorldZoneCodes
{
    public const string UrbanLogisticsCenter = "urban-logistics-center";
}

public static class RolePerspectiveViewerScopeCodes
{
    public const string AuthorizedParty = "AuthorizedParty";
}

public static class RolePerspectiveSourceTypeCodes
{
    public const string OperationalProjection = "OperationalProjection";
}

public static class RolePerspectiveEmphasisCodes
{
    public const string Primary = "Primary";
    public const string Related = "Related";
    public const string Destination = "Destination";
}

public static class RolePerspectiveInteractionEffectCodes
{
    public const string ReadOnly = "ReadOnly";
    public const string ServerCommand = "ServerCommand";
}

public sealed class RoleObjectEmphasisResponse
{
    public string TargetStableId { get; set; } = string.Empty;

    public string EmphasisCode { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string DetailPanelCode { get; set; } = string.Empty;
}

public sealed class RoleAllowedInteractionResponse
{
    public string InteractionCode { get; set; } = string.Empty;

    public string TargetStableId { get; set; } = string.Empty;

    public string EffectCode { get; set; } = string.Empty;

    public bool RequiresExplicitConfirmation { get; set; }

    public bool RequiresCanonicalStateRefresh { get; set; }
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Contract,
    "인증된 사용자의 활성 역할과 Zone에 허용된 Unity object 강조와 interaction 계약을 정의한다.",
    FlowOrder = 10,
    Boundary = "요청 role은 권한 증명이 아니며 서버 projection에 포함되지 않은 개인정보와 interaction을 Unity가 추론하지 않는다.")]
public sealed class RolePerspectiveResponse
{
    public string StableId { get; set; } = string.Empty;

    public long Revision { get; set; }

    public string AuthorizedRoleCode { get; set; } = string.Empty;

    public string WorldZoneCode { get; set; } = string.Empty;

    public string ViewerScopeCode { get; set; } = string.Empty;

    public string SourceTypeCode { get; set; } = string.Empty;

    public string AuthorizationDecisionId { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public IReadOnlyList<RoleObjectEmphasisResponse> ObjectEmphases { get; set; } = [];

    public IReadOnlyList<RoleAllowedInteractionResponse> AllowedInteractions { get; set; } = [];
}
