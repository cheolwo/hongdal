namespace HongdalAdmin.Services;

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
}
