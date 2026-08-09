using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.WorldProjection;

public static class ResidentialPickupPerspectiveRoutes
{
    public const string Orderer =
        "api/v1/orderer/world/zones/residential-pickup/perspective";

    public const string Transporter =
        "api/v1/driver/world/zones/residential-pickup/perspective";
}

public static class ResidentialPickupRoleCodes
{
    public const string Orderer = "Orderer";
    public const string Transporter = "Transporter";
}

public static class ResidentialPickupStatusCodes
{
    public const string Waiting = "Waiting";
    public const string Arrived = "Arrived";
    public const string Completed = "Completed";
}

public sealed class ResidentialPickupPointResponse
{
    public string StableId { get; set; } = string.Empty;
    public string CanonicalTaskStableId { get; set; } = string.Empty;
    public string PickupPointLabel { get; set; } = string.Empty;
    public string ProductLabel { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = string.Empty;
    public bool CanInspect { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Contract,
    "주문자 본인 수령 또는 배정 운송에 필요한 공동수령 World object만 개인정보 없이 제공한다.",
    FlowOrder = 10,
    Boundary = "주소, 상세주소, 연락처, 사용자 ID, 주문번호, 결제와 계약 정보는 Unity projection에 포함하지 않는다.")]
public sealed class ResidentialPickupPerspectiveResponse
{
    public string StableId { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string AuthorizedRoleCode { get; set; } = string.Empty;
    public string WorldZoneCode { get; set; } = "residential-pickup";
    public string ViewerScopeCode { get; set; } = RolePerspectiveViewerScopeCodes.AuthorizedParty;
    public string SourceTypeCode { get; set; } = RolePerspectiveSourceTypeCodes.OperationalProjection;
    public string AuthorizationDecisionId { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public IReadOnlyList<ResidentialPickupPointResponse> PickupPoints { get; set; } = [];
}
