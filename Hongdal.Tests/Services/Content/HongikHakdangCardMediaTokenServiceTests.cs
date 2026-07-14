using Hongdal.Services.Content;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Tests.Services.Content;

public sealed class HongikHakdangCardMediaTokenServiceTests
{
    private readonly HongikHakdangCardMediaTokenService _service = new(
        new EphemeralDataProtectionProvider(),
        Options.Create(new HongikHakdangCardOptions { MediaUrlLifetimeMinutes = 60 }));

    [Fact]
    public void CreateRelativeUrl_ProducesAValidExpiringGrant()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(10);
        var relativeUrl = _service.CreateRelativeUrl(42, expiry);
        var token = ExtractToken(relativeUrl);

        var valid = _service.TryValidate(token, out var grant);

        Assert.True(valid);
        Assert.NotNull(grant);
        Assert.Equal(42, grant.VariantId);
        Assert.Equal(expiry.ToUnixTimeSeconds(), grant.ExpiresAtUtc.ToUnixTimeSeconds());
    }

    [Fact]
    public void TryValidate_RejectsExpiredOrTamperedTokens()
    {
        var expired = ExtractToken(_service.CreateRelativeUrl(42, DateTimeOffset.UtcNow.AddMinutes(-1)));
        var current = ExtractToken(_service.CreateRelativeUrl(42));
        var tampered = $"{current[..^1]}{(current[^1] == 'a' ? 'b' : 'a')}";

        Assert.False(_service.TryValidate(expired, out _));
        Assert.False(_service.TryValidate(tampered, out _));
    }

    private static string ExtractToken(string relativeUrl)
    {
        var fileName = relativeUrl[(relativeUrl.LastIndexOf('/') + 1)..];
        return fileName[..^".jpg".Length];
    }
}
