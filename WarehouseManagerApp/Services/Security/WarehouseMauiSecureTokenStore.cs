using System.Text.Json;
using Ssalddel.Client.Infrastructure.Security;

namespace WarehouseManagerApp.Services.Security;

public sealed class WarehouseMauiSecureTokenStore : IClientSecureTokenStore
{
    private const string StorageKey = "ssalddel.warehouse.authToken.v1";
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

        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        await SecureStorage.Default.SetAsync(StorageKey, json);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SecureStorage.Default.Remove(StorageKey);
        return Task.CompletedTask;
    }
}
