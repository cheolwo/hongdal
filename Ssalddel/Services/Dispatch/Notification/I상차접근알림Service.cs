using 살뜰.Services.Storage.Local;

namespace 살뜰.Services.Dispatch.Notification;

public interface I상차접근알림Service
{
    Task<int> 상차지접근알림검사Async(
        DriverLocationSnapshot location,
        decimal 접근반경Km = 10m,
        CancellationToken cancellationToken = default);
}
