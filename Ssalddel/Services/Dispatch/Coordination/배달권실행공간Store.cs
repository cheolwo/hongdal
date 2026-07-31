namespace 살뜰.Services.Dispatch.Coordination;

public interface I음식배달권실행공간Store
{
    Task Upsert기사Async(
        string 음식배달공간키,
        string 기사Id,
        IReadOnlyList<string> 인접음식배달공간Keys,
        CancellationToken cancellationToken = default);

    Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default);

    Task Upsert운송의뢰Async(
        string 음식배달공간키,
        string 의뢰Id,
        IReadOnlyList<string> 인접음식배달공간Keys,
        CancellationToken cancellationToken = default);

    Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default);

    Task<음식배달권실행공간Snapshot?> GetAsync(
        string 음식배달공간키,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<음식배달권실행공간Snapshot>> SnapshotAsync(
        CancellationToken cancellationToken = default);
}

public interface I국내화물배달권실행공간Store
{
    Task Upsert기사Async(
        string 화물배달공간키,
        string 기사Id,
        IReadOnlyList<string> 인접화물배달공간Keys,
        CancellationToken cancellationToken = default);

    Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default);

    Task Upsert운송의뢰Async(
        string 화물배달공간키,
        string 의뢰Id,
        IReadOnlyList<string> 인접화물배달공간Keys,
        CancellationToken cancellationToken = default);

    Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default);

    Task<국내화물배달권실행공간Snapshot?> GetAsync(
        string 화물배달공간키,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<국내화물배달권실행공간Snapshot>> SnapshotAsync(
        CancellationToken cancellationToken = default);
}

public sealed record 음식배달권실행공간Snapshot(
    string 배달권키,
    IReadOnlyList<string> 인접배달권Keys,
    IReadOnlyList<string> 운행중기사Ids,
    IReadOnlyList<string> 미처리운송의뢰Ids,
    DateTime UpdatedAtUtc);

public sealed record 국내화물배달권실행공간Snapshot(
    string 배달권키,
    IReadOnlyList<string> 인접배달권Keys,
    IReadOnlyList<string> 운행중기사Ids,
    IReadOnlyList<string> 미처리운송의뢰Ids,
    DateTime UpdatedAtUtc);

public sealed class InMemory음식배달권실행공간Store : I음식배달권실행공간Store
{
    public const string 물리공간식별자 = "dispatch:food:v1";

    private readonly InMemory실행공간Index _index = new();

    public Task Upsert기사Async(
        string 음식배달공간키,
        string 기사Id,
        IReadOnlyList<string> 인접음식배달공간Keys,
        CancellationToken cancellationToken = default)
        => _index.Upsert기사Async(
            Validate음식배달공간키(음식배달공간키),
            기사Id,
            인접음식배달공간Keys.Select(Validate음식배달공간키).ToArray(),
            cancellationToken);

    public Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default)
        => _index.Remove기사Async(기사Id, cancellationToken);

    public Task Upsert운송의뢰Async(
        string 음식배달공간키,
        string 의뢰Id,
        IReadOnlyList<string> 인접음식배달공간Keys,
        CancellationToken cancellationToken = default)
        => _index.Upsert운송의뢰Async(
            Validate음식배달공간키(음식배달공간키),
            의뢰Id,
            인접음식배달공간Keys.Select(Validate음식배달공간키).ToArray(),
            cancellationToken);

    public Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default)
        => _index.Remove운송의뢰Async(의뢰Id, cancellationToken);

    public async Task<음식배달권실행공간Snapshot?> GetAsync(
        string 음식배달공간키,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _index.GetAsync(
            Validate음식배달공간키(음식배달공간키),
            cancellationToken);
        return snapshot is null
            ? null
            : new 음식배달권실행공간Snapshot(
                snapshot.공간키,
                snapshot.인접공간Keys,
                snapshot.운행중기사Ids,
                snapshot.미처리운송의뢰Ids,
                snapshot.UpdatedAtUtc);
    }

    public async Task<IReadOnlyList<음식배달권실행공간Snapshot>> SnapshotAsync(
        CancellationToken cancellationToken = default)
        => (await _index.SnapshotAsync(cancellationToken))
            .Select(snapshot => new 음식배달권실행공간Snapshot(
                snapshot.공간키,
                snapshot.인접공간Keys,
                snapshot.운행중기사Ids,
                snapshot.미처리운송의뢰Ids,
                snapshot.UpdatedAtUtc))
            .ToArray();

    private static string Validate음식배달공간키(string value)
    {
        var key = NormalizeRequired(value, "음식배달 공간 키가 필요합니다.");
        if (!key.StartsWith("food-cell:", StringComparison.Ordinal)
            && !key.StartsWith("food-scope:", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"음식배달 실행 공간에는 음식배달 전용 키만 사용할 수 있습니다: {key}",
                nameof(value));
        }

        return key;
    }

    private static string NormalizeRequired(string value, string message)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(message, nameof(value))
            : value.Trim();
}

public sealed class InMemory국내화물배달권실행공간Store : I국내화물배달권실행공간Store
{
    public const string 물리공간식별자 = "dispatch:cargo:v1";

    private readonly InMemory실행공간Index _index = new();

    public Task Upsert기사Async(
        string 화물배달공간키,
        string 기사Id,
        IReadOnlyList<string> 인접화물배달공간Keys,
        CancellationToken cancellationToken = default)
        => _index.Upsert기사Async(
            Validate화물배달공간키(화물배달공간키),
            기사Id,
            인접화물배달공간Keys.Select(Validate화물배달공간키).ToArray(),
            cancellationToken);

    public Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default)
        => _index.Remove기사Async(기사Id, cancellationToken);

    public Task Upsert운송의뢰Async(
        string 화물배달공간키,
        string 의뢰Id,
        IReadOnlyList<string> 인접화물배달공간Keys,
        CancellationToken cancellationToken = default)
        => _index.Upsert운송의뢰Async(
            Validate화물배달공간키(화물배달공간키),
            의뢰Id,
            인접화물배달공간Keys.Select(Validate화물배달공간키).ToArray(),
            cancellationToken);

    public Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default)
        => _index.Remove운송의뢰Async(의뢰Id, cancellationToken);

    public async Task<국내화물배달권실행공간Snapshot?> GetAsync(
        string 화물배달공간키,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _index.GetAsync(
            Validate화물배달공간키(화물배달공간키),
            cancellationToken);
        return snapshot is null
            ? null
            : new 국내화물배달권실행공간Snapshot(
                snapshot.공간키,
                snapshot.인접공간Keys,
                snapshot.운행중기사Ids,
                snapshot.미처리운송의뢰Ids,
                snapshot.UpdatedAtUtc);
    }

    public async Task<IReadOnlyList<국내화물배달권실행공간Snapshot>> SnapshotAsync(
        CancellationToken cancellationToken = default)
        => (await _index.SnapshotAsync(cancellationToken))
            .Select(snapshot => new 국내화물배달권실행공간Snapshot(
                snapshot.공간키,
                snapshot.인접공간Keys,
                snapshot.운행중기사Ids,
                snapshot.미처리운송의뢰Ids,
                snapshot.UpdatedAtUtc))
            .ToArray();

    private static string Validate화물배달공간키(string value)
    {
        var key = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("국내화물 공간 키가 필요합니다.", nameof(value))
            : value.Trim();
        if (key.StartsWith("food-cell:", StringComparison.Ordinal)
            || key.StartsWith("food-scope:", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"국내화물 실행 공간에는 음식배달 키를 사용할 수 없습니다: {key}",
                nameof(value));
        }

        return key;
    }
}
