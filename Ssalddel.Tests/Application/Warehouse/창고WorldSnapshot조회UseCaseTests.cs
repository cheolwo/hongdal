using System.Reflection;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Controllers.Common;
using Ssalddel.Security;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 창고WorldSnapshot조회UseCaseTests
{
    [Fact]
    public async Task 권한필터된_재고적재피킹을_WorldSnapshot으로_압축한다()
    {
        var inventory = new 재고현황Fake();
        var putAway = new 적재작업Fake();
        var picking = new 피킹작업Fake();
        var useCase = new 창고WorldSnapshot조회UseCase(inventory, putAway, picking);

        var result = await useCase.조회Async(7, default);

        Assert.True(result.IsSuccess);
        var snapshot = result.Value;
        Assert.Equal("warehouse-zone:7", snapshot.StableId);
        Assert.Equal(64, snapshot.Revision.Length);
        Assert.Equal(2, snapshot.InventoryItems.Count);
        Assert.Equal(2, snapshot.Tasks.Count);
        Assert.Equal(2, snapshot.Npcs.Count);
        Assert.Contains(snapshot.Npcs, npc => npc.RoleCode == "DockWorker" && npc.DestinationWaypointKey == "warehouse.storage-zone");
        Assert.Contains(snapshot.Npcs, npc => npc.RoleCode == "Picker" && npc.DestinationWaypointKey == "warehouse.outbound-staging");
        Assert.Equal(7, inventory.LastRequest?.WarehouseId);
        Assert.Equal(50, inventory.LastRequest?.PageSize);
        Assert.Equal(7, putAway.LastRequest?.WarehouseId);
        Assert.Equal(7, picking.LastRequest?.WarehouseId);
    }

    [Fact]
    public async Task 잘못된_창고Id는_하위조회를_실행하지_않는다()
    {
        var inventory = new 재고현황Fake();
        var useCase = new 창고WorldSnapshot조회UseCase(inventory, new 적재작업Fake(), new 피킹작업Fake());

        var result = await useCase.조회Async(0, default);

        Assert.True(result.IsFailed);
        Assert.Equal("WarehouseIdInvalid", result.Errors[0].Message);
        Assert.Null(inventory.LastRequest);
    }

    [Fact]
    public void World계약은_작업자와주문참조_연락처를_포함하지_않는다()
    {
        var propertyNames = typeof(WarehouseWorldSnapshotResponse).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(WarehouseWorldSnapshotResponse).Namespace)
            .Where(type => type.Name.StartsWith("WarehouseWorld", StringComparison.Ordinal))
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("WorkerDisplayName", propertyNames);
        Assert.DoesNotContain("UserId", propertyNames);
        Assert.DoesNotContain("OrderReference", propertyNames);
        Assert.DoesNotContain("Contact", propertyNames);
        Assert.DoesNotContain("Address", propertyNames);
    }

    [Fact]
    public void Controller는_운영사용자와_창고관리자_권한을_모두_요구한다()
    {
        var type = typeof(창고WorldSnapshotController);

        Assert.Equal("운영사용자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        var role = type.GetCustomAttribute<RequireHrRoleAttribute>();
        Assert.NotNull(role);
        Assert.Equal([HrDetailedRoleCodes.WarehouseManager], role.RoleCodes);
        Assert.Equal(WarehouseWorldSnapshotRoutes.AuthorizedSnapshot, type.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    private sealed class 재고현황Fake : I재고현황UseCase
    {
        public 창고재고현황목록조회요청? LastRequest { get; private set; }
        public Task<Result<창고재고현황목록페이지응답>> 목록Async(창고재고현황목록조회요청 request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Result.Ok(new 창고재고현황목록페이지응답
            {
                Items =
                [
                    new 창고재고현황목록항목응답
                    {
                        InboundItemId = 31, WarehouseId = 7, WarehouseName = "도심 창고", ProductName = "감자",
                        Sku = "POTATO-20", OptionName = "20kg", AvailableQuantity = 12, ReservedQuantity = 3,
                        StorageLocation = "A-01", Status = "적재완료", HasCommunityLedger = true,
                        UpdatedAtUtc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
                    },
                    new 창고재고현황목록항목응답
                    {
                        InboundItemId = 32, WarehouseId = 7, WarehouseName = "도심 창고", ProductName = "양파",
                        Sku = "ONION-10", OptionName = "10kg", AvailableQuantity = 4, ReservedQuantity = 0,
                        StorageLocation = string.Empty, Status = "검수완료",
                        UpdatedAtUtc = new DateTime(2026, 8, 8, 1, 1, 0, DateTimeKind.Utc),
                    },
                ],
                TotalCount = 2, TotalAvailableQuantity = 16, TotalReservedQuantity = 3, UnassignedLocationCount = 1, PageSize = 50,
            }));
        }

        public Task<Result<창고재고현황상세응답>> 상세Async(long inboundItemId, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<창고재고현황상세응답>("unused"));
    }

    private sealed class 적재작업Fake : I적재작업UseCase
    {
        public 적재작업목록조회요청? LastRequest { get; private set; }
        public Task<Result<적재작업목록페이지응답>> 목록Async(적재작업목록조회요청 request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Result.Ok(new 적재작업목록페이지응답
            {
                Items =
                [
                    new 적재작업목록항목응답
                    {
                        InboundItemId = 32, WarehouseId = 7, WarehouseName = "도심 창고", ProductName = "양파",
                        Sku = "ONION-10", AvailableQuantity = 4, InventoryStatus = "검수완료", CanPutAway = true,
                        UpdatedAtUtc = new DateTime(2026, 8, 8, 1, 1, 0, DateTimeKind.Utc),
                    },
                ],
                TotalCount = 1, PageSize = 50,
            }));
        }
        public Task<Result<적재작업상세응답>> 상세Async(long inboundItemId, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<적재작업상세응답>("unused"));
        public Task<Result<적재작업결과응답>> 완료Async(long inboundItemId, 적재작업완료요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<적재작업결과응답>("unused"));
    }

    private sealed class 피킹작업Fake : I피킹작업UseCase
    {
        public 피킹작업목록조회요청? LastRequest { get; private set; }
        public Task<Result<피킹작업목록페이지응답>> 목록Async(피킹작업목록조회요청 request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Result.Ok(new 피킹작업목록페이지응답
            {
                Items =
                [
                    new 피킹작업목록항목응답
                    {
                        TaskKey = "pick:private-internal-key", WarehouseId = 7, WarehouseName = "도심 창고",
                        ProductName = "감자", Sku = "POTATO-20", Quantity = 3, RackCode = "A-01", Status = "대기",
                        WorkerDisplayName = "계약으로 복사하면 안 되는 작업자", UpdatedAtUtc = new DateTime(2026, 8, 8, 1, 2, 0, DateTimeKind.Utc),
                    },
                ],
                TotalCount = 1, PageSize = 50,
            }));
        }
        public Task<Result<피킹작업상세응답>> 상세Async(string taskKey, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<피킹작업상세응답>("unused"));
        public Task<Result<피킹작업결과응답>> 시작Async(string taskKey, 창고작업요청Context context, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<피킹작업결과응답>("unused"));
        public Task<Result<피킹작업결과응답>> 완료Async(string taskKey, 피킹작업완료요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<피킹작업결과응답>("unused"));
    }
}
