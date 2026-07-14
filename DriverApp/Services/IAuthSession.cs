using Hongdal.Client.Infrastructure.Security;
using Hongdal.Ui.Common.Areas.App.Services;

namespace DriverApp.Services;

public interface IAuthSession : IHongdalAccessTokenProvider
{
    string? RefreshToken { get; }
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    event Action? Changed;
    Task RestoreAsync(CancellationToken cancellationToken = default);
    Task ApplyAsync(ClientAuthTokenSnapshot snapshot, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
