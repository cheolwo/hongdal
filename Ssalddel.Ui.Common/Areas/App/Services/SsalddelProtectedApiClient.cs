using System.Net.Http.Json;
using System.Net.Http.Headers;
using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class SsalddelProtectedApiClient
{
    private const string PublicKeyPath = "api/v1/security/isms-p/transport/public-key";

    private readonly HttpClient httpClient;
    private readonly SsalddelIsmsPClientEncryptionService encryptionService;
    private readonly ISsalddelAccessTokenProvider accessTokenProvider;
    private IsmsPClientEncryptionPublicKeyResponse? cachedPublicKey;

    public SsalddelProtectedApiClient(
        HttpClient httpClient,
        SsalddelIsmsPClientEncryptionService encryptionService,
        ISsalddelAccessTokenProvider accessTokenProvider)
    {
        this.httpClient = httpClient;
        this.encryptionService = encryptionService;
        this.accessTokenProvider = accessTokenProvider;
    }

    public async Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        ApplyAuthorization(message);
        return await httpClient.SendAsync(message, cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
        using var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        ApplyAuthorization(message);
        return await httpClient.SendAsync(message, cancellationToken);
    }

    public Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string requestUri,
        CancellationToken cancellationToken = default)
        => SendAsync(method, requestUri, headers: null, cancellationToken: cancellationToken);

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string requestUri,
        IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
        using var message = new HttpRequestMessage(method, requestUri);
        ApplyAuthorization(message);
        ApplyHeaders(message, headers);
        return await httpClient.SendAsync(message, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsProtectedJsonAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        => await SendAsProtectedJsonAsync(HttpMethod.Post, requestUri, request, cancellationToken);

    public async Task<TResponse?> PostAsProtectedJsonAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostAsProtectedJsonAsync(requestUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> PutAsProtectedJsonAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        => await SendAsProtectedJsonAsync(HttpMethod.Put, requestUri, request, cancellationToken);

    public async Task<HttpResponseMessage> PostAsync(
        string requestUri,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
        ArgumentNullException.ThrowIfNull(content);

        using var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };
        ApplyAuthorization(message);
        return await httpClient.SendAsync(message, cancellationToken);
    }

    public async Task<TResponse?> PutAsProtectedJsonAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await PutAsProtectedJsonAsync(requestUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public Task<HttpResponseMessage> SendAsProtectedJsonAsync<TRequest>(
        HttpMethod method,
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        => SendAsProtectedJsonAsync(
            method,
            requestUri,
            request,
            headers: null,
            cancellationToken: cancellationToken);

    public async Task<HttpResponseMessage> SendAsProtectedJsonAsync<TRequest>(
        HttpMethod method,
        string requestUri,
        TRequest request,
        IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);

        using var message = new HttpRequestMessage(method, requestUri);
        ApplyAuthorization(message);
        ApplyHeaders(message, headers);
        message.Content = await CreateProtectedJsonContentAsync(requestUri, request, cancellationToken);
        return await httpClient.SendAsync(message, cancellationToken);
    }

    private void ApplyAuthorization(HttpRequestMessage message)
    {
        var token = accessTokenProvider.AccessToken?.Trim();
        if (!string.IsNullOrWhiteSpace(token))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static void ApplyHeaders(
        HttpRequestMessage message,
        IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var (name, value) in headers)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            if (string.Equals(name, "Authorization", StringComparison.OrdinalIgnoreCase)
                || !message.Headers.TryAddWithoutValidation(name, value.Trim()))
            {
                throw new InvalidOperationException($"요청 헤더 '{name}'을(를) 적용할 수 없습니다.");
            }
        }
    }

    public async Task<JsonContent> CreateProtectedJsonContentAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SsalddelIsmsPClientEncryptionService.RequiresEncryptedTransport<TRequest>())
        {
            return JsonContent.Create(request);
        }

        var publicKey = await GetPublicKeyAsync(cancellationToken);
        var envelope = await encryptionService.EncryptJsonAsync(
            publicKey,
            request,
            associatedData: requestUri);

        return JsonContent.Create(envelope);
    }

    private async Task<IsmsPClientEncryptionPublicKeyResponse> GetPublicKeyAsync(
        CancellationToken cancellationToken)
    {
        if (cachedPublicKey is not null &&
            cachedPublicKey.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return cachedPublicKey;
        }

        cachedPublicKey = await httpClient.GetFromJsonAsync<IsmsPClientEncryptionPublicKeyResponse>(
            PublicKeyPath,
            cancellationToken)
            ?? throw new InvalidOperationException("ISMS-P transport public key response was empty.");

        return cachedPublicKey;
    }
}
