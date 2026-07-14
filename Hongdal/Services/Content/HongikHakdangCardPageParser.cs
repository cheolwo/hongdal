using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.Content;

public sealed record HongikHakdangParsedCard(
    string SourceKey,
    string? Title,
    string? Description,
    string OriginalImageUrl,
    string? ThumbnailImageUrl,
    string? RelatedUrl,
    int SortOrder);

public sealed record HongikHakdangParsedCollection(
    string SourceKey,
    string Name,
    int SortOrder,
    IReadOnlyList<HongikHakdangParsedCard> Cards);

public interface IHongikHakdangCardPageParser
{
    IReadOnlyList<HongikHakdangParsedCollection> Parse(string html);
}

public sealed partial class HongikHakdangCardPageParser : IHongikHakdangCardPageParser
{
    private readonly Uri _originalImageBaseUri;

    public HongikHakdangCardPageParser(IOptions<HongikHakdangCardOptions> options)
    {
        if (!Uri.TryCreate(options.Value.OriginalImageBaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme != Uri.UriSchemeHttps
            || !string.Equals(baseUri.Host, "cdn.imweb.me", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "HongikHakdangCards:OriginalImageBaseUrl은 https://cdn.imweb.me 주소여야 합니다.");
        }

        _originalImageBaseUri = baseUri;
    }

    public IReadOnlyList<HongikHakdangParsedCollection> Parse(string html)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(html);

        var galleryMatches = GalleryStartRegex().Matches(html);
        if (galleryMatches.Count == 0)
        {
            throw new InvalidOperationException("홍익학당 카드 페이지에서 갤러리를 찾지 못했습니다.");
        }

        var collections = new List<HongikHakdangParsedCollection>(galleryMatches.Count);
        var currentCollectionName = "카드 모음";
        var previousGalleryStart = 0;

        for (var galleryIndex = 0; galleryIndex < galleryMatches.Count; galleryIndex++)
        {
            var galleryMatch = galleryMatches[galleryIndex];
            var headingArea = html[previousGalleryStart..galleryMatch.Index];
            currentCollectionName = ResolveCollectionName(headingArea) ?? currentCollectionName;

            var galleryEnd = galleryIndex + 1 < galleryMatches.Count
                ? galleryMatches[galleryIndex + 1].Index
                : html.Length;
            var galleryHtml = html[galleryMatch.Index..galleryEnd];
            var cards = ParseCards(galleryHtml);
            if (cards.Count > 0)
            {
                collections.Add(new HongikHakdangParsedCollection(
                    NormalizeCollectionKey(galleryMatch.Groups["id"].Value),
                    Limit(currentCollectionName, 300) ?? $"카드 모음 {collections.Count + 1}",
                    collections.Count,
                    cards));
            }

            previousGalleryStart = galleryMatch.Index;
        }

        if (collections.Count == 0)
        {
            throw new InvalidOperationException("홍익학당 카드 페이지에서 카드 이미지를 찾지 못했습니다.");
        }

        return collections;
    }

    private IReadOnlyList<HongikHakdangParsedCard> ParseCards(string galleryHtml)
    {
        var itemMatches = CardItemStartRegex().Matches(galleryHtml);
        var cards = new List<HongikHakdangParsedCard>(itemMatches.Count);
        var sourceKeys = new HashSet<string>(StringComparer.Ordinal);

        for (var itemIndex = 0; itemIndex < itemMatches.Count; itemIndex++)
        {
            var itemMatch = itemMatches[itemIndex];
            var itemEnd = itemIndex + 1 < itemMatches.Count
                ? itemMatches[itemIndex + 1].Index
                : galleryHtml.Length;
            var itemHtml = galleryHtml[itemMatch.Index..itemEnd];
            var sourceKey = NormalizeCardSourceKey(itemMatch.Groups["sourceKey"].Value);
            if (!sourceKeys.Add(sourceKey))
            {
                continue;
            }

            var caption = CaptionRegex().Match(itemHtml);
            var title = caption.Success ? CleanText(caption.Groups["title"].Value) : null;
            var description = caption.Success ? CleanText(caption.Groups["description"].Value) : null;
            var thumbnailMatch = ThumbnailUrlRegex().Match(itemHtml);
            var thumbnailUrl = thumbnailMatch.Success
                ? NormalizeThumbnailUrl(WebUtility.HtmlDecode(thumbnailMatch.Groups["url"].Value))
                : null;

            cards.Add(new HongikHakdangParsedCard(
                sourceKey,
                Limit(title, 500),
                Limit(description, 10_000),
                new Uri(_originalImageBaseUri, sourceKey).AbsoluteUri,
                thumbnailUrl,
                ExtractRelatedUrl(description),
                cards.Count));
        }

        return cards;
    }

