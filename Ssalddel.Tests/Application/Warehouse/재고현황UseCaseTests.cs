using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.ViewSettings;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 재고현황UseCaseTests
{
    [Fact]
    public async Task 목록은_창고소유배정범위만반환하고_상품소유자만으로는노출하지않는다()
    {
        await using var context = CreateContext();
        var ids = await SeedAsync(context);
        var useCase = new 재고현황UseCase(context, new FakeCurrentUserAccessor("worker-a", 역할명.창고관리자));

        var result = await useCase.목록Async(new 창고재고현황목록조회요청 { PageSize = 10 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(ids.AccessibleItemId, item.InboundItemId);
        Assert.Equal(8, result.Value.TotalAvailableQuantity);
        Assert.Equal(2, result.Value.TotalReservedQuantity);
    }

    [Fact]
    public async Task 목록은_검색상태와서버집계를같은조건에적용한다()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var useCase = new 재고현황UseCase(context, new FakeCurrentUserAccessor("worker-a", 역할명.창고관리자));

        var result = await useCase.목록Async(new 창고재고현황목록조회요청
        {
            Search = "ORDER-A",
            Status = 창고재고조회상태코드.예약,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(8, result.Value.TotalAvailableQuantity);
        Assert.Equal(2, result.Value.TotalReservedQuantity);
        Assert.Equal(0, result.Value.UnassignedLocationCount);
    }

    [Fact]
    public async Task 상세는_정확한입고상품Id와최소근거만반환하고_범위밖은404로숨긴다()
    {
        await using var context = CreateContext();
        var ids = await SeedAsync(context);
        var useCase = new 재고현황UseCase(context, new FakeCurrentUserAccessor("worker-a", 역할명.창고관리자));

        var found = await useCase.상세Async(ids.AccessibleItemId, CancellationToken.None);
        var hidden = await useCase.상세Async(ids.HiddenItemId, CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal("ORDER-A", found.Value.OrderReference);
        Assert.Equal("ledger-a", found.Value.CommunityLedgerId);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
        Assert.DoesNotContain(typeof(창고재고현황상세응답).GetProperties(), property =>
            property.Name.Contains("UserId", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Contract", StringComparison.OrdinalIgnoreCase));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<(long AccessibleItemId, long HiddenItemId)> SeedAsync(SsalddelContext context)
    {
        var now = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        var accessibleWarehouse = new 창고 { 소유자UserId = "owner-a", 창고명 = "공동 창고 A", CreatedAt = now, UpdatedAt = now };
        var hiddenWarehouse = new 창고 { 소유자UserId = "owner-b", 창고명 = "공동 창고 B", CreatedAt = now, UpdatedAt = now };
        context.창고.AddRange(accessibleWarehouse, hiddenWarehouse);
        await context.SaveChangesAsync();
        context.창고사용자.Add(new 창고사용자 { 창고Id = accessibleWarehouse.Id, UserId = "worker-a", 역할명 = "재고", CreatedAt = now, UpdatedAt = now });

        var accessibleInbound = new 입고요청 { 창고Id = accessibleWarehouse.Id, 주문참조번호 = "ORDER-A", 보관조건 = "상온", 입고묶음바코드 = "BUNDLE-A", CreatedAt = now, UpdatedAt = now };
        var hiddenInbound = new 입고요청 { 창고Id = hiddenWarehouse.Id, 주문참조번호 = "ORDER-B", CreatedAt = now, UpdatedAt = now };
        context.입고요청.AddRange(accessibleInbound, hiddenInbound);
        await context.SaveChangesAsync();

        var accessible = new 입고상품
        {
            입고요청Id = accessibleInbound.Id, 창고Id = accessibleWarehouse.Id, 소유자UserId = "owner-item",
            상품명 = "공동구매 감자", SKU = "POTATO-A", 입고수량 = 10, 가용수량 = 8, 예약수량 = 2,
            보관위치 = "A-01", 상태 = "보관중", 커뮤니티원장Id = "ledger-a", CreatedAt = now, UpdatedAt = now
        };
        var hidden = new 입고상품
        {
            입고요청Id = hiddenInbound.Id, 창고Id = hiddenWarehouse.Id, 소유자UserId = "worker-a",
            상품명 = "숨김 상품", SKU = "HIDDEN", 입고수량 = 5, 가용수량 = 5, 상태 = "보관중", CreatedAt = now, UpdatedAt = now
        };
        context.입고상품.AddRange(accessible, hidden);
        await context.SaveChangesAsync();
        return (accessible.Id, hidden.Id);
    }

    private sealed class FakeCurrentUserAccessor(string? userId, string? role) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role { get; } = role;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
