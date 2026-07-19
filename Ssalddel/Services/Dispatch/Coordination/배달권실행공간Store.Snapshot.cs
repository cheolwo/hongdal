namespace 살뜰.Services.Dispatch.Coordination;

public sealed partial class InMemory배달권실행공간Store
{
    public Task<배달권실행공간Snapshot?> GetAsync(string 배달권키, CancellationToken cancellationToken = default)
    {
        배달권키 = Normalize배달권키(배달권키);
        lock (_gate)
        {
            return Task.FromResult(_공간Map.TryGetValue(배달권키, out var 공간)
                ? ToSnapshot(공간)
                : null);
        }
    }

    public Task<IReadOnlyList<배달권실행공간Snapshot>> SnapshotAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<배달권실행공간Snapshot>>(
                _공간Map.Values
                    .Select(ToSnapshot)
                    .OrderBy(x => x.배달권키, StringComparer.Ordinal)
                    .ToArray());
        }
    }

    private 배달권실행공간 GetOrCreate(string 배달권키)
        => _공간Map.GetOrAdd(배달권키, key => new 배달권실행공간(key));

    private static 배달권실행공간Snapshot ToSnapshot(배달권실행공간 공간)
        => new(
            공간.배달권키,
            공간.인접배달권Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            공간.운행중기사Ids.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            공간.미처리운송의뢰Ids.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            공간.UpdatedAtUtc);

    private static string Normalize배달권키(string value)
        => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();

    private static string NormalizeId(string value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private sealed class 배달권실행공간(string 배달권키)
    {
        public string 배달권키 { get; } = 배달권키;

        public HashSet<string> 인접배달권Keys { get; } = new(StringComparer.Ordinal);

        public HashSet<string> 운행중기사Ids { get; } = new(StringComparer.Ordinal);

        public HashSet<string> 미처리운송의뢰Ids { get; } = new(StringComparer.Ordinal);

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
