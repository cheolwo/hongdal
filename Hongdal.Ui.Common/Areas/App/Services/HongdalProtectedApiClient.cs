using System.Net.Http.Json;
using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class HongdalProtectedApiClient
{
    private const string PublicKeyPath = "api/v1/security/isms-p/transport/public-key";

    private readonly HttpClient httpClient;
    private readonly HongdalIsmsPClientEncryptionService encryptionService;
    private IsmsPClientEncryptionPublicKeyResponse? cachedPublicKey;

    public HongdalProtectedApiClient(
        HttpClient httpClient,
        HongdalIsmsPClientEncryptionService encryptionService)
    {
        this.httpClient = httpClient;
        this.encryptionService = encryptionService;
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

    public async Task<TResponse?> PutAsProtectedJsonAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await PutAsProtectedJsonAsync(requestUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsProtectedJsonAsync<TRequest>(
        HttpMethod method,
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);

        using var message = new HttpRequestMessage(method, requestUri);
        message.Content = await CreateProtectedJsonContentAsync(requestUri, request, cancellationToken);
        return await httpClient.SendAsync(message, cancellationToken);
    }

    public async Task<JsonContent> CreateProtectedJsonContentAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!HongdalIsmsPClientEncryptionService.RequiresEncryptedTransport<TRequest>())
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
