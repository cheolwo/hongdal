using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 적재작업페이지ViewModelTests
{
    [Fact] public async Task 초기화는_첫항목을자동선택하지않는다()
    { var service=new FakeService{List=new(){Items=[new(){InboundItemId=1}]}}; var page=Create(service); Assert.True(await page.초기화Async()); Assert.Null(page.상세.조회대상Id); Assert.Empty(service.DetailIds); }
    [Fact] public async Task 완료는_두확인을요구하고_성공뒤같은Id를재조회한다()
    {
        var service=new FakeService{Detail=new(){InboundItemId=7,CanPutAway=true,InventoryStatus="검수완료"}}; var page=Create(service); await page.초기화Async(7);
        page.작성.보관위치="A-01"; page.작성.검수결과확인=true; page.작성.위치표찰확인=true;
        service.Complete=new(){InboundItemId=7,InventoryStatus="적재완료",StorageLocation="A-01"}; service.Detail=new(){InboundItemId=7,InventoryStatus="적재완료",StorageLocation="A-01"};
        Assert.True(await page.완료후재조회Async()); Assert.Equal([7L,7L],service.DetailIds); Assert.Equal("적재완료",page.상세.항목!.InventoryStatus);
    }
    [Fact] public async Task 검색은_상태와검색어를첫페이지로전달한다()
    { var service=new FakeService(); var page=Create(service); page.목록.검색어=" 감자 "; page.목록.조회상태=적재작업조회상태코드.완료; await page.검색Async(); var r=Assert.Single(service.Requests); Assert.Equal("감자",r.Search); Assert.Equal(0,r.Page); }
    private static 적재작업PageViewModel Create(FakeService s)=>new(new(s),new(s),new(s));
    private sealed class FakeService:I적재작업페이지Service
    {
        public 적재작업목록페이지응답 List{get;set;}=new(); public 적재작업상세응답? Detail{get;set;} public 적재작업결과응답? Complete{get;set;}
        public List<적재작업목록조회요청> Requests{get;}=[]; public List<long> DetailIds{get;}=[];
        public Task<적재작업목록페이지응답> 목록조회Async(적재작업목록조회요청 request,CancellationToken cancellationToken=default){Requests.Add(request);return Task.FromResult(List);}
        public Task<적재작업상세응답?> 상세조회Async(long id,CancellationToken cancellationToken=default){DetailIds.Add(id);return Task.FromResult(Detail);}
        public Task<적재작업결과응답?> 완료Async(long id,적재작업완료요청 request,CancellationToken cancellationToken=default)=>Task.FromResult(Complete);
    }
}
