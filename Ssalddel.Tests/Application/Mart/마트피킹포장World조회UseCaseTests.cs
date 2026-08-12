using System.Reflection;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.Mart;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Mart;
using Ssalddel.Controllers.Common;
using Ssalddel.Security;

namespace Ssalddel.Tests.Application.Mart;

public sealed class 마트피킹포장World조회UseCaseTests
{
    [Fact]
    public async Task 피킹완료작업은_정확한Rack에서포장대로이동하고_후속포장은준비상태가된다()
    {
        var reader = new 마트피킹조회Fake();
        var useCase = UseCase(reader);

        var result = await useCase.조회Async(7, default);

        Assert.True(result.IsSuccess);
        var snapshot = result.Value;
        Assert.Equal("warehouse:7", snapshot.WarehouseStableId);
        Assert.Equal(64, snapshot.Revision.Length);
        Assert.Equal(new DateTime(2026, 8, 12, 1, 1, 0, DateTimeKind.Utc).Ticks, snapshot.RevisionNumber);
        Assert.Single(snapshot.Workflows);
        var shelf = Assert.Single(snapshot.Shelves);
        Assert.Equal("seedbed-object:city.operator-inventory-shelf.a", shelf.SeedbedObjectStableId);
        Assert.Equal("A-03-02", shelf.LocationCode);
        Assert.Equal("PickCompleted", shelf.StateCode);
        Assert.Equal(8, shelf.TotalAvailableQuantity);
        Assert.Equal(2, shelf.TotalReservedQuantity);
        Assert.Equal(["warehouse-inventory:31"], shelf.InventoryItemStableIds);
        Assert.Equal("market.rack:a-03-02:approach", shelf.PickApproachWaypointKey);
        Assert.Equal("market.rack:a-03-02:pick", shelf.PickPointWaypointKey);
        Assert.True(shelf.IsPresentationReady);
        Assert.Equal(3, snapshot.Tasks.Count);

        var picked = snapshot.Tasks.Single(task => task.ProductName == "감자" && task.TaskKindCode == "Picking");
        Assert.Equal("MovingToPacking", picked.ActivityCode);
        Assert.Equal("A-03-02", picked.LocationCode);
        Assert.Equal(MarketWorldLocationMappingStateCodes.Mapped, picked.LocationMappingStateCode);
        Assert.Equal("warehouse-inventory:31", picked.InventoryItemStableId);
        Assert.NotEmpty(picked.NextTaskStableId);
        Assert.DoesNotContain("ORDER-PRIVATE-001", picked.WorkflowStableId, StringComparison.Ordinal);

        Assert.Contains(snapshot.Npcs, npc =>
            npc.SourceTaskStableId == picked.StableId
            && npc.CurrentWaypointKey == "market.rack:a-03-02:pick"
            && npc.DestinationWaypointKey == "market.packing:station-01:input"
            && npc.ActivityCode == "MovingToPacking");

        var packing = snapshot.Tasks.Single(task => task.TaskKindCode == "Packing");
        Assert.Equal("PackingReady", packing.ActivityCode);
        Assert.True(packing.IsPresentationReady);
        Assert.Contains(snapshot.Npcs, npc =>
            npc.SourceTaskStableId == packing.StableId
            && npc.CurrentWaypointKey == "market.packing:queue"
            && npc.DestinationWaypointKey == "market.packing:station-01:input");

        Assert.Equal(7, reader.LastListRequest?.창고Id);
        Assert.Equal(50, reader.LastListRequest?.PageSize);
        Assert.Equal([101L], reader.DetailOrderIds);
    }

    [Fact]
    public async Task 위치가없는피킹작업은_임의Rack으로보내지않고_위치연결필요로남긴다()
    {
        var reader = new 마트피킹조회Fake(includeUnmappedOnly: true);
        var useCase = UseCase(reader);

        var result = await useCase.조회Async(7, default);

        Assert.True(result.IsSuccess);
        var task = Assert.Single(result.Value.Tasks);
        Assert.Equal("LocationUnmapped", task.ActivityCode);
        Assert.Equal(MarketWorldLocationMappingStateCodes.LocationUnmapped, task.LocationMappingStateCode);
        Assert.False(task.IsPresentationReady);
        Assert.Empty(result.Value.Npcs);
        Assert.Single(result.Value.Shelves);
    }

