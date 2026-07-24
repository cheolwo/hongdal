using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매개별주문원장ServiceTests
{
    [Fact]
    public async Task B2B거래문맥은_주문원장사슬에전파하고_입고원장에는조직식별정보를복제하지않는다()
    {
        var store = new FakeLedgerStore(new 커뮤니티원장Dto
        {
            원장Id = "group-purchase-1",
            Revision = 1,
            커뮤니티Id = "community-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
            제목 = "사업용 감자 공동구매",
            상태 = 커뮤니티원장상태.진행중,
            생성자UserId = "coordinator-1",
            생성자표시명 = "공동구매 대표"
        });
        var service = new 공동구매개별주문원장Service(
            store,
            new 주문원장통합UseCase(store));
        var demand = new 공동구매자동수요응답
        {
            수요Id = "demand-1",
            커뮤니티원장Id = "group-purchase-1",
            거래유형 = 공동구매거래유형코드.B2B,
            가격표시기준 = 공동구매가격표시기준코드.부가세별도,
            구매조직참조키 = "organization-1",
            구매조직표시명 = "동네 식당",
            사업자검증상태 = 주문자집단사업자검증상태코드.대기,
            세금계산서필요 = true,
            주문자키 = "orderer-1",
            주문자표시명 = "식당 구매 담당자",
            도착창고Id = 17,
            도착창고유형 = "Virtual",
            도착창고명 = "공동 수령 창고",
            희망수량 = 50,
            수량단위 = "kg"
        };
        var group = new 공동구매자동집단응답
        {
            자동집단Id = "auto-group-b2b-potato",
            상품키 = "potato",
            상품명 = "감자",
            거래유형 = 공동구매거래유형코드.B2B,
            가격표시기준 = 공동구매가격표시기준코드.부가세별도,
            수요목록 = [demand]
        };

        var result = await service.생성및연결Async(group, demand);

        var groupOrder = await store.원장조회Async(result.공동구매주문집계원장Id);
        Assert.NotNull(groupOrder);
        Assert.Equal(공동구매거래유형코드.B2B, groupOrder.외부참조[공동구매거래문맥원장키.거래유형]);
        Assert.Equal("1", groupOrder.외부참조[공동구매거래문맥원장키.구매조직수]);

        var individualOrder = await store.원장조회Async(result.개별주문원장Id);
        Assert.NotNull(individualOrder);
        Assert.Equal("organization-1", individualOrder.외부참조[공동구매거래문맥원장키.구매조직참조키]);
        Assert.Equal(result.공동구매주문집계원장Id, individualOrder.외부참조[공동구매거래문맥원장키.원천거래문맥원장Id]);

        var inbound = await store.원장조회Async(result.입고예정원장Id);
        Assert.NotNull(inbound);
        Assert.Equal(공동구매거래유형코드.B2B, inbound.외부참조[공동구매거래문맥원장키.거래유형]);
        Assert.Equal(result.개별주문원장Id, inbound.외부참조[공동구매거래문맥원장키.원천거래문맥원장Id]);
        Assert.False(inbound.외부참조.ContainsKey(공동구매거래문맥원장키.구매조직참조키));
        Assert.False(inbound.외부참조.ContainsKey(공동구매거래문맥원장키.구매조직표시명));
    }

    private sealed class FakeLedgerStore(params 커뮤니티원장Dto[] ledgers) : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> _items =
            ledgers.ToDictionary(ledger => ledger.원장Id, StringComparer.OrdinalIgnoreCase);

        public Task<커뮤니티원장Dto?> 원장조회Async(string 원장Id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(_items.Values.ToArray());

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var ledgerId = request.원장Id
                ?? throw new InvalidOperationException("테스트 원장 ID가 필요합니다.");
            var existingRevision = _items.TryGetValue(ledgerId, out var existing)
                ? existing.Revision
                : 0;
            var ledger = new 커뮤니티원장Dto
            {
                원장Id = ledgerId,
                Revision = existingRevision + 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                원함 = request.원함,
                상태 = request.상태 ?? 커뮤니티원장상태.초안,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? "테스트 사용자",
                블록목록 = request.블록목록 ?? [],
                참여자목록 = request.참여자목록 ?? [],
                포함원장목록 = request.포함원장목록 ?? [],
                다이어그램스냅샷 = request.다이어그램스냅샷,
                외부참조 = request.외부참조 ?? new Dictionary<string, string>(),
                확장속성 = request.확장속성 ?? new Dictionary<string, string>(),
                생성시각Utc = DateTime.UtcNow,
                수정시각Utc = DateTime.UtcNow
            };
            _items[ledger.원장Id] = ledger;
            return Task.FromResult(ledger);
        }

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
