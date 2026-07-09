namespace 홍달.Services.Dispatch.Coordination;

public sealed class 배달권기반배차조율계획Service : I배달권기반배차조율계획Service
{
    private readonly I배달권실행공간Store _배달권실행공간Store;

    public 배달권기반배차조율계획Service(I배달권실행공간Store 배달권실행공간Store)
    {
        _배달권실행공간Store = 배달권실행공간Store;
    }

    public async Task<IReadOnlyList<배달권배차조율실행계획>> 계획Async(
        배달권기반배차조율요청 요청,
        CancellationToken cancellationToken = default)
    {
        var 전체공간목록 = await _배달권실행공간Store.SnapshotAsync(cancellationToken);
        var 공간Map = 전체공간목록.ToDictionary(x => x.배달권키, StringComparer.Ordinal);
        var 계획목록 = new List<배달권배차조율실행계획>();

        foreach (var 주배달권공간 in 전체공간목록.Where(x => x.미처리운송의뢰Ids.Count > 0))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var 의뢰Ids = 주배달권공간.미처리운송의뢰Ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .Take(Math.Max(1, 요청.최대운송의뢰수))
                .ToArray();
            if (의뢰Ids.Length == 0)
            {
                continue;
            }

            var 기사Ids = new HashSet<string>(주배달권공간.운행중기사Ids, StringComparer.Ordinal);
            var 인접배달권Keys = 요청.인접배달권기사포함
                ? 주배달권공간.인접배달권Keys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
                : [];

            foreach (var 인접배달권키 in 인접배달권Keys)
            {
                if (공간Map.TryGetValue(인접배달권키, out var 인접공간))
                {
                    기사Ids.UnionWith(인접공간.운행중기사Ids);
                }
            }

            var 후보기사Ids = 기사Ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .Take(Math.Max(1, 요청.최대기사수))
                .ToArray();
            if (후보기사Ids.Length == 0)
            {
                continue;
            }

            계획목록.Add(new 배달권배차조율실행계획(
                주배달권공간.배달권키,
                의뢰Ids,
                후보기사Ids,
                인접배달권Keys));
        }

        return 계획목록;
    }
}
