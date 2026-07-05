using DriverApp.Avalonia.Models.Auth;

namespace DriverApp.Avalonia.Services;

public interface IAuthSessionStore
{
    DriverSession? CurrentSession { get; }
    bool IsLoggedIn { get; }
    void SignIn(DriverSession session);
    void SignOut();
}
