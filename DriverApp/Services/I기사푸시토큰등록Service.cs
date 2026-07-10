namespace DriverApp.Services;

public interface I기사푸시토큰등록Service
{
    Task 수신토큰저장및등록Async(string? pushToken, CancellationToken cancellationToken = default);

    Task 저장토큰등록Async(CancellationToken cancellationToken = default);
}
