using Hongdal.Contracts.Common;

namespace ShipperApp.Services;

public sealed class AuthApiService
{
    private readonly IAuthSession _authSession;

    public AuthApiService(IAuthSession authSession)
    {
        _authSession = authSession;
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "아이디와 비밀번호를 입력해 주세요.");
        }

        var displayName = userNameOrEmail.Contains('@', StringComparison.Ordinal)
            ? userNameOrEmail.Split('@')[0]
            : userNameOrEmail;

        await _authSession.ApplyAsync(new 토큰응답
        {
            AccessToken = $"offline-token-{Guid.NewGuid():N}",
            RefreshToken = $"offline-refresh-{Guid.NewGuid():N}",
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddHours(8),
            RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            UserId = $"shipper-{displayName}".ToLowerInvariant(),
            UserName = displayName,
            Roles = ["화주"]
        }, cancellationToken);

        return (true, string.Empty);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        return _authSession.ClearAsync(cancellationToken);
    }
}
