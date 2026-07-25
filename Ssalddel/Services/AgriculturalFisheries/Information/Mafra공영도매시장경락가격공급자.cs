using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed class Mafra공영도매시장경락가격공급자
    : I국내농산물경락가격공급자
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<Mafra공영도매시장경락가격공급자> _logger;

    public Mafra공영도매시장경락가격공급자(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        TimeProvider timeProvider,
        ILogger<Mafra공영도매시장경락가격공급자> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public string SourceKey
        => 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;

    public 국내농산물경락가격원천응답 GetSource()
    {
        var sourceOptions = _options.DomesticAgriculturalAuctionPrices;
        var secureTransport = Uri.TryCreate(
                                  sourceOptions.BaseUrl,
                                  UriKind.Absolute,
                                  out var baseUri)
                              && (baseUri.Scheme == Uri.UriSchemeHttps
                                  || sourceOptions.AllowInsecureHttp);
        return new 국내농산물경락가격원천응답
        {
            Key = SourceKey,
            Provider = "농림축산식품부",
            DisplayName = "전국 공영도매시장 경매원천 정산가격",
            UpdateCycle = "일간 원천자료",
            DocumentationUrl = sourceOptions.DocumentationUrl,
            IsConfigured = !string.IsNullOrWhiteSpace(sourceOptions.ApiKey)
                           && secureTransport
        };
    }

    public async Task<국내농산물경락가격조회응답> 조회Async(
        국내농산물경락가격조회요청 request,
        CancellationToken cancellationToken = default)
    {
        var source = GetSource();
        if (!source.IsConfigured)
        {
            var options = _options.DomesticAgriculturalAuctionPrices;
            var errorMessage = string.IsNullOrWhiteSpace(options.ApiKey)
                ? "PublicData:DomesticAgriculturalAuctionPrices:ApiKey 설정이 필요합니다."
                : "경락가격 원천이 HTTPS를 제공하지 않습니다. 보호된 중계 URL을 사용하거나 운영 검토 뒤 AllowInsecureHttp를 명시적으로 설정해야 합니다.";
            return Fail(
                request,
                source,
                국내농산물경락가격조회상태Codes.설정안됨,
                errorMessage);
        }

        try
        {
            var options = _options.DomesticAgriculturalAuctionPrices;
            var pageSize = Math.Clamp(request.PageSize, 1, Math.Max(1, options.MaxPageSize));
            var startIndex = checked(((request.Page - 1) * pageSize) + 1);
            var endIndex = checked(startIndex + pageSize - 1);
            var uri = BuildUri(options, request, startIndex, endIndex);
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);
            return Parse(document.RootElement, request, source);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "농림축산식품부 공영도매시장 경락가격 조회 실패. SettlementDate={SettlementDate}, WholesaleMarketCode={WholesaleMarketCode}, CorporationCode={CorporationCode}",
                request.SettlementDate,
                request.WholesaleMarketCode,
                request.CorporationCode);
            return Fail(
                request,
                source,
                국내농산물경락가격조회상태Codes.자료조회불가,
                "공영도매시장 경락가격 원천을 조회하지 못했습니다.");
        }
    }

    internal 국내농산물경락가격조회응답 Parse(
        JsonElement root,
        국내농산물경락가격조회요청 request,
        국내농산물경락가격원천응답 source)
    {
        var options = _options.DomesticAgriculturalAuctionPrices;
        if (!TryGetProperty(root, options.DatasetName, out var payload)
            || payload.ValueKind != JsonValueKind.Object)
        {
            return Fail(
                request,
                source,
                국내농산물경락가격조회상태Codes.자료조회불가,
                "경락가격 응답의 dataset 구조를 확인할 수 없습니다.");
        }

        var resultCode = ReadNestedString(payload, "result", "code");
        if (!string.Equals(resultCode, "INFO-000", StringComparison.OrdinalIgnoreCase))
        {
            var message = ReadNestedString(payload, "result", "message");
            return Fail(
                request,
                source,
                resultCode is "INFO-100"
                    ? 국내농산물경락가격조회상태Codes.설정안됨
                    : 국내농산물경락가격조회상태Codes.자료조회불가,
                string.IsNullOrWhiteSpace(message)
                    ? $"경락가격 원천 요청이 거부되었습니다. Code={resultCode}"
                    : message);
        }

        var collectedAt = _timeProvider.GetUtcNow();
        var items = ReadRows(payload)
            .Select(row => Map(row, collectedAt))
            .Where(item => item is not null)
            .Select(item => item!)
            .Where(item => string.IsNullOrWhiteSpace(request.ItemName)
                           || item.ItemName.Contains(
                               request.ItemName,
                               StringComparison.OrdinalIgnoreCase)
                           || item.VarietyName.Contains(
                               request.ItemName,
                               StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return new 국내농산물경락가격조회응답
        {
            Success = true,
            StatusCode = 국내농산물경락가격조회상태Codes.완료,
            Source = source,
            Query = request,
            Items = items,
            TotalCount = ReadInt32(payload, "totalCnt") ?? items.Length,
            LatestCollectedAtUtc = collectedAt,
            Notices = 국내농산물경락가격조회Service.DefaultNotices
        };
    }

    private static string BuildUri(
        DomesticAgriculturalAuctionPricesOptions options,
        국내농산물경락가격조회요청 request,
        int startIndex,
        int endIndex)
    {
        var date = DateOnly.ParseExact(
            request.SettlementDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);
        var path = $"/openapi/{Uri.EscapeDataString(options.ApiKey.Trim())}/json/"
                   + $"{Uri.EscapeDataString(options.DatasetName.Trim())}/{startIndex}/{endIndex}";
        var query = new List<string>
        {
            $"SALEDATE={date:yyyyMMdd}"
        };
        if (!string.IsNullOrWhiteSpace(request.WholesaleMarketCode))
        {
            query.Add($"WHSALCD={Uri.EscapeDataString(request.WholesaleMarketCode)}");
        }

        if (!string.IsNullOrWhiteSpace(request.CorporationCode))
        {
            query.Add($"CMPCD={Uri.EscapeDataString(request.CorporationCode)}");
        }

        return $"{path}?{string.Join("&", query)}";
    }

    private static 국내농산물경락가격항목? Map(
        JsonElement row,
        DateTimeOffset collectedAt)
    {
        var settlementDateRaw = ReadString(row, "SALEDATE");
        if (!DateOnly.TryParseExact(
                settlementDateRaw,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var settlementDate))
        {
            return null;
        }

        var identity = string.Join(
            '|',
            국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
            settlementDateRaw,
            ReadString(row, "WHSALCD"),
            ReadString(row, "CMPCD"),
            ReadString(row, "SEQ"),
            ReadString(row, "NO1"),
            ReadString(row, "NO2"),
            ReadString(row, "CMPGOOD"));

        return new 국내농산물경락가격항목
        {
            RecordKey = Sha256(identity),
            SourceKey = 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
            SettlementDate = settlementDate,
            WholesaleMarketCode = ReadString(row, "WHSALCD"),
            CorporationCode = ReadString(row, "CMPCD"),
            SlipNumber = ReadString(row, "SEQ"),
            AuctionSequence1 = ReadString(row, "NO1"),
            AuctionSequence2 = ReadString(row, "NO2"),
            TradingMethodCode = ReadString(row, "MMCD"),
            LargeCategoryCode = ReadString(row, "LARGE"),
            MiddleCategoryCode = ReadString(row, "MID"),
            SmallCategoryCode = ReadString(row, "SMALL"),
            CorporationItemCode = ReadString(row, "CMPGOOD"),
            ItemName = ReadString(row, "PUMNAME"),
            VarietyName = ReadString(row, "GOODNAME"),
            UnitWeight = ReadDecimal(row, "DANQ"),
            UnitCode = ReadString(row, "DANCD"),
            PackageCode = ReadString(row, "POJCD"),
            SizeCode = ReadString(row, "SIZECD"),
            GradeCode = ReadString(row, "LVCD"),
            Quantity = ReadDecimal(row, "QTY"),
            AuctionPriceKrw = ReadDecimal(row, "COST"),
            OriginCode = ReadString(row, "SANCD"),
            OriginName = ReadString(row, "SANNAME"),
            TotalQuantity = ReadDecimal(row, "TOTQTY"),
            TotalAmountKrw = ReadDecimal(row, "TOTAMT"),
            AwardedTime = ReadString(row, "SBIDTIME"),
            CollectedAtUtc = collectedAt
        };
    }

    private static IEnumerable<JsonElement> ReadRows(JsonElement payload)
    {
        if (!TryGetProperty(payload, "row", out var rows))
        {
            return [];
        }

        return rows.ValueKind switch
        {
            JsonValueKind.Array => rows.EnumerateArray().ToArray(),
            JsonValueKind.Object => [rows],
            _ => []
        };
    }

    private static string ReadNestedString(
        JsonElement element,
        string parentName,
        string propertyName)
        => TryGetProperty(element, parentName, out var parent)
            ? ReadString(parent, propertyName)
            : string.Empty;

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Number => property.GetRawText(),
            _ => string.Empty
        };
    }

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
        => decimal.TryParse(
            ReadString(element, propertyName),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;

    private static int? ReadInt32(JsonElement element, string propertyName)
        => int.TryParse(
            ReadString(element, propertyName),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;

    private static bool TryGetProperty(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(
                        property.Name,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static 국내농산물경락가격조회응답 Fail(
        국내농산물경락가격조회요청 request,
        국내농산물경락가격원천응답 source,
        string statusCode,
        string errorMessage)
        => new()
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            Source = source,
            Query = request,
            Notices = 국내농산물경락가격조회Service.DefaultNotices
        };
}
