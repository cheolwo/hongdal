using System.Text.Json;
using Ssalddel.Client.Infrastructure.Security;

namespace OrdererApp.Services.Security;

/// <summary>주문자 앱 인증 토큰을 다른 역할 앱과 분리된 MAUI 보안 저장소에 보관합니다.</summary>
public sealed class OrdererMauiSecureTokenStore : IClientSecureTokenStore
{
    private const string StorageKey = "ssalddel.orderer.authToken.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ClientAuthTokenSnapshot?> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var json = await SecureStorage.Default.GetAsync(StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ClientAuthTokenSnapshot>(json, JsonOptions);
        }
        catch (JsonException)
        {
            SecureStorage.Default.Remove(StorageKey);
            return null;
        }
    }

    public async Task SaveAsync(
        ClientAuthTokenSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        await SecureStorage.Default.SetAsync(
            StorageKey,
            JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SecureStorage.Default.Remove(StorageKey);
        return Task.CompletedTask;
    }
}