    [Fact]
    public async Task 잘못된창고Id는_하위조회를실행하지않는다()
    {
        var reader = new 마트피킹조회Fake();
        var useCase = UseCase(reader);

        var result = await useCase.조회Async(0, default);

        Assert.True(result.IsFailed);
        Assert.Equal("WarehouseIdInvalid", result.Errors.Single().Message);
        Assert.Null(reader.LastListRequest);
    }

    [Fact]
    public void World계약은_주문참조와작업자개인정보를포함하지않는다()
    {
        var propertyNames = typeof(MarketPickingPackingWorldSnapshotResponse).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(MarketPickingPackingWorldSnapshotResponse).Namespace)
            .Where(type => type.Name.StartsWith("MarketPickingPacking", StringComparison.Ordinal))
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("OrderReference", propertyNames);
        Assert.DoesNotContain("WorkerDisplayName", propertyNames);
        Assert.DoesNotContain("UserId", propertyNames);
        Assert.DoesNotContain("Contact", propertyNames);
        Assert.DoesNotContain("Address", propertyNames);
        Assert.DoesNotContain("Payment", propertyNames);
    }

    [Fact]
    public void Controller는_운영사용자와창고업무권한을요구한다()
    {
        var type = typeof(마트피킹포장WorldController);

        Assert.Equal("운영사용자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        var role = type.GetCustomAttribute<RequireHrRoleAttribute>();
        Assert.NotNull(role);
        Assert.Contains(HrDetailedRoleCodes.WarehouseManager, role.RoleCodes);
        Assert.Equal(
            MarketPickingPackingWorldSnapshotRoutes.AuthorizedSnapshot,
            type.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void 지원하지않는작업유형은_포장으로추정하지않고_NPC를만들지않는다()
    {
        var projector = new 마트피킹포장WorldProjector();
        var generatedAt = new DateTimeOffset(2026, 8, 12, 2, 0, 0, TimeSpan.Zero);
        var order = new 마트피킹주문상세응답
        {
            주문참조번호 = "ORDER-PRIVATE-UNSUPPORTED",
            주문상태 = "출고 예정",
            현재단계 = "검수",
            수정일시Utc = generatedAt.UtcDateTime,
            작업목록 =
            [
                new 마트피킹작업응답
                {
                    작업Key = "INSPECT-1",
                    작업유형 = "검수",
                    상태 = "대기",
                    창고Id = 7,
                    라인Key = "LINE-INSPECT",
                    상품명 = "감자",
                    SKU = "POTATO-2",
                    수량 = 1,
                    수정일시Utc = generatedAt.UtcDateTime
                }
            ]
        };

        var snapshot = projector.Project(7, 1, 50, [order], WarehouseSnapshot(), generatedAt);

        var task = Assert.Single(snapshot.Tasks);
        Assert.Equal("Unsupported", task.TaskKindCode);
        Assert.Equal("TaskKindUnsupported", task.ActivityCode);
        Assert.False(task.IsPresentationReady);
        Assert.Empty(snapshot.Npcs);
        Assert.Equal(generatedAt, snapshot.GeneratedAtUtc);
    }

    private static 마트피킹포장World조회UseCase UseCase(I마트피킹조회UseCase reader)
        => new(reader, new 창고World조회Fake(), new 마트피킹포장WorldProjector());

    private static WarehouseWorldSnapshotResponse WarehouseSnapshot()
        => new()
        {
            StableId = "warehouse-zone:7",
            Revision = "warehouse-revision",
            GeneratedAtUtc = new DateTimeOffset(2026, 8, 12, 1, 0, 0, TimeSpan.Zero),
            InventoryItems =
            [
                new WarehouseWorldInventoryItemResponse
                {
                    StableId = "warehouse-inventory:31",
                    WarehouseStableId = "warehouse:7",
                    ProductName = "감자",
                    Sku = "POTATO-2",
                    AvailableQuantity = 8,
                    ReservedQuantity = 2,
                    StorageLocation = "A-03-02",
                    Status = "적재완료",
                    UpdatedAtUtc = new DateTimeOffset(2026, 8, 12, 0, 59, 0, TimeSpan.Zero)
                },
                new WarehouseWorldInventoryItemResponse
                {
                    StableId = "warehouse-inventory:unassigned",
                    WarehouseStableId = "warehouse:7",
                    ProductName = "양파",
                    Sku = "ONION-1",
                    AvailableQuantity = 4,
                    StorageLocation = string.Empty,
                    Status = "검수완료",
                    UpdatedAtUtc = new DateTimeOffset(2026, 8, 12, 0, 58, 0, TimeSpan.Zero)
                }
            ]
        };

    private sealed class 창고World조회Fake : I창고WorldSnapshot조회UseCase
    {
        public Task<Result<WarehouseWorldSnapshotResponse>> 조회Async(
            long? warehouseId,
            CancellationToken cancellationToken)
            => Task.FromResult(Result.Ok(WarehouseSnapshot()));
    }

    private sealed class 마트피킹조회Fake(bool includeUnmappedOnly = false) : I마트피킹조회UseCase
    {
        public 마트피킹주문목록조회요청? LastListRequest { get; private set; }
        public List<long> DetailOrderIds { get; } = [];

        public Task<Result<마트피킹주문목록응답>> 목록Async(
            마트피킹주문목록조회요청 request,
            CancellationToken cancellationToken)
        {
            LastListRequest = request;
            return Task.FromResult(Result.Ok(new 마트피킹주문목록응답
            {
                Items =
                [
                    new 마트피킹주문요약응답
                    {
                        주문Id = 101,
                        주문참조번호 = "ORDER-PRIVATE-001",
                        주문상태 = "출고 예정",
                        현재단계 = "피킹",
                        상품종류수 = includeUnmappedOnly ? 1 : 2,
                        작업수 = includeUnmappedOnly ? 1 : 3
                    }
                ],
                TotalCount = 1,
                Page = 1,
                PageSize = 50
            }));
        }

        public Task<Result<마트피킹주문상세응답>> 상세Async(
            long orderId,
            CancellationToken cancellationToken)
        {
            DetailOrderIds.Add(orderId);
            var now = new DateTime(2026, 8, 12, 1, 0, 0, DateTimeKind.Utc);
            var unmapped = new 마트피킹작업응답
            {
                작업Id = 3,
                작업Key = "PICK-ONION",
                작업유형 = "피킹",
                상태 = "대기",
                창고Id = 7,
                라인Key = "LINE-ONION",
                상품명 = "양파",
                SKU = "ONION-1",
                수량 = 1,
                묶음바코드 = "TOTE-001",
                수정일시Utc = now
            };
            var tasks = includeUnmappedOnly
                ? [unmapped]
                : new 마트피킹작업응답[]
                {
                    new()
                    {
                        작업Id = 1,
                        작업Key = "PICK-POTATO",
                        작업유형 = "피킹",
                        상태 = "완료",
                        입고상품Id = 31,
                        다음작업Key = "PACK-POTATO",
                        창고Id = 7,
                        라인Key = "LINE-POTATO",
                        상품명 = "감자",
                        SKU = "POTATO-2",
                        수량 = 2,
                        적재대코드 = "A-03-02",
                        묶음바코드 = "TOTE-001",
                        수정일시Utc = now
                    },
                    new()
                    {
                        작업Id = 2,
                        작업Key = "PACK-POTATO",
                        작업유형 = "포장",
                        상태 = "대기",
                        이전작업Key = "PICK-POTATO",
                        창고Id = 7,
                        라인Key = "LINE-POTATO",
                        상품명 = "감자",
                        SKU = "POTATO-2",
                        수량 = 2,
                        묶음바코드 = "TOTE-001",
                        수정일시Utc = now.AddMinutes(1)
                    },
                    unmapped,
                    new()
                    {
                        작업Id = 4,
                        작업Key = "PICK-OTHER-WAREHOUSE",
                        작업유형 = "피킹",
                        상태 = "대기",
                        창고Id = 8,
                        라인Key = "LINE-OTHER",
                        상품명 = "다른 창고 상품",
                        SKU = "OTHER-1",
                        수량 = 1,
                        적재대코드 = "Z-01",
                        수정일시Utc = now
                    }
                };

            return Task.FromResult(Result.Ok(new 마트피킹주문상세응답
            {
                주문Id = orderId,
                주문참조번호 = "ORDER-PRIVATE-001",
                주문상태 = "출고 예정",
                현재단계 = "피킹",
                상품목록 =
                [
                    new 마트피킹주문상품응답
                    {
                        상품라인Id = 1,
                        상품명 = "감자",
                        SKU = "POTATO-2",
                        수량 = 2,
                        상태 = "출고 예정"
                    }
                ],
                작업목록 = tasks,
                수정일시Utc = now
            }));
        }
    }
}
