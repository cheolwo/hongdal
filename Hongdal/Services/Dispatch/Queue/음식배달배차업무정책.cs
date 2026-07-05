namespace 홍달.Services.Dispatch.Queue
{
    public sealed class 음식배달배차업무정책 : I배차업무정책
    {
        private readonly ILogger<음식배달배차업무정책> _logger;

        public 음식배달배차업무정책(ILogger<음식배달배차업무정책> logger)
        {
            _logger = logger;
        }

        public int 배차업무유형 => 홍달.도메인.공통.상태값.배차업무유형.음식배달;

        public Task<배차추천후보?> 다음후보선정Async(
            홍달.도메인.배차.배차대기 queue,
            string? 제외기사Id = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("음식배달 배차업무정책은 아직 후보 선정 구현 전입니다. QueueId={QueueId} RequestId={RequestId}", queue.Id, queue.의뢰Id);
            return Task.FromResult<배차추천후보?>(null);
        }
    }
}
