using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Inventory;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 출고예정검토UseCaseTests
{
    [Fact]
    public async Task 기본목록은_접근가능한준비중출고예정만반환한다()
    {
        await using var db = CreateContext();
        var ids = await SeedAsync(db);

        var result = await CreateUseCase(db, "worker-a").목록Async(new 출고예정검토목록조회요청(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(ids.ReadyPlanId, item.OutboundPlanId);
        Assert.Equal("운송 초안 검토", item.ReviewStatus);
        Assert.DoesNotContain(result.Value.Items, candidate => candidate.OutboundPlanId == ids.HiddenPlanId);
    }

    [Fact]
    public async Task 상세는_운송전확인항목을읽기전용으로계산한다()
    {
        await using var db = CreateContext();
        var ids = await SeedAsync(db);
        var beforePlans = await db.출고예정.CountAsync();
        var beforeHistories = await db.재고이력.CountAsync();

        var result = await CreateUseCase(db, "worker-a").상세Async(ids.ReadyPlanId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanStartTransportRequestDraft);
        Assert.Equal("초안 입력 가능", result.Value.ReviewStatus);
        Assert.Equal("냉장포장", result.Value.PackagingType);
        Assert.Contains(result.Value.Checks, item => item.Code == "destination" && item.Status == 출고예정검토항목상태코드.입력필요);
        Assert.Contains(result.Value.Checks, item => item.Code == "quantity" && item.Status == 출고예정검토항목상태코드.확인완료);
        Assert.Equal(beforePlans, await db.출고예정.CountAsync());
        Assert.Equal(beforeHistories, await db.재고이력.CountAsync());
    }

    [Fact]
    public async Task 수량이달라지면_초안진입을차단하고_범위밖원장은404로숨긴다()
    {
        await using var db = CreateContext();
        var ids = await SeedAsync(db);
        var inventory = await db.입고상품.SingleAsync(item => item.Id == ids.InboundItemId);
        inventory.가용수량 = 8;
        await db.SaveChangesAsync();
        var useCase = CreateUseCase(db, "worker-a");

        var mismatch = await useCase.상세Async(ids.ReadyPlanId, CancellationToken.None);
        var hidden = await useCase.상세Async(ids.HiddenPlanId, CancellationToken.None);

        Assert.True(mismatch.IsSuccess);
        Assert.False(mismatch.Value.CanStartTransportRequestDraft);
        Assert.Equal("원장 보완 필요", mismatch.Value.ReviewStatus);
        Assert.Contains(mismatch.Value.Checks, item => item.Code == "quantity" && item.Status == 출고예정검토항목상태코드.차단);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    private static 출고예정검토UseCase CreateUseCase(SsalddelContext db, string userId)
        => new(db, new FakeCurrentUserAccessor(userId, 역할명.창고관리자));

    private static SsalddelContext CreateContext()
        => new(new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options, new DummyEncryption());

    private static async Task<(long ReadyPlanId, long HiddenPlanId, long InboundItemId)> SeedAsync(SsalddelContext db)
    {
        var now = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        var warehouseA = new 창고 { 소유자UserId = "owner-a", 창고명 = "공동 창고 A", 주소 = "서울시 테스트로 1", IsActive = true, CreatedAt = now, UpdatedAt = now };
        var warehouseB = new 창고 { 소유자UserId = "owner-b", 창고명 = "공동 창고 B", 주소 = "부산시 테스트로 2", IsActive = true, CreatedAt = now, UpdatedAt = now };
        db.창고.AddRange(warehouseA, warehouseB);
        await db.SaveChangesAsync();
        db.창고사용자.Add(new 창고사용자 { 창고Id = warehouseA.Id, UserId = "worker-a", 역할명 = "출고", CreatedAt = now, UpdatedAt = now });
        var inbound = new 입고요청 { 창고Id = warehouseA.Id, 주문참조번호 = "ORDER-A", 보관조건 = "냉장", CreatedAt = now, UpdatedAt = now };
        db.입고요청.Add(inbound);
        await db.SaveChangesAsync();
        var inventory = new 입고상품 { 입고요청Id = inbound.Id, 창고Id = warehouseA.Id, 상품명 = "감자", SKU = "POTATO", 가용수량 = 9, 예약수량 = 2, 보관위치 = "A-02", 상태 = "보관중", CreatedAt = now, UpdatedAt = now };
        db.입고상품.Add(inventory);
        await db.SaveChangesAsync();
        var ready = new 출고예정 { 입고상품Id = inventory.Id, 입고요청Id = inbound.Id, 출고창고Id = warehouseA.Id, 상품명 = "감자", SKU = "POTATO", 주문참조번호 = "ORDER-A", 수량 = 9, 상태 = 출고상태.준비중, CreatedAt = now, UpdatedAt = now };
        var hidden = new 출고예정 { 출고창고Id = warehouseB.Id, 상품명 = "숨김", SKU = "HIDDEN", 주문참조번호 = "ORDER-B", 수량 = 5, 상태 = 출고상태.준비중, CreatedAt = now, UpdatedAt = now };
        var linked = new 출고예정 { 입고상품Id = inventory.Id, 출고창고Id = warehouseA.Id, 상품명 = "연결됨", SKU = "LINKED", 수량 = 1, 상태 = 출고상태.준비중, 운송의뢰Id = "transport-1", CreatedAt = now, UpdatedAt = now };
        db.출고예정.AddRange(ready, hidden, linked);
        db.재고이력.AddRange(
            new 재고이력 { 입고상품Id = inventory.Id, 이력유형 = "포장", 변경후수량 = 9, 메모 = "포장 9개 / 냉장포장", 처리일시 = now },
            new 재고이력 { 입고상품Id = inventory.Id, 이력유형 = "출고인계준비", 변경후수량 = 9, 메모 = "출고 인계 준비 9개", 처리일시 = now.AddMinutes(5) });
        await db.SaveChangesAsync();
        return (ready.Id, hidden.Id, inventory.Id);
    }

    private sealed class FakeCurrentUserAccessor(string? userId, string? role) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role { get; } = role;
    }

    private sealed class DummyEncryption : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
