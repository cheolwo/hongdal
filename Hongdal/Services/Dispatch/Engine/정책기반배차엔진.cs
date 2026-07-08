using 홍달.도메인.배차;
using 홍달.Services.Dispatch.Queue;

namespace 홍달.Services.Dispatch.Engine;

public abstract class 정책기반배차엔진 : I운송의뢰배차엔진
{
    private readonly IReadOnlyDictionary<int, I배차업무정책> _policies;

    protected 정책기반배차엔진(IEnumerable<I배차업무정책> policies)
    {
        _policies = policies
            .GroupBy(x => x.배차업무유형)
            .ToDictionary(x => x.Key, x => x.First());
    }

    public abstract string 엔진코드 { get; }

    public abstract string 표시명 { get; }

    public abstract int 배차업무유형 { get; }

    public virtual Task<배차추천후보?> 다음후보선정Async(
        배차대기 queue,
        string? 제외기사Id = null,
        CancellationToken cancellationToken = default)
    {
        if (!_policies.TryGetValue(배차업무유형, out var policy))
        {
            return Task.FromResult<배차추천후보?>(null);
        }

        return policy.다음후보선정Async(queue, 제외기사Id, cancellationToken);
    }
}
