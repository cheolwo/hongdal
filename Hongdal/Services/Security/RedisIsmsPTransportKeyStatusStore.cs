using System.Globalization;
using Hongdal.Contracts.Common.Privacy;
using StackExchange.Redis;

namespace Hongdal.Services.Security;

public sealed class RedisIsmsPTransportKeyStatusStore : IIsmsPTransportKeyStatusStore
{
    private const string KeyPrefix = "hongdal:isms-p:transport:key:";
    private const string ActiveStatus = "active";

    private readonly IDatabase database;

    public RedisIsmsPTransportKeyStatusStore(IConnectionMultiplexer connectionMultiplexer)
    {
        database = connectionMultiplexer.GetDatabase();
    }

    public async Task MarkActiveAsync(
        IsmsPClientEncryptionPublicKeyResponse publicKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKey.KeyId);

        var now = DateTimeOffset.UtcNow;
        var ttl = publicKey.ExpiresAtUtc - now;
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        var redisKey = BuildKey(publicKey.KeyId);
        await database.HashSetAsync(
            redisKey,
            [
                new HashEntry("keyId", publicKey.KeyId),
                new HashEntry("algorithmCode", publicKey.AlgorithmCode),
                new HashEntry("status", ActiveStatus),
                new HashEntry("issuedAtUtc", publicKey.IssuedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)),
                new HashEntry("expiresAtUtc", publicKey.ExpiresAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture))
            ]).ConfigureAwait(false);
        await database.KeyExpireAsync(redisKey, ttl).ConfigureAwait(false);
    }

    public async Task<bool> IsActiveAsync(
        string keyId,
        string algorithmCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(algorithmCode))
        {
            return false;
        }

        var values = await database.HashGetAllAsync(BuildKey(keyId)).ConfigureAwait(false);
        if (values.Length == 0)
        {
            return false;
        }

        var fields = values.ToDictionary(
            x => x.Name.ToString(),
            x => x.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        if (!fields.TryGetValue("status", out var status) ||
            !string.Equals(status, ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!fields.TryGetValue("algorithmCode", out var storedAlgorithmCode) ||
            !string.Equals(storedAlgorithmCode, algorithmCode, StringComparison.Ordinal))
        {
            return false;
        }

        if (!fields.TryGetValue("expiresAtUtc", out var expiresAtValue) ||
            !DateTimeOffset.TryParse(
                expiresAtValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var expiresAtUtc))
        {
            return false;
        }

        return expiresAtUtc > DateTimeOffset.UtcNow;
    }

    private static string BuildKey(string keyId) => KeyPrefix + keyId.Trim();
}
