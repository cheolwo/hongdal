using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Application.Warehouse.Handlers;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 창고출고문서생성EventHandlerTests
{
    [Fact]
    public async Task 출고준비와_기사인계를_같은_출고원장에_연결된_두_문서로_보관한다()
    {
        var documents = new RecordingDocumentService();
        var handler = new 창고출고문서생성EventHandler(documents);
        var now = DateTime.UtcNow;

        await handler.Handle(
            new 창고출고인계준비완료됨Event(
                "warehouse-user",
                "창고관리자",
                31,
                41,
                12,
                "/warehouse/outbound",
                "trace-1",
                now,
                "WarehouseManagerApp",
                "ORDER-700",
                21,
                "ledger-700"),
            CancellationToken.None);
        await handler.Handle(
            new 창고출고운송인계완료됨Event(
                "warehouse-user",
                "창고관리자",
                41,
                31,
                "TR-900",
                "driver-1",
                "서울80바1234",
                12,
                "/warehouse/handoff",
                "trace-2",
                now.AddMinutes(5),
                "WarehouseManagerApp",
                "ORDER-700",
                21,
                "ledger-700"),
            CancellationToken.None);

        Assert.Equal(2, documents.Requests.Count);
        var expectedItems = documents.Requests[0];
        Assert.Equal("출고예정목록", expectedItems.문서코드);
        Assert.Equal(문서분류코드.업무작업지, expectedItems.문서분류코드);
        Assert.Equal(문서생명주기상태코드.확인완료, expectedItems.생명주기상태코드);
        Assert.Equal("41", expectedItems.원천원장Id);
        Assert.Equal("WarehouseOutboundPlan", expectedItems.원천원장종류코드);
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-700"),
            ParseStableIds(expectedItems.관련StableId목록Json));
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.출고예정, 41),
            ParseStableIds(expectedItems.관련StableId목록Json));

        var handoff = documents.Requests[1];
        Assert.Equal("출고인계확인서", handoff.문서코드);
        Assert.Equal(문서분류코드.수행증빙, handoff.문서분류코드);
        Assert.Equal(문서생명주기상태코드.발행완료, handoff.생명주기상태코드);
        Assert.Equal("41", handoff.원천원장Id);
        Assert.Contains("TR-900", documents.Contents[1]);
        Assert.Contains(
            문서StableId.만들기(문서StableId종류코드.운송의뢰, "TR-900"),
            ParseStableIds(handoff.관련StableId목록Json));
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
