namespace DriverApp.Avalonia.ViewModels;

public sealed class ShellHeaderViewModel : ViewModelBase
{
    private string _title = "DriverApp";
    private string _subtitle = "메모리 기반 기사 업무 앱";
    private string _userLabel = "미로그인";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => SetProperty(ref _subtitle, value);
    }

    public string UserLabel
    {
        get => _userLabel;
        set => SetProperty(ref _userLabel, value);
    }
}