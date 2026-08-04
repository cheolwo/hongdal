using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Content;

internal sealed record 공식뉴스RssFeedDefinition(
    string SourceKey,
    string Provider,
    string DisplayName,
    string FeedUrl,
    string DocumentationUrl,
    string FeedKind,
    IReadOnlySet<string> AllowedArticleHosts);

internal static class 공식뉴스RssFeedCatalog
{
    public static 공식뉴스RssFeedDefinition MafraPressReleases { get; } = new(
        CommunityInformationSourceKeys.MafraPressReleases,
        "농림축산식품부",
        "농림축산식품부 보도자료",
        "https://www.mafra.go.kr/bbs/home/792/rssList.do?row=50",
        "https://www.mafra.go.kr/home/5327/subview.do",
        "보도자료",
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "mafra.go.kr" });

    public static 공식뉴스RssFeedDefinition MafraExplanations { get; } = new(
        CommunityInformationSourceKeys.MafraExplanations,
        "농림축산식품부",
        "농림축산식품부 설명·반박자료",
        "https://www.mafra.go.kr/bbs/home/793/rssList.do?row=50",
        "https://www.mafra.go.kr/home/5327/subview.do",
        "설명자료",
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "mafra.go.kr" });

    public static 공식뉴스RssFeedDefinition MfdsPressReleases { get; } = new(
        CommunityInformationSourceKeys.MfdsPressReleases,
        "식품의약품안전처",
        "식품의약품안전처 보도자료",
        "https://www.mfds.go.kr/www/rss/brd.do?brdId=ntc0021",
        "https://www.mfds.go.kr/www/rss/list.do",
        "보도자료",
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "mfds.go.kr" });

    public static IReadOnlyList<공식뉴스RssFeedDefinition> All { get; } =
    [
        MafraPressReleases,
        MafraExplanations,
        MfdsPressReleases
    ];
}

internal sealed record 공식뉴스RssCacheEntry(
    string Xml,
    string? ETag,
    DateTimeOffset? LastModified);

internal sealed class 공식뉴스RssConditionalCache
{
    private readonly Dictionary<string, 공식뉴스RssCacheEntry> _entries =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    public bool TryGet(string feedUrl, out 공식뉴스RssCacheEntry? entry)
    {
        lock (_gate)
        {
            return _entries.TryGetValue(feedUrl, out entry);
        }
    }

    public void Set(string feedUrl, 공식뉴스RssCacheEntry entry)
    {
        lock (_gate)
        {
            _entries[feedUrl] = entry;
        }
    }
}

internal sealed class 공식뉴스RssClient
{
    private readonly HttpClient _httpClient;
    private readonly OfficialNewsRssOptions _options;
    private readonly 공식뉴스RssConditionalCache _cache;

    public 공식뉴스RssClient(
        HttpClient httpClient,
        IOptions<OfficialNewsRssOptions> options,
        공식뉴스RssConditionalCache cache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
    }

    public async Task<string> ReadAsync(
        공식뉴스RssFeedDefinition feed,
        CancellationToken cancellationToken)
    {
        _cache.TryGet(feed.FeedUrl, out var cached);
        using var request = new HttpRequestMessage(HttpMethod.Get, feed.FeedUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/rss+xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        if (cached?.ETag is not null
            && EntityTagHeaderValue.TryParse(cached.ETag, out var entityTag))
        {
            request.Headers.IfNoneMatch.Add(entityTag);
        }

        if (cached?.LastModified is not null)
        {
            request.Headers.IfModifiedSince = cached.LastModified;
        }

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotModified && cached is not null)
        {
            return cached.Xml;
        }

        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > _options.MaxResponseCharacters * 4L)
        {
            throw new InvalidDataException("공식뉴스 RSS 응답이 허용 크기를 초과했습니다.");
        }

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);
        if (xml.Length > Math.Clamp(_options.MaxResponseCharacters, 10_000, 5_000_000))
        {
            throw new InvalidDataException("공식뉴스 RSS 응답이 허용 크기를 초과했습니다.");
        }

        _cache.Set(feed.FeedUrl, new 공식뉴스RssCacheEntry(
            xml,
            response.Headers.ETag?.ToString(),
            response.Content.Headers.LastModified));
        return xml;
    }
}

