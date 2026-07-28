using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Services.Outbox;
using 살뜰.Infrastructure.Storage.Local;
using 살뜰.Services.Documents;

namespace Ssalddel.Tests.Services.Documents;

public sealed class 문서생성OutboxServiceTests
{
    [Fact]
    public async Task 생성실패를_영속대기로남기고_재시작후_재처리한다()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var environment = new TestWebHostEnvironment(root);
            var firstStore = new 문서관리Store(
                environment,
                NullLogger<문서관리Store>.Instance);
            var firstDocumentService = new FailOnceDocumentService();
            var firstOutbox = new 문서생성OutboxService(
                firstStore,
                firstDocumentService,
                environment,
                NullLogger<문서생성OutboxService>.Instance);
            var request = new 문서생성요청
            {
                의뢰Id = "ORDER-RETRY-1",
                문서코드 = "주문확인서",
                문서명 = "주문 확인서",
                파일명 = "order-retry.txt"
            };

            await firstOutbox.예약후즉시처리Async(
                request,
                System.Text.Encoding.UTF8.GetBytes("retry-payload"),
                "order-confirmation:ORDER-RETRY-1");

            var pending = Assert.Single(firstStore.ListDocumentGenerationOutbox());
            Assert.Equal(문서생성Outbox상태값.대기, pending.처리상태);
            Assert.Equal(1, pending.시도횟수);
            Assert.NotEmpty(pending.마지막오류);
            pending.수정일시Utc = DateTime.UtcNow - OutboxProcessingPolicy.RetryDelay - TimeSpan.FromSeconds(1);
            firstStore.UpdateDocumentGenerationOutbox(pending);

            var restartedStore = new 문서관리Store(
                environment,
                NullLogger<문서관리Store>.Instance);
            var recoveredDocumentService = new RecordingDocumentService();
            var restartedOutbox = new 문서생성OutboxService(
                restartedStore,
                recoveredDocumentService,
                environment,
                NullLogger<문서생성OutboxService>.Instance);

            var processed = await restartedOutbox.대기문서생성Async();

            Assert.Equal(1, processed);
            var completed = Assert.Single(restartedStore.ListDocumentGenerationOutbox());
            Assert.Equal(문서생성Outbox상태값.완료, completed.처리상태);
            Assert.Equal(2, completed.시도횟수);
            Assert.Empty(completed.Payload상대경로);
            Assert.Equal("retry-payload", Assert.Single(recoveredDocumentService.Contents));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    private static string CreateTemporaryRoot()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "ssalddel-document-outbox-tests",
            Guid.NewGuid().ToString("N"));
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

    private sealed class FailOnceDocumentService : DocumentServiceStub
    {
        private int _attemptCount;

        public override Task<문서조회요약응답?> CreateDocumentAsync(
            문서생성요청 request,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            _attemptCount++;
            return _attemptCount == 1
                ? Task.FromException<문서조회요약응답?>(
                    new IOException("temporary document storage failure"))
                : Task.FromResult<문서조회요약응답?>(new 문서조회요약응답());
        }
    }

    private sealed class RecordingDocumentService : DocumentServiceStub
    {
        public List<string> Contents { get; } = [];

        public override async Task<문서조회요약응답?> CreateDocumentAsync(
            문서생성요청 request,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(content);
            Contents.Add(await reader.ReadToEndAsync(cancellationToken));
            return new 문서조회요약응답
            {
                Id = 1,
                의뢰Id = request.의뢰Id,
                문서코드 = request.문서코드,
                파일명 = request.파일명,
                생성상태 = 문서상태값.생성완료
            };
        }
    }

    private abstract class DocumentServiceStub : I문서관리Service
    {
        public abstract Task<문서조회요약응답?> CreateDocumentAsync(
            문서생성요청 request,
            Stream content,
            CancellationToken cancellationToken = default);

        public Task<IReadOnlyList<문서정책요약응답>> GetPoliciesAsync(
            CancellationToken cancellationToken = default)
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
}
