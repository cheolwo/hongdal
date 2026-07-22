using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.FoodCulture;

public sealed record OfficialFoodIngredientDomesticCompanyRecord(
    string LicenseNumber,
    string OrganizationName,
    string ProductReportNumber,
    string ReportDate,
    string ProductName,
    string ProductCategory,
    string RawIngredientText,
    string RawIngredientOrder,
    string ChangedDate);

public interface IOfficialFoodIngredientDomesticCompanySource
{
    bool IsConfigured { get; }

    Task<IReadOnlyList<OfficialFoodIngredientDomesticCompanyRecord>> SearchAsync(
        string ingredientName,
        int take,
        CancellationToken cancellationToken = default);
}

public sealed class MfdsIngredientProductCompanySource
    : IOfficialFoodIngredientDomesticCompanySource
{
    public const string SourceKey = "mfds-domestic-product-ingredient-report";

    public const string DocumentationUrl =
        "https://www.foodsafetykorea.go.kr/api/openApiInfo.do?menu_grp=MENU_GRP31&menu_no=661&show_cnt=10&start_idx=1&svc_no=C002";

    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;

    public MfdsIngredientProductCompanySource(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<IReadOnlyList<OfficialFoodIngredientDomesticCompanyRecord>> SearchAsync(
        string ingredientName,
        int take,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ingredientName);
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "PublicData:MfdsIngredientCompanies:ApiKey가 필요합니다.");
        }

        var serviceId = string.IsNullOrWhiteSpace(_options.MfdsIngredientCompanies.ServiceId)
            ? "C002"
            : _options.MfdsIngredientCompanies.ServiceId.Trim();
        var pageSize = Math.Clamp(
            Math.Min(take, _options.MfdsIngredientCompanies.PageSize),
            1,
            1000);
        var path = string.Join(
            '/',
            "api",
            Uri.EscapeDataString(ApiKey),
            Uri.EscapeDataString(serviceId),
            "json",
            "1",
            pageSize.ToString(CultureInfo.InvariantCulture),
            $"RAWMTRL_NM={Uri.EscapeDataString(ingredientName.Trim())}");

        using var response = await _httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);
            return Parse(document.RootElement, serviceId);
        }
        catch (JsonException exception)
        {
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "unknown";
            throw new InvalidOperationException(
                $"식품안전나라 {serviceId}가 JSON이 아닌 응답을 반환했습니다. "
                + $"HTTP={(int)response.StatusCode}, Content-Type={contentType}. "
                + "인증키의 서비스 권한과 호출 제한을 확인해야 합니다.",
                exception);
        }
    }

    internal static IReadOnlyList<OfficialFoodIngredientDomesticCompanyRecord> Parse(
        JsonElement root,
        string serviceId)
    {
        if (!TryGetProperty(root, serviceId, out var payload))
        {
            throw CreateApiException(root);
        }

        if (TryGetProperty(payload, "RESULT", out var result)
            && !IsSuccessCode(ReadString(result, "CODE")))
        {
            throw CreateApiException(result, "식품안전나라 C002");
        }

        if (!TryGetProperty(payload, "row", out var rows)
            || rows.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return rows
            .EnumerateArray()
            .Select(ToRecord)
            .Where(record => record is not null)
            .Cast<OfficialFoodIngredientDomesticCompanyRecord>()
            .ToArray();
    }

    private string ApiKey
        => FirstNonEmpty(
            _options.MfdsIngredientCompanies.ApiKey,
            _options.MfdsCookRecipe.ApiKey);

    private static OfficialFoodIngredientDomesticCompanyRecord? ToRecord(JsonElement row)
    {
        var licenseNumber = ReadString(row, "LCNS_NO");
        var organizationName = ReadString(row, "BSSH_NM");
        var productName = ReadString(row, "PRDLST_NM");
        if (string.IsNullOrWhiteSpace(licenseNumber)
            || string.IsNullOrWhiteSpace(organizationName)
            || string.IsNullOrWhiteSpace(productName))
        {
            return null;
        }

        return new OfficialFoodIngredientDomesticCompanyRecord(
            licenseNumber,
            organizationName,
            ReadString(row, "PRDLST_REPORT_NO"),
            ReadString(row, "PRMS_DT"),
            productName,
            ReadString(row, "PRDLST_DCNM"),
            ReadString(row, "RAWMTRL_NM"),
            ReadString(row, "RAWMTRL_ORDNO"),
            ReadString(row, "CHNG_DT"));
    }

    private static Exception CreateApiException(JsonElement root)
    {
        if (TryGetProperty(root, "RESULT", out var result))
        {
            return CreateApiException(result, "식품안전나라 C002");
        }

        return new InvalidOperationException(
            "식품안전나라 C002 응답에서 서비스 payload를 찾지 못했습니다.");
    }

    private static Exception CreateApiException(JsonElement result, string sourceName)
        => new InvalidOperationException(
            $"{sourceName} 응답 오류입니다. Code={ReadString(result, "CODE")}, Message={ReadString(result, "MSG")}");

    private static bool IsSuccessCode(string code)
        => string.IsNullOrWhiteSpace(code)
           || string.Equals(code, "INFO-000", StringComparison.OrdinalIgnoreCase)
           || string.Equals(code, "00", StringComparison.OrdinalIgnoreCase);

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

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
           ?? string.Empty;
}
