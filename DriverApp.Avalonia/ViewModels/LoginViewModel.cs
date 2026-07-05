using DriverApp.Avalonia.Models.Auth;
using DriverApp.Avalonia.Services;

namespace DriverApp.Avalonia.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store;
    private readonly DriverApp.Avalonia.Services.IAuthSessionStore _sessionStore;
    private string _loginId = string.Empty;
    private string _password = string.Empty;
    private string _message = "기사 아이디를 입력하고 로그인하세요.";

    public LoginViewModel(InMemoryDriverStore store, DriverApp.Avalonia.Services.IAuthSessionStore sessionStore)
    {
        _store = store;
        _sessionStore = sessionStore;
    }

    public string LoginId
    {
        get => _loginId;
        set => SetProperty(ref _loginId, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public DriverSession? LoggedInSession { get; private set; }

    public bool Login()
    {
        if (string.IsNullOrWhiteSpace(LoginId))
        {
            Message = "아이디를 입력하세요.";
            return false;
        }

        LoggedInSession = new DriverSession
        {
            IsLoggedIn = true,
            DriverId = LoginId.Trim(),
            DriverName = LoginId.Trim(),
            Roles = new[] { "Driver" }
        };

        _sessionStore.SignIn(LoggedInSession);
        _store.SeedSession(LoggedInSession.DriverId);
        Message = $"{LoggedInSession.DriverName} 님 로그인됨";
        return true;
    }
}
