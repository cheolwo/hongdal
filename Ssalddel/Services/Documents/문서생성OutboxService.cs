using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using 살뜰.Infrastructure.Storage.Local;
using Ssalddel.Services.Outbox;

namespace 살뜰.Services.Documents;

public interface I문서생성OutboxService
{
    Task 예약후즉시처리Async(
        문서생성요청 request,
        ReadOnlyMemory<byte> content,
        string 중복방지Key,
        CancellationToken cancellationToken = default);

    Task<int> 대기문서생성Async(
        int take = 100,
        CancellationToken cancellationToken = default);
}

public sealed class 문서생성OutboxService : I문서생성OutboxService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly I문서관리Store _store;
    private readonly I문서관리Service _documentService;
    private readonly ILogger<문서생성OutboxService> _logger;
    private readonly string _outboxRoot;

    public 문서생성OutboxService(
        I문서관리Store store,
        I문서관리Service documentService,
        IWebHostEnvironment environment,
        ILogger<문서생성OutboxService> logger)
    {
        _store = store;
        _documentService = documentService;
        _logger = logger;
        _outboxRoot = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "documents",
            "outbox");
        Directory.CreateDirectory(_outboxRoot);
    }

    public async Task 예약후즉시처리Async(
        문서생성요청 request,
        ReadOnlyMemory<byte> content,
        string 중복방지Key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.문서코드))
        {
            throw new InvalidOperationException("문서 생성 Outbox에 문서코드가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.의뢰Id))
        {
            throw new InvalidOperationException("문서 생성 Outbox에 의뢰Id가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(중복방지Key))
        {
            throw new InvalidOperationException("문서 생성 Outbox에 중복방지Key가 필요합니다.");
        }

        var payloadBytes = content.ToArray();
        var payloadHash = Convert.ToHexString(SHA256.HashData(payloadBytes));
        var normalizedDeduplicationKey = $"{중복방지Key.Trim()}:{payloadHash}";
        var relativePayloadPath = $"{Guid.NewGuid():N}.payload";
        var payloadPath = ResolvePayloadPath(relativePayloadPath);
        var temporaryPath = payloadPath + ".tmp";

        try
        {
            await File.WriteAllBytesAsync(temporaryPath, payloadBytes, cancellationToken);
            File.Move(temporaryPath, payloadPath, overwrite: false);

            var now = DateTime.UtcNow;
            var candidate = new 문서생성Outbox항목
            {
                중복방지Key = normalizedDeduplicationKey,
                문서코드 = request.문서코드.Trim(),
                의뢰Id = request.의뢰Id.Trim(),
                요청Json = JsonSerializer.Serialize(request, JsonOptions),
                Payload상대경로 = relativePayloadPath,
                PayloadSha256 = payloadHash,
                처리상태 = 문서생성Outbox상태값.대기,
                생성일시Utc = now,
                수정일시Utc = now
            };
            var stored = _store.AddOrGetDocumentGenerationOutbox(candidate);
            if (!ReferenceEquals(stored, candidate))
            {
                File.Delete(payloadPath);
            }
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            if (File.Exists(payloadPath)
                && !_store.ListDocumentGenerationOutbox()
                    .Any(item => string.Equals(
                        item.Payload상대경로,
                        relativePayloadPath,
                        StringComparison.Ordinal)))
            {
                File.Delete(payloadPath);
            }

            throw;
        }

        await 대기문서생성Async(100, cancellationToken);
    }

    public async Task<int> 대기문서생성Async(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var items = _store.ClaimDocumentGenerationOutbox(
            take,
            now,
            now - OutboxProcessingPolicy.RetryDelay,
            now - OutboxProcessingPolicy.LeaseTimeout);
        var processed = 0;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            processed++;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = JsonSerializer.Deserialize<문서생성요청>(item.요청Json, JsonOptions)
                    ?? throw new InvalidOperationException("문서 생성 요청 payload가 비어 있습니다.");
                var payloadPath = ResolvePayloadPath(item.Payload상대경로);
                var payload = await File.ReadAllBytesAsync(payloadPath, cancellationToken);
                var actualHash = Convert.ToHexString(SHA256.HashData(payload));
                if (!string.Equals(actualHash, item.PayloadSha256, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("문서 생성 Outbox payload 무결성 검증에 실패했습니다.");
                }

                await using var content = new MemoryStream(payload, writable: false);
                await _documentService.CreateDocumentAsync(request, content, cancellationToken);

                item.처리상태 = 문서생성Outbox상태값.완료;
                item.마지막오류 = string.Empty;
                item.수정일시Utc = DateTime.UtcNow;
                _store.UpdateDocumentGenerationOutbox(item);

                try
                {
                    File.Delete(payloadPath);
                    item.Payload상대경로 = string.Empty;
                    item.수정일시Utc = DateTime.UtcNow;
                    _store.UpdateDocumentGenerationOutbox(item);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "완료된 문서 생성 Outbox payload 정리에 실패했습니다. OutboxId={OutboxId}",
                        item.Id);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                for (var pendingIndex = index; pendingIndex < items.Count; pendingIndex++)
                {
                    var pendingItem = items[pendingIndex];
                    pendingItem.처리상태 = 문서생성Outbox상태값.대기;
                    pendingItem.수정일시Utc = DateTime.UtcNow;
                    _store.UpdateDocumentGenerationOutbox(pendingItem);
                }

                throw;
            }
            catch (Exception exception)
            {
                var retry = OutboxProcessingPolicy.CanRetry(item.시도횟수);
                item.처리상태 = retry
                    ? 문서생성Outbox상태값.대기
                    : 문서생성Outbox상태값.실패;
                item.마지막오류 = Truncate(exception.Message, 2000);
                item.수정일시Utc = DateTime.UtcNow;
                _store.UpdateDocumentGenerationOutbox(item);

                _logger.LogWarning(
                    exception,
                    "문서 생성 Outbox 처리 실패. OutboxId={OutboxId} DocumentCode={DocumentCode} Attempt={Attempt} WillRetry={WillRetry}",
                    item.Id,
                    item.문서코드,
                    item.시도횟수,
                    retry);
            }
        }

        return processed;
    }

    private string ResolvePayloadPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || Path.IsPathRooted(relativePath)
            || relativePath.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("문서 생성 Outbox payload 경로가 올바르지 않습니다.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(_outboxRoot, relativePath));
        var rootPath = Path.GetFullPath(_outboxRoot) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("문서 생성 Outbox payload 경로가 저장소 범위를 벗어났습니다.");
        }

        return fullPath;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
