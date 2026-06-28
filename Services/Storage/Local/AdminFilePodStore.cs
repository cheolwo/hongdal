using System.Collections.Concurrent;

namespace 홍달.Services.Storage.Local;

public interface IAdminFilePodStore
{
    AdminFilePodMetadata Add(AdminFilePodMetadata item);
    IReadOnlyList<AdminFilePodMetadata> List(string? fileType = null, string? requestId = null);
    AdminFilePodMetadata? UpdateStatus(Guid id, string status);
}

public sealed class AdminFilePodStore : IAdminFilePodStore
{
    private readonly ConcurrentDictionary<Guid, AdminFilePodMetadata> _items = new();

    public AdminFilePodMetadata Add(AdminFilePodMetadata item)
    {
        _items[item.Id] = item;
        return item;
    }

    public IReadOnlyList<AdminFilePodMetadata> List(string? fileType = null, string? requestId = null)
    {
        IEnumerable<AdminFilePodMetadata> query = _items.Values;

        if (!string.IsNullOrWhiteSpace(fileType))
        {
            var type = fileType.Trim();
            query = query.Where(x => string.Equals(x.FileType, type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            var rid = requestId.Trim();
            query = query.Where(x => string.Equals(x.RequestId, rid, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(x => x.UploadedAtUtc)
            .ToList();
    }

    public AdminFilePodMetadata? UpdateStatus(Guid id, string status)
    {
        if (!_items.TryGetValue(id, out var current))
        {
            return null;
        }

        var updated = current with { UploadStatus = status, UpdatedAtUtc = DateTime.UtcNow };
        _items[id] = updated;
        return updated;
    }
}

public sealed record AdminFilePodMetadata(
    Guid Id,
    string FileType,
    string RequestId,
    string BucketName,
    string ObjectName,
    string Url,
    string OriginalFileName,
    string UploadStatus,
    DateTime UploadedAtUtc,
    DateTime UpdatedAtUtc);



