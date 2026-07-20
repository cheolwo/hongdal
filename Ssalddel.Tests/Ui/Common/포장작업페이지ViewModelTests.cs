using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 포장작업페이지ViewModelTests
{
    [Fact] public async Task 초기화는_첫항목을자동선택하지않는다()
    { var service=new FakeService{List=new(){Items=[new(){InboundItemId=1}]}}; var page=Create(service); Assert.True(await page.초기화Async()); Assert.Null(page.상세.조회대상Id); Assert.Empty(service.DetailIds); }
    [Fact] public async Task 완료는_전체가용수량과두확인을요구하고_성공뒤같은Id를재조회한다()
    {
        var service=new FakeService{Detail=new(){InboundItemId=7,CanPack=true,AvailableQuantity=9,InventoryStatus="적재완료"}}; var page=Create(service); await page.초기화Async(7);
        page.작성.포장유형=포장유형코드.냉장포장; page.작성.재고확인=true; page.작성.포장표찰확인=true;
        service.Complete=new(){InboundItemId=7,InventoryStatus="포장완료-냉장포장",PackagingQuantity=9}; service.Detail=new(){InboundItemId=7,AvailableQuantity=9,InventoryStatus="포장완료-냉장포장"};
        Assert.True(await page.완료후재조회Async()); Assert.Equal([7L,7L],service.DetailIds); Assert.Equal("포장완료-냉장포장",page.상세.항목!.InventoryStatus);
    }
    [Fact] public async Task 검색은_상태와검색어를첫페이지로전달한다()
    { var service=new FakeService(); var page=Create(service); page.목록.검색어=" 감자 "; page.목록.조회상태=포장작업조회상태코드.완료; await page.검색Async(); var r=Assert.Single(service.Requests); Assert.Equal("감자",r.Search); Assert.Equal(0,r.Page); }
    private static 포장작업PageViewModel Create(FakeService s)=>new(new(s),new(s),new(s));
    private sealed class FakeService:I포장작업페이지Service
    {
        public 포장작업목록페이지응답 List{get;set;}=new(); public 포장작업상세응답? Detail{get;set;} public 포장작업결과응답? Complete{get;set;}
        public List<포장작업목록조회요청> Requests{get;}=[]; public List<long> DetailIds{get;}=[];
        public Task<포장작업목록페이지응답> 목록조회Async(포장작업목록조회요청 request,CancellationToken cancellationToken=default){Requests.Add(request);return Task.FromResult(List);}
        public Task<포장작업상세응답?> 상세조회Async(long id,CancellationToken cancellationToken=default){DetailIds.Add(id);return Task.FromResult(Detail);}
        public Task<포장작업결과응답?> 완료Async(long id,포장작업완료요청 request,CancellationToken cancellationToken=default)=>Task.FromResult(Complete);
    }
}
