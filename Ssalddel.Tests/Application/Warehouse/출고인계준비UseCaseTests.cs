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

public sealed class 출고인계준비UseCaseTests
{
    [Fact]
    public async Task 목록은_창고범위의포장완료재고만반환한다()
    {
        await using var db=CreateContext(); var ids=await SeedAsync(db);
        var result=await CreateUseCase(db,"worker-a").목록Async(new 출고인계준비목록조회요청(),CancellationToken.None);
        Assert.True(result.IsSuccess); var item=Assert.Single(result.Value.Items); Assert.Equal(ids.AccessibleId,item.InboundItemId); Assert.False(item.IsHandoffReady);
    }

    [Fact]
    public async Task 상세는_포장근거를제공하고_범위밖재고를404로숨긴다()
    {
        await using var db=CreateContext(); var ids=await SeedAsync(db); var useCase=CreateUseCase(db,"worker-a");
        var found=await useCase.상세Async(ids.AccessibleId,CancellationToken.None); var hidden=await useCase.상세Async(ids.HiddenId,CancellationToken.None);
        Assert.True(found.IsSuccess); Assert.Equal("냉장포장",found.Value.PackagingType); Assert.NotNull(found.Value.PackedAtUtc);
        Assert.True(hidden.IsFailed); Assert.Equal(404,hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 완료는_두확인과전체가용수량을요구하고_출고예정이력감사Event를한번만생성한다()
    {
        await using var db=CreateContext(); var ids=await SeedAsync(db); var logs=new RecordingLog(); var publisher=new RecordingPublisher();
        var useCase=CreateUseCase(db,"worker-a",logs,publisher);
        var invalid=await useCase.완료Async(ids.AccessibleId,new 출고인계준비완료요청{HandoffQuantity=9},RequestContext(),CancellationToken.None);
        var request=Request(9); var first=await useCase.완료Async(ids.AccessibleId,request,RequestContext(),CancellationToken.None); var replay=await useCase.완료Async(ids.AccessibleId,request,RequestContext(),CancellationToken.None);
        Assert.True(invalid.IsFailed); Assert.Equal(400,invalid.Errors.Single().Metadata["StatusCode"]);
        Assert.True(first.IsSuccess); Assert.Equal(출고상태.준비중,first.Value.OutboundStatus); Assert.False(first.Value.IdempotentReplay);
        Assert.True(replay.IsSuccess); Assert.True(replay.Value.IdempotentReplay);
        Assert.Single(await db.출고예정.Where(x=>x.입고상품Id==ids.AccessibleId).ToArrayAsync());
        Assert.Single(await db.재고이력.Where(x=>x.입고상품Id==ids.AccessibleId&&x.이력유형=="출고인계준비").ToArrayAsync());
        Assert.Empty(await db.재고이동.Where(x=>x.입고상품Id==ids.AccessibleId&&x.이동유형==재고이동유형.예약).ToArrayAsync());
        var item=await db.입고상품.SingleAsync(x=>x.Id==ids.AccessibleId); Assert.Equal(9,item.가용수량); Assert.Equal(2,item.예약수량);
        Assert.Empty(db.화주운송의뢰); Assert.Single(logs.Entries); Assert.IsType<창고출고인계준비완료됨Event>(Assert.Single(publisher.Notifications));
    }

    [Fact]
    public async Task 부분인계와_완료후다른수량재요청은_별도조정업무로분리한다()
    {
        await using var db=CreateContext(); var ids=await SeedAsync(db); var useCase=CreateUseCase(db,"worker-a");
        var partial=await useCase.완료Async(ids.AccessibleId,Request(4),RequestContext(),CancellationToken.None);
        await useCase.완료Async(ids.AccessibleId,Request(9),RequestContext(),CancellationToken.None);
        var changed=await useCase.완료Async(ids.AccessibleId,Request(8),RequestContext(),CancellationToken.None);
        Assert.True(partial.IsFailed); Assert.Equal(409,partial.Errors.Single().Metadata["StatusCode"]);
        Assert.True(changed.IsFailed); Assert.Equal(409,changed.Errors.Single().Metadata["StatusCode"]);
    }

    private static 출고인계준비완료요청 Request(int quantity)=>new(){HandoffQuantity=quantity,PackageSealConfirmed=true,TransportConditionsConfirmed=true};
    private static 출고인계준비UseCase CreateUseCase(SsalddelContext db,string userId,RecordingLog? logs=null,RecordingPublisher? publisher=null)
        =>new(db,new FakeCurrentUserAccessor(userId,역할명.창고관리자),logs??new(),publisher??new());
    private static 창고작업요청Context RequestContext()=>new("WarehouseManagerApp","worker-a","출고 작업자",역할명.창고관리자,"/warehouse/general/transport-handoff","trace-handoff","127.0.0.1","test");
    private static SsalddelContext CreateContext()=>new(new DbContextOptionsBuilder<SsalddelContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options,new DummyEncryption());
    private static async Task<(long AccessibleId,long HiddenId)> SeedAsync(SsalddelContext db)
    {
        var now=new DateTime(2026,7,20,11,0,0,DateTimeKind.Utc);
        var a=new 창고{소유자UserId="owner-a",창고명="공동 창고 A",CreatedAt=now,UpdatedAt=now}; var b=new 창고{소유자UserId="owner-b",창고명="공동 창고 B",CreatedAt=now,UpdatedAt=now};
        db.창고.AddRange(a,b); await db.SaveChangesAsync(); db.창고사용자.Add(new 창고사용자{창고Id=a.Id,UserId="worker-a",역할명="출고",CreatedAt=now,UpdatedAt=now});
        var ia=new 입고요청{창고Id=a.Id,주문참조번호="ORDER-A",주문자UserId="buyer-a",판매자UserId="seller-a",보관조건="냉장",CreatedAt=now,UpdatedAt=now};
        var ib=new 입고요청{창고Id=b.Id,주문참조번호="ORDER-B",주문자UserId="buyer-b",판매자UserId="seller-b",CreatedAt=now,UpdatedAt=now}; db.입고요청.AddRange(ia,ib); await db.SaveChangesAsync();
        var itemA=new 입고상품{입고요청Id=ia.Id,창고Id=a.Id,상품명="감자",SKU="POTATO",입고수량=11,가용수량=9,예약수량=2,보관위치="A-02",상태="포장완료-냉장포장",CreatedAt=now,UpdatedAt=now};
        var itemB=new 입고상품{입고요청Id=ib.Id,창고Id=b.Id,소유자UserId="worker-a",상품명="숨김",SKU="HIDDEN",가용수량=5,상태="포장완료-일반포장",CreatedAt=now,UpdatedAt=now};
        var pending=new 입고상품{입고요청Id=ia.Id,창고Id=a.Id,상품명="미포장",SKU="PENDING",가용수량=3,상태="적재완료",CreatedAt=now,UpdatedAt=now}; db.입고상품.AddRange(itemA,itemB,pending); await db.SaveChangesAsync();
        db.재고이력.Add(new 재고이력{입고상품Id=itemA.Id,이력유형="포장",변경후수량=9,처리UserId="worker-a",메모="포장 9개 / 냉장포장",처리일시=now}); await db.SaveChangesAsync(); return(itemA.Id,itemB.Id);
    }
    private sealed class FakeCurrentUserAccessor(string? userId,string? role):ICurrentUserAccessor{public string? UserId{get;}=userId;public string? Role{get;}=role;}
    private sealed class DummyEncryption:IPersonalDataEncryptionService{public string? Protect(string? value)=>value;public string? Unprotect(string? value)=>value;}
    private sealed class RecordingLog:I사용자행위로그Service{public List<사용자행위로그기록> Entries{get;}=[];public Task 기록Async(사용자행위로그기록 entry,CancellationToken cancellationToken=default){Entries.Add(entry);return Task.CompletedTask;}}
    private sealed class RecordingPublisher:IPublisher
    {
        public List<object> Notifications{get;}=[]; public Task Publish(object notification,CancellationToken cancellationToken=default){Notifications.Add(notification);return Task.CompletedTask;}
        public Task Publish<TNotification>(TNotification notification,CancellationToken cancellationToken=default) where TNotification:INotification{Notifications.Add(notification);return Task.CompletedTask;}
    }
}
