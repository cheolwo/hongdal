using System.Collections.Concurrent;

namespace 홍달.Services.Dispatch.Coordination;

public interface I배달권실행공간Store
{
    Task Upsert기사Async(
        string 배달권키,
        string 기사Id,
        IReadOnlyList<string> 인접배달권Keys,
        CancellationToken cancellationToken = default);

    Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default);

    Task Upsert운송의뢰Async(
        string 배달권키,
        string 의뢰Id,
        IReadOnlyList<string> 인접배달권Keys,
        CancellationToken cancellationToken = default);

    Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default);

    Task<배달권실행공간Snapshot?> GetAsync(string 배달권키, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<배달권실행공간Snapshot>> SnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed record 배달권실행공간Snapshot(
    string 배달권키,
    IReadOnlyList<string> 인접배달권Keys,
    IReadOnlyList<string> 운행중기사Ids,
    IReadOnlyList<string> 미처리운송의뢰Ids,
    DateTime UpdatedAtUtc);

public sealed partial class InMemory배달권실행공간Store : I배달권실행공간Store
{
    private readonly ConcurrentDictionary<string, 배달권실행공간> _공간Map = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _기사배달권Map = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _운송의뢰배달권Map = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public Task Upsert기사Async(
        string 배달권키,
        string 기사Id,
        IReadOnlyList<string> 인접배달권Keys,
        CancellationToken cancellationToken = default)
    {
        배달권키 = Normalize배달권키(배달권키);
        기사Id = NormalizeId(기사Id);
        if (string.IsNullOrWhiteSpace(기사Id))
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (_기사배달권Map.TryGetValue(기사Id, out var 이전배달권키)
                && !string.Equals(이전배달권키, 배달권키, StringComparison.Ordinal)
                && _공간Map.TryGetValue(이전배달권키, out var 이전공간))
            {
                이전공간.운행중기사Ids.Remove(기사Id);
                이전공간.UpdatedAtUtc = DateTime.UtcNow;
            }

            var 공간 = GetOrCreate(배달권키);
            공간.운행중기사Ids.Add(기사Id);
            공간.인접배달권Keys.UnionWith(인접배달권Keys.Where(x => !string.IsNullOrWhiteSpace(x)));
            공간.UpdatedAtUtc = DateTime.UtcNow;
            _기사배달권Map[기사Id] = 배달권키;
        }

        return Task.CompletedTask;
    }

    public Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default)
    {
        기사Id = NormalizeId(기사Id);
        if (string.IsNullOrWhiteSpace(기사Id))
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (_기사배달권Map.TryRemove(기사Id, out var 배달권키)
                && _공간Map.TryGetValue(배달권키, out var 공간))
            {
                공간.운행중기사Ids.Remove(기사Id);
                공간.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    public Task Upsert운송의뢰Async(
        string 배달권키,
        string 의뢰Id,
        IReadOnlyList<string> 인접배달권Keys,
        CancellationToken cancellationToken = default)
    {
        배달권키 = Normalize배달권키(배달권키);
        의뢰Id = NormalizeId(의뢰Id);
        if (string.IsNullOrWhiteSpace(의뢰Id))
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (_운송의뢰배달권Map.TryGetValue(의뢰Id, out var 이전배달권키)
                && !string.Equals(이전배달권키, 배달권키, StringComparison.Ordinal)
                && _공간Map.TryGetValue(이전배달권키, out var 이전공간))
            {
                이전공간.미처리운송의뢰Ids.Remove(의뢰Id);
                이전공간.UpdatedAtUtc = DateTime.UtcNow;
            }

            var 공간 = GetOrCreate(배달권키);
            공간.미처리운송의뢰Ids.Add(의뢰Id);
            공간.인접배달권Keys.UnionWith(인접배달권Keys.Where(x => !string.IsNullOrWhiteSpace(x)));
            공간.UpdatedAtUtc = DateTime.UtcNow;
            _운송의뢰배달권Map[의뢰Id] = 배달권키;
        }

        return Task.CompletedTask;
    }

    public Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default)
    {
        의뢰Id = NormalizeId(의뢰Id);
        if (string.IsNullOrWhiteSpace(의뢰Id))
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (_운송의뢰배달권Map.TryRemove(의뢰Id, out var 배달권키)
                && _공간Map.TryGetValue(배달권키, out var 공간))
            {
                공간.미처리운송의뢰Ids.Remove(의뢰Id);
                공간.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

}
