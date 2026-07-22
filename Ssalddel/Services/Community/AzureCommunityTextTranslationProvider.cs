using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Ssalddel.Contracts.Common.Localization;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed class AzureCommunityTextTranslationProvider : ICommunityTextTranslationProvider
{
    private const string ProviderName = "AzureTranslator";
    private const string GlobalEndpointHost = "api.cognitive.microsofttranslator.com";
    private readonly HttpClient _httpClient;
    private readonly CommunityPostTranslationOptions _options;
    private readonly IAzureTranslatorAccessTokenProvider _accessTokenProvider;

    public AzureCommunityTextTranslationProvider(
        HttpClient httpClient,
        IOptions<CommunityPostTranslationOptions> options,
        IAzureTranslatorAccessTokenProvider accessTokenProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _accessTokenProvider = accessTokenProvider;
    }

    public bool IsAvailable
        => _options.Enabled
           && string.Equals(_options.Provider, ProviderName, StringComparison.OrdinalIgnoreCase)
           && HasValidAuthenticationConfiguration();

    public async Task<CommunityTextTranslationResult> TranslateAsync(
        string title,
        string body,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException("Azure 게시글 번역 설정이 활성화되지 않았습니다.");
        }

        var source = DisplayLanguageCodes.ToNeutralCode(sourceLanguageCode);
        var target = DisplayLanguageCodes.ToNeutralCode(targetLanguageCode);
        var path = $"translate?api-version=3.0&from={Uri.EscapeDataString(source)}&to={Uri.EscapeDataString(target)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new[]
            {
                new AzureTranslationRequest(title),
                new AzureTranslationRequest(body)
            })
        };
        await ApplyAuthenticationAsync(request, cancellationToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AzureTranslationResponse[]>(
            cancellationToken: cancellationToken);

        if (payload is not { Length: >= 2 }
            || payload[0].Translations.Count == 0
            || payload[1].Translations.Count == 0)
        {
            throw new InvalidOperationException("Azure Translator가 게시글 번역 결과를 반환하지 않았습니다.");
        }

        return new CommunityTextTranslationResult(
            payload[0].Translations[0].Text,
            payload[1].Translations[0].Text,
            ProviderName,
            "v3.0-general");
    }

    private bool HasValidAuthenticationConfiguration()
    {
        if (string.Equals(
                _options.AuthenticationMode,
                AzureTranslatorAuthenticationModes.MicrosoftEntraId,
                StringComparison.OrdinalIgnoreCase))
        {
            return !UsesGlobalEndpoint()
                   || (!string.IsNullOrWhiteSpace(_options.ResourceId)
                       && !string.IsNullOrWhiteSpace(_options.Region));
        }

        return string.Equals(
                   _options.AuthenticationMode,
                   AzureTranslatorAuthenticationModes.ApiKey,
                   StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrWhiteSpace(_options.ApiKey);
    }

    private async Task ApplyAuthenticationAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (string.Equals(
                _options.AuthenticationMode,
                AzureTranslatorAuthenticationModes.MicrosoftEntraId,
                StringComparison.OrdinalIgnoreCase))
        {
            var accessToken = await _accessTokenProvider.GetTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            AddOptionalHeader(request, "Ocp-Apim-ResourceId", _options.ResourceId);
            AddOptionalHeader(request, "Ocp-Apim-Subscription-Region", _options.Region);
            return;
        }

        request.Headers.Add("Ocp-Apim-Subscription-Key", _options.ApiKey.Trim());
        AddOptionalHeader(request, "Ocp-Apim-Subscription-Region", _options.Region);
    }

    private bool UsesGlobalEndpoint()
        => Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out var endpoint)
           && string.Equals(endpoint.Host, GlobalEndpointHost, StringComparison.OrdinalIgnoreCase);

    private static void AddOptionalHeader(
        HttpRequestMessage request,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            request.Headers.Add(name, value.Trim());
        }
    }

    private sealed record AzureTranslationRequest(
        [property: JsonPropertyName("Text")] string Text);

    private sealed class AzureTranslationResponse
    {
        [JsonPropertyName("translations")]
        public List<AzureTranslatedText> Translations { get; set; } = [];
    }

    private sealed class AzureTranslatedText
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
