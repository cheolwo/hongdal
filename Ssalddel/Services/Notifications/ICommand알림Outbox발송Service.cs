namespace 살뜰.Services.Notifications;

public interface ICommand알림Outbox발송Service
{
    Task<int> 대기알림발송Async(int take = 100, CancellationToken cancellationToken = default);
}
