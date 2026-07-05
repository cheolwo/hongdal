namespace Hongdal.Client.Infrastructure.Security;

public interface IClientSessionGuard
{
    bool IsAccessTokenUsable(ClientAuthTokenSnapshot? snapshot, DateTime utcNow);
    bool IsRefreshTokenUsable(ClientAuthTokenSnapshot? snapshot, DateTime utcNow);
}

public sealed class ClientSessionGuard : IClientSessionGuard
{
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromMinutes(2);

    public bool IsAccessTokenUsable(ClientAuthTokenSnapshot? snapshot, DateTime utcNow)
    {
        return snapshot is not null
            && !string.IsNullOrWhiteSpace(snapshot.AccessToken)
            && snapshot.AccessTokenExpiresAtUtc > utcNow.Add(ExpirySkew);
    }

    public bool IsRefreshTokenUsable(ClientAuthTokenSnapshot? snapshot, DateTime utcNow)
    {
        return snapshot is not null
            && !string.IsNullOrWhiteSpace(snapshot.RefreshToken)
            && snapshot.RefreshTokenExpiresAtUtc > utcNow.Add(ExpirySkew);
    }
}
