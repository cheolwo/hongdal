using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record UsdaAms공개사업체원본Row(
    string DirectoryTypeCode,
    string ExternalListingId,
    string BusinessName,
    string LocationAddress,
    int? EstablishedYear,
    string LegalStatus,
    IReadOnlyList<string> Products,
    bool HasRetailChannel,
    bool HasWholesaleChannel,
    bool HasProducerService,
    bool HasProcurementService,
    DateTime? SourceUpdatedAt);

public interface IUsdaAms공개사업체DirectoryClient
{
    Task<IReadOnlyList<UsdaAms공개사업체원본Row>> GetDirectoryAsync(
        string directoryTypeCode,
        CancellationToken cancellationToken = default);
}

public sealed class UsdaAms공개사업체DirectoryClient(
    HttpClient httpClient,
    IOptions<PublicDataOptions> options)
    : IUsdaAms공개사업체DirectoryClient
{
    private readonly UsdaAmsLocalFoodDirectoryOptions _options =
        options.Value.UsdaAmsLocalFoodDirectory;

    public async Task<IReadOnlyList<UsdaAms공개사업체원본Row>> GetDirectoryAsync(
        string directoryTypeCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedDirectoryType =
            UsdaAms공개사업체DirectoryCatalog.Normalize(directoryTypeCode);
        var directorySlug =
            UsdaAms공개사업체DirectoryCatalog.GetSlug(normalizedDirectoryType);
        var path =
            $"{_options.BulkDownloadPath}?directory={Uri.EscapeDataString(directorySlug)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Referrer = new Uri(_options.DataSharingUrl);
        request.Headers.TryAddWithoutValidation(
            "Origin",
            new Uri(_options.BaseUrl).GetLeftPart(UriPartial.Authority));
        request.Headers.TryAddWithoutValidation(
            "X-Requested-With",
            "XMLHttpRequest");
        request.Headers.TryAddWithoutValidation(
            "Accept",
            "application/json, text/javascript, */*; q=0.01");

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"USDA AMS Local Food Directory {directorySlug} 응답이 배열 형식이 아닙니다.");
        }

        return document.RootElement
            .EnumerateArray()
            .Select(row => ReadRow(row, normalizedDirectoryType))
            .ToArray();
    }

    private static UsdaAms공개사업체원본Row ReadRow(
        JsonElement row,
        string directoryTypeCode)
        => new(
            directoryTypeCode,
            Read(row, "listing_id"),
            Read(row, "listing_name"),
            Read(row, "location_address"),
            ReadYear(row, "establish_year"),
            Read(row, "legal_status"),
            ReadProducts(row),
            HasValue(row, "saleschannel_retail"),
            HasValue(row, "saleschannel_wholesale"),
            HasValue(row, "service_producer"),
            HasValue(row, "service_procurement"),
            ReadDateTime(row, "update_time"));

    private static IReadOnlyList<string> ReadProducts(JsonElement row)
    {
        var products = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDelimited(products, Read(row, "products"));

        if (row.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in row.EnumerateObject())
            {
                if (!property.Name.StartsWith(
                        "productslocality_",
                        StringComparison.OrdinalIgnoreCase)
                    || ReadValue(property.Value).Length == 0)
                {
                    continue;
                }

                var suffix = property.Name["productslocality_".Length..];
                if (ProductNames.TryGetValue(suffix, out var displayName))
                {
                    products.Add(displayName);
                }
            }
        }

        return products
            .Where(value => value.Length is > 0 and <= 300)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddDelimited(ISet<string> target, string value)
    {
        foreach (var item in value.Split(
                     [';', '|'],
                     StringSplitOptions.RemoveEmptyEntries
                     | StringSplitOptions.TrimEntries))
        {
            var normalized = item.Trim();
            if (normalized.Length is > 0 and <= 300)
            {
                target.Add(normalized);
            }
        }
    }

    private static bool HasValue(JsonElement row, string propertyName)
        => Read(row, propertyName).Length > 0;

    private static int? ReadYear(JsonElement row, string propertyName)
        => int.TryParse(
               Read(row, propertyName),
               NumberStyles.None,
               CultureInfo.InvariantCulture,
               out var year)
           && year is >= 1600 and <= 2200
            ? year
            : null;

    private static DateTime? ReadDateTime(JsonElement row, string propertyName)
        => DateTime.TryParse(
            Read(row, propertyName),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var result)
            ? DateTime.SpecifyKind(result, DateTimeKind.Unspecified)
            : null;

    private static string Read(JsonElement row, string propertyName)
        => row.ValueKind == JsonValueKind.Object
           && row.TryGetProperty(propertyName, out var value)
            ? ReadValue(value)
            : string.Empty;

    private static string ReadValue(JsonElement value)
        => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False =>
                value.ToString().Trim(),
            _ => string.Empty
        };

    private static readonly IReadOnlyDictionary<string, string> ProductNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["bakedgoods"] = "Baked goods",
            ["beans"] = "Beans",
            ["canfood"] = "Canned or preserved foods",
            ["coffee"] = "Coffee or tea",
            ["dairyproducts"] = "Dairy products",
            ["eggs"] = "Eggs",
            ["flowers"] = "Flowers",
            ["fruits"] = "Fresh fruits",
            ["grains"] = "Grains",
            ["greenproducts"] = "Green household products",
            ["herbs"] = "Herbs",
            ["honey"] = "Honey",
            ["juices"] = "Juices or non-alcoholic cider",
            ["mapleproducts"] = "Maple products",
            ["meat"] = "Poultry or fowl meat and products",
            ["mushrooms"] = "Mushrooms",
            ["nuts"] = "Nuts",
            ["other"] = "Other products",
            ["petfood"] = "Pet food",
            ["redmeat"] = "Red or other non-poultry meat and products",
            ["seafood"] = "Seafood",
            ["soap"] = "Soap or body care products",
            ["tofu"] = "Tofu or soy products",
            ["trees"] = "Trees or nursery products",
            ["vegetables"] = "Fresh vegetables",
            ["wildproducts"] = "Wild harvested products",
            ["wine"] = "Wine or alcoholic cider"
        };
}
