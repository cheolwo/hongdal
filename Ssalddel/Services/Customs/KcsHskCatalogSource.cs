using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Ssalddel.Domain.HsCodes;

namespace Ssalddel.Services.Customs;

internal interface IKcsHskCatalogSource
{
    Task<KcsHskCatalogSnapshot> FetchFoodScopeAsync(
        int year,
        IReadOnlyCollection<int> chapters,
        int requestDelayMilliseconds,
        CancellationToken cancellationToken = default);
}

internal sealed class KcsHskCatalogSource(
    HttpClient httpClient,
    ILogger<KcsHskCatalogSource> logger) : IKcsHskCatalogSource
{
    public const string SourceName = "관세청 관세법령정보포털(CLIP) 관세율표";
    public const string SourceUrl = "https://unipass.customs.go.kr/clip/hsinfosrch/openULS0201002Q.do?cntyCd=KR";

    public async Task<KcsHskCatalogSnapshot> FetchFoodScopeAsync(
        int year,
        IReadOnlyCollection<int> chapters,
        int requestDelayMilliseconds,
        CancellationToken cancellationToken = default)
    {
        var normalizedChapters = chapters
            .Distinct()
            .OrderBy(chapter => chapter)
            .ToArray();
        if (normalizedChapters.Length == 0)
        {
            throw new ArgumentException("가져올 HSK 류를 하나 이상 지정해야 합니다.", nameof(chapters));
        }

        var delay = Math.Clamp(requestDelayMilliseconds, 0, 5000);
        var chapterIndexHtml = await PostAsync(
            "hsinfosrch/retrieveBscsLst.do",
            CreateForm(year),
            cancellationToken);
        var chapterIndex = KcsHskCatalogHtmlParser.ParseChapterIndex(chapterIndexHtml, year);
        var entries = new Dictionary<string, KcsHskCatalogSourceEntry>(StringComparer.Ordinal);
        var requestCount = 1;

        foreach (var chapter in normalizedChapters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chapterCode = chapter.ToString("00", CultureInfo.InvariantCulture);
            var chapterName = chapterIndex.Chapters
                .FirstOrDefault(item => item.Code == chapterCode)?.KoreanName;
            if (string.IsNullOrWhiteSpace(chapterName))
            {
                throw new InvalidOperationException($"관세청 응답에서 제{chapterCode}류를 찾지 못했습니다.");
            }

            AddOrMerge(entries, new KcsHskCatalogSourceEntry(
                chapterCode,
                chapterName,
                string.Empty,
                HsCodeLevel.Chapter));

            await DelayAsync(delay, cancellationToken);
            var headingHtml = await PostAsync(
                "hsinfosrch/openULS0201004Q.do",
                CreateForm(year, chapterIndex, chapterCode),
                cancellationToken);
            requestCount++;
            var headings = KcsHskCatalogHtmlParser.ParseHeadings(headingHtml)
                .Where(heading => heading.Code.StartsWith(chapterCode, StringComparison.Ordinal))
                .Where(heading => chapter != 25 || heading.Code == "2501")
                .ToArray();
            if (headings.Length == 0)
            {
                throw new InvalidOperationException($"관세청 응답에서 제{chapterCode}류의 HSK 호를 찾지 못했습니다.");
            }

            foreach (var heading in headings)
            {
                AddOrMerge(entries, heading);
                await DelayAsync(delay, cancellationToken);
                var detailHtml = await PostAsync(
                    "hsinfosrch/openULS0201005Q.do",
                    CreateForm(year, chapterIndex, heading.Code),
                    cancellationToken);
                requestCount++;
                var detailEntries = KcsHskCatalogHtmlParser.ParseHeadingDetails(detailHtml)
                    .Where(entry => entry.Code.StartsWith(heading.Code, StringComparison.Ordinal));
                foreach (var detailEntry in detailEntries)
                {
                    AddOrMerge(entries, detailEntry);
                }
            }

            logger.LogInformation(
                "관세청 HSK 식품 범위 수집 진행. Chapter={Chapter}, Headings={HeadingCount}, Entries={EntryCount}",
                chapterCode,
                headings.Length,
                entries.Count);
        }

        if (!entries.Values.Any(entry => entry.Level == HsCodeLevel.National))
        {
            throw new InvalidOperationException("관세청 응답에 10자리 HSK 품목이 없어 카탈로그를 갱신하지 않았습니다.");
        }

        return new KcsHskCatalogSnapshot(
            year,
            chapterIndex.EffectiveFrom,
            entries.Values
                .OrderBy(entry => entry.Code, StringComparer.Ordinal)
                .ToArray(),
            requestCount,
            SourceName,
            SourceUrl);
    }

    private async Task<string> PostAsync(
        string relativeUrl,
        IReadOnlyDictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(form);
        using var response = await httpClient.PostAsync(relativeUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static Dictionary<string, string> CreateForm(
        int year,
        KcsHskChapterIndex? chapterIndex = null,
        string searchValue = "")
        => new(StringComparer.Ordinal)
        {
            ["aplyYy"] = year.ToString(CultureInfo.InvariantCulture),
            ["cntyCd"] = "KR",
            ["cntyNm"] = "한국",
            ["hsSgn"] = string.Empty,
            ["compareCrrspndNation"] = "KR",
            ["sctYear"] = chapterIndex?.SourceSectionDate ?? string.Empty,
            ["hstdYear"] = chapterIndex?.SourceChapterDate ?? string.Empty,
            ["tabTpcd"] = string.Empty,
            ["manlOrgnTpcd"] = string.Empty,
            ["searchVal"] = searchValue,
            ["aplyStrtDt"] = string.Empty,
            ["aplyEndDt"] = string.Empty
        };

    private static void AddOrMerge(
        IDictionary<string, KcsHskCatalogSourceEntry> entries,
        KcsHskCatalogSourceEntry candidate)
    {
        if (!entries.TryGetValue(candidate.Code, out var existing))
        {
            entries.Add(candidate.Code, candidate);
            return;
        }

        entries[candidate.Code] = existing with
        {
            KoreanName = Prefer(candidate.KoreanName, existing.KoreanName),
            EnglishName = Prefer(candidate.EnglishName, existing.EnglishName),
            Level = candidate.Level
        };
    }

    private static string Prefer(string candidate, string fallback)
        => string.IsNullOrWhiteSpace(candidate) ? fallback : candidate;

    private static Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
        => milliseconds <= 0
            ? Task.CompletedTask
            : Task.Delay(milliseconds, cancellationToken);
}

internal static class KcsHskCatalogHtmlParser
{
    private static readonly Regex RowRegex = CreateRegex("<tr\\b[^>]*>(?<body>.*?)</tr>");
    private static readonly Regex CellRegex = CreateRegex("<t[dh]\\b[^>]*>(?<body>.*?)</t[dh]>");
    private static readonly Regex InputRegex = CreateRegex("<input\\b(?<attributes>[^>]*)>");
    private static readonly Regex AttributeRegex = CreateRegex(
        "(?<name>[\\w:-]+)\\s*=\\s*(?:\"(?<double>[^\"]*)\"|'(?<single>[^']*)'|(?<bare>[^\\s>]+))");
    private static readonly Regex CommentRegex = CreateRegex("<!--.*?-->");
    private static readonly Regex TagRegex = CreateRegex("<[^>]+>");
    private static readonly Regex WhitespaceRegex = CreateRegex("\\s+");

    public static KcsHskChapterIndex ParseChapterIndex(string html, int fallbackYear)
    {
        var sourceSectionDate = FindInputValue(html, "sctYear") ?? $"{fallbackYear}0101";
        var sourceChapterDate = FindInputValue(html, "hstdYear") ?? sourceSectionDate;
        var effectiveFrom = DateTime.TryParseExact(
            sourceChapterDate,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsedEffectiveFrom)
            ? parsedEffectiveFrom
            : new DateTime(fallbackYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var chapters = new Dictionary<string, KcsHskChapter>(StringComparer.Ordinal);

        foreach (Match rowMatch in RowRegex.Matches(html))
        {
            var row = rowMatch.Value;
            var code = FindInputValue(row, "hstdCd");
            if (code is null || code.Length != 2 || !code.All(char.IsDigit))
            {
                continue;
            }

            var nameCell = CellRegex.Matches(row)
                .Cast<Match>()
                .FirstOrDefault(cell => cell.Value.Contains("no_line_r", StringComparison.OrdinalIgnoreCase));
            var koreanName = nameCell is null ? string.Empty : ToText(nameCell.Groups["body"].Value);
            if (!string.IsNullOrWhiteSpace(koreanName))
            {
                chapters[code] = new KcsHskChapter(code, koreanName);
            }
        }

        return new KcsHskChapterIndex(
            effectiveFrom,
            sourceSectionDate,
            sourceChapterDate,
            chapters.Values.OrderBy(chapter => chapter.Code, StringComparer.Ordinal).ToArray());
    }

    public static IReadOnlyList<KcsHskCatalogSourceEntry> ParseHeadings(string html)
    {
        var headings = new Dictionary<string, KcsHskCatalogSourceEntry>(StringComparer.Ordinal);
        foreach (Match rowMatch in RowRegex.Matches(html))
        {
            var row = rowMatch.Value;
            var code = FindInputValue(row, "hsfdCd_Mn");
            if (code is null || code.Length != 4 || !code.All(char.IsDigit))
            {
                continue;
            }

            var cells = GetCellTexts(row);
            if (cells.Count < 3)
            {
                continue;
            }

            headings[code] = new KcsHskCatalogSourceEntry(
                code,
                cells[1],
                cells[2],
                HsCodeLevel.Heading);
        }

        return headings.Values.OrderBy(heading => heading.Code, StringComparer.Ordinal).ToArray();
    }

    public static IReadOnlyList<KcsHskCatalogSourceEntry> ParseHeadingDetails(string html)
    {
        var entries = new Dictionary<string, KcsHskCatalogSourceEntry>(StringComparer.Ordinal);
        foreach (Match rowMatch in RowRegex.Matches(html))
        {
            var row = rowMatch.Value;
            var code = FindInputValue(row, "hsSgn_Mn");
            if (code is null || !code.All(char.IsDigit) || code.Length is not (4 or 6 or 10))
            {
                continue;
            }

            var cells = GetCellTexts(row);
            if (cells.Count < 5)
            {
                continue;
            }

            var level = code.Length switch
            {
                4 => HsCodeLevel.Heading,
                6 => HsCodeLevel.Subheading,
                10 => HsCodeLevel.National,
                _ => throw new InvalidOperationException("지원하지 않는 HSK 자릿수입니다.")
            };
            entries[code] = new KcsHskCatalogSourceEntry(code, cells[3], cells[4], level);
        }

        return entries.Values.OrderBy(entry => entry.Code, StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> GetCellTexts(string html)
        => CellRegex.Matches(html)
            .Cast<Match>()
            .Select(match => ToText(match.Groups["body"].Value))
            .ToArray();

    private static string? FindInputValue(string html, string expectedName)
    {
        foreach (Match inputMatch in InputRegex.Matches(html))
        {
            var attributes = ParseAttributes(inputMatch.Groups["attributes"].Value);
            if (attributes.TryGetValue("name", out var name)
                && string.Equals(name, expectedName, StringComparison.OrdinalIgnoreCase)
                && attributes.TryGetValue("value", out var value))
            {
                return WebUtility.HtmlDecode(value).Trim();
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string> ParseAttributes(string value)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in AttributeRegex.Matches(value))
        {
            var attributeValue = match.Groups["double"].Success
                ? match.Groups["double"].Value
                : match.Groups["single"].Success
                    ? match.Groups["single"].Value
                    : match.Groups["bare"].Value;
            attributes[match.Groups["name"].Value] = attributeValue;
        }

        return attributes;
    }

    private static string ToText(string html)
    {
        var withoutComments = CommentRegex.Replace(html, " ");
        var withoutTags = TagRegex.Replace(withoutComments, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags).Replace('\u3000', ' ');
        return WhitespaceRegex.Replace(decoded, " ").Trim();
    }

    private static Regex CreateRegex(string pattern)
        => new(
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(3));
}

internal sealed record KcsHskChapterIndex(
    DateTime EffectiveFrom,
    string SourceSectionDate,
    string SourceChapterDate,
    IReadOnlyList<KcsHskChapter> Chapters);

internal sealed record KcsHskChapter(string Code, string KoreanName);

internal sealed record KcsHskCatalogSourceEntry(
    string Code,
    string KoreanName,
    string EnglishName,
    HsCodeLevel Level);

internal sealed record KcsHskCatalogSnapshot(
    int Year,
    DateTime EffectiveFrom,
    IReadOnlyList<KcsHskCatalogSourceEntry> Entries,
    int RequestCount,
    string SourceName,
    string SourceUrl);
