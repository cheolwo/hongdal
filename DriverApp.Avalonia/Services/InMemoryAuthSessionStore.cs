using DriverApp.Avalonia.Models.Auth;

namespace DriverApp.Avalonia.Services;

public sealed class InMemoryAuthSessionStore : IAuthSessionStore
{
    public DriverSession? CurrentSession { get; private set; }

    public bool IsLoggedIn => CurrentSession?.IsLoggedIn == true;

    public void SignIn(DriverSession session)
    {
        CurrentSession = session;
    }

    public void SignOut()
    {
        CurrentSession = null;
    }
}
