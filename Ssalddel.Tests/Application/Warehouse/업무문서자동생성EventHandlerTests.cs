using Ssalddel.Application.Warehouse;
using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Application.Warehouse.Handlers;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 업무문서자동생성EventHandlerTests
{
    [Fact]
    public async Task 결제완료주문을_고정된_주문확인서로_보관한다()
    {
        var documents = new RecordingDocumentService();
        var handler = new 주문확인문서생성EventHandler(documents);
        var occurredAt = new DateTime(2026, 7, 28, 1, 30, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 주문결제완료됨Event(
                71,
                "ORDER-700",
                "orderer-1",
                "seller-1",
                [
                    new 주문결제완료상품항목(101, 31, "공동구매 감자", "POTATO-01", 12),
                    new 주문결제완료상품항목(102, null, "양파", "ONION-01", 4)
                ],
                occurredAt,
                "trace-order"),
            CancellationToken.None);

        var request = Assert.Single(documents.Requests);
        Assert.Equal("주문확인서", request.문서코드);
        Assert.Equal(문서분류코드.거래명세, request.문서분류코드);
        Assert.Equal(문서생명주기상태코드.발행완료, request.생명주기상태코드);
        Assert.Equal(문서StableId종류코드.주문참조, request.원천원장종류코드);
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-700"),
            ParseStableIds(request.관련StableId목록Json));
        Assert.Contains("공동구매 감자 / SKU POTATO-01 / 수량 12", Assert.Single(documents.Contents));
        Assert.Contains("양파 / SKU ONION-01 / 수량 4", Assert.Single(documents.Contents));
    }

    [Fact]
    public async Task 피킹과포장완료를_주문입고출고StableId에_연결된_작업표로_보관한다()
    {
        var documents = new RecordingDocumentService();
        var handler = new 창고피킹포장문서생성EventHandler(documents);
        var occurredAt = new DateTime(2026, 7, 28, 2, 0, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 창고피킹완료됨Event(
                사용자Id: "warehouse-1",
                역할명: "창고관리자",
                피킹작업Key: "PICK-700",
                창고Id: 11,
                피킹수량: 12,
                Route: "/warehouse/picking",
                TraceId: "trace-pick",
                발생시각Utc: occurredAt,
                AppKey: "WarehouseManagerApp",
                입고상품Id: 31,
                출고예정Id: 41,
                주문참조번호: "ORDER-700",
                라인Key: "LINE-1",
                상품명: "공동구매 감자",
                SKU: "POTATO-01",
                적재대코드: "A-01-02",
                묶음바코드: "BUNDLE-700",
                커뮤니티원장Id: "ledger-700"),
            CancellationToken.None);
        await handler.Handle(
            new 창고포장완료됨Event(
                사용자Id: "warehouse-1",
                역할명: "창고관리자",
                입고상품Id: 31,
                포장수량: 12,
                Route: "/warehouse/packing",
                TraceId: "trace-pack",
                발생시각Utc: occurredAt.AddMinutes(5),
                AppKey: "WarehouseManagerApp",
                입고요청Id: 21,
                창고Id: 11,
                출고예정Id: 41,
                주문참조번호: "ORDER-700",
                상품명: "공동구매 감자",
                SKU: "POTATO-01",
                포장유형: "냉장포장",
                보관위치: "A-01-02",
                커뮤니티원장Id: "ledger-700"),
            CancellationToken.None);

        Assert.Equal(2, documents.Requests.Count);
        var picking = documents.Requests[0];
        Assert.Equal("피킹완료표", picking.문서코드);
        Assert.Equal(문서분류코드.업무작업지, picking.문서분류코드);
        Assert.Equal("WarehousePickingTask", picking.원천원장종류코드);
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.입고상품, 31),
            ParseStableIds(picking.관련StableId목록Json));
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.출고예정, 41),
            ParseStableIds(picking.관련StableId목록Json));

        var packing = documents.Requests[1];
        Assert.Equal("포장완료표", packing.문서코드);
        Assert.Equal(문서생명주기상태코드.확인완료, packing.생명주기상태코드);
        Assert.Equal("WarehouseInventory", packing.원천원장종류코드);
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.입고요청, 21),
            ParseStableIds(packing.관련StableId목록Json));
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.커뮤니티원장, "ledger-700"),
            ParseStableIds(packing.관련StableId목록Json));
        Assert.Contains("포장 유형: 냉장포장", documents.Contents[1]);
    }

    [Fact]
    public async Task 주문자입고확인을_주문과입고요청에_연결된_수령확인서로_보관한다()
    {
        var documents = new RecordingDocumentService();
        var handler = new 주문자수령확인문서생성EventHandler(documents);

        await handler.Handle(
            new 주문자상품입고완료됨Event(
                71,
                "ORDER-700",
                "orderer-1",
                [21, 22],
                new DateTime(2026, 7, 28, 4, 0, 0, DateTimeKind.Utc),
                "trace-received"),
            CancellationToken.None);

        var request = Assert.Single(documents.Requests);
        Assert.Equal("수령확인서", request.문서코드);
        Assert.Equal(문서분류코드.수행증빙, request.문서분류코드);
        Assert.Equal(문서생명주기상태코드.수령확인, request.생명주기상태코드);
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-700"),
            ParseStableIds(request.관련StableId목록Json));
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.입고요청, 21),
            ParseStableIds(request.관련StableId목록Json));
        Assert.Contains("입고 요청 ID: 21, 22", Assert.Single(documents.Contents));
    }

    private static IReadOnlyList<string> ParseStableIds(string? json)
        => System.Text.Json.JsonSerializer.Deserialize<string[]>(json ?? "[]") ?? [];

    private sealed class RecordingDocumentService : I문서관리Service, I문서생성OutboxService
    {
        public List<문서생성요청> Requests { get; } = [];
        public List<string> Contents { get; } = [];

        public async Task<문서조회요약응답?> CreateDocumentAsync(
            문서생성요청 request,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            using var reader = new StreamReader(content);
            Contents.Add(await reader.ReadToEndAsync(cancellationToken));
            return new 문서조회요약응답
            {
                Id = Requests.Count,
                의뢰Id = request.의뢰Id,
                문서코드 = request.문서코드,
                문서명 = request.문서명,
                파일명 = request.파일명,
                생성상태 = 문서상태값.생성완료,
                문서분류코드 = request.문서분류코드 ?? string.Empty,
                생명주기상태코드 = request.생명주기상태코드 ?? string.Empty
            };
        }

        public Task<IReadOnlyList<문서정책요약응답>> GetPoliciesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<문서정책요약응답>>([]);

        public Task<문서정책요약응답?> UpdatePolicyAsync(
            string 문서코드,
            문서정책수정요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<문서정책요약응답?>(null);

        public Task<IReadOnlyList<문서조회요약응답>> ListDocumentsAsync(
            string? 문서코드 = null,
            string? 의뢰Id = null,
            string? 생성상태 = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<문서조회요약응답>>([]);

        public Task<문서관계그래프응답> GetRelationshipGraphAsync(
            string 기준StableId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 문서관계그래프응답 { 기준StableId = 기준StableId });

        public Task<IReadOnlyList<문서조회로그요약응답>> ListLogsAsync(
            long? 문서Id = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<문서조회로그요약응답>>([]);

        public Task<문서조회요약응답?> TransitionLifecycleAsync(
            long id,
            문서생명주기변경요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<문서조회요약응답?>(null);

        public Task<문서다운로드응답?> DownloadAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult<문서다운로드응답?>(null);

        public Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public async Task 예약후즉시처리Async(
            문서생성요청 request,
            ReadOnlyMemory<byte> content,
            string 중복방지Key,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(content.ToArray(), writable: false);
            await CreateDocumentAsync(request, stream, cancellationToken);
        }

        public Task<int> 대기문서생성Async(
            int take = 100,
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
