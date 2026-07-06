using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통;
using 홍달.도메인.배차;
using 홍달.Services.Dispatch.Queue;

namespace 홍달.Services.Dispatch.Engine;

public sealed class 화물용달배차엔진 : 정책기반배차엔진
{
    private readonly HongdalContext _db;
    private readonly I화물용달배차흐름Resolver _flowResolver;
    private readonly ILogger<화물용달배차엔진> _logger;

    public 화물용달배차엔진(
        IEnumerable<I배차업무정책> policies,
        HongdalContext db,
        I화물용달배차흐름Resolver flowResolver,
        ILogger<화물용달배차엔진> logger)
        : base(policies)
    {
        _db = db;
        _flowResolver = flowResolver;
        _logger = logger;
    }

    public override string 엔진코드 => "CargoYongdalDispatchEngine";

    public override string 표시명 => "화물/용달 배차 엔진";

    public override int 배차업무유형 => 상태값.배차업무유형.용달운송;

    public override async Task<배차추천후보?> 다음후보선정Async(
        배차대기 queue,
        string? 제외기사Id = null,
        CancellationToken cancellationToken = default)
    {
        var request = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == queue.의뢰Id, cancellationToken);
        var flow = _flowResolver.Resolve(queue, request);

        _logger.LogDebug(
            "화물/용달 배차 흐름을 확인했습니다. QueueId={QueueId} RequestId={RequestId} SourceType={SourceType} Flow={Flow} Unit={Unit}",
            queue.Id,
            queue.의뢰Id,
            queue.원본의뢰유형,
            flow.표시명,
            flow.운송단위);

        return await base.다음후보선정Async(queue, 제외기사Id, cancellationToken);
    }
}
