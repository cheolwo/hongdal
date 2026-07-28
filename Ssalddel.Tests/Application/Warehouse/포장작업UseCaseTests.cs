using MediatR;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Warehouse;
using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.ViewSettings;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Audit;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 포장작업UseCaseTests
{
    [Fact]
    public async Task 목록은_창고범위의적재완료재고만반환한다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var result = await CreateUseCase(db, "worker-a").목록Async(new 포장작업목록조회요청(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(ids.AccessibleId, item.InboundItemId);
        Assert.True(item.CanPack);
    }

    [Fact]
    public async Task 상세는_적재근거를제공하고_범위밖재고를404로숨긴다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var useCase = CreateUseCase(db, "worker-a");
        var found = await useCase.상세Async(ids.AccessibleId, CancellationToken.None);
        var hidden = await useCase.상세Async(ids.HiddenId, CancellationToken.None);
        Assert.True(found.IsSuccess); Assert.Equal("ORDER-A", found.Value.OrderReference); Assert.NotNull(found.Value.PutAwayAtUtc);
        Assert.True(hidden.IsFailed); Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 완료는_전체가용수량과두확인을요구하고_상태이력이동감사Event를한번만생성한다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var logs = new RecordingLog(); var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(db, "worker-a", logs, publisher);
        var invalid = await useCase.완료Async(ids.AccessibleId, new 포장작업완료요청 { PackagingQuantity = 9, PackagingType = 포장유형코드.냉장포장 }, RequestContext(), CancellationToken.None);
        var request = new 포장작업완료요청 { PackagingQuantity = 9, PackagingType = 포장유형코드.냉장포장, Memo = "라벨 확인", InventoryConfirmed = true, PackageLabelConfirmed = true };
        var first = await useCase.완료Async(ids.AccessibleId, request, RequestContext(), CancellationToken.None);
        var replay = await useCase.완료Async(ids.AccessibleId, request, RequestContext(), CancellationToken.None);
        Assert.True(invalid.IsFailed); Assert.Equal(400, invalid.Errors.Single().Metadata["StatusCode"]);
        Assert.True(first.IsSuccess); Assert.Equal("포장완료-냉장포장", first.Value.InventoryStatus); Assert.False(first.Value.IdempotentReplay);
        Assert.True(replay.IsSuccess); Assert.True(replay.Value.IdempotentReplay);
        Assert.Single(await db.재고이력.Where(x => x.입고상품Id == ids.AccessibleId && x.이력유형 == "포장").ToArrayAsync());
        Assert.Single(await db.재고이동.Where(x => x.입고상품Id == ids.AccessibleId && x.이동유형 == "포장").ToArrayAsync());
        Assert.Single(logs.Entries);
        var notification = Assert.IsType<창고포장완료됨Event>(Assert.Single(publisher.Notifications));
        Assert.Equal("ORDER-A", notification.주문참조번호);
        Assert.Equal("감자", notification.상품명);
        Assert.Equal("POTATO", notification.SKU);
        Assert.Equal(포장유형코드.냉장포장, notification.포장유형);
        Assert.Equal("A-02-01", notification.보관위치);
    }

    [Fact]
    public async Task 부분포장과_완료후다른유형재요청은_별도작업으로분리하도록거부한다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db); var useCase = CreateUseCase(db, "worker-a");
        var partial = await useCase.완료Async(ids.AccessibleId, Request(4, 포장유형코드.일반포장), RequestContext(), CancellationToken.None);
        await useCase.완료Async(ids.AccessibleId, Request(9, 포장유형코드.일반포장), RequestContext(), CancellationToken.None);
        var repack = await useCase.완료Async(ids.AccessibleId, Request(9, 포장유형코드.완충포장), RequestContext(), CancellationToken.None);
        Assert.True(partial.IsFailed); Assert.Equal(409, partial.Errors.Single().Metadata["StatusCode"]);
        Assert.True(repack.IsFailed); Assert.Equal(409, repack.Errors.Single().Metadata["StatusCode"]);
    }

    private static 포장작업완료요청 Request(int quantity, string type) => new() { PackagingQuantity = quantity, PackagingType = type, InventoryConfirmed = true, PackageLabelConfirmed = true };
    private static 포장작업UseCase CreateUseCase(SsalddelContext db, string userId, RecordingLog? logs = null, RecordingPublisher? publisher = null)
        => new(db, new FakeCurrentUserAccessor(userId, 역할명.창고관리자), logs ?? new(), publisher ?? new());
    private static 창고작업요청Context RequestContext() => new("WarehouseManagerApp", "worker-a", "포장 작업자", 역할명.창고관리자, "/work/outbound/packing", "trace-pack", "127.0.0.1", "test");
    private static SsalddelContext CreateContext() => new(new DbContextOptionsBuilder<SsalddelContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options, new DummyEncryption());
    private static async Task<(long AccessibleId, long HiddenId)> SeedAsync(SsalddelContext db)
    {
        var now = new DateTime(2026,7,20,10,0,0,DateTimeKind.Utc);
        var a = new 창고 { 소유자UserId="owner-a", 창고명="공동 창고 A", CreatedAt=now, UpdatedAt=now };
        var b = new 창고 { 소유자UserId="owner-b", 창고명="공동 창고 B", CreatedAt=now, UpdatedAt=now };
        db.창고.AddRange(a,b); await db.SaveChangesAsync();
        db.창고사용자.Add(new 창고사용자 { 창고Id=a.Id, UserId="worker-a", 역할명="출고", CreatedAt=now, UpdatedAt=now });
        var ia = new 입고요청 { 창고Id=a.Id, 주문참조번호="ORDER-A", 보관조건="냉장", CreatedAt=now, UpdatedAt=now };
        var ib = new 입고요청 { 창고Id=b.Id, 주문참조번호="ORDER-B", CreatedAt=now, UpdatedAt=now };
        db.입고요청.AddRange(ia,ib); await db.SaveChangesAsync();
        var itemA = new 입고상품 { 입고요청Id=ia.Id, 창고Id=a.Id, 상품명="감자", SKU="POTATO", 입고수량=10, 가용수량=9, 불량수량=1, 보관위치="A-02-01", 상태="적재완료", CreatedAt=now, UpdatedAt=now };
        var itemB = new 입고상품 { 입고요청Id=ib.Id, 창고Id=b.Id, 소유자UserId="worker-a", 상품명="숨김", SKU="HIDDEN", 입고수량=5, 가용수량=5, 보관위치="B-01", 상태="적재완료", CreatedAt=now, UpdatedAt=now };
        var pending = new 입고상품 { 입고요청Id=ia.Id, 창고Id=a.Id, 상품명="미적재", SKU="PENDING", 가용수량=3, 상태="검수완료", CreatedAt=now, UpdatedAt=now };
        db.입고상품.AddRange(itemA,itemB,pending); await db.SaveChangesAsync();
        db.재고이력.Add(new 재고이력 { 입고상품Id=itemA.Id, 이력유형="적재", 변경후수량=9, 처리UserId="worker-a", 메모="보관위치 A-02-01", 처리일시=now }); await db.SaveChangesAsync();
        return (itemA.Id,itemB.Id);
    }
    private sealed class FakeCurrentUserAccessor(string? userId,string? role):ICurrentUserAccessor { public string? UserId{get;}=userId; public string? Role{get;}=role; }
    private sealed class DummyEncryption:IPersonalDataEncryptionService { public string? Protect(string? value)=>value; public string? Unprotect(string? value)=>value; }
    private sealed class RecordingLog:I사용자행위로그Service { public List<사용자행위로그기록> Entries{get;}=[]; public Task 기록Async(사용자행위로그기록 entry,CancellationToken cancellationToken=default){Entries.Add(entry);return Task.CompletedTask;} }
    private sealed class RecordingPublisher:IPublisher
    {
        public List<object> Notifications{get;}=[];
        public Task Publish(object notification,CancellationToken cancellationToken=default){Notifications.Add(notification);return Task.CompletedTask;}
        public Task Publish<TNotification>(TNotification notification,CancellationToken cancellationToken=default) where TNotification:INotification { Notifications.Add(notification); return Task.CompletedTask; }
    }
}
