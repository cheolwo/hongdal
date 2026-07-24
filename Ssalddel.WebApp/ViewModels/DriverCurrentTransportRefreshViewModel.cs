using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.WebApp.ViewModels;

public sealed class DriverCurrentTransportRefreshViewModel(
    Func<CancellationToken, Task> refresh,
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
