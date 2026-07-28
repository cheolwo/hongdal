namespace SsalddelAdmin.Services;

public sealed partial class 백오피스메모리Service
{
    public Task<IReadOnlyList<파일POD응답>> 파일POD목록조회Async(string? fileType = null, string? requestId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<파일POD응답> query = _filePods;
        if (!string.IsNullOrWhiteSpace(fileType))
        {
            query = query.Where(x => x.FileType == fileType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            query = query.Where(x => x.RequestId == requestId.Trim());
        }

        return Task.FromResult<IReadOnlyList<파일POD응답>>(query.ToArray());
    }

    public Task<파일POD응답?> 파일POD상태변경Async(Guid id, string uploadStatus, CancellationToken cancellationToken = default)
    {
        var item = _filePods.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            return Task.FromResult<파일POD응답?>(null);
        }

        item.UploadStatus = uploadStatus;
        item.UpdatedAtUtc = DateTime.UtcNow;
        return Task.FromResult<파일POD응답?>(item);
    }

    public async Task<파일POD응답?> 파일POD업로드Async(Stream fileStream, string fileName, string contentType, string fileType, string? requestId, CancellationToken cancellationToken = default)
    {
        await using var _ = fileStream;
        using var memory = new MemoryStream();
        await fileStream.CopyToAsync(memory, cancellationToken);
        var item = new 파일POD응답
        {
            Id = Guid.NewGuid(),
            FileType = fileType,
            RequestId = requestId ?? string.Empty,
            BucketName = "local",
            ObjectName = fileName,
            Url = "#",
            OriginalFileName = fileName,
            UploadStatus = "업로드완료",
            UploadedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _filePods.Insert(0, item);
        return item;
    }

    public Task<IReadOnlyList<문서정책요약응답>> 문서정책목록조회Async(CancellationToken cancellationToken = default)
        => Task.FromResult(_documentMemory.GetPolicies());

    public Task<문서정책요약응답?> 문서정책수정Async(string documentCode, 문서정책수정요청 request, CancellationToken cancellationToken = default)
        => Task.FromResult(_documentMemory.UpdatePolicy(documentCode, request));

    public Task<IReadOnlyList<문서조회요약응답>> 문서목록조회Async(string? documentCode = null, string? requestId = null, string? status = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_documentMemory.GetDocuments(documentCode, requestId, status));

    public Task<Ssalddel.Contracts.Common.Documents.문서관계그래프응답> 문서관계그래프조회Async(
        string stableId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_documentMemory.GetRelationshipGraph(stableId));

    public Task<IReadOnlyList<문서조회로그요약응답>> 문서로그목록조회Async(long? documentId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_documentMemory.GetLogs(documentId));

    public Task<문서조회요약응답?> 문서생명주기변경Async(
        long documentId,
        문서생명주기변경요청 request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_documentMemory.TransitionLifecycle(documentId, request));

    public async Task<문서조회요약응답?> 문서업로드Async(
        Stream fileStream,
        string fileName,
        string contentType,
        string documentCode,
        string documentName,
        string requestId,
        long? transportId = null,
        bool? encrypt = null,
        bool? allowDownload = null,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        return await _documentMemory.UploadDocumentAsync(
            fileStream,
            fileName,
            contentType,
            documentCode,
            documentName,
            requestId,
            transportId,
            encrypt,
            allowDownload,
            createdBy,
            cancellationToken);
    }
}
