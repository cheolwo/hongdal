using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Services.Community;
using Hongdal.Services.Orderer;

namespace Hongdal.Tests.Services.Orderer;

public sealed class 공동구매개별주문원장ServiceTests
{
    [Fact]
    public async Task 개별주문은_가상창고_입고예정원장을_포함하고_공동구매에_연결된다()
    {
        var store = new InMemoryLedgerStore();
        store.Seed(new 커뮤니티원장Dto
        {
            원장Id = "group-ledger-1",
            Revision = 1,
            커뮤니티Id = "community-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
            제목 = "사과 공동구매",
            상태 = 커뮤니티원장상태.진행중
        });
        var service = new 공동구매개별주문원장Service(
            store,
            new 주문원장통합UseCase(store));
        var group = new 공동구매자동집단응답
        {
            자동집단Id = "auto-group-1",
            상품키 = "apple",
            상품명 = "사과"
        };
        var demand = new 공동구매자동수요응답
        {
            수요Id = "demand-1",
            커뮤니티원장Id = "group-ledger-1",
            주문자키 = "orderer-1",
            주문자표시명 = "주문자 1",
            도착창고Id = 101,
            도착창고유형 = 창고유형코드.가상창고,
            도착창고명 = "자택 수령지 가상 창고",
            수령지주소참조키 = "warehouse:101:receiving-address",
            희망수량 = 5,
            수량단위 = "kg"
        };

        var result = await service.생성및연결Async(group, demand);

        var groupLedger = await store.원장조회Async("group-ledger-1");
        var orderLedger = await store.원장조회Async(result.개별주문원장Id);
        var inboundLedger = await store.원장조회Async(result.입고예정원장Id);
        Assert.NotNull(groupLedger);
        Assert.NotNull(orderLedger);
        Assert.NotNull(inboundLedger);
        Assert.Contains(groupLedger.포함원장목록, x =>
            x.원장Id == orderLedger.원장Id && x.역할 == 주문원장포함역할.개별주문);
        Assert.Contains(orderLedger.포함원장목록, x =>
            x.원장Id == inboundLedger.원장Id && x.역할 == 주문원장포함역할.창고입고);
        Assert.Equal("101", inboundLedger.외부참조["DestinationWarehouseId"]);
        Assert.Equal("True", inboundLedger.외부참조["VirtualWarehouse"]);
        Assert.Equal(
            공동구매개별주문입고상태코드.입고예정,
            inboundLedger.외부참조["InboundMeaningStatus"]);
        Assert.Equal(
            "warehouse:101:receiving-address",
            inboundLedger.외부참조["ReceivingAddressReference"]);
        Assert.DoesNotContain(inboundLedger.외부참조.Keys, key => key.Contains("도로명주소", StringComparison.Ordinal));
        Assert.DoesNotContain(inboundLedger.외부참조.Keys, key => key.Contains("상세주소", StringComparison.Ordinal));
    }

    private sealed class InMemoryLedgerStore : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> _items = new(StringComparer.OrdinalIgnoreCase);

        public void Seed(커뮤니티원장Dto ledger) => _items[ledger.원장Id] = ledger;

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var id = request.원장Id ?? $"ledger-{Guid.NewGuid():N}";
            _items.TryGetValue(id, out var existing);
            var saved = new 커뮤니티원장Dto
            {
                원장Id = id,
                Revision = (existing?.Revision ?? 0) + 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                원함 = request.원함,
                상태 = request.상태 ?? 커뮤니티원장상태.초안,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? "익명 참여자",
                블록목록 = request.블록목록,
                참여자목록 = request.참여자목록,
                포함원장목록 = request.포함원장목록 ?? existing?.포함원장목록 ?? [],
                외부참조 = request.외부참조,
                확장속성 = request.확장속성,
                생성시각Utc = existing?.생성시각Utc ?? DateTime.UtcNow,
                수정시각Utc = DateTime.UtcNow
            };
            _items[id] = saved;
            return Task.FromResult(saved);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(_items.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            if (!_items.TryGetValue(request.원장Id, out var existing))
            {
                return Task.FromResult<커뮤니티원장Dto?>(null);
            }

            existing.상태 = request.상태;
            existing.현재단계Key = request.현재단계Key;
            existing.Revision++;
            return Task.FromResult<커뮤니티원장Dto?>(existing);
        }
    }
}
