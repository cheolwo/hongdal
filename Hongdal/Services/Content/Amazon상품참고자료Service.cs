using System.Text.RegularExpressions;
using Hongdal.Contracts.Common.Content;
using Hongdal.Services.External.Apify;

namespace Hongdal.Services.Content;

public interface IAmazon상품참고자료Service
{
    Task<Amazon상품참고자료Dto> 미리보기Async(
        Amazon상품참고자료조회요청Dto 요청,
        CancellationToken cancellationToken);
}

public sealed partial class Amazon상품참고자료Service : IAmazon상품참고자료Service
{
    private static readonly IReadOnlySet<string> AmazonDomains = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "amazon.com",
        "amazon.co.uk",
        "amazon.de",
        "amazon.fr",
        "amazon.it",
        "amazon.es",
        "amazon.ca",
        "amazon.co.jp",
        "amazon.in",
        "amazon.com.au",
        "amazon.com.mx"
    };

    private readonly IApifyAmazonProductClient _client;

    public Amazon상품참고자료Service(IApifyAmazonProductClient client)
    {
        _client = client;
    }

    public async Task<Amazon상품참고자료Dto> 미리보기Async(
        Amazon상품참고자료조회요청Dto 요청,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(요청);
        var productUri = ValidateProductUrl(요청.상품Url);
        var inputAsin = ExtractAsin(productUri);
        var observedAtUtc = DateTime.UtcNow;
        var product = await _client.상품상세조회Async(productUri, cancellationToken)
            ?? throw new InvalidOperationException("Apify가 Amazon 상품 상세 결과를 반환하지 않았습니다.");

        var countryCode = product.국가코드 ?? ResolveCountryCode(productUri.Host);
        var canonicalUrl = ValidateReturnedUrl(product.원문Url, productUri).AbsoluteUri;
        var referenceKey = $"amazon:{countryCode.ToLowerInvariant()}:{product.Asin.ToLowerInvariant()}";
        var currency = product.현재가격.통화코드
            ?? product.정가.통화코드
            ?? product.배송비.통화코드;
        var externalReferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SourceProvider"] = "Apify",
            ["SourceMarketplace"] = "Amazon",
            ["AmazonAsin"] = product.Asin,
            ["AmazonInputAsin"] = inputAsin,
            ["AmazonProductUrl"] = canonicalUrl,
            ["MarketplaceCountryCode"] = countryCode,
            ["ObservedAtUtc"] = observedAtUtc.ToString("O")
        };

        return new Amazon상품참고자료Dto(
            referenceKey,
            product.Asin,
            product.상품명,
            product.브랜드명,
            canonicalUrl,
            countryCode,
            new 외부상품가격스냅샷Dto(
                product.현재가격.금액,
                product.정가.금액,
                product.배송비.금액,
                currency),
            product.재고여부,
            product.재고표시문구,
            product.평점,
            product.리뷰수,
            product.카테고리경로,
            product.썸네일Url,
            product.이미지Url목록,
            product.특징목록,
            product.속성목록
                .Select(attribute => new 외부상품속성Dto(attribute.항목명, attribute.값))
                .ToArray(),
            observedAtUtc,
            외부상품참고자료검수상태코드.대기,
            externalReferences,
            "Amazon 페이지의 외부 관측 자료입니다. 가격·재고·브랜드·원산지·수입 가능성을 Hongdal의 확정 상품 정보로 자동 전환하지 말고 운영자 검수와 참여자 직접 판단에만 사용하세요.");
    }

    private static Uri ValidateProductUrl(string value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !IsAmazonHost(uri.Host))
        {
            throw new ArgumentException("지원하는 Amazon HTTPS 상품 URL이 필요합니다.", nameof(value));
        }

        _ = ExtractAsin(uri);
        return uri;
    }

    private static Uri ValidateReturnedUrl(string? value, Uri fallback)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !IsAmazonHost(uri.Host))
        {
            return fallback;
        }

        return uri;
    }

    private static bool IsAmazonHost(string host)
        => AmazonDomains.Any(domain =>
            string.Equals(host, domain, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase));

    private static string ExtractAsin(Uri uri)
    {
        var match = AmazonProductPathRegex().Match(uri.AbsolutePath);
        if (!match.Success)
        {
            throw new ArgumentException("Amazon 검색·카테고리 URL이 아닌 상품 상세 URL이 필요합니다.", nameof(uri));
        }

        return match.Groups[1].Value.ToUpperInvariant();
    }

    private static string ResolveCountryCode(string host)
    {
        var normalized = host.ToLowerInvariant();
        if (normalized.EndsWith("amazon.co.uk", StringComparison.Ordinal)) return "GB";
        if (normalized.EndsWith("amazon.co.jp", StringComparison.Ordinal)) return "JP";
        if (normalized.EndsWith("amazon.com.au", StringComparison.Ordinal)) return "AU";
        if (normalized.EndsWith("amazon.com.mx", StringComparison.Ordinal)) return "MX";
        if (normalized.EndsWith("amazon.de", StringComparison.Ordinal)) return "DE";
        if (normalized.EndsWith("amazon.fr", StringComparison.Ordinal)) return "FR";
        if (normalized.EndsWith("amazon.it", StringComparison.Ordinal)) return "IT";
        if (normalized.EndsWith("amazon.es", StringComparison.Ordinal)) return "ES";
        if (normalized.EndsWith("amazon.ca", StringComparison.Ordinal)) return "CA";
        if (normalized.EndsWith("amazon.in", StringComparison.Ordinal)) return "IN";
        return "US";
    }

    [GeneratedRegex(@"/(?:dp|gp/product)/([A-Za-z0-9]{10})(?:[/?]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AmazonProductPathRegex();
}