    private static string? ResolveCollectionName(string headingArea)
    {
        var markerMatches = TextWidgetMarkerRegex().Matches(headingArea);
        if (markerMatches.Count == 0)
        {
            return null;
        }

        var latestWidgetHtml = headingArea[markerMatches[^1].Index..];
        var strongMatches = StrongTextRegex().Matches(latestWidgetHtml);
        for (var index = strongMatches.Count - 1; index >= 0; index--)
        {
            var text = CleanText(strongMatches[index].Groups["text"].Value);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return null;
    }

    private static string NormalizeCollectionKey(string value)
    {
        var key = value.Trim();
        if (key.Length is 0 or > 200)
        {
            throw new InvalidOperationException("홍익학당 카드 갤러리 식별자가 올바르지 않습니다.");
        }

        return key;
    }

    private static string NormalizeCardSourceKey(string value)
    {
        var key = WebUtility.HtmlDecode(value).Trim().TrimStart('/');
        if (key.Length is 0 or > 500
            || key.Contains("..", StringComparison.Ordinal)
            || !AllowedSourceKeyRegex().IsMatch(key))
        {
            throw new InvalidOperationException("홍익학당 카드 원본 이미지 경로가 올바르지 않습니다.");
        }

        return key;
    }

    private static string? NormalizeThumbnailUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.Equals(uri.Host, "cdn.imweb.me", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return uri.AbsoluteUri;
    }

    private static string? ExtractRelatedUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = RelatedUrlRegex().Match(value);
        if (!match.Success)
        {
            return null;
        }

        var candidate = match.Value.TrimEnd('.', ',', ';', ')', ']', '}');
        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? Limit(uri.AbsoluteUri, 1500)
                : null;
    }

    private static string? CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var withoutTags = HtmlTagRegex().Replace(value, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags).Replace('\u00a0', ' ');
        var normalized = WhitespaceRegex().Replace(decoded, " ").Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    private static string? Limit(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Length <= maxLength
                ? value
                : value[..maxLength];

    [GeneratedRegex("<div\\s+id=\\\"container_(?<id>[^\\\"]+)\\\"\\s+class=\\\"[^\\\"]*\\bgallery2\\b[^\\\"]*\\\"[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GalleryStartRegex();

    [GeneratedRegex("<div\\s+class=\\\"_item\\s+item_gallary[^\\\"]*\\\"[^>]*\\sdata-org=\\\"(?<sourceKey>[^\\\"]+)\\\"[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CardItemStartRegex();

    [GeneratedRegex("<div\\s+id=\\\"caption_[^\\\"]+\\\"[^>]*>\\s*<h4[^>]*>(?<title>.*?)</h4>\\s*<p[^>]*>(?<description>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex CaptionRegex();

    [GeneratedRegex("data-src=\\\"(?<url>https://cdn\\.imweb\\.me/thumbnail/[^\\\"]+)\\\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ThumbnailUrlRegex();

    [GeneratedRegex("data-widget-type=\\\"text\\\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TextWidgetMarkerRegex();

    [GeneratedRegex("<strong[^>]*>(?<text>.*?)</strong>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex StrongTextRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("https?://[^\\s<>\\\"']+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RelatedUrlRegex();

    [GeneratedRegex("^[A-Za-z0-9_-]+/[A-Za-z0-9._-]+\\.(?:jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AllowedSourceKeyRegex();
}
