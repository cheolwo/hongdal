using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 무역확장원장UseCaseTests
{
    [Fact]
    public async Task 개별수입생성_원천주문을복제하지않고멱등하게연결한다()
    {
        var store = new FakeLedgerStore(OrderLedger());
        var sut = new 무역확장원장UseCase(store, new 주문원장통합UseCase(store));
        var request = new 개별수입원장생성요청
        {
            요청멱등키 = "import-1",
            기대원천Revision = 1,
            수입주체 = "주문자",
            해외판매자 = "Sample Foods",
            Incoterms후보 = "FOB"
        };

        var first = await sut.개별수입생성Async(
            "order-1",
            request,
            "user-1",
            false);
        var retry = await sut.개별수입생성Async(
            "order-1",
            request,
            "user-1",
            false);

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.False(first.Value.외부실행발생여부);
        Assert.True(retry.Value.이미처리됨);
        Assert.Equal(first.Value.원장.원장Id, retry.Value.원장.원장Id);

        var extension = await store.원장조회Async(first.Value.원장.원장Id);
        Assert.NotNull(extension);
        Assert.Equal(CommunityLedgerTemplateKeys.IndividualImport, extension!.원장템플릿Key);
        Assert.Equal("order-1", extension.외부참조["SourceOrderLedgerId"]);
        Assert.Equal("false", extension.확장속성["ContractExecutionAllowed"]);
        Assert.DoesNotContain(extension.블록목록, block =>
            block.Data.ContainsKey("상품") || block.Data.ContainsKey("가격"));

        var order = await store.원장조회Async("order-1");
        var link = Assert.Single(order!.포함원장목록);
        Assert.Equal(주문원장포함역할.개별수입, link.역할);
    }

    [Fact]
    public async Task 공동수출생성_개별수출원장만집계하고개별신고를보존한다()
    {
        var store = new FakeLedgerStore(OrderLedger());
        var sut = new 무역확장원장UseCase(store, new 주문원장통합UseCase(store));
        var export = await sut.개별수출생성Async(
            "order-1",
            new 개별수출원장생성요청
            {
                요청멱등키 = "export-1",
                기대원천Revision = 1,
                수출자 = "user-1",
                해외구매자 = "Buyer",
                목적국가코드 = "US"
            },
            "user-1",
            false);

        var group = await sut.공동수출생성Async(
            new 공동수출원장생성요청
            {
                요청멱등키 = "group-export-1",
                개별수출원장Ids = [export.Value.원장.원장Id],
                집하마감 = "2026-08-01",
                공통비배부근거 = "포장 부피"
            },
            "user-1",
            false);

        Assert.True(group.IsSuccess);
        Assert.Equal(CommunityLedgerTemplateKeys.GroupExport, group.Value.원장.원장템플릿Key);
        Assert.False(group.Value.외부실행발생여부);
        var ledger = await store.원장조회Async(group.Value.원장.원장Id);
        var included = Assert.Single(ledger!.포함원장목록);
        Assert.Equal(export.Value.원장.원장Id, included.원장Id);
        Assert.Equal(CommunityLedgerTemplateKeys.IndividualExport, included.원장템플릿Key);
        Assert.Equal(주문원장포함역할.개별수출, included.역할);
        Assert.Equal("true", ledger.블록목록[0].Data["개별신고보존"]);
        Assert.Equal("false", ledger.확장속성["ExternalTransmissionAllowed"]);
    }

    [Fact]
    public async Task 판매자수출목록_본인이참여한준비원장만_외부실행없이반환한다()
    {
        var store = new FakeLedgerStore(OrderLedger());
        var sut = new 무역확장원장UseCase(store, new 주문원장통합UseCase(store));
        var export = await sut.개별수출생성Async(
            "order-1",
            new 개별수출원장생성요청
            {
                요청멱등키 = "seller-export-1",
                수출자 = "user-1",
                해외구매자 = "Overseas Buyer",
                목적국가코드 = "US"
            },
            "user-1",
            false);

        var own = await sut.판매자수출목록조회Async(
            new 판매자수출원장목록조회요청 { PageSize = 10 },
            "user-1",
            false);
        var other = await sut.판매자수출목록조회Async(
            new 판매자수출원장목록조회요청 { PageSize = 10 },
            "seller-2",
            false);

        Assert.True(own.IsSuccess);
        Assert.False(own.Value.외부실행발생여부);
        Assert.Equal("Simulation", own.Value.실행모드);
        Assert.Equal(export.Value.원장.원장Id, Assert.Single(own.Value.Items).원장Id);
        Assert.True(other.IsSuccess);
        Assert.Empty(other.Value.Items);
    }

    [Fact]
    public async Task 판매자수출목록_원천주문의판매원장참여자에게_수출요약을연결한다()
    {
        var order = OrderLedger();
        order.포함원장목록 =
        [
            new 커뮤니티포함원장참조Dto
            {
                원장Id = "sale-1",
                원장템플릿Key = CommunityLedgerTemplateKeys.LocalSale,
                역할 = 주문원장포함역할.판매
            }
        ];
        var sale = new 커뮤니티원장Dto
        {
            원장Id = "sale-1",
            Revision = 1,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.LocalSale,
            제목 = "판매 대응",
            생성자UserId = "user-1",
            참여자목록 =
            [
                new 커뮤니티원장참여자Dto
                {
                    UserId = "seller-2",
                    DisplayName = "판매자",
                    RoleLabel = "판매자"
                }
            ]
        };
        var store = new FakeLedgerStore(order, sale);
        var sut = new 무역확장원장UseCase(store, new 주문원장통합UseCase(store));
        var export = await sut.개별수출생성Async(
            "order-1",
            new 개별수출원장생성요청
            {
                요청멱등키 = "linked-seller-export",
                수출자 = "seller-2",
                해외구매자 = "Overseas Buyer",
                목적국가코드 = "US"
            },
            "user-1",
            false);

        var result = await sut.판매자수출목록조회Async(
            new 판매자수출원장목록조회요청(),
            "seller-2",
            false);

        Assert.True(result.IsSuccess);
        Assert.Equal(export.Value.원장.원장Id, Assert.Single(result.Value.Items).원장Id);
    }

    private static 커뮤니티원장Dto OrderLedger()
        => new()
        {
            원장Id = "order-1",
            Revision = 1,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.Order,
            제목 = "원천 주문",
            상태 = 커뮤니티원장상태.진행중,
            생성자UserId = "user-1",
            생성자표시명 = "주문자",
            참여자목록 =
            [
                new 커뮤니티원장참여자Dto
                {
                    UserId = "user-1",
                    DisplayName = "주문자",
                    RoleLabel = "주문자"
                }
            ]
        };

    private sealed class FakeLedgerStore(params 커뮤니티원장Dto[] ledgers)
        : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> _ledgers =
            ledgers.ToDictionary(x => x.원장Id, StringComparer.OrdinalIgnoreCase);

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            주문원장구성정책.저장요청검증(request);
            _ledgers.TryGetValue(request.원장Id!, out var existing);
            var currentRevision = existing?.Revision ?? 0;
            if (request.기대Revision.HasValue && request.기대Revision.Value != currentRevision)
            {
                throw new InvalidOperationException("revision conflict");
            }

            var saved = new 커뮤니티원장Dto
            {
                원장Id = request.원장Id!,
                Revision = currentRevision + 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                원함 = request.원함,
                상태 = request.상태 ?? existing?.상태 ?? 커뮤니티원장상태.초안,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? "익명 참여자",
                블록목록 = request.블록목록,
                참여자목록 = request.참여자목록,
                포함원장목록 = request.포함원장목록 ?? existing?.포함원장목록 ?? [],
                다이어그램스냅샷 = request.다이어그램스냅샷,
                외부참조 = request.외부참조,
                확장속성 = request.확장속성
            };
            _ledgers[saved.원장Id] = saved;
            return Task.FromResult(saved);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_ledgers.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(_ledgers.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(null);
    }
}
