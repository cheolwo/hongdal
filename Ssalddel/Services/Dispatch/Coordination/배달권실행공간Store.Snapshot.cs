using System.Collections.Concurrent;

namespace 살뜰.Services.Dispatch.Coordination;

internal sealed class InMemory실행공간Index
{
    private readonly ConcurrentDictionary<string, 실행공간> _공간Map = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _기사공간Map = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _운송의뢰공간Map = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public Task Upsert기사Async(
        string 공간키,
        string 기사Id,
        IReadOnlyList<string> 인접공간Keys,
        CancellationToken cancellationToken = default)
    {
        기사Id = NormalizeId(기사Id);
        if (string.IsNullOrWhiteSpace(기사Id))
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (_기사공간Map.TryGetValue(기사Id, out var 이전공간키)
                && !string.Equals(이전공간키, 공간키, StringComparison.Ordinal)
                && _공간Map.TryGetValue(이전공간키, out var 이전공간))
            {
                이전공간.운행중기사Ids.Remove(기사Id);
                이전공간.UpdatedAtUtc = DateTime.UtcNow;
            }

            var 공간 = GetOrCreate(공간키);
            공간.운행중기사Ids.Add(기사Id);
            공간.인접공간Keys.UnionWith(인접공간Keys.Where(x => !string.IsNullOrWhiteSpace(x)));
            공간.UpdatedAtUtc = DateTime.UtcNow;
            _기사공간Map[기사Id] = 공간키;
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
            if (_기사공간Map.TryRemove(기사Id, out var 공간키)
                && _공간Map.TryGetValue(공간키, out var 공간))
            {
                공간.운행중기사Ids.Remove(기사Id);
                공간.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    public Task Upsert운송의뢰Async(
        string 공간키,
        string 의뢰Id,
        IReadOnlyList<string> 인접공간Keys,
        CancellationToken cancellationToken = default)
    {
        의뢰Id = NormalizeId(의뢰Id);
        if (string.IsNullOrWhiteSpace(의뢰Id))
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (_운송의뢰공간Map.TryGetValue(의뢰Id, out var 이전공간키)
                && !string.Equals(이전공간키, 공간키, StringComparison.Ordinal)
                && _공간Map.TryGetValue(이전공간키, out var 이전공간))
            {
                이전공간.미처리운송의뢰Ids.Remove(의뢰Id);
                이전공간.UpdatedAtUtc = DateTime.UtcNow;
            }

            var 공간 = GetOrCreate(공간키);
            공간.미처리운송의뢰Ids.Add(의뢰Id);
            공간.인접공간Keys.UnionWith(인접공간Keys.Where(x => !string.IsNullOrWhiteSpace(x)));
            공간.UpdatedAtUtc = DateTime.UtcNow;
            _운송의뢰공간Map[의뢰Id] = 공간키;
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
            if (_운송의뢰공간Map.TryRemove(의뢰Id, out var 공간키)
                && _공간Map.TryGetValue(공간키, out var 공간))
            {
                공간.미처리운송의뢰Ids.Remove(의뢰Id);
                공간.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    public Task<실행공간IndexSnapshot?> GetAsync(
        string 공간키,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_공간Map.TryGetValue(공간키, out var 공간)
                ? ToSnapshot(공간)
                : null);
        }
    }

    public Task<IReadOnlyList<실행공간IndexSnapshot>> SnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<실행공간IndexSnapshot>>(
                _공간Map.Values
                    .Select(ToSnapshot)
                    .OrderBy(x => x.공간키, StringComparer.Ordinal)
                    .ToArray());
        }
    }

    private 실행공간 GetOrCreate(string 공간키)
        => _공간Map.GetOrAdd(공간키, key => new 실행공간(key));

    private static 실행공간IndexSnapshot ToSnapshot(실행공간 공간)
        => new(
            공간.공간키,
            공간.인접공간Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            공간.운행중기사Ids.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            공간.미처리운송의뢰Ids.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            공간.UpdatedAtUtc);

    private static string NormalizeId(string value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    internal sealed record 실행공간IndexSnapshot(
        string 공간키,
        IReadOnlyList<string> 인접공간Keys,
        IReadOnlyList<string> 운행중기사Ids,
        IReadOnlyList<string> 미처리운송의뢰Ids,
        DateTime UpdatedAtUtc);

    private sealed class 실행공간(string 공간키)
    {
        public string 공간키 { get; } = 공간키;

        public HashSet<string> 인접공간Keys { get; } = new(StringComparer.Ordinal);

        public HashSet<string> 운행중기사Ids { get; } = new(StringComparer.Ordinal);

        public HashSet<string> 미처리운송의뢰Ids { get; } = new(StringComparer.Ordinal);

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
