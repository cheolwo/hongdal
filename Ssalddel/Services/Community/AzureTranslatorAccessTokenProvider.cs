using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public interface IAzureTranslatorAccessTokenProvider
{
    ValueTask<string> GetTokenAsync(CancellationToken cancellationToken);
}

public sealed class AzureTranslatorAccessTokenProvider : IAzureTranslatorAccessTokenProvider
{
    private static readonly TokenRequestContext TokenRequest = new(
        ["https://cognitiveservices.azure.com/.default"]);

    private readonly Lazy<TokenCredential> _credential;

    public AzureTranslatorAccessTokenProvider(IOptions<CommunityPostTranslationOptions> options)
    {
        var managedIdentityClientId = options.Value.ManagedIdentityClientId?.Trim();
        _credential = new Lazy<TokenCredential>(
            () => CreateCredential(managedIdentityClientId),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var accessToken = await _credential.Value.GetTokenAsync(TokenRequest, cancellationToken);
        return accessToken.Token;
    }

    private static TokenCredential CreateCredential(string? managedIdentityClientId)
    {
        var options = new DefaultAzureCredentialOptions();
        if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            options.ManagedIdentityClientId = managedIdentityClientId;
        }

        return new DefaultAzureCredential(options);
    }
}
