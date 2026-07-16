using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class 공동구매창고ViewModelTests
{
    [Fact]
    public async Task 초기화_선택창고를기준으로입고원장과출고원장을함께필터링한다()
    {
        var service = new Fake공동구매창고Service
        {
            창고목록응답 =
            [
                new 창고요약응답 { Id = 1, 창고명 = "서울", 기본창고여부 = true },
                new 창고요약응답 { Id = 2, 창고명 = "부산" }
            ],
            입고목록응답 =
            [
                new 입고요청항목응답 { Id = 101, 창고Id = 1 },
                new 입고요청항목응답 { Id = 202, 창고Id = 2 }
            ],
            재고목록응답 =
            [
                new 재고항목응답 { 입고상품Id = 11, 창고Id = 1 },
                new 재고항목응답 { 입고상품Id = 22, 창고Id = 2 }
            ]
        };
        var state = new 공동구매창고상태ViewModel();
        using var viewModel = CreateViewModel(service, state);

        Assert.True(await viewModel.초기화Async());
        Assert.Equal(101, Assert.Single(viewModel.입고원장.입고요청목록).Id);
        Assert.Equal(11, Assert.Single(viewModel.출고원장.출고가능재고목록).입고상품Id);

        Assert.True(viewModel.기준정보.창고선택(2));

        Assert.Equal(202, Assert.Single(viewModel.입고원장.입고요청목록).Id);
        Assert.Equal(22, Assert.Single(viewModel.출고원장.출고가능재고목록).입고상품Id);
        Assert.Equal(202, viewModel.입고원장.선택된입고요청?.Id);
        Assert.Equal(22, viewModel.출고원장.선택된재고?.입고상품Id);
    }

    [Fact]
    public async Task 입고완료_입고원장에상품을반영하고재고를다시조회한다()
    {
        var service = new Fake공동구매창고Service
        {
            입고완료응답 =
            [
                new 입고상품항목응답 { Id = 71, 입고요청Id = 7, 창고Id = 1, 상품명 = "감자" }
            ],
            재고목록응답 =
            [
                new 재고항목응답 { 입고상품Id = 71, 창고Id = 1, 상품명 = "감자", 가용수량 = 10 }
            ]
        };
        var state = new 공동구매창고상태ViewModel();
        state.창고목록적용([new 창고요약응답 { Id = 1, 기본창고여부 = true }]);
        state.입고목록적용([new 입고요청항목응답 { Id = 7, 창고Id = 1 }]);
        using var viewModel = new 공동구매입고원장ViewModel(service, state);
        viewModel.입고완료초안.Items =
        [
            new 입고상품저장요청 { 상품명 = "감자", 입고수량 = 10 }
        ];

        Assert.True(await viewModel.입고완료Async());

        Assert.Equal(7, service.마지막입고완료Id);
        Assert.Equal(71, Assert.Single(viewModel.최근입고상품목록).Id);
        Assert.Equal(71, Assert.Single(viewModel.재고목록).입고상품Id);
    }

    [Fact]
    public async Task 출고원장_가용재고를포장하고운송원장으로인계한다()
    {
        var service = new Fake공동구매창고Service
        {
            포장응답 = new 창고작업결과응답 { 입고상품Id = 51, 작업유형 = "Pack" },
            운송인계응답 = new 화주운송의뢰응답 { 의뢰Id = "transport-51" }
        };
        var state = new 공동구매창고상태ViewModel();
        state.창고목록적용([new 창고요약응답 { Id = 1, 기본창고여부 = true }]);
        state.재고목록적용(
        [
            new 재고항목응답 { 입고상품Id = 51, 창고Id = 1, 상품명 = "감자", 가용수량 = 8 }
        ]);
        using var viewModel = new 공동구매출고원장ViewModel(service, state);
        viewModel.포장초안.포장수량 = 5;
        viewModel.운송인계초안.요청수량 = 5;
        viewModel.운송인계초안.하차지주소 = "서울시 중구";

        Assert.True(await viewModel.포장Async());
        Assert.True(await viewModel.운송인계Async());

        Assert.Equal(주문원장포함역할.창고출고, viewModel.주문원장역할코드);
        Assert.Equal(51, service.마지막포장입고상품Id);
        Assert.Equal(51, service.마지막운송인계요청?.입고상품Id);
        Assert.Equal("transport-51", viewModel.최근운송의뢰?.의뢰Id);
        Assert.False(viewModel.출고목록Api지원됨);
        Assert.False(viewModel.출고완료Api지원됨);
    }

    private static 공동구매창고기능ViewModel CreateViewModel(
        I공동구매창고Service service,
        공동구매창고상태ViewModel state)
        => new(
            state,
            new 공동구매창고기준정보ViewModel(service, state),
            new 공동구매입고원장ViewModel(service, state),
            new 공동구매출고원장ViewModel(service, state));

    private sealed class Fake공동구매창고Service : I공동구매창고Service
    {
        public IReadOnlyList<창고요약응답> 창고목록응답 { get; set; } = [];
        public IReadOnlyList<입고요청항목응답> 입고목록응답 { get; set; } = [];
        public IReadOnlyList<입고상품항목응답> 입고완료응답 { get; set; } = [];
        public IReadOnlyList<재고항목응답> 재고목록응답 { get; set; } = [];
        public 창고작업결과응답? 포장응답 { get; set; }
        public 화주운송의뢰응답? 운송인계응답 { get; set; }
        public long? 마지막입고완료Id { get; private set; }
        public long? 마지막포장입고상품Id { get; private set; }
        public 재고운송의뢰생성요청? 마지막운송인계요청 { get; private set; }

        public Task<IReadOnlyList<창고요약응답>> 창고목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(창고목록응답);

        public Task<창고요약응답?> 창고생성Async(
            창고저장요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<창고요약응답?>(null);

        public Task<IReadOnlyList<창고사용자항목응답>> 창고사용자목록조회Async(
            long warehouseId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<창고사용자항목응답>>([]);

        public Task<창고사용자항목응답?> 창고사용자추가Async(
            long warehouseId,
            창고사용자저장요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<창고사용자항목응답?>(null);

        public Task<IReadOnlyList<입고요청항목응답>> 입고목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(입고목록응답);

        public Task<입고요청항목응답?> 입고요청생성Async(
            입고요청저장요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<입고요청항목응답?>(null);

        public Task<IReadOnlyList<입고상품항목응답>> 입고완료Async(
            long inboundId,
            입고완료요청 request,
            CancellationToken cancellationToken = default)
        {
            마지막입고완료Id = inboundId;
            return Task.FromResult(입고완료응답);
        }

        public Task<IReadOnlyList<재고항목응답>> 재고목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(재고목록응답);

        public Task<창고작업결과응답?> 입고검수Async(
            long inboundItemId,
            입고검수요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<창고작업결과응답?>(null);

        public Task<창고작업결과응답?> 적재위치배정Async(
            long inboundItemId,
            적재위치배정요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<창고작업결과응답?>(null);

        public Task<창고작업결과응답?> 포장작업Async(
            long inboundItemId,
            포장작업요청 request,
            CancellationToken cancellationToken = default)
        {
            마지막포장입고상품Id = inboundItemId;
            return Task.FromResult(포장응답);
        }

        public Task<화주운송의뢰응답?> 운송인계Async(
            재고운송의뢰생성요청 request,
            CancellationToken cancellationToken = default)
        {
            마지막운송인계요청 = request;
            return Task.FromResult(운송인계응답);
        }
    }
}
