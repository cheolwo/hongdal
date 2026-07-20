namespace Ssalddel.Client.Infrastructure.Security;

public enum ClientAuthSessionRestoreState
{
    Anonymous,
    Authenticated,
    RefreshRequired
}

/// <summary>
/// 클라이언트 인증 토큰의 메모리 상태와 보안 저장소 동기화만 담당합니다.
/// 로그인·갱신 HTTP 호출과 앱별 역할 판단은 각 클라이언트에서 처리합니다.
/// </summary>
public sealed class ClientAuthSession
{
    private readonly IClientSecureTokenStore _tokenStore;
    private readonly IClientSessionGuard _sessionGuard;
    private bool _restored;
    private ClientAuthSessionRestoreState _restoreState = ClientAuthSessionRestoreState.Anonymous;

    public ClientAuthSession(
        IClientSecureTokenStore tokenStore,
        IClientSessionGuard sessionGuard)
    {
        _tokenStore = tokenStore;
        _sessionGuard = sessionGuard;
    }

    public string? AccessToken { get; private set; }
    public DateTime AccessTokenExpiresAtUtc { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime RefreshTokenExpiresAtUtc { get; private set; }
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public bool IsAuthenticated => _restoreState == ClientAuthSessionRestoreState.Authenticated
                                   && !string.IsNullOrWhiteSpace(AccessToken)
                                   && !string.IsNullOrWhiteSpace(UserId);

    public async Task<ClientAuthSessionRestoreState> RestoreAsync(
        CancellationToken cancellationToken = default)
    {
        if (_restored)
        {
            return _restoreState;
        }

        _restored = true;
        var snapshot = await _tokenStore.LoadAsync(cancellationToken);
        if (_sessionGuard.IsAccessTokenUsable(snapshot, DateTime.UtcNow))
        {
            ApplySnapshot(snapshot!);
            _restoreState = ClientAuthSessionRestoreState.Authenticated;
            return _restoreState;
        }

        if (_sessionGuard.IsRefreshTokenUsable(snapshot, DateTime.UtcNow))
        {
            ApplySnapshot(snapshot!);
            _restoreState = ClientAuthSessionRestoreState.RefreshRequired;
            return _restoreState;
        }

        await ClearAsync(cancellationToken);
        return _restoreState;
    }

    public async Task ApplyAsync(
        ClientAuthTokenSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        ApplySnapshot(snapshot);
        _restored = true;
        _restoreState = ClientAuthSessionRestoreState.Authenticated;
        await _tokenStore.SaveAsync(snapshot, cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        AccessToken = null;
        AccessTokenExpiresAtUtc = default;
        RefreshToken = null;
        RefreshTokenExpiresAtUtc = default;
        UserId = null;
        UserName = null;
        Roles = [];
        _restored = true;
        _restoreState = ClientAuthSessionRestoreState.Anonymous;
        await _tokenStore.ClearAsync(cancellationToken);
    }

    private void ApplySnapshot(ClientAuthTokenSnapshot snapshot)
    {
        AccessToken = snapshot.AccessToken;
        AccessTokenExpiresAtUtc = snapshot.AccessTokenExpiresAtUtc;
        RefreshToken = snapshot.RefreshToken;
        RefreshTokenExpiresAtUtc = snapshot.RefreshTokenExpiresAtUtc;
        UserId = snapshot.UserId;
        UserName = snapshot.UserName;
        Roles = snapshot.Roles.ToArray();
    }
}
