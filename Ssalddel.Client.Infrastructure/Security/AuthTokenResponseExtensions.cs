using Ssalddel.Contracts.Common;

namespace Ssalddel.Client.Infrastructure.Security;

public static class AuthTokenResponseExtensions
{
    public static ClientAuthTokenSnapshot ToClientAuthTokenSnapshot(this 토큰응답 response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new ClientAuthTokenSnapshot(
            response.AccessToken,
            response.AccessTokenExpiresAtUtc,
            response.RefreshToken,
            response.RefreshTokenExpiresAtUtc,
            response.UserId,
            response.UserName,
            response.Roles ?? [],
            response.PreferredLanguageCode);
    }
}
