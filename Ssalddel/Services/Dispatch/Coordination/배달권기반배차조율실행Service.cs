namespace 살뜰.Services.Dispatch.Coordination;

public sealed class 배달권기반배차조율실행Service : I배달권기반배차조율실행Service
{
    private readonly I배달권기반배차조율계획Service _계획Service;
    private readonly I국내화물배차조율실행Service _조율실행Service;

    public 배달권기반배차조율실행Service(
        I배달권기반배차조율계획Service 계획Service,
        I국내화물배차조율실행Service 조율실행Service)
    {
        _계획Service = 계획Service;
        _조율실행Service = 조율실행Service;
    }

    public async Task<IReadOnlyList<배달권기반배차조율실행결과>> 실행Async(
        배달권기반배차조율요청 요청,
        CancellationToken cancellationToken = default)
    {
        var 계획목록 = await _계획Service.계획Async(요청, cancellationToken);
        var 결과목록 = new List<배달권기반배차조율실행결과>();
        var 기사당최대추천건수 = Math.Max(1, 요청.기사당최대추천건수);
        var 실행상태 = new 배달권조율실행상태(기사당최대추천건수);

        foreach (var 계획 in 계획목록)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var 실행대상의뢰Ids = 실행상태.실행대상의뢰Ids(계획.의뢰Ids);
            var 실행대상기사Ids = 실행상태.실행대상기사Ids(계획.기사Ids);
            if (실행대상의뢰Ids.Length == 0 || 실행대상기사Ids.Length == 0)
            {
                continue;
            }

            var 조율요청 = new 국내화물배차조율입력요청
            {
                의뢰Ids = 실행대상의뢰Ids,
                기사Ids = 실행대상기사Ids,
                최대운송의뢰수 = 실행대상의뢰Ids.Length,
                최대기사수 = 실행대상기사Ids.Length,
                기사당최대추천건수 = 기사당최대추천건수
            };
            var 실행결과 = await _조율실행Service.실행Async(조율요청, cancellationToken);
            실행상태.잠금반영(실행결과.ApplyResult.잠금목록);

            결과목록.Add(new 배달권기반배차조율실행결과(
                계획.배달권키,
                실행결과.Input,
                실행결과.Result,
                실행결과.ApplyResult));
        }

        return 결과목록;
    }

    private sealed class 배달권조율실행상태(int 기사당최대추천건수)
    {
        private readonly HashSet<string> _실행중잠금의뢰Ids = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _실행중기사추천잠금수 = new(StringComparer.Ordinal);

        public string[] 실행대상의뢰Ids(IReadOnlyList<string> 의뢰Ids)
            => 의뢰Ids
                .Where(x => !_실행중잠금의뢰Ids.Contains(x))
                .ToArray();

        public string[] 실행대상기사Ids(IReadOnlyList<string> 기사Ids)
            => 기사Ids
                .Where(x => !_실행중기사추천잠금수.TryGetValue(x, out var 잠금수) || 잠금수 < 기사당최대추천건수)
                .ToArray();

        public void 잠금반영(IReadOnlyList<국내화물배차추천잠금> 잠금목록)
        {
            foreach (var 잠금 in 잠금목록)
            {
                _실행중잠금의뢰Ids.Add(잠금.의뢰Id);
                _실행중기사추천잠금수.TryGetValue(잠금.기사Id, out var 현재잠금수);
                _실행중기사추천잠금수[잠금.기사Id] = 현재잠금수 + 1;
            }
        }
    }
}