public sealed class 공식뉴스RssCandidateSource : ICommunityInformationCandidateSource
{
    private static readonly Regex HtmlTagPattern = new(
        "<[^>]+>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    private readonly 공식뉴스RssFeedDefinition _feed;
    private readonly 공식뉴스RssClient _client;
    private readonly OfficialNewsRssOptions _options;
    private readonly TimeProvider _timeProvider;

    internal 공식뉴스RssCandidateSource(
        공식뉴스RssFeedDefinition feed,
        공식뉴스RssClient client,
        IOptions<OfficialNewsRssOptions> options,
        TimeProvider timeProvider)
    {
        _feed = feed;
        _client = client;
        _options = options.Value;
        _timeProvider = timeProvider;
        Source = new CommunityInformationSourceDto(
            feed.SourceKey,
            CommunityInformationSourceTypes.OfficialNews,
            feed.Provider,
            feed.DisplayName,
            CommunityInformationCollectionModes.OnDemandOfficialNewsQuery,
            "운영자 요청 시 공식 RSS 확인",
            "제목·300자 이하 요약·발행 시각·원문 링크만 검토 대기 후보로 반환하며 자동 게시하지 않습니다.",
            feed.DocumentationUrl,
            true);
    }

    public CommunityInformationSourceDto Source { get; }

    public async Task<IReadOnlyList<CommunityInformationCandidateDto>> ReadAsync(
        CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (!_options.Enabled)
        {
            return [];
        }

        var xml = await _client.ReadAsync(_feed, cancellationToken);
        return Parse(xml, Math.Clamp(query.Take, 1, Math.Clamp(_options.MaxItemsPerFeed, 1, 100)));
    }

    private IReadOnlyList<CommunityInformationCandidateDto> Parse(string xml, int take)
    {
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = Math.Clamp(_options.MaxResponseCharacters, 10_000, 5_000_000)
        });
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        var collectedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var candidates = new List<CommunityInformationCandidateDto>();

        foreach (var entry in document.Descendants()
                     .Where(element => element.Name.LocalName is "item" or "entry"))
        {
            var title = NormalizeText(ElementText(entry, "title"), 200);
            var summary = NormalizeText(
                ElementText(entry, "description", "summary", "content"),
                300);
            var originalUrl = NormalizeOfficialLink(ResolveLink(entry));
            if (title is null || originalUrl is null)
            {
                continue;
            }

            summary ??= title;
            var publishedAtUtc = ParseDateTime(
                ElementText(entry, "published", "updated", "pubDate", "date"));
            var sourceId = NormalizeText(ElementText(entry, "guid", "id"), 300)
                           ?? originalUrl;
            var topicTags = BuildTopicTags(entry, title, summary);

            candidates.Add(new CommunityInformationCandidateDto(
                BuildCandidateKey(sourceId),
                _feed.SourceKey,
                CommunityInformationSourceTypes.OfficialNews,
                _feed.Provider,
                title,
                summary,
                originalUrl,
                null,
                publishedAtUtc,
                publishedAtUtc.HasValue ? DateOnly.FromDateTime(publishedAtUtc.Value) : null,
                collectedAtUtc,
                "KR",
                "ko",
                null,
                null,
                CommunityInformationReviewStates.PendingReview,
                topicTags,
                $"{_feed.Provider} 공식 RSS에서 수집한 {_feed.FeedKind}의 제목·요약·원문 링크입니다.",
                "기사 내용은 운영자 검토 전이며, 제목만으로 가격·재고·공급 가능성·지역 사실을 확정하지 않습니다.",
                SourceFeedUrl: _feed.FeedUrl));

            if (candidates.Count >= take)
            {
                break;
            }
        }

        return candidates
            .GroupBy(candidate => candidate.CandidateKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(candidate => candidate.PublishedAtUtc ?? candidate.CollectedAtUtc)
            .ToArray();
    }

    private IReadOnlyList<string> BuildTopicTags(XElement entry, string title, string summary)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "공식뉴스",
            _feed.FeedKind
        };
        foreach (var category in entry.Elements()
                     .Where(element => element.Name.LocalName is "category" or "tag")
                     .Select(element => NormalizeText(element.Value, 80))
                     .Where(value => value is not null)
                     .Cast<string>())
        {
            tags.Add(category);
        }

        var searchable = $"{title} {summary}";
        AddTopic(searchable, tags, "회수", "위해·회수");
        AddTopic(searchable, tags, "안전", "식품안전");
        AddTopic(searchable, tags, "가격", "가격동향");
        AddTopic(searchable, tags, "수입", "수입·통관");
        AddTopic(searchable, tags, "수출", "수출·통관");
        AddTopic(searchable, tags, "지역", "지역문화");
        AddTopic(searchable, tags, "축제", "지역문화");
        return tags.Take(10).ToArray();
    }

    private static void AddTopic(
        string searchable,
        ISet<string> tags,
        string keyword,
        string tag)
    {
        if (searchable.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            tags.Add(tag);
        }
    }

    private string? NormalizeOfficialLink(string? value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || !_feed.AllowedArticleHosts.Any(host =>
                string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith($".{host}", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        return new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri.AbsoluteUri;
    }

    private string BuildCandidateKey(string sourceId)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sourceId)))
            .ToLowerInvariant();
        return $"{_feed.SourceKey}:{hash}";
    }

    private static string? ResolveLink(XElement entry)
    {
        var link = entry.Elements()
            .FirstOrDefault(element => element.Name.LocalName == "link");
        return link?.Attribute("href")?.Value ?? link?.Value;
    }

    private static string? ElementText(XElement element, params string[] names)
        => element.Elements()
            .FirstOrDefault(child => names.Contains(
                child.Name.LocalName,
                StringComparer.OrdinalIgnoreCase))?
            .Value;

    private static string? NormalizeText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var decoded = WebUtility.HtmlDecode(HtmlTagPattern.Replace(value, " "));
        var normalized = string.Join(
            ' ',
            decoded.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength].TrimEnd();
    }

    private static DateTime? ParseDateTime(string? value)
        => DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var timestamp)
            ? timestamp.UtcDateTime
            : null;
}
