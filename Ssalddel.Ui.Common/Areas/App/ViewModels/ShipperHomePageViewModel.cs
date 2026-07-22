using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class ShipperHomePageViewModel(IShipperHomeDashboardClient client)
{
    public ShipperHomeDashboardSnapshot Snapshot { get; private set; }
        = ShipperHomeDashboardSnapshot.Empty;

    public bool IsLoading { get; private set; }

    public bool HasLoaded { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            Snapshot = await client.LoadAsync(cancellationToken);
            HasLoaded = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"화주 허브 상태를 불러오지 못했습니다. {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
