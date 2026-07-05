using DriverApp.Models.Auth;

namespace DriverApp.Services;

public interface IAuthSession
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    event Action? Changed;
    Task RestoreAsync(CancellationToken cancellationToken = default);
    Task ApplyAsync(DriverApp.Models.Auth.TokenResponse tokenResponse, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
