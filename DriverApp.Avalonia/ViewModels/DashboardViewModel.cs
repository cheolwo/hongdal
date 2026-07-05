using Avalonia.Controls;
using DriverApp.Avalonia.Services;

namespace DriverApp.Avalonia.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store;
    private readonly IAuthSessionStore _sessionStore;
    private readonly MainShellViewModel _shell;
    private string _welcome = "로그인 후 업무를 확인하세요.";

    public DashboardViewModel(InMemoryDriverStore store, IAuthSessionStore sessionStore, MainShellViewModel shell)
    {
        _store = store;
        _sessionStore = sessionStore;
        _shell = shell;
    }

    public string Welcome
    {
        get => _welcome;
        private set => SetProperty(ref _welcome, value);
    }

    public void Refresh()
    {
        Welcome = _sessionStore.CurrentSession is null
            ? "로그인이 필요합니다."
            : $"{_sessionStore.CurrentSession.DriverName} 님, 업무를 선택하세요.";
    }
}
