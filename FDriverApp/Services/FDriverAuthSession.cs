using System.Text.Json;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace FDriverApp.Services;

public interface IFDriverAuthSession : ISsalddelAccessTokenProvider
{
    DateTime AccessTokenExpiresAtUtc { get; }
    string? RefreshToken { get; }
    DateTime RefreshTokenExpiresAtUtc { get; }
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    ClientAuthSessionRestoreState CurrentState { get; }
    Task<ClientAuthSessionRestoreState> RestoreAsync(CancellationToken cancellationToken = default);
    Task ApplyAsync(ClientAuthTokenSnapshot snapshot, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

public sealed class FDriverAuthSession : IFDriverAuthSession
{
    private const string StorageKey = "ssalddel.fdriver.authToken.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IClientSessionGuard _sessionGuard;
    private bool _restored;

    public FDriverAuthSession(IClientSessionGuard sessionGuard)
    {
        _sessionGuard = sessionGuard;
    }

    public string? AccessToken { get; private set; }
    public DateTime AccessTokenExpiresAtUtc { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime RefreshTokenExpiresAtUtc { get; private set; }
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public ClientAuthSessionRestoreState CurrentState
    {
        get
        {
            var snapshot = CreateSnapshot();
            if (_sessionGuard.IsAccessTokenUsable(snapshot, DateTime.UtcNow))
            {
                return ClientAuthSessionRestoreState.Authenticated;
            }

            return _sessionGuard.IsRefreshTokenUsable(snapshot, DateTime.UtcNow)
                ? ClientAuthSessionRestoreState.RefreshRequired
                : ClientAuthSessionRestoreState.Anonymous;
        }
    }
    public bool IsAuthenticated => CurrentState == ClientAuthSessionRestoreState.Authenticated;

    public async Task<ClientAuthSessionRestoreState> RestoreAsync(CancellationToken cancellationToken = default)
    {
        if (_restored)
        {
            return CurrentState;
        }

        _restored = true;
        try
        {
            var json = await SecureStorage.Default.GetAsync(StorageKey);
            var snapshot = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<ClientAuthTokenSnapshot>(json, JsonOptions);
            if (!_sessionGuard.IsAccessTokenUsable(snapshot, DateTime.UtcNow)
                && !_sessionGuard.IsRefreshTokenUsable(snapshot, DateTime.UtcNow))
            {
                await ClearAsync(cancellationToken);
                return ClientAuthSessionRestoreState.Anonymous;
            }

            ApplySnapshot(snapshot!);
        }
        catch (Exception) when (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsWindows())
        {
            await ClearAsync(cancellationToken);
        }

        return CurrentState;
    }

    public async Task ApplyAsync(ClientAuthTokenSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        ApplySnapshot(snapshot);
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        await SecureStorage.Default.SetAsync(StorageKey, json);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AccessToken = null;
        AccessTokenExpiresAtUtc = default;
        RefreshToken = null;
        RefreshTokenExpiresAtUtc = default;
        UserId = null;
        UserName = null;
        Roles = [];
        SecureStorage.Default.Remove(StorageKey);
        return Task.CompletedTask;
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

    private ClientAuthTokenSnapshot? CreateSnapshot()
        => string.IsNullOrWhiteSpace(UserId)
            ? null
            : new ClientAuthTokenSnapshot(
                AccessToken ?? string.Empty,
                AccessTokenExpiresAtUtc,
                RefreshToken ?? string.Empty,
                RefreshTokenExpiresAtUtc,
                UserId,
                UserName ?? string.Empty,
                Roles);
}
