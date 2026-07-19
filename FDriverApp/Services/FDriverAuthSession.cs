using System.Text.Json;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace FDriverApp.Services;

public interface IFDriverAuthSession : ISsalddelAccessTokenProvider
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    Task RestoreAsync(CancellationToken cancellationToken = default);
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
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(UserId);

    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        if (_restored)
        {
            return;
        }

        _restored = true;
        try
        {
            var json = await SecureStorage.Default.GetAsync(StorageKey);
            var snapshot = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<ClientAuthTokenSnapshot>(json, JsonOptions);
            if (!_sessionGuard.IsAccessTokenUsable(snapshot, DateTime.UtcNow))
            {
                await ClearAsync(cancellationToken);
                return;
            }

            ApplySnapshot(snapshot!);
        }
        catch (Exception) when (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsWindows())
        {
            await ClearAsync(cancellationToken);
        }
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
        UserId = null;
        UserName = null;
        SecureStorage.Default.Remove(StorageKey);
        return Task.CompletedTask;
    }

    private void ApplySnapshot(ClientAuthTokenSnapshot snapshot)
    {
        AccessToken = snapshot.AccessToken;
        UserId = snapshot.UserId;
        UserName = snapshot.UserName;
    }
}
