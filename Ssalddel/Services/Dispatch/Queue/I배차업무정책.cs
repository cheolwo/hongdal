using 살뜰.도메인.배차;

namespace 살뜰.Services.Dispatch.Queue
{
    public interface I배차업무정책
    {
        int 배차업무유형 { get; }

        Task<배차추천후보?> 다음후보선정Async(
            운송원장 queue,
            string? 제외기사Id = null,
            CancellationToken cancellationToken = default);
    }
}
