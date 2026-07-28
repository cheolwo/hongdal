using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Warehouse;
using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Application.Warehouse.Handlers;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Infrastructure.Storage.Local;
using 살뜰.Services.Documents;

namespace Ssalddel.Tests.Services.Documents;

public sealed class 문서관리ServiceTests
{
    [Fact]
    public async Task 생성한_문서에_원천Revision과_내용해시를_고정하고_다운로드에서_검증한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            await service.SeedDefaultsAsync();
            var contentBytes = "immutable document snapshot"u8.ToArray();

            await using var content = new MemoryStream(contentBytes);
            var created = await service.CreateDocumentAsync(
                new 문서생성요청
                {
                    의뢰Id = "ORDER-100",
                    문서코드 = "인수증",
                    문서명 = "주문 인수증",
                    파일명 = "receipt.pdf",
                    생명주기상태코드 = 문서생명주기상태코드.발행완료,
                    원천원장Id = "ORDER-100",
                    원천원장종류코드 = "FoodOrder",
                    원천원장Revision = 8,
                    원천문서종류코드 = "DELIVERY_RECEIPT",
                    생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                    발급주체코드 = 문서발급주체코드.플랫폼
                },
                content);

            Assert.NotNull(created);
            Assert.Equal(문서분류코드.수행증빙, created.문서분류코드);
            Assert.Equal(문서생명주기상태코드.발행완료, created.생명주기상태코드);
            Assert.Equal(8, created.원천원장Revision);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(contentBytes)), created.내용Sha256);
            Assert.False(created.수정가능여부);

            var downloaded = await service.DownloadAsync(created.Id);

            Assert.NotNull(downloaded);
            Assert.Equal(contentBytes, downloaded.내용);
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task 확정문서는_덮어쓰지_않고_대체문서_관계로_전이한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            await service.SeedDefaultsAsync();
            var original = await CreateDraftAsync(service, "original.pdf");
            var replacement = await CreateDraftAsync(service, "replacement.pdf");

            var confirmed = await service.TransitionLifecycleAsync(
                original.Id,
                new 문서생명주기변경요청
                {
                    대상상태코드 = 문서생명주기상태코드.확인완료,
                    변경자 = "admin"
                });
            Assert.Equal(문서생명주기상태코드.확인완료, confirmed?.생명주기상태코드);

            var superseded = await service.TransitionLifecycleAsync(
                original.Id,
                new 문서생명주기변경요청
                {
                    대상상태코드 = 문서생명주기상태코드.대체됨,
                    대체문서Id = replacement.Id,
                    변경사유 = "수량 정정",
                    변경자 = "admin"
                });

            Assert.Equal(문서생명주기상태코드.대체됨, superseded?.생명주기상태코드);
            Assert.Equal(replacement.Id, superseded?.대체문서Id);

            var replacementView = (await service.ListDocumentsAsync()).Single(x => x.Id == replacement.Id);
            Assert.Equal(original.Id, replacementView.이전문서Id);
            var lifecycleLog = (await service.ListLogsAsync(original.Id))
                .Single(log => log.행위.Contains("Superseded", StringComparison.Ordinal));
            using var lifecycleMetadata = System.Text.Json.JsonDocument.Parse(lifecycleLog.MetadataJson);
            Assert.Equal("수량 정정", lifecycleMetadata.RootElement.GetProperty("reason").GetString());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.TransitionLifecycleAsync(
                    original.Id,
                    new 문서생명주기변경요청
                    {
                        대상상태코드 = 문서생명주기상태코드.발행완료
                    }));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task 같은_원천Revision과_내용의_재처리는_기존_스냅샷을_반환한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            await service.SeedDefaultsAsync();
            var request = new 문서생성요청
            {
                의뢰Id = "OUTBOUND-300",
                문서코드 = "인수증",
                파일명 = "receipt.pdf",
                생명주기상태코드 = 문서생명주기상태코드.발행완료,
                원천원장Id = "OUTBOUND-300",
                원천원장종류코드 = "WarehouseOutboundPlan",
                원천원장Revision = 4,
                원천문서종류코드 = "OUTBOUND_HANDOFF_CONFIRMATION",
                템플릿버전 = "1.0"
            };

            await using var firstContent = new MemoryStream("same snapshot"u8.ToArray());
            var first = await service.CreateDocumentAsync(request, firstContent);
            await using var retriedContent = new MemoryStream("same snapshot"u8.ToArray());
            var retried = await service.CreateDocumentAsync(request, retriedContent);

            Assert.Equal(first?.Id, retried?.Id);
            Assert.Single(await service.ListDocumentsAsync());
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task 주문부터_출고와_운송까지_종류가_붙은_StableId로_문서관계를_추적한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            await service.SeedDefaultsAsync();
            var orderId = 문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-700");
            var outboundId = 문서StableId.만들기(문서StableId종류코드.출고예정, 41);
            var transportId = 문서StableId.만들기(문서StableId종류코드.운송의뢰, "TR-900");

            await CreateRelatedDocumentAsync(
                service,
                "출고예정목록",
                "41",
                "WarehouseOutboundPlan",
                "outbound.txt",
                [orderId, outboundId]);
            await CreateRelatedDocumentAsync(
                service,
                "출고인계확인서",
                "TR-900",
                "WarehouseOutboundPlan",
                "handoff.txt",
                [outboundId, transportId]);
            await CreateRelatedDocumentAsync(
                service,
                "인수증",
                "TR-900",
                "TransportExecution",
                "receipt.pdf",
                [
                    transportId,
                    문서StableId.만들기(문서StableId종류코드.운송실행, 91)
                ]);
            await CreateRelatedDocumentAsync(
                service,
                "출고예정목록",
                "other",
                "WarehouseInventory",
                "unrelated.txt",
                [문서StableId.만들기(문서StableId종류코드.입고상품, 41)]);

            var graph = await service.GetRelationshipGraphAsync(orderId);

            Assert.Equal(orderId, graph.기준StableId);
            Assert.Equal(3, graph.문서목록.Count);
            Assert.Contains(outboundId, graph.발견StableId목록);
            Assert.Contains(transportId, graph.발견StableId목록);
            Assert.DoesNotContain(
                graph.문서목록,
                document => document.연결StableId목록.Contains(
                    문서StableId.만들기(문서StableId종류코드.입고상품, 41)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task 주문결제부터_피킹포장과_출고인계까지_자동문서Chain을_복구한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            await service.SeedDefaultsAsync();
            var outbox = new ImmediateDocumentOutboxService(service);
            var orderHandler = new 주문확인문서생성EventHandler(outbox);
            var warehouseHandler = new 창고피킹포장문서생성EventHandler(outbox);
            var outboundHandler = new 창고출고문서생성EventHandler(outbox);
            var receiptHandler = new 주문자수령확인문서생성EventHandler(outbox);
            var now = new DateTime(2026, 7, 28, 3, 0, 0, DateTimeKind.Utc);

            await orderHandler.Handle(
                new 주문결제완료됨Event(
                    71,
                    "ORDER-700",
                    "orderer-1",
                    "seller-1",
                    [new 주문결제완료상품항목(101, 31, "공동구매 감자", "POTATO-01", 12)],
                    now,
                    "trace-order"),
                CancellationToken.None);
            await warehouseHandler.Handle(
                new 창고피킹완료됨Event(
                    "warehouse-1",
                    "창고관리자",
                    "PICK-700",
                    11,
                    12,
                    "/warehouse/picking",
                    "trace-pick",
                    now.AddMinutes(5),
                    "WarehouseManagerApp",
                    31,
                    41,
                    "ORDER-700",
                    "LINE-1",
                    "공동구매 감자",
                    "POTATO-01",
                    "A-01-02",
                    "BUNDLE-700",
                    "ledger-700"),
                CancellationToken.None);
            await warehouseHandler.Handle(
                new 창고포장완료됨Event(
                    "warehouse-1",
                    "창고관리자",
                    31,
                    12,
                    "/warehouse/packing",
                    "trace-pack",
                    now.AddMinutes(10),
                    "WarehouseManagerApp",
                    21,
                    11,
                    41,
                    "ORDER-700",
                    "공동구매 감자",
                    "POTATO-01",
                    "냉장포장",
                    "A-01-02",
                    "ledger-700"),
                CancellationToken.None);
            await outboundHandler.Handle(
                new 창고출고인계준비완료됨Event(
                    "warehouse-1",
                    "창고관리자",
                    31,
                    41,
                    12,
                    "/warehouse/outbound",
                    "trace-outbound",
                    now.AddMinutes(15),
                    "WarehouseManagerApp",
                    "ORDER-700",
                    21,
                    "ledger-700"),
                CancellationToken.None);
            await outboundHandler.Handle(
                new 창고출고운송인계완료됨Event(
                    "warehouse-1",
                    "창고관리자",
                    41,
                    31,
                    "TR-900",
                    "driver-1",
                    "서울80바1234",
                    12,
                    "/warehouse/handoff",
                    "trace-handoff",
                    now.AddMinutes(20),
                    "WarehouseManagerApp",
                    "ORDER-700",
                    21,
                    "ledger-700"),
                CancellationToken.None);
            await receiptHandler.Handle(
                new 주문자상품입고완료됨Event(
                    71,
                    "ORDER-700",
                    "orderer-1",
                    [21],
                    now.AddMinutes(25),
                    "trace-received"),
                CancellationToken.None);

            var graph = await service.GetRelationshipGraphAsync(
                문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-700"));

            Assert.Equal(
                [
                    "주문확인서",
                    "피킹완료표",
                    "포장완료표",
                    "출고예정목록",
                    "출고인계확인서",
                    "수령확인서"
                ],
                graph.문서목록.Select(document => document.문서코드).ToArray());
            Assert.Contains(
                문서StableId.만들기(문서StableId종류코드.입고요청, 21),
                graph.발견StableId목록);
            Assert.Contains(
                문서StableId.만들기(문서StableId종류코드.출고예정, 41),
                graph.발견StableId목록);
            Assert.Contains(
                문서StableId.만들기(문서StableId종류코드.운송의뢰, "TR-900"),
                graph.발견StableId목록);
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task 문서메타데이터와_감사로그를_재시작후_복구한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var environment = new TestWebHostEnvironment(root);
            var firstStore = new 문서관리Store(
                environment,
                NullLogger<문서관리Store>.Instance);
            var firstService = new 문서관리Service(
                firstStore,
                DataProtectionProvider.Create(Path.Combine(root, "keys")),
                environment);
            await firstService.SeedDefaultsAsync();
            await using (var content = new MemoryStream(
                             System.Text.Encoding.UTF8.GetBytes("persistent-document")))
            {
                await firstService.CreateDocumentAsync(
                    new 문서생성요청
                    {
                        의뢰Id = "ORDER-PERSIST-1",
                        문서코드 = "주문확인서",
                        파일명 = "persistent-order.txt",
                        생명주기상태코드 = 문서생명주기상태코드.발행완료
                    },
                    content);
            }

            var restartedStore = new 문서관리Store(
                environment,
                NullLogger<문서관리Store>.Instance);
            var restartedService = new 문서관리Service(
                restartedStore,
                DataProtectionProvider.Create(Path.Combine(root, "keys")),
                environment);

            var document = Assert.Single(await restartedService.ListDocumentsAsync());
            var download = await restartedService.DownloadAsync(document.Id);

            Assert.NotNull(download);
            Assert.Equal("persistent-document", System.Text.Encoding.UTF8.GetString(download!.내용));
            Assert.Contains(
                await restartedService.ListLogsAsync(document.Id),
                log => log.행위 == "생성");
            Assert.Contains(
                await restartedService.ListLogsAsync(document.Id),
                log => log.행위 == 문서상태값.다운로드);
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    private static async Task CreateRelatedDocumentAsync(
        I문서관리Service service,
        string documentCode,
        string sourceId,
        string sourceType,
        string fileName,
        IReadOnlyList<string> stableIds)
    {
        await using var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileName));
        await service.CreateDocumentAsync(
            new 문서생성요청
            {
                의뢰Id = sourceId,
                문서코드 = documentCode,
                파일명 = fileName,
                생명주기상태코드 = 문서생명주기상태코드.발행완료,
                원천원장Id = sourceId,
                원천원장종류코드 = sourceType,
                관련StableId목록Json = JsonSerializer.Serialize(stableIds)
            },
            content);
    }

    private static async Task<문서조회요약응답> CreateDraftAsync(
        I문서관리Service service,
        string fileName)
    {
        await using var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes($"draft:{fileName}"));
        return (await service.CreateDocumentAsync(
            new 문서생성요청
            {
                의뢰Id = "ORDER-200",
                문서코드 = "인수증",
                파일명 = fileName,
                생명주기상태코드 = 문서생명주기상태코드.검토준비
            },
            content))!;
    }

    private static I문서관리Service CreateService(string root)
        => new 문서관리Service(
            new 문서관리Store(),
            DataProtectionProvider.Create(Path.Combine(root, "keys")),
            new TestWebHostEnvironment(root));

    private static string CreateTemporaryRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), "ssalddel-document-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTemporaryRoot(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private sealed class TestWebHostEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Ssalddel.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(root);
    }

    private sealed class ImmediateDocumentOutboxService(I문서관리Service documentService)
        : I문서생성OutboxService
    {
        public async Task 예약후즉시처리Async(
            문서생성요청 request,
            ReadOnlyMemory<byte> content,
            string 중복방지Key,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(content.ToArray(), writable: false);
            await documentService.CreateDocumentAsync(request, stream, cancellationToken);
        }

        public Task<int> 대기문서생성Async(
            int take = 100,
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
