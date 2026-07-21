using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Services.FoodCulture;

public sealed partial class NhsHealthierFamiliesRecipeRemoteSource : IOfficialFoodRecipeRemoteSource
{
    private const string RecipePathPrefix = "/healthier-families/recipes/";
    private readonly HttpClient _httpClient;
    private readonly NhsHealthierFamiliesRecipeOptions _options;
    private readonly TimeProvider _timeProvider;

    public NhsHealthierFamiliesRecipeRemoteSource(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        TimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _options = options.Value.NhsHealthierFamiliesRecipes;
        _timeProvider = timeProvider;
    }

    public string SourceKey => OfficialFoodRecipeSourceKeys.NhsHealthierFamilies;

    public async Task<IReadOnlyList<OfficialFoodRecipeCollectedRecord>> FetchAsync(
        int maxPages,
        int maxItems,
        CancellationToken cancellationToken = default)
    {
        _ = maxPages;
        var indexUri = new Uri(_httpClient.BaseAddress!, _options.IndexPath);
        var indexPage = await GetPageAsync(indexUri, cancellationToken);
        var recipeUris = ExtractRecipeLinks(indexPage.Html, indexUri)
            .DistinctBy(uri => uri.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
            .Take(maxItems)
            .ToArray();

        var records = new List<OfficialFoodRecipeCollectedRecord>(recipeUris.Length);
        foreach (var recipeUri in recipeUris)
        {
            var page = await GetPageAsync(recipeUri, cancellationToken);
            var record = ParseRecipe(
                recipeUri,
                page,
                _timeProvider.GetUtcNow().UtcDateTime.AddDays(7));
            if (record is not null)
            {
                records.Add(record);
            }
        }

        return records;
    }

    internal static OfficialFoodRecipeCollectedRecord? ParseRecipe(
        Uri pageUri,
        FetchedHtmlPage page,
        DateTime contentExpiresAtUtc)
    {
        var externalId = pageUri.AbsolutePath
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault()
            ?? string.Empty;
        var recipe = FindRecipeJsonLd(page.Html);
        if (recipe is null)
        {
            return null;
        }

        using (recipe)
        {
            var root = recipe.RootElement;
            var name = Clean(ReadString(root, "name"));
            if (string.IsNullOrWhiteSpace(externalId) || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var ingredients = ReadStringArray(root, "recipeIngredient");
            var instructions = ExtractMethodSteps(page.Html);
            if (instructions.Count == 0)
            {
                instructions = ReadInstructions(root);
            }

            var description = Clean(ReadString(root, "description"));
            var nutrition = ExtractNutrition(page.Html);
            var servingText = ExtractServingText(page.Html);
            var tags = new[] { "healthy family recipe", "NHS England" };

            return new OfficialFoodRecipeCollectedRecord(
                externalId,
                name,
                name,
                name,
                description,
                "England",
                "Healthy family recipe",
                servingText,
                ingredients,
                instructions,
                nutrition,
                tags,
                string.Empty,
                pageUri.AbsoluteUri,
                string.Empty,
                page.Html,
                page.LastModifiedUtc,
                DateTime.SpecifyKind(contentExpiresAtUtc, DateTimeKind.Utc));
        }
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

    private IEnumerable<Uri> ExtractRecipeLinks(string html, Uri baseUri)
    {
        foreach (Match match in LinkRegex().Matches(html))
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value).Trim();
            if (!Uri.TryCreate(baseUri, href, out var uri)
                || !string.Equals(uri.Host, _httpClient.BaseAddress!.Host, StringComparison.OrdinalIgnoreCase)
                || !uri.AbsolutePath.StartsWith(RecipePathPrefix, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.AbsolutePath, RecipePathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = uri.AbsolutePath[RecipePathPrefix.Length..].Trim('/');
            if (remainder.Length > 0 && !remainder.Contains('/'))
            {
                yield return uri;
            }
        }
    }

    private static JsonDocument? FindRecipeJsonLd(string html)
    {
        foreach (Match match in JsonLdRegex().Matches(html))
        {
            try
            {
                using var candidate = JsonDocument.Parse(match.Groups["json"].Value);
                var recipeElement = FindRecipeElement(candidate.RootElement);
                if (recipeElement.HasValue)
                {
                    return JsonDocument.Parse(recipeElement.Value.GetRawText());
                }
            }
            catch (JsonException)
            {
                // 다른 구조화 데이터 script가 손상되어도 다음 script를 계속 확인합니다.
            }
        }

        return null;
    }

    private static JsonElement? FindRecipeElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (IsRecipeType(element))
            {
                return element;
            }

            foreach (var property in element.EnumerateObject())
            {
                var nested = FindRecipeElement(property.Value);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindRecipeElement(item);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static bool IsRecipeType(JsonElement element)
    {
        if (!TryGetProperty(element, "@type", out var type))
        {
            return false;
        }

        return type.ValueKind switch
        {
            JsonValueKind.String => string.Equals(
                type.GetString(),
                "Recipe",
                StringComparison.OrdinalIgnoreCase),
            JsonValueKind.Array => type.EnumerateArray().Any(item =>
                item.ValueKind == JsonValueKind.String
                && string.Equals(item.GetString(), "Recipe", StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    private static IReadOnlyList<string> ExtractMethodSteps(string html)
    {
        var block = MethodRegex().Match(html).Groups["value"].Value;
        return ListItemRegex().Matches(block)
            .Select(match => Clean(match.Groups["value"].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static IReadOnlyList<string> ReadInstructions(JsonElement recipe)
    {
        if (!TryGetProperty(recipe, "recipeInstructions", out var value))
        {
            return [];
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var instruction = Clean(value.GetString() ?? string.Empty);
            return string.IsNullOrWhiteSpace(instruction) ? [] : [instruction];
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String
                ? Clean(item.GetString() ?? string.Empty)
                : Clean(ReadString(item, "text")))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string> ExtractNutrition(string html)
    {
        var block = NutritionRegex().Match(html).Groups["value"].Value;
        var basis = Clean(ParagraphRegex().Match(block).Groups["value"].Value).TrimEnd(':');
        var values = ListItemRegex().Matches(block)
            .Select(match => Clean(match.Groups["value"].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(basis))
        {
            result["basis"] = basis;
        }

        for (var index = 0; index < values.Length; index++)
        {
            result[$"item_{index + 1}"] = values[index];
        }

        return result;
    }

    private static string ExtractServingText(string html)
    {
        var description = Clean(DescriptionRegex().Match(html).Groups["value"].Value);
        var match = Regex.Match(
            description,
            "(?:Makes|Serves)\\s+[^.]+",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Value.Trim() : string.Empty;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out var value))
        {
            return [];
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => Clean(item.GetString() ?? string.Empty))
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
        }

        var single = value.ValueKind == JsonValueKind.String
            ? Clean(value.GetString() ?? string.Empty)
            : string.Empty;
        return string.IsNullOrWhiteSpace(single) ? [] : [single];
    }

    private static string ReadString(JsonElement element, string name)
        => TryGetProperty(element, name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static bool TryGetProperty(
        JsonElement element,
        string name,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string Clean(string value)
    {
        var withLineBreaks = BreakRegex().Replace(value, " ");
        var withoutTags = TagRegex().Replace(withLineBreaks, " ");
        var decoded = WebUtility.HtmlDecode(WebUtility.HtmlDecode(withoutTags));
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    public sealed record FetchedHtmlPage(string Html, DateTime? LastModifiedUtc);

    [GeneratedRegex("href\\s*=\\s*[\"'](?<href>[^\"']+)[\"']", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LinkRegex();

    [GeneratedRegex("<script[^>]*type=[\"']application/ld\\+json[\"'][^>]*>(?<json>.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex JsonLdRegex();

    [GeneratedRegex("<h2[^>]*>\\s*Method\\s*</h2>\\s*<ol[^>]*>(?<value>.*?)</ol>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex MethodRegex();

    [GeneratedRegex("<li[^>]*>(?<value>.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ListItemRegex();

    [GeneratedRegex("Nutritional information.*?<div[^>]*class=[\"'][^\"']*nhsuk-details__text[^\"']*[\"'][^>]*>(?<value>.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex NutritionRegex();

    [GeneratedRegex("<p[^>]*>(?<value>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ParagraphRegex();

    [GeneratedRegex("<div[^>]*class=[\"'][^\"']*bh-recipe__description[^\"']*[\"'][^>]*>(?<value>.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex DescriptionRegex();

    [GeneratedRegex("<br\\s*/?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BreakRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex TagRegex();

    [GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
