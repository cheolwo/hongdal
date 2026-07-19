using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Content;

public sealed record HongikHakdangCardMediaGrant(
    long VariantId,
    DateTimeOffset ExpiresAtUtc);

public interface IHongikHakdangCardMediaTokenService
{
    string CreateRelativeUrl(long variantId, DateTimeOffset? expiresAtUtc = null);

    bool TryValidate(string token, out HongikHakdangCardMediaGrant? grant);
}

public sealed class HongikHakdangCardMediaTokenService : IHongikHakdangCardMediaTokenService
{
    private const string RoutePrefix = "/api/v1/content/hongik-hakdang/cards/media/";
    private readonly IDataProtector _protector;
    private readonly HongikHakdangCardOptions _options;

    public HongikHakdangCardMediaTokenService(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<HongikHakdangCardOptions> options)
    {
        _protector = dataProtectionProvider.CreateProtector("Ssalddel.HongikHakdangCardMedia.v1");
        _options = options.Value;
    }

    public string CreateRelativeUrl(long variantId, DateTimeOffset? expiresAtUtc = null)
    {
        if (variantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(variantId));
        }

        var expiry = expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(
            Math.Clamp(_options.MediaUrlLifetimeMinutes, 5, 7 * 24 * 60));
        var payload = Encoding.UTF8.GetBytes($"{variantId}|{expiry.ToUnixTimeSeconds()}");
        var token = WebEncoders.Base64UrlEncode(_protector.Protect(payload));
        return $"{RoutePrefix}{token}.jpg";
    }

    public bool TryValidate(string token, out HongikHakdangCardMediaGrant? grant)
    {
        grant = null;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var plaintext = Encoding.UTF8.GetString(
                _protector.Unprotect(WebEncoders.Base64UrlDecode(token)));
            var parts = plaintext.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length != 2
                || !long.TryParse(parts[0], out var variantId)
                || variantId <= 0
                || !long.TryParse(parts[1], out var expirySeconds))
            {
                return false;
            }

            var expiry = DateTimeOffset.FromUnixTimeSeconds(expirySeconds);
            if (expiry <= DateTimeOffset.UtcNow)
            {
                return false;
            }

            grant = new HongikHakdangCardMediaGrant(variantId, expiry);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
