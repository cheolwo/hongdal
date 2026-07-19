namespace Ssalddel.Client.Infrastructure.Security;

public sealed record ClientAuthTokenSnapshot(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string UserId,
    string UserName,
    IReadOnlyList<string> Roles);
