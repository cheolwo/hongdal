using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매개별원함원장ServiceTests
{
    [Fact]
    public async Task 비구속수요를_사용자별_개별원함원장으로_먼저보존한다()
    {
        var store = new FakeLedgerStore();
        var service = new 공동구매개별원함원장Service(store);
        var command = Demand();
        command.예약결제금액 = 150_000;
        command.수령도로명주소 = "서울특별시 마포구 월드컵로 1";
        command.수령상세주소 = "101동 101호";

        var result = await service.저장Async(command, "auto-group-1");

        var ledger = Assert.Single(store.Items.Values);
        Assert.Equal(result.개별원함원장Id, ledger.원장Id);
        Assert.Equal(CommunityLedgerTemplateKeys.IndividualDemand, ledger.원장템플릿Key);
        Assert.Equal("orderer-1", ledger.생성자UserId);
        Assert.Equal(커뮤니티원장상태.진행중, ledger.상태);
        Assert.Equal("auto-group-1", ledger.외부참조["AutomaticGroupId"]);
        Assert.Equal("3", ledger.외부참조["DesiredQuantity"]);
        Assert.Equal("IndividualDemandLedger", ledger.확장속성["SourceOfTruth"]);
        Assert.DoesNotContain("서울특별시 마포구 월드컵로 1", ledger.외부참조.Values);
        Assert.DoesNotContain("101동 101호", ledger.외부참조.Values);
        Assert.DoesNotContain("150000", ledger.외부참조.Values);
    }

    [Fact]
    public async Task 같은사용자와출처의_원함변경은_같은원장의_revision을_올린다()
    {
        var store = new FakeLedgerStore();
        var service = new 공동구매개별원함원장Service(store);
        var command = Demand();
        var first = await service.저장Async(command, "auto-group-1");
        command.요청멱등키 = "save-2";
        command.개별원함기대Revision = first.Revision;
        command.희망수량 = 7;

        var second = await service.저장Async(command, "auto-group-1");

        Assert.Equal(first.개별원함원장Id, second.개별원함원장Id);
        Assert.Equal(2, second.Revision);
        Assert.Equal("7", Assert.Single(store.Items.Values).외부참조["DesiredQuantity"]);
    }

    [Fact]
    public async Task 같은저장요청을_재시도하면_revision을_올리지않는다()
    {
        var store = new FakeLedgerStore();
        var service = new 공동구매개별원함원장Service(store);
        var command = Demand();

        var first = await service.저장Async(command, "auto-group-1");
        var retry = await service.저장Async(command, "auto-group-1");

        Assert.Equal(first, retry);
        Assert.Equal(1, Assert.Single(store.Items.Values).Revision);
    }

    [Fact]
    public async Task 철회뒤_이전revision의_수량수정은_원함을_재활성화하지않는다()
    {
        var store = new FakeLedgerStore();
        var service = new 공동구매개별원함원장Service(store);
        var command = Demand();
        var saved = await service.저장Async(command, "auto-group-1");
        await service.철회Async(new 공동구매자동수요철회Command
        {
            요청멱등키 = "withdraw-1",
            수요출처키 = command.수요출처키,
            주문자키 = command.주문자키
        });
        command.요청멱등키 = "save-2";
        command.개별원함기대Revision = saved.Revision;
        command.희망수량 = 9;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.저장Async(command, "auto-group-1"));

        Assert.Contains("닫힌", exception.Message);
        Assert.Equal(커뮤니티원장상태.닫힘, Assert.Single(store.Items.Values).상태);
        Assert.Equal("3", Assert.Single(store.Items.Values).외부참조["DesiredQuantity"]);
    }

    [Fact]
    public async Task 철회뒤_기대revision없는_과거저장요청도_원함을_재활성화하지않는다()
    {
        var store = new FakeLedgerStore();
        var service = new 공동구매개별원함원장Service(store);
        var original = Demand();
        var first = await service.저장Async(original, "auto-group-1");
        var update = Demand();
        update.요청멱등키 = "save-2";
        update.개별원함기대Revision = first.Revision;
        update.희망수량 = 5;
        await service.저장Async(update, "auto-group-1");
        await service.철회Async(new 공동구매자동수요철회Command
        {
            요청멱등키 = "withdraw-1",
            수요출처키 = original.수요출처키,
            주문자키 = original.주문자키
        });
        var delayed = Demand();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.저장Async(delayed, "auto-group-1"));

        Assert.Contains("새 원함 회차", exception.Message);
        var ledger = Assert.Single(store.Items.Values);
        Assert.Equal(커뮤니티원장상태.닫힘, ledger.상태);
        Assert.Equal("5", ledger.외부참조["DesiredQuantity"]);
    }

    [Fact]
    public async Task 본인철회는_개별원함원장을_닫고_재시도해도_멱등하다()
    {
        var store = new FakeLedgerStore();
        var service = new 공동구매개별원함원장Service(store);
        var saved = await service.저장Async(Demand(), "auto-group-1");
        var command = new 공동구매자동수요철회Command
        {
            요청멱등키 = "withdraw-1",
            수요출처키 = "ingredient:garlic:seoul",
            주문자키 = "orderer-1",
            철회사유 = "필요 수량을 다시 검토함"
        };

        var first = await service.철회Async(command);
        var second = await service.철회Async(command);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(saved.개별원함원장Id, first!.개별원함원장Id);
        Assert.Equal(first.Revision, second!.Revision);
        var ledger = Assert.Single(store.Items.Values);
        Assert.Equal(커뮤니티원장상태.닫힘, ledger.상태);
        Assert.Equal("individual-demand-withdrawn", ledger.현재단계Key);
        Assert.Equal(2, ledger.Revision);
    }

    private static 공동구매자동수요등록Command Demand()
        => new()
        {
            요청멱등키 = "save-1",
            수요출처키 = "ingredient:garlic:seoul",
            상품키 = "garlic",
            상품명 = "마늘",
            온도코드 = "상온",
            거래유형 = 공동구매거래유형코드.B2C,
            주문자키 = "orderer-1",
            주문자표시명 = "주문자 1",
            배송권키 = "delivery:kr:04000:apartment",
            배송권명 = "KR 04000 · 공동주택 수령",
            희망수량 = 3,
            수량단위 = "kg",
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제
        };

    private sealed class FakeLedgerStore : I커뮤니티원장저장소
    {
        public Dictionary<string, 커뮤니티원장Dto> Items { get; } = new(StringComparer.Ordinal);

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var id = request.원장Id ?? throw new InvalidOperationException("원장 ID가 필요합니다.");
            Items.TryGetValue(id, out var existing);
            if (request.기대Revision.HasValue
                && request.기대Revision.Value != (existing?.Revision ?? 0))
            {
                throw new InvalidOperationException("Revision conflict");
            }

            var ledger = new 커뮤니티원장Dto
            {
                원장Id = id,
                Revision = (existing?.Revision ?? 0) + 1,
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
                포함원장목록 = request.포함원장목록 ?? [],
                외부참조 = request.외부참조,
                확장속성 = request.확장속성,
                생성시각Utc = existing?.생성시각Utc ?? DateTime.UtcNow,
                수정시각Utc = DateTime.UtcNow
            };
            Items[id] = ledger;
            return Task.FromResult(ledger);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(Items.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            if (!Items.TryGetValue(request.원장Id, out var existing))
            {
                return Task.FromResult<커뮤니티원장Dto?>(null);
            }
            if (request.기대Revision.HasValue && request.기대Revision.Value != existing.Revision)
            {
                throw new InvalidOperationException("Revision conflict");
            }

            existing.Revision++;
            existing.상태 = request.상태;
            existing.현재단계Key = request.현재단계Key;
            existing.수정시각Utc = DateTime.UtcNow;
            return Task.FromResult<커뮤니티원장Dto?>(existing);
        }
    }
}
