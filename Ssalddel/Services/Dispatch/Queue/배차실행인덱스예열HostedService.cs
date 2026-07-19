namespace 살뜰.Services.Dispatch.Queue;

public sealed class 배차실행인덱스예열HostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<배차실행인덱스예열HostedService> _logger;

    public 배차실행인덱스예열HostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<배차실행인덱스예열HostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var 예열Service = scope.ServiceProvider.GetRequiredService<I배차실행인덱스예열Service>();
            var 예열결과 = await 예열Service.예열Async(cancellationToken);

            _logger.LogInformation(
                "Action={Action} ActiveDrivers={ActiveDrivers} DriverStateIndex={DriverStateIndex} LocationIndex={LocationIndex} WorkQueueIndex={WorkQueueIndex} PendingCargoRequests={PendingCargoRequests} OccurredAt={OccurredAt}",
                "DispatchExecutionIndexWarmed",
                예열결과.운행중기사수,
                예열결과.기사상태인덱스예열수,
                예열결과.위치인덱스예열수,
                예열결과.근무큐예열수,
                예열결과.미처리운송의뢰수,
                예열결과.기준시각Utc);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차 실행 인덱스 예열에 실패했습니다. DB 원장을 기준으로 이후 배차 스캔과 위치 heartbeat에서 다시 보정합니다.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
