using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class OrderLedgerRoleQueryServiceTests
{
    [Fact]
    public async Task Orderer_view_masks_other_owners_child_ledgers()
    {
        var ledgers = CreateLedgers();
        var service = new 주문원장역할별조회Service(ledgers, new FakeDisclosureStore());

        var result = await service.조회Async("order-1", "orderer-1", 주문원장조회역할.주문자);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.주문원장상세);
        Assert.Equal(2, result.Value.관련원장목록.Count);
        Assert.All(result.Value.관련원장목록, item =>
        {
            Assert.Null(item.원장상세);
            Assert.Equal(원장조회근거.공개요청필요, item.조회근거);
            Assert.True(item.공개요청가능여부);
        });
    }

    [Fact]
    public async Task Seller_view_returns_only_the_sales_ledger_owned_by_the_seller()
    {
        var service = new 주문원장역할별조회Service(CreateLedgers(), new FakeDisclosureStore());

        var result = await service.조회Async("order-1", "seller-1", 주문원장조회역할.판매자);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.관련원장목록);
        Assert.Equal(주문원장포함역할.판매, item.주문안역할);
        Assert.Equal(원장조회근거.소유자, item.조회근거);
        Assert.NotNull(item.원장상세);
        Assert.Null(result.Value.주문원장상세);
    }

    [Fact]
    public async Task Unrelated_user_cannot_enter_a_role_view()
    {
        var service = new 주문원장역할별조회Service(CreateLedgers(), new FakeDisclosureStore());

        var result = await service.조회Async("order-1", "stranger-1", 주문원장조회역할.운송담당자);

        Assert.True(result.IsFailed);
        Assert.Equal(403, result.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task Approved_disclosure_reveals_the_requested_child_ledger_to_the_orderer()
    {
        var ledgers = CreateLedgers();
        var disclosureStore = new FakeDisclosureStore();
        var disclosureService = new 주문원장공개요청Service(ledgers, disclosureStore);
        var queryService = new 주문원장역할별조회Service(ledgers, disclosureStore);

        var requested = await disclosureService.요청Async(
            "order-1",
            new 원장공개요청입력 { 대상원장Id = "sale-1", 사유 = "주문 이행 상태 확인" },
            "orderer-1");
        var decided = await disclosureService.결정Async(
            "order-1",
            requested.Value.요청Id,
            new 원장공개결정입력 { 승인여부 = true, 공개일수 = 7 },
            "seller-1");
        var viewed = await queryService.조회Async("order-1", "orderer-1", 주문원장조회역할.주문자);

        Assert.True(requested.IsSuccess);
        Assert.True(decided.IsSuccess);
        var sale = viewed.Value.관련원장목록.Single(x => x.원장Id == "sale-1");
        Assert.Equal(원장조회근거.승인공개, sale.조회근거);
        Assert.NotNull(sale.원장상세);
        Assert.False(sale.공개요청가능여부);
    }

    [Fact]
    public async Task User_who_does_not_own_the_target_ledger_cannot_approve_disclosure()
    {
        var ledgers = CreateLedgers();
        var service = new 주문원장공개요청Service(ledgers, new FakeDisclosureStore());
        var requested = await service.요청Async(
            "order-1",
            new 원장공개요청입력 { 대상원장Id = "sale-1", 사유 = "주문 이행 상태 확인" },
            "orderer-1");

        var result = await service.결정Async(
            "order-1",
            requested.Value.요청Id,
            new 원장공개결정입력 { 승인여부 = true },
            "driver-1");

        Assert.True(result.IsFailed);
        Assert.Equal(403, result.Errors.Single().Metadata["StatusCode"]);
    }

    private static FakeLedgerStore CreateLedgers()
    {
        var root = Ledger("order-1", CommunityLedgerTemplateKeys.Order, "orderer-1", "주문자");
        root.포함원장목록 =
        [
            Reference("sale-1", CommunityLedgerTemplateKeys.LocalSale, 주문원장포함역할.판매, 0),
            Reference("transport-1", CommunityLedgerTemplateKeys.CargoTransport, 주문원장포함역할.운송, 1)
        ];
        return new FakeLedgerStore(
            root,
            Ledger("sale-1", CommunityLedgerTemplateKeys.LocalSale, "seller-1", "판매자"),
            Ledger("transport-1", CommunityLedgerTemplateKeys.CargoTransport, "driver-1", "운송 기사"));
    }

    private static 커뮤니티원장Dto Ledger(string id, string templateKey, string ownerId, string ownerName)
        => new()
        {
            원장Id = id,
            Revision = 1,
            커뮤니티Id = "platform",
            원장템플릿Key = templateKey,
            제목 = $"{id} 상세 정보",
            상태 = 커뮤니티원장상태.진행중,
            생성자UserId = ownerId,
            생성자표시명 = ownerName
        };

    private static 커뮤니티포함원장참조Dto Reference(
        string ledgerId,
        string templateKey,
        string role,
        int order)
        => new()
        {
            원장Id = ledgerId,
            원장템플릿Key = templateKey,
            역할 = role,
            필수여부 = true,
            표시순서 = order
        };

    private sealed class FakeLedgerStore : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> _ledgers;

        public FakeLedgerStore(params 커뮤니티원장Dto[] ledgers)
        {
            _ledgers = ledgers.ToDictionary(x => x.원장Id, StringComparer.OrdinalIgnoreCase);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(string 원장Id, CancellationToken cancellationToken = default)
            => Task.FromResult(_ledgers.GetValueOrDefault(원장Id));

        public Task<커뮤니티원장Dto> 원장저장Async(커뮤니티원장저장요청 request, string updatedBy, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(커뮤니티원장조회조건 query, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(_ledgers.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(커뮤니티원장상태변경요청 request, string updatedBy, CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(null);
    }

    private sealed class FakeDisclosureStore : I주문원장공개요청저장소
    {
        private readonly Dictionary<string, 원장공개요청기록> _records = new(StringComparer.OrdinalIgnoreCase);

        public Task<원장공개요청기록> 요청생성Async(원장공개요청기록 요청, CancellationToken cancellationToken = default)
        {
            _records[요청.요청Id] = 요청;
            return Task.FromResult(요청);
        }

        public Task<원장공개요청기록?> 요청조회Async(string 요청Id, CancellationToken cancellationToken = default)
            => Task.FromResult(_records.GetValueOrDefault(요청Id));

        public Task<IReadOnlyList<원장공개요청기록>> 받은요청목록Async(string 승인자UserId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<원장공개요청기록>>(
                _records.Values.Where(x => x.승인자UserId == 승인자UserId).ToArray());

        public Task<IReadOnlySet<string>> 승인된대상원장Ids조회Async(
            string 주문원장Id,
            string 요청자UserId,
            IEnumerable<string> 대상원장Ids,
            DateTimeOffset 기준시각Utc,
            CancellationToken cancellationToken = default)
        {
            var targetIds = 대상원장Ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
            IReadOnlySet<string> result = _records.Values
                .Where(x => x.주문원장Id == 주문원장Id
                            && x.요청자UserId == 요청자UserId
                            && x.상태 == 원장공개요청상태.승인
                            && x.만료시각Utc > 기준시각Utc
                            && targetIds.Contains(x.대상원장Id))
                .Select(x => x.대상원장Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(result);
        }

        public Task<원장공개요청기록?> 요청결정Async(
            string 요청Id,
            string 승인자UserId,
            bool 승인여부,
            string? 처리메모,
            DateTimeOffset 처리시각Utc,
            DateTimeOffset 승인만료시각Utc,
            CancellationToken cancellationToken = default)
        {
            var existing = _records.GetValueOrDefault(요청Id);
            if (existing is null || existing.승인자UserId != 승인자UserId || existing.상태 != 원장공개요청상태.승인대기)
            {
                return Task.FromResult<원장공개요청기록?>(null);
            }

            var updated = existing with
            {
                상태 = 승인여부 ? 원장공개요청상태.승인 : 원장공개요청상태.거절,
                처리시각Utc = 처리시각Utc,
                만료시각Utc = 승인여부 ? 승인만료시각Utc : existing.만료시각Utc,
                처리메모 = 처리메모
            };
            _records[요청Id] = updated;
            return Task.FromResult<원장공개요청기록?>(updated);
        }
    }
}
