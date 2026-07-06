using 홍달.도메인.공통;
using 홍달.도메인.배차;
using 홍달.Services.Dispatch.Queue;

namespace 홍달.Services.Dispatch.Engine;

public sealed class 음식배달배차엔진 : 정책기반배차엔진
{
    private readonly I음식배달배차흐름Resolver _flowResolver;
    private readonly ILogger<음식배달배차엔진> _logger;

    public 음식배달배차엔진(
        IEnumerable<I배차업무정책> policies,
        I음식배달배차흐름Resolver flowResolver,
        ILogger<음식배달배차엔진> logger)
        : base(policies)
    {
        _flowResolver = flowResolver;
        _logger = logger;
    }

    public override string 엔진코드 => "FoodDeliveryDispatchEngine";

    public override string 표시명 => "음식 배달 배차 엔진";

    public override int 배차업무유형 => 상태값.배차업무유형.음식배달;

    public override Task<배차추천후보?> 다음후보선정Async(
        배차대기 queue,
        string? 제외기사Id = null,
        CancellationToken cancellationToken = default)
    {
        var flow = _flowResolver.Resolve(queue);
        if (!flow.배차시작가능)
        {
            _logger.LogInformation(
                "음식배달 배차 시작 전 선행 작업이 필요합니다. QueueId={QueueId} RequestId={RequestId} SourceType={SourceType} Flow={Flow} Condition={Condition}",
                queue.Id,
                queue.의뢰Id,
                queue.원본의뢰유형,
                flow.표시명,
                flow.배차시작조건);

            return Task.FromResult<배차추천후보?>(null);
        }

        return base.다음후보선정Async(queue, 제외기사Id, cancellationToken);
    }
}
