using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.ViewSettings;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public interface I커뮤니티세계지도RoleDetailEntryUseCase
{
    커뮤니티세계지도RoleDetailEntryResponse? Resolve(string entryCode, string? role);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Application,
    "공개 지도에서 역할 앱의 권한 검증 상세 작업대로 안전하게 인계",
    ContractType = typeof(I커뮤니티세계지도RoleDetailEntryUseCase),
    FlowOrder = 24,
    Effects = SsalddelCodeEffect.None,
    Boundary = "개별 원장 ID, 위치, 연락처, 거래처, 재고, 계약 또는 배정 상태를 반환하지 않습니다.")]
public sealed class 커뮤니티세계지도RoleDetailEntryUseCase
    : I커뮤니티세계지도RoleDetailEntryUseCase
{
    private const string Notice =
        "이 응답은 역할 앱 작업대 진입만 허용합니다. 개별 업무 조회와 상태 변경은 대상 API가 사용자·역할·원장 범위를 다시 검증합니다.";

    public 커뮤니티세계지도RoleDetailEntryResponse? Resolve(string entryCode, string? role)
    {
        if (string.IsNullOrWhiteSpace(entryCode) || string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        var normalizedEntry = entryCode.Trim().ToLowerInvariant();
        var normalizedRole = role.Trim();

        return normalizedEntry switch
        {
            커뮤니티세계지도RoleDetailEntryCodes.GroupPurchase
                when IsAny(normalizedRole, 역할명.커뮤니티회원)
                => Entry(normalizedEntry, App식별자.OrdererApp, GroupPurchasePageRoutes.GroupsRoot),
            커뮤니티세계지도RoleDetailEntryCodes.ImportReadiness
                when IsAny(normalizedRole, 역할명.커뮤니티회원)
                => Entry(normalizedEntry, App식별자.OrdererApp, GroupPurchasePageRoutes.ImportsRoot),
            커뮤니티세계지도RoleDetailEntryCodes.Transport
                when IsAny(normalizedRole, 역할명.기사, 역할명.용달기사, 역할명.배달기사)
                => Entry(normalizedEntry, App식별자.DriverApp, "/driver/transports/current"),
            커뮤니티세계지도RoleDetailEntryCodes.Transport
                when IsAny(normalizedRole, 역할명.화주, 역할명.판매자)
                => Entry(normalizedEntry, App식별자.SsalddelApp, "/shipper/transport"),
            커뮤니티세계지도RoleDetailEntryCodes.WarehouseInbound
                when IsAny(normalizedRole, 역할명.창고관리자)
                => Entry(normalizedEntry, App식별자.WarehouseManagerApp, InboundInspectionPageRoutes.Root),
            커뮤니티세계지도RoleDetailEntryCodes.WarehouseInbound
                when IsAny(normalizedRole, 역할명.화주, 역할명.판매자)
                => Entry(normalizedEntry, App식별자.SsalddelApp, InboundRequestPageRoutes.Root),
            _ => null
        };
    }

    private static 커뮤니티세계지도RoleDetailEntryResponse Entry(
        string entryCode,
        string appKey,
        string route)
        => new(
            entryCode,
            appKey,
            route,
            커뮤니티세계지도ExecutionBoundaryCodes.RoleAppAuthorizedDetail,
            Notice);

    private static bool IsAny(string role, params string[] allowedRoles)
        => allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
