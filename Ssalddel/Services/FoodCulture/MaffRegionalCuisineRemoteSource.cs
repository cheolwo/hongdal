using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Services.FoodCulture;

public sealed partial class MaffRegionalCuisineRemoteSource : IOfficialFoodRecipeRemoteSource
{
    private const string RecipePathPrefix = "/e/policies/market/k_ryouri/search_menu/";
    private readonly HttpClient _httpClient;
    private readonly MaffRegionalCuisineOptions _options;

    public MaffRegionalCuisineRemoteSource(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.MaffRegionalCuisine;
    }

    public string SourceKey => OfficialFoodRecipeSourceKeys.MaffRegionalCuisine;

    public async Task<IReadOnlyList<OfficialFoodRecipeCollectedRecord>> FetchAsync(
        int maxPages,
        int maxItems,
        CancellationToken cancellationToken = default)
    {
        var indexUri = new Uri(_httpClient.BaseAddress!, _options.IndexPath);
        var indexPage = await GetPageAsync(indexUri, cancellationToken);
        var prefectureUris = ExtractLinks(indexPage.Html, indexUri)
            .Where(uri => uri.AbsolutePath.Contains(
                "/search_menu/pref/",
                StringComparison.OrdinalIgnoreCase))
            .DistinctBy(uri => uri.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
            .Take(maxPages)
            .ToArray();
        if (prefectureUris.Length == 0
            && indexUri.AbsolutePath.Contains("/search_menu/pref/", StringComparison.OrdinalIgnoreCase))
        {
            prefectureUris = [indexUri];
        }

        var records = new List<OfficialFoodRecipeCollectedRecord>();
        foreach (var prefectureUri in prefectureUris)
        {
            if (records.Count >= maxItems)
            {
                break;
            }

            var prefecturePage = prefectureUri == indexUri
                ? indexPage
                : await GetPageAsync(prefectureUri, cancellationToken);
            var recipeUris = ExtractLinks(prefecturePage.Html, prefectureUri)
                .Where(IsRecipeDetailUri)
                .DistinctBy(uri => uri.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var recipeUri in recipeUris)
            {
                if (records.Count >= maxItems)
                {
                    break;
                }

                var recipePage = await GetPageAsync(recipeUri, cancellationToken);
                var record = ParseRecipe(recipeUri, recipePage);
                if (record is not null)
                {
                    records.Add(record);
                }
            }
        }

        return records;
    }

    internal static OfficialFoodRecipeCollectedRecord? ParseRecipe(
        Uri pageUri,
        FetchedHtmlPage page)
    {
        var idMatch = RecipeIdRegex().Match(pageUri.AbsolutePath);
        var externalId = idMatch.Success ? idMatch.Groups["id"].Value : string.Empty;
        var title = DecodeAndClean(RecipeTitleRegex().Match(page.Html).Groups["value"].Value);
        if (string.IsNullOrWhiteSpace(externalId) || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var region = DecodeAndClean(PrefectureRegex().Match(page.Html).Groups["value"].Value)
            .Replace(" Prefecture", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
        var summary = ExtractHeadingParagraph(page.Html, "History/origin/related events");
        var loreArea = ExtractHeadingParagraph(page.Html, "Main lore areas");
        var mainIngredients = ExtractHeadingParagraph(page.Html, "Main ingredients used");
        var servingText = DecodeAndClean(ServingRegex().Match(page.Html).Groups["value"].Value)
            .Replace("Ingredients", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
        var ingredientBlock = MaterialListRegex().Match(page.Html).Groups["value"].Value;
        var ingredients = ListItemRegex().Matches(ingredientBlock)
            .Select(match => DecodeAndClean(match.Groups["value"].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        var instructionBlock = HowToListRegex().Match(page.Html).Groups["value"].Value;
        var instructions = ListItemRegex().Matches(instructionBlock)
            .Select(match => DecodeAndClean(match.Groups["value"].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        var tips = DecodeAndClean(RecipeNotesRegex().Match(page.Html).Groups["value"].Value);
        var tags = new[] { region, loreArea, mainIngredients, "Japanese regional cuisine" }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var imageReference = ResolveFirstImage(page.Html, pageUri);

        return new OfficialFoodRecipeCollectedRecord(
            externalId,
            title,
            title,
            title,
            summary,
            region,
            "Regional cuisine",
            servingText,
            ingredients,
            instructions,
            new Dictionary<string, string>(),
            tags,
            tips,
            pageUri.AbsoluteUri,
            imageReference,
            page.Html,
            page.LastModifiedUtc);
    }

    private async Task<FetchedHtmlPage> GetPageAsync(
        Uri uri,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        return new FetchedHtmlPage(
            html,
            response.Content.Headers.LastModified?.UtcDateTime);
    }

    private IEnumerable<Uri> ExtractLinks(string html, Uri baseUri)
    {
        foreach (Match match in LinkRegex().Matches(html))
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value).Trim();
            if (!Uri.TryCreate(baseUri, href, out var uri)
                || !string.Equals(uri.Host, _httpClient.BaseAddress!.Host, StringComparison.OrdinalIgnoreCase)
                || !uri.AbsolutePath.StartsWith(RecipePathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            yield return uri;
        }
    }

    private static bool IsRecipeDetailUri(Uri uri)
        => RecipeIdRegex().IsMatch(uri.AbsolutePath);

    private static string ExtractHeadingParagraph(string html, string heading)
    {
        var pattern = $"<h3[^>]*>.*?{Regex.Escape(heading)}.*?</h3>\\s*<p[^>]*>(?<value>.*?)</p>";
        var match = Regex.Match(
            html,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        return DecodeAndClean(match.Groups["value"].Value);
    }

    private static string ResolveFirstImage(string html, Uri pageUri)
    {
        var match = MainImageRegex().Match(html);
        var source = WebUtility.HtmlDecode(match.Groups["src"].Value).Trim();
        return Uri.TryCreate(pageUri, source, out var imageUri)
               && imageUri.Scheme is "http" or "https"
            ? imageUri.AbsoluteUri
            : string.Empty;
    }

    private static string DecodeAndClean(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var withoutScripts = ScriptStyleRegex().Replace(value, " ");
        var withLineBreaks = BreakRegex().Replace(withoutScripts, "\n");
        var withoutTags = TagRegex().Replace(withLineBreaks, " ");
        var decoded = WebUtility.HtmlDecode(WebUtility.HtmlDecode(withoutTags));
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    public sealed record FetchedHtmlPage(string Html, DateTime? LastModifiedUtc);

    [GeneratedRegex("href\\s*=\\s*[\"'](?<href>[^\"']+)[\"']", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LinkRegex();

    [GeneratedRegex("/search_menu/(?<id>[0-9]+)/index\\.html$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RecipeIdRegex();

    [GeneratedRegex("<h2[^>]*class=[\"'][^\"']*tit06[^\"']*[\"'][^>]*>\\s*<span[^>]*class=[\"']name[\"'][^>]*>(?<value>.*?)</span>\\s*</h2>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex RecipeTitleRegex();

    [GeneratedRegex("<h2[^>]*class=[\"'][^\"']*-prefecture[^\"']*[\"'][^>]*>.*?<span[^>]*class=[\"'][^\"']*pref[^\"']*[\"'][^>]*>(?<value>.*?)</span>\\s*</h2>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex PrefectureRegex();

    [GeneratedRegex("<h4[^>]*>\\s*Ingredients(?<value>.*?)</h4>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ServingRegex();

    [GeneratedRegex("<div[^>]*class=[\"'][^\"']*material-list[^\"']*[\"'][^>]*>(?<value>.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex MaterialListRegex();

    [GeneratedRegex("<div[^>]*class=[\"'][^\"']*howto-list[^\"']*[\"'][^>]*>(?<value>.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex HowToListRegex();

    [GeneratedRegex("<li[^>]*>(?<value>.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ListItemRegex();

    [GeneratedRegex("<div[^>]*class=[\"'][^\"']*recipe-notes[^\"']*[\"'][^>]*>(?<value>.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex RecipeNotesRegex();

    [GeneratedRegex("<h2[^>]*>\\s*<img[^>]*src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex MainImageRegex();

    [GeneratedRegex("<(script|style)[^>]*>.*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ScriptStyleRegex();

    [GeneratedRegex("<br\\s*/?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BreakRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex TagRegex();

    [GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
