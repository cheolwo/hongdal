namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class PlatformHomeModeStateService
{
    public event Action? Changed;

    public bool IsWorkMode { get; private set; }

    public void SetWorkMode(bool isWorkMode)
    {
        if (IsWorkMode == isWorkMode)
        {
            return;
        }

        IsWorkMode = isWorkMode;
        Changed?.Invoke();
    }

    public void ToggleMode()
    {
        SetWorkMode(!IsWorkMode);
    }
}
