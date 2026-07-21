using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.WebApp.ViewModels;

public sealed class DriverCurrentTransportActionsViewModel(
    Func<기사운송요약응답?> currentTransport,
    Func<CancellationToken, Task> refresh,
    Func<long, CancellationToken, Task> arrivePickup,
    Func<long, CancellationToken, Task> arriveDropoff,
    Action<string, DriverCurrentTransportMessageTone> publishStatus,
    CancellationToken lifetimeCancellation) : 조립ViewModelBase
{
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public Task RefreshAsync()
        => RunAsync("현재 운송을 조회했습니다.", refresh);

    public async Task ArrivePickupAsync()
    {
        var transport = currentTransport();
        if (transport is null)
        {
            publishStatus("먼저 현재 운송을 조회해 주세요.", DriverCurrentTransportMessageTone.Warning);
            return;
        }

        await RunAsync(
            "상차지 도착 상태를 서버에 반영했습니다.",
            async cancellationToken =>
            {
                await arrivePickup(transport.Id, cancellationToken);
                await refresh(cancellationToken);
            });
    }

    public async Task ArriveDropoffAsync()
    {
        var transport = currentTransport();
        if (transport is null)
        {
            publishStatus("먼저 현재 운송을 조회해 주세요.", DriverCurrentTransportMessageTone.Warning);
            return;
        }

        await RunAsync(
            "하차지 도착 상태를 서버에 반영했습니다.",
            async cancellationToken =>
            {
                await arriveDropoff(transport.Id, cancellationToken);
                await refresh(cancellationToken);
            });
    }

    private async Task RunAsync(
        string successMessage,
        Func<CancellationToken, Task> action)
    {
        IsBusy = true;
        publishStatus("서버 요청을 처리하는 중입니다.", DriverCurrentTransportMessageTone.Info);

        try
        {
            await action(lifetimeCancellation);
            publishStatus(successMessage, DriverCurrentTransportMessageTone.Success);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            publishStatus(ex.Message, DriverCurrentTransportMessageTone.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
