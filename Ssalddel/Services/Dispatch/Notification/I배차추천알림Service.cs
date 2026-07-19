namespace 살뜰.Services.Dispatch.Notification
{
    public interface I배차추천알림Service
    {
        Task 추천알림요청생성Async(long 배차대기Id, string 의뢰Id, string 기사Id, int 추천라운드, CancellationToken cancellationToken = default);
        Task<int> 대기알림발송Async(int take = 100, CancellationToken cancellationToken = default);
    }
}
