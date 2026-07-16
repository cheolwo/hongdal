using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Sales;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class 기본판매주문ViewModelTests
{
    [Fact]
    public void 판매와주문은업무단위계층으로구성된다()
    {
        Assert.True(typeof(판매업무ViewModelBase).IsAssignableFrom(typeof(판매채널계정ViewModel)));
        Assert.True(typeof(판매업무ViewModelBase).IsAssignableFrom(typeof(상품등록ViewModel)));
        Assert.True(typeof(판매업무ViewModelBase).IsAssignableFrom(typeof(채널출품ViewModel)));
        Assert.True(typeof(주문업무ViewModelBase).IsAssignableFrom(typeof(주문조회ViewModel)));
        Assert.True(typeof(주문업무ViewModelBase).IsAssignableFrom(typeof(주문하위원장ViewModel)));
        Assert.True(typeof(주문업무ViewModelBase).IsAssignableFrom(typeof(주문서명ViewModel)));

        Assert.Contains(
            typeof(판매ViewModel),
            typeof(국내판매ViewModel).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Contains(
            typeof(판매ViewModel),
            typeof(해외수출ViewModel).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Contains(
            typeof(주문ViewModel),
            typeof(공동구매주문원장ViewModel).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public async Task 기본판매는계정상품출품을같은상태로조립한다()
    {
        var service = new Fake판매Service();
        var state = new 판매업무상태ViewModel();
        var account = new 판매채널계정ViewModel(service, state);
        var product = new 상품등록ViewModel(service, state);
        var listing = new 채널출품ViewModel(service, state);
        var accountQuery = new 판매채널계정조회ViewModel(service, state);
        var accountCreate = new 판매채널계정등록ViewModel(service, state);
        var productQuery = new 판매상품조회ViewModel(service, state);
        var productCreate = new 판매상품등록ViewModel(service, state);
        var listingQuery = new 채널출품조회ViewModel(service, state);
        var listingCreate = new 채널출품등록ViewModel(service, state);
        using var sales = new 판매ViewModel(
            state,
            account,
            product,
            listing,
            accountQuery,
            accountCreate,
            productQuery,
            productCreate,
            listingQuery,
            listingCreate);

        account.지원채널설정(["domestic"]);
        account.초안.채널종류 = "domestic";
        account.초안.상점명 = "국내 상점";
        Assert.True(await account.생성Async());

        product.입고상품연결(10, "사과", "APPLE-01");
        product.초안.판매가 = 12000m;
        Assert.True(await product.등록Async());

        Assert.True(listing.계정선택(state.선택된계정!.Id));
        Assert.True(listing.상품선택(state.선택된상품!.Id));
        Assert.True(await listing.생성Async());

        Assert.Single(sales.계정.계정목록);
        Assert.Single(sales.상품등록.상품목록);
        Assert.Single(sales.출품.출품목록);
        Assert.NotNull(state.선택된출품);
        Assert.Equal(6, sales.세부업무목록.Count);
        Assert.All(sales.세부업무목록, item => Assert.False(string.IsNullOrWhiteSpace(item.업무코드)));
        Assert.IsAssignableFrom<I목록조회ViewModel<판매상품항목응답>>(sales.상품조회);
        Assert.IsAssignableFrom<I명령ViewModel<채널출품저장요청>>(sales.출품등록);
    }

    [Fact]
    public async Task 기본주문은선택한원장을조회하고하위원장을연결한다()
    {
        var service = new Fake주문Service();
        var state = new 주문업무상태ViewModel();
        var query = new 주문조회ViewModel(service, state);
        var child = new 주문하위원장ViewModel(service, state);
        var signature = new 주문서명ViewModel(service, state);
        var childConnect = new 주문하위원장연결ViewModel(service, state);
        var childDetach = new 주문하위원장분리ViewModel(service, state);
        var signatureQuery = new 주문서명상태조회ViewModel(service, state);
        var signaturePrepare = new 주문서명준비ViewModel(service, state);
        var signatureCreate = new 주문서명등록ViewModel(service, state);
        using var order = new 주문ViewModel(
            state,
            query,
            child,
            signature,
            childConnect,
            childDetach,
            signatureQuery,
            signaturePrepare,
            signatureCreate);

        order.조회.주문원장선택("order-root-1");
        Assert.True(await order.조회.조회Async());

        order.하위원장.연결초안.하위원장Id = "sales-ledger-1";
        order.하위원장.연결초안.역할 = 주문원장포함역할.판매;
        Assert.True(await order.하위원장.연결Async());

        Assert.Equal("order-root-1", state.선택된주문원장Id);
        Assert.Equal("orderer", state.역할별결과?.조회역할);
        Assert.Equal(2, state.현재Revision);
        Assert.Equal(6, order.세부업무목록.Count);
    }

    private sealed class Fake판매Service : I판매채널Client
    {
        private long _id;

        public Task<IReadOnlyList<판매채널계정항목응답>> 계정목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<판매채널계정항목응답>>([]);

        public Task<판매채널계정항목응답?> 계정생성Async(
            판매채널계정저장요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<판매채널계정항목응답?>(new()
            {
                Id = ++_id,
                채널종류 = request.채널종류,
                상점명 = request.상점명
            });

        public Task<IReadOnlyList<판매상품항목응답>> 상품목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<판매상품항목응답>>([]);

        public Task<판매상품항목응답?> 상품생성Async(
            판매상품저장요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<판매상품항목응답?>(new()
            {
                Id = ++_id,
                입고상품Id = request.입고상품Id,
                대표상품명 = request.대표상품명,
                판매SKU = request.판매SKU,
                판매가 = request.판매가
            });

        public Task<IReadOnlyList<채널출품항목응답>> 출품목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<채널출품항목응답>>([]);

        public Task<채널출품항목응답?> 출품생성Async(
            채널출품저장요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<채널출품항목응답?>(new()
            {
                Id = ++_id,
                판매상품Id = request.판매상품Id,
                판매채널계정Id = request.판매채널계정Id
            });
    }

    private sealed class Fake주문Service : I주문원장Service
    {
        public Task<주문원장역할별조회공개Dto?> 주문원장보호조회Async(
            string orderLedgerId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장역할별조회공개Dto?>(new()
            {
                주문원장Id = orderLedgerId,
                조회역할 = "orderer",
                주문원장상세 = new 주문원장원장요약Dto { 원장Id = orderLedgerId, Revision = 1 }
            });

        public Task<주문원장역할별조회공개Dto?> 주문원장역할조회Async(
            string orderLedgerId,
            string viewCode,
            CancellationToken cancellationToken = default)
            => 주문원장보호조회Async(orderLedgerId, cancellationToken);

        public Task<주문원장통합공개Dto?> 하위원장연결Async(
            string orderLedgerId,
            주문하위원장연결ClientRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장통합공개Dto?>(new()
            {
                주문원장 = new 주문원장원장요약Dto { 원장Id = orderLedgerId, Revision = 2 }
            });

        public Task<주문원장통합공개Dto?> 하위원장분리Async(
            string orderLedgerId,
            string childLedgerId,
            long? expectedRevision = null,
            CancellationToken cancellationToken = default)
            => 하위원장연결Async(orderLedgerId, new 주문하위원장연결ClientRequest(), cancellationToken);

        public Task<주문원장서명상태공개Dto?> 주문원장서명상태조회Async(
            string orderLedgerId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<주문원장서명상태공개Dto?>(new() { 주문원장Id = orderLedgerId, Revision = 2 });

        public Task<주문원장서명상태공개Dto?> 주문원장서명준비Async(
            string orderLedgerId,
            주문원장서명준비ClientRequest request,
            CancellationToken cancellationToken = default)
            => 주문원장서명상태조회Async(orderLedgerId, cancellationToken);

        public Task<주문원장서명상태공개Dto?> 주문원장서명등록Async(
            string orderLedgerId,
            주문원장서명등록ClientRequest request,
            CancellationToken cancellationToken = default)
            => 주문원장서명상태조회Async(orderLedgerId, cancellationToken);
    }
}
