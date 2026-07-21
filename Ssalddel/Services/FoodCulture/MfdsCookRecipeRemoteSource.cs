using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Services.FoodCulture;

public sealed class MfdsCookRecipeRemoteSource : IOfficialFoodRecipeRemoteSource
{
    private const string DocumentationUrl =
        "https://www.foodsafetykorea.go.kr/api/openApiInfo.do?menu_grp=MENU_GRP31&menu_no=661&show_cnt=10&start_idx=1&svc_no=COOKRCP01";
    private readonly HttpClient _httpClient;
    private readonly MfdsCookRecipeOptions _options;

    public MfdsCookRecipeRemoteSource(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.MfdsCookRecipe;
    }

    public string SourceKey => OfficialFoodRecipeSourceKeys.MfdsCookRecipe;

    public async Task<IReadOnlyList<OfficialFoodRecipeCollectedRecord>> FetchAsync(
        int maxPages,
        int maxItems,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "PublicData:MfdsCookRecipe:ApiKey가 필요합니다. 키는 appsettings.Local.json 또는 환경 변수에만 설정하세요.");
        }

        var pageSize = Math.Clamp(_options.PageSize, 1, 1000);
        var serviceId = string.IsNullOrWhiteSpace(_options.ServiceId)
            ? "COOKRCP01"
            : _options.ServiceId.Trim();
        var records = new List<OfficialFoodRecipeCollectedRecord>();
        var totalCount = int.MaxValue;

        for (var page = 0;
             page < maxPages && records.Count < maxItems && page * pageSize < totalCount;
             page++)
        {
            var startIndex = page * pageSize + 1;
            var endIndex = Math.Min(startIndex + pageSize - 1, startIndex + maxItems - records.Count - 1);
            var path = string.Join(
                '/',
                "api",
                Uri.EscapeDataString(_options.ApiKey.Trim()),
                Uri.EscapeDataString(serviceId),
                "json",
                startIndex.ToString(CultureInfo.InvariantCulture),
                endIndex.ToString(CultureInfo.InvariantCulture));

            using var response = await SendAsync(path, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;
            if (!TryGetProperty(root, serviceId, out var payload))
            {
                throw CreateApiException(root);
            }

            totalCount = ReadInt(payload, "total_count") ?? 0;
            if (!TryGetProperty(payload, "row", out var rows)
                || rows.ValueKind != JsonValueKind.Array)
            {
                break;
            }

            foreach (var row in rows.EnumerateArray())
            {
                if (records.Count >= maxItems)
                {
                    break;
                }

                var item = ToRecord(row);
                if (item is not null)
                {
                    records.Add(item);
                }
            }
        }

        return records;
    }

    internal static OfficialFoodRecipeCollectedRecord? ToRecord(JsonElement row)
    {
        var externalId = ReadString(row, "RCP_SEQ");
        var name = ReadString(row, "RCP_NM");
        if (string.IsNullOrWhiteSpace(externalId) || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var cookingMethod = ReadString(row, "RCP_WAY2");
        var category = ReadString(row, "RCP_PAT2");
        var ingredients = SplitLines(ReadString(row, "RCP_PARTS_DTLS"));
        var instructions = Enumerable.Range(1, 20)
            .Select(index => ReadString(row, $"MANUAL{index:00}"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();
        var nutrition = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddNutrition(nutrition, "serving_weight_g", ReadString(row, "INFO_WGT"));
        AddNutrition(nutrition, "energy_kcal", ReadString(row, "INFO_ENG"));
        AddNutrition(nutrition, "carbohydrate_g", ReadString(row, "INFO_CAR"));
        AddNutrition(nutrition, "protein_g", ReadString(row, "INFO_PRO"));
        AddNutrition(nutrition, "fat_g", ReadString(row, "INFO_FAT"));
        AddNutrition(nutrition, "sodium_mg", ReadString(row, "INFO_NA"));

        var tags = new[]
            {
                ReadString(row, "HASH_TAG"),
                category,
                cookingMethod,
                "식약처 레시피"
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var summary = string.Join(
            " · ",
            new[] { category, cookingMethod }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new OfficialFoodRecipeCollectedRecord(
            externalId.Trim(),
            name.Trim(),
            name.Trim(),
            string.Empty,
            summary,
            "대한민국",
            category,
            ReadString(row, "INFO_WGT"),
            ingredients,
            instructions,
            nutrition,
            tags,
            ReadString(row, "RCP_NA_TIP"),
            DocumentationUrl,
            FirstNonEmpty(
                ReadString(row, "ATT_FILE_NO_MAIN"),
                ReadString(row, "ATT_FILE_NO_MK")),
            row.GetRawText(),
            ParseSourceDate(ReadString(row, "CHNG_DT")));
    }

    private async Task<HttpResponseMessage> SendAsync(
        string path,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await _httpClient.GetAsync(
                path,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (attempt >= 3 || !IsTransient(response.StatusCode))
            {
                response.EnsureSuccessStatusCode();
                return response;
            }

            response.Dispose();
            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
        }
    }

    private static Exception CreateApiException(JsonElement root)
    {
        if (TryGetProperty(root, "RESULT", out var result))
        {
            var code = ReadString(result, "CODE");
            var message = ReadString(result, "MSG");
            return new InvalidOperationException(
                $"식품안전나라 COOKRCP01 응답 오류입니다. Code={code}, Message={message}");
        }

        return new InvalidOperationException(
            "식품안전나라 COOKRCP01 응답에서 서비스 payload를 찾지 못했습니다.");
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

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

    private static string ReadString(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out var value))
        {
            return string.Empty;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            _ => string.Empty
        };
    }

    private static int? ReadInt(JsonElement element, string name)
        => int.TryParse(
            ReadString(element, name),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : null;

    private static IReadOnlyList<string> SplitLines(string value)
        => value
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

    private static void AddNutrition(
        IDictionary<string, string> nutrition,
        string name,
        string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            nutrition[name] = value.Trim();
        }
    }

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
           ?? string.Empty;

    private static DateTime? ParseSourceDate(string value)
        => DateTime.TryParseExact(
            value,
            ["yyyyMMdd", "yyyy-MM-dd", "yyyyMMddHHmmss"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var result)
            ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
            : null;
}
