using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using 살뜰.Data;
using 살뜰.도메인.공통;

namespace Ssalddel.Application.Warehouse;

public interface I창고입고화물인계조회UseCase
{
    Task<IReadOnlyList<CargoWarehouseHandoffResponse>> 조회Async(
        long warehouseId,
        CancellationToken cancellationToken);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Application,
    "현재 창고에 도착하거나 입고 중인 화물을 canonical 운송·입고 관계로 투영한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(CargoWarehouseHandoffResponse),
    FlowOrder = 50,
    Boundary = "현재 계정이 접근 가능한 창고만 조회하며 주소·연락처·운임·주문 식별자를 반환하지 않는다.")]
public sealed class 창고입고화물인계조회UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : I창고입고화물인계조회UseCase
{
    private const int MaximumHandoffs = 50;

    public async Task<IReadOnlyList<CargoWarehouseHandoffResponse>> 조회Async(
        long warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId <= 0)
        {
            return [];
        }

        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return [];
        }

        var isServerAdministrator = string.Equals(
            currentUserAccessor.Role,
            역할명.서버관리자,
            StringComparison.OrdinalIgnoreCase);
        var canAccessWarehouse = isServerAdministrator
            || await db.창고.AsNoTracking().AnyAsync(
                warehouse => warehouse.Id == warehouseId
                    && (warehouse.소유자UserId == userId
                        || db.창고사용자.Any(warehouseUser =>
                            warehouseUser.창고Id == warehouse.Id
                            && warehouseUser.UserId == userId)),
                cancellationToken);
        if (!canAccessWarehouse)
        {
            return [];
        }

        var rows = await (
                from inbound in db.입고요청.AsNoTracking()
                join transport in db.운송원장.AsNoTracking()
                    on inbound.운송의뢰Id equals transport.운송번호
                where inbound.창고Id == warehouseId
                orderby inbound.UpdatedAt descending, inbound.Id descending
                select new
                {
                    TransportId = transport.Id,
                    TransportStatus = transport.상태,
                    TransportUpdatedAt = transport.UpdatedAt,
                    InboundId = inbound.Id,
                    InboundStatus = inbound.상태,
                    InboundUpdatedAt = inbound.UpdatedAt,
                })
            .Take(MaximumHandoffs)
            .ToArrayAsync(cancellationToken);

        var generatedAt = DateTimeOffset.UtcNow;
        return rows
            .Select(row => CargoWarehouseHandoffProjectionBuilder.Build(
                row.TransportId,
                row.TransportStatus,
                row.TransportUpdatedAt,
                row.InboundId,
                row.InboundStatus,
                row.InboundUpdatedAt,
                generatedAt))
            .Where(item => item is not null)
            .Cast<CargoWarehouseHandoffResponse>()
            .ToArray();
    }
}
