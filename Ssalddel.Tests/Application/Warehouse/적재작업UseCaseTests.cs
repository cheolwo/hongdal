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

public sealed class 적재작업UseCaseTests
{
    [Fact]
    public async Task 목록은_창고범위의검수완료재고만반환한다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var useCase = CreateUseCase(db, "worker-a");
        var result = await useCase.목록Async(new 적재작업목록조회요청(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(ids.AccessibleId, item.InboundItemId);
        Assert.True(item.CanPutAway);
    }

    [Fact]
    public async Task 상세는_정확한Id만조회하고_범위밖은404로숨긴다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var useCase = CreateUseCase(db, "worker-a");
        var found = await useCase.상세Async(ids.AccessibleId, CancellationToken.None);
        var hidden = await useCase.상세Async(ids.HiddenId, CancellationToken.None);
        Assert.True(found.IsSuccess);
        Assert.Equal("ORDER-A", found.Value.OrderReference);
        Assert.NotNull(found.Value.InspectedAtUtc);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 완료는_두확인을요구하고_상태이력이동감사Event를한번만생성한다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var logs = new RecordingLog(); var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(db, "worker-a", logs, publisher);
        var invalid = await useCase.완료Async(ids.AccessibleId, new 적재작업완료요청 { StorageLocation = "A-02-01" }, RequestContext(), CancellationToken.None);
        var request = new 적재작업완료요청 { StorageLocation = "A-02-01", Memo = "표찰 확인", InspectionResultConfirmed = true, LocationLabelConfirmed = true };
        var first = await useCase.완료Async(ids.AccessibleId, request, RequestContext(), CancellationToken.None);
        var replay = await useCase.완료Async(ids.AccessibleId, request, RequestContext(), CancellationToken.None);

        Assert.True(invalid.IsFailed); Assert.Equal(400, invalid.Errors.Single().Metadata["StatusCode"]);
        Assert.True(first.IsSuccess); Assert.Equal("적재완료", first.Value.InventoryStatus); Assert.False(first.Value.IdempotentReplay);
        Assert.True(replay.IsSuccess); Assert.True(replay.Value.IdempotentReplay);
        Assert.Single(await db.재고이력.Where(x => x.입고상품Id == ids.AccessibleId && x.이력유형 == "적재").ToArrayAsync());
        Assert.Single(await db.재고이동.Where(x => x.입고상품Id == ids.AccessibleId && x.이동유형 == "적재").ToArrayAsync());
        Assert.Single(logs.Entries);
        Assert.IsType<창고적재위치배정됨Event>(Assert.Single(publisher.Notifications));
    }

    [Fact]
    public async Task 적재완료후_다른위치재요청은_재고이동작업으로분리하도록거부한다()
    {
        await using var db = CreateContext(); var ids = await SeedAsync(db);
        var useCase = CreateUseCase(db, "worker-a");
        var request = new 적재작업완료요청 { StorageLocation = "A-02-01", InspectionResultConfirmed = true, LocationLabelConfirmed = true };
        await useCase.완료Async(ids.AccessibleId, request, RequestContext(), CancellationToken.None);
        request.StorageLocation = "B-01-01";
        var result = await useCase.완료Async(ids.AccessibleId, request, RequestContext(), CancellationToken.None);
        Assert.True(result.IsFailed); Assert.Equal(409, result.Errors.Single().Metadata["StatusCode"]);
    }

    private static 적재작업UseCase CreateUseCase(SsalddelContext db, string userId, RecordingLog? logs = null, RecordingPublisher? publisher = null)
        => new(db, new FakeCurrentUserAccessor(userId, 역할명.창고관리자), logs ?? new(), publisher ?? new());
    private static 창고작업요청Context RequestContext() => new("WarehouseManagerApp", "worker-a", "적재 작업자", 역할명.창고관리자, "/work/inbound/put-away", "trace-put", "127.0.0.1", "test");
    private static SsalddelContext CreateContext() => new(new DbContextOptionsBuilder<SsalddelContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options, new DummyEncryption());
    private static async Task<(long AccessibleId, long HiddenId)> SeedAsync(SsalddelContext db)
    {
        var now = new DateTime(2026,7,20,9,0,0,DateTimeKind.Utc);
        var a = new 창고 { 소유자UserId="owner-a", 창고명="공동 창고 A", CreatedAt=now, UpdatedAt=now };
        var b = new 창고 { 소유자UserId="owner-b", 창고명="공동 창고 B", CreatedAt=now, UpdatedAt=now };
        db.창고.AddRange(a,b); await db.SaveChangesAsync();
        db.창고사용자.Add(new 창고사용자 { 창고Id=a.Id, UserId="worker-a", 역할명="재고", CreatedAt=now, UpdatedAt=now });
        var ia = new 입고요청 { 창고Id=a.Id, 주문참조번호="ORDER-A", 보관조건="냉장", CreatedAt=now, UpdatedAt=now };
        var ib = new 입고요청 { 창고Id=b.Id, 주문참조번호="ORDER-B", CreatedAt=now, UpdatedAt=now };
        db.입고요청.AddRange(ia,ib); await db.SaveChangesAsync();
        var itemA = new 입고상품 { 입고요청Id=ia.Id, 창고Id=a.Id, 상품명="감자", SKU="POTATO", 입고수량=10, 가용수량=9, 불량수량=1, 상태="검수완료-불량포함", CreatedAt=now, UpdatedAt=now };
        var itemB = new 입고상품 { 입고요청Id=ib.Id, 창고Id=b.Id, 소유자UserId="worker-a", 상품명="숨김", SKU="HIDDEN", 입고수량=5, 가용수량=5, 상태="검수완료", CreatedAt=now, UpdatedAt=now };
        var pending = new 입고상품 { 입고요청Id=ia.Id, 창고Id=a.Id, 상품명="미검수", SKU="PENDING", 상태="보관중", CreatedAt=now, UpdatedAt=now };
        db.입고상품.AddRange(itemA,itemB,pending); await db.SaveChangesAsync();
        db.재고이력.Add(new 재고이력 { 입고상품Id=itemA.Id, 이력유형="입고검수", 변경후수량=9, 처리UserId="worker-a", 메모="검수 10, 불량 1", 처리일시=now }); await db.SaveChangesAsync();
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
