#if !ANDROID
namespace DriverApp.Services;

public sealed class Noop기사위치송신Service : I기사위치송신Service
{
    public bool IsRunning { get; private set; }
    public event Action? Changed;

    public Task StartAsync(기사위치송신시작요청 request, CancellationToken cancellationToken = default)
    {
        IsRunning = true;
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        IsRunning = false;
        Changed?.Invoke();
        return Task.CompletedTask;
    }
}
#endif
