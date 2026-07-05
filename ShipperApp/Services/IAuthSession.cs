using Hongdal.Contracts.Common;

namespace ShipperApp.Services;

public interface IAuthSession
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsLoggedIn { get; }
    Task RestoreAsync(CancellationToken cancellationToken = default);
    Task ApplyAsync(토큰응답 tokenResponse, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
