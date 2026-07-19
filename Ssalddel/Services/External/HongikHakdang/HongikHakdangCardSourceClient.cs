using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.External.HongikHakdang;

public sealed record HongikHakdangCardImageContent(
    byte[] Bytes,
    string? ContentType);

public interface IHongikHakdangCardSourceClient
{
    Task<string> GetCardPageHtmlAsync(CancellationToken cancellationToken);

    Task<HongikHakdangCardImageContent> DownloadImageAsync(
        string imageUrl,
        CancellationToken cancellationToken);
}

public sealed class HongikHakdangCardSourceClient : IHongikHakdangCardSourceClient
{
    private readonly HttpClient _httpClient;
    private readonly HongikHakdangCardOptions _options;

    public HongikHakdangCardSourceClient(
        HttpClient httpClient,
        IOptions<HongikHakdangCardOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetCardPageHtmlAsync(CancellationToken cancellationToken)
    {
        var sourceUri = ValidateSourcePageUri(_options.SourcePageUrl);
        using var request = new HttpRequestMessage(HttpMethod.Get, sourceUri);
        request.Headers.UserAgent.ParseAdd("Ssalddel-Internal-Content-Collector/1.0");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<HongikHakdangCardImageContent> DownloadImageAsync(
        string imageUrl,
        CancellationToken cancellationToken)
    {
        var imageUri = ValidateImageUri(imageUrl);
        using var request = new HttpRequestMessage(HttpMethod.Get, imageUri);
        request.Headers.UserAgent.ParseAdd("Ssalddel-Internal-Content-Collector/1.0");
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var maxBytes = Math.Clamp(_options.MaxImageBytes, 1024, 100 * 1024 * 1024);
        if (response.Content.Headers.ContentLength is > 0
            && response.Content.Headers.ContentLength > maxBytes)
        {
            throw new InvalidOperationException($"카드 이미지가 허용 크기 {maxBytes:N0}바이트를 초과합니다.");
        }

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var buffer = new MemoryStream();
        var chunk = new byte[64 * 1024];
        while (true)
        {
            var read = await source.ReadAsync(chunk, cancellationToken);
            if (read == 0)
            {
                break;
            }

            if (buffer.Length + read > maxBytes)
            {
                throw new InvalidOperationException($"카드 이미지가 허용 크기 {maxBytes:N0}바이트를 초과합니다.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        return new HongikHakdangCardImageContent(
            buffer.ToArray(),
            response.Content.Headers.ContentType?.MediaType);
    }

    private static Uri ValidateSourcePageUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.Equals(uri.Host, "hihd.imweb.me", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "HongikHakdangCards:SourcePageUrl은 https://hihd.imweb.me 주소여야 합니다.");
        }

        return uri;
    }

    private static Uri ValidateImageUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.Equals(uri.Host, "cdn.imweb.me", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("허용되지 않은 홍익학당 카드 이미지 주소입니다.");
        }

        return uri;
    }
}
