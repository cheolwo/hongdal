using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Contracts.Common;

namespace Ssalddel.Tests.Client.Infrastructure;

public sealed class AuthTokenResponseExtensionsTests
{
    [Fact]
    public void ToClientAuthTokenSnapshot_MapsSharedAuthContract()
    {
        var accessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7);
        var response = new 토큰응답
        {
            AccessToken = "access-token",
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = "refresh-token",
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
            UserId = "driver-1",
            UserName = "driver",
            Roles = ["Driver"]
        };

        var snapshot = response.ToClientAuthTokenSnapshot();

        Assert.Equal(response.AccessToken, snapshot.AccessToken);
        Assert.Equal(accessTokenExpiresAtUtc, snapshot.AccessTokenExpiresAtUtc);
        Assert.Equal(response.RefreshToken, snapshot.RefreshToken);
        Assert.Equal(refreshTokenExpiresAtUtc, snapshot.RefreshTokenExpiresAtUtc);
        Assert.Equal(response.UserId, snapshot.UserId);
        Assert.Equal(response.UserName, snapshot.UserName);
        Assert.Equal(response.Roles, snapshot.Roles);
    }
}
