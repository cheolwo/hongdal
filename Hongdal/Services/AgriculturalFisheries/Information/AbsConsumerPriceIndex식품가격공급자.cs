using System.Globalization;
using System.Text.Json;
using Hongdal.Contracts.Common.AgriculturalFisheries;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.AgriculturalFisheries.Information;

public sealed class AbsConsumerPriceIndex식품가격공급자 : I호주농수산식품가격공급자
{
    private const string OriginalSeriesCode = "10";
    private const string MonthlyFrequencyCode = "M";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;

    public AbsConsumerPriceIndex식품가격공급자(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string SourceKey => 호주농수산식품가격출처Keys.AbsConsumerPriceIndex;

    public string ProviderName => "Australian Bureau of Statistics (ABS)";

    public string DocumentationUrl =>
        "https://www.abs.gov.au/statistics/application-programming-interfaces-apis/data-api-user-guide";

    public async Task<호주농수산식품가격조회응답> 조회Async(
        호주농수산식품가격조회요청 request,
        CancellationToken cancellationToken = default)
    {
        var normalized = 호주농수산식품가격조회요청Rules.Normalize(request);
        var validationError = 호주농수산식품가격조회요청Rules.Validate(normalized);
        if (validationError is not null)
        {
            return Fail(normalized, 호주농수산식품가격조회상태Codes.잘못된요청, validationError);
        }

        using var response = await _httpClient.GetAsync(
            BuildRequestPath(normalized),
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Fail(
                normalized,
                호주농수산식품가격조회상태Codes.자료조회불가,
                $"ABS 식품 가격지수 조회에 실패했습니다. HTTP {(int)response.StatusCode}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<AbsSdmxResponse>(
            stream,
            JsonOptions,
            cancellationToken);
        if (payload is null || payload.Errors.Count > 0)
        {
            return Fail(
                normalized,
                호주농수산식품가격조회상태Codes.자료조회불가,
                "ABS가 현재 식품 가격지수 조회 조건을 처리하지 못했습니다.");
        }

        var dataSet = payload.Data.DataSets.FirstOrDefault();
        var structure = payload.Data.Structures.FirstOrDefault();
        var series = dataSet?.Series.Values.FirstOrDefault();
        var timeDimension = structure?.Dimensions.Observation.FirstOrDefault(dimension =>
            string.Equals(dimension.Id, "TIME_PERIOD", StringComparison.OrdinalIgnoreCase));
        if (dataSet is null
            || structure is null
            || series is null
            || timeDimension is null
            || series.Observations.Count == 0)
        {
            return Success(
                normalized,
                호주농수산식품가격조회상태Codes.자료없음,
                [],
                0,
                false,
                payload.Meta.Prepared,
                "조회 조건에 맞는 ABS 식품 가격지수 자료가 없습니다.");
        }

        var officialIndexLabel = ResolveDimensionLabel(
            structure.Dimensions.Series,
            "INDEX",
            normalized.IndexCode,
            호주농수산식품가격Catalog.OfficialIndexLabel(normalized.IndexCode));
        var unitCode = ResolveAttributeValue(
            series.Attributes,
            structure.Attributes.Series,
            "UNIT_MEASURE",
            value => value.Id);
        var unitLabel = ResolveAttributeValue(
            series.Attributes,
            structure.Attributes.Series,
            "UNIT_MEASURE",
            value => value.Name);
        var basePeriod = ResolveAttributeValue(
            dataSet.Attributes,
            structure.Attributes.DataSet,
            "BASE_PERIOD",
            value => value.Name);
        var allItems = series.Observations
            .Select(observation => MapItem(
                observation,
                timeDimension,
                normalized,
                officialIndexLabel,
                unitCode,
                unitLabel,
                basePeriod))
            .Where(item => item is not null)
            .Cast<호주농수산식품가격항목>()
            .OrderBy(item => item.ReferencePeriod, StringComparer.Ordinal)
            .ToArray();
        var items = allItems.Take(normalized.MaxItems).ToArray();
        var truncated = allItems.Length > items.Length;

        return Success(
            normalized,
            allItems.Length == 0
                ? 호주농수산식품가격조회상태Codes.자료없음
                : 호주농수산식품가격조회상태Codes.완료,
            items,
            allItems.Length,
            truncated,
            payload.Meta.Prepared,
            allItems.Length == 0
                ? "조회 조건에 맞는 ABS 식품 가격지수 자료가 없습니다."
                : truncated
                    ? $"ABS 식품 가격지수 {allItems.Length:N0}건 중 {items.Length:N0}건을 제공합니다."
                    : $"ABS 식품 가격지수 {items.Length:N0}건을 제공합니다.");
    }

    private string BuildRequestPath(호주농수산식품가격조회요청 request)
    {
        var seriesKey = string.Join(
            '.',
            request.MeasureCode,
            request.IndexCode,
            OriginalSeriesCode,
            request.RegionCode,
            MonthlyFrequencyCode);
        var path = $"{_options.AbsConsumerPriceIndex.DataPath.Trim('/')}/{seriesKey}";
        return QueryHelpers.AddQueryString(
            path,
            new Dictionary<string, string?>
            {
                ["startPeriod"] = request.StartPeriod,
                ["endPeriod"] = request.EndPeriod,
                ["dimensionAtObservation"] = "TIME_PERIOD",
                ["format"] = "jsondata"
            });
    }

    private static 호주농수산식품가격항목? MapItem(
        KeyValuePair<string, JsonElement[]> observation,
        AbsSdmxDimension timeDimension,
        호주농수산식품가격조회요청 request,
        string officialIndexLabel,
        string unitCode,
        string unitLabel,
        string basePeriod)
    {
        if (!int.TryParse(
                observation.Key,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var periodIndex)
            || periodIndex < 0
            || periodIndex >= timeDimension.Values.Count
            || observation.Value.Length == 0)
        {
            return null;
        }

        var value = observation.Value[0];
        var rawValue = value.ValueKind switch
        {
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.String => value.GetString() ?? string.Empty,
            _ => string.Empty
        };
        decimal? numericValue = value.ValueKind == JsonValueKind.Number
            && value.TryGetDecimal(out var parsed)
                ? parsed
                : decimal.TryParse(
                    rawValue,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out parsed)
                    ? parsed
                    : null;

        return new 호주농수산식품가격항목
        {
            IndexCode = request.IndexCode,
            IndexLabel = 호주농수산식품가격Catalog.IndexLabel(request.IndexCode),
            OfficialIndexLabel = officialIndexLabel,
            MeasureCode = request.MeasureCode,
            MeasureLabel = 호주농수산식품가격Catalog.MeasureLabel(request.MeasureCode),
            RegionCode = request.RegionCode,
            RegionLabel = 호주농수산식품가격Catalog.RegionLabel(request.RegionCode),
            ReferencePeriod = timeDimension.Values[periodIndex].Id,
            RawValue = rawValue,
            NumericValue = numericValue,
            UnitCode = unitCode,
            UnitLabel = unitLabel,
            BasePeriod = basePeriod
        };
    }

    private static string ResolveDimensionLabel(
        IReadOnlyList<AbsSdmxDimension> dimensions,
        string dimensionId,
        string valueId,
        string fallback)
        => dimensions
               .FirstOrDefault(dimension => string.Equals(
                   dimension.Id,
                   dimensionId,
                   StringComparison.OrdinalIgnoreCase))
               ?.Values
               .FirstOrDefault(value => string.Equals(
                   value.Id,
                   valueId,
                   StringComparison.Ordinal))
               ?.Name
           ?? fallback;

    private static string ResolveAttributeValue(
        IReadOnlyList<int?> selectedIndexes,
        IReadOnlyList<AbsSdmxAttribute> definitions,
        string attributeId,
        Func<AbsSdmxCodeValue, string> selector)
    {
        var position = definitions
            .Select((definition, index) => new { definition, index })
            .FirstOrDefault(candidate => string.Equals(
                candidate.definition.Id,
                attributeId,
                StringComparison.OrdinalIgnoreCase))
            ?.index;
        if (position is null
            || position.Value >= selectedIndexes.Count
            || selectedIndexes[position.Value] is not int valueIndex)
        {
            return string.Empty;
        }

        var values = definitions[position.Value].Values;
        return valueIndex >= 0 && valueIndex < values.Count
            ? selector(values[valueIndex])
            : string.Empty;
    }

    private 호주농수산식품가격조회응답 Success(
        호주농수산식품가격조회요청 request,
        string statusCode,
        IReadOnlyList<호주농수산식품가격항목> items,
        int totalCount,
        bool isTruncated,
        DateTime? sourcePreparedAtUtc,
        string summary)
        => new()
        {
            Success = statusCode is 호주농수산식품가격조회상태Codes.완료
                or 호주농수산식품가격조회상태Codes.자료없음,
            StatusCode = statusCode,
            SourceKey = SourceKey,
            Provider = ProviderName,
            DocumentationUrl = DocumentationUrl,
            Query = request,
            Items = items,
            TotalCount = totalCount,
            IsTruncated = isTruncated,
            SourcePreparedAtUtc = sourcePreparedAtUtc?.ToUniversalTime(),
            CollectedAtUtc = DateTime.UtcNow,
            Summary = summary,
            Notices = BuildNotices()
        };

    private 호주농수산식품가격조회응답 Fail(
        호주농수산식품가격조회요청 request,
        string statusCode,
        string errorMessage)
        => new()
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            SourceKey = SourceKey,
            Provider = ProviderName,
            DocumentationUrl = DocumentationUrl,
            Query = request,
            CollectedAtUtc = DateTime.UtcNow,
            Summary = errorMessage,
            Notices = BuildNotices()
        };

    private static IReadOnlyList<string> BuildNotices()
        =>
        [
            "Based on Australian Bureau of Statistics data.",
            "ABS CPI는 실제 A$/kg 또는 개별 상품 단가가 아니라 소비자 가격 변동을 나타내는 지수입니다.",
            "주도시 지수는 각 도시의 시간에 따른 변화를 보여 주며 도시 간 절대 소매가격 수준을 비교하지 않습니다.",
            "공동구매 매입가·운송비·관세·도착원가와 직접 같은 값으로 사용하지 않습니다.",
            "ABS Data API는 Beta 서비스이므로 데이터흐름과 응답 계약을 주기적으로 재검증합니다."
        ];

    private sealed class AbsSdmxResponse
    {
        public AbsSdmxMeta Meta { get; init; } = new();

        public AbsSdmxData Data { get; init; } = new();

        public IReadOnlyList<JsonElement> Errors { get; init; } = [];
    }

    private sealed class AbsSdmxMeta
    {
        public DateTime? Prepared { get; init; }
    }

    private sealed class AbsSdmxData
    {
        public IReadOnlyList<AbsSdmxDataSet> DataSets { get; init; } = [];

        public IReadOnlyList<AbsSdmxStructure> Structures { get; init; } = [];
    }

    private sealed class AbsSdmxDataSet
    {
        public IReadOnlyList<int?> Attributes { get; init; } = [];

        public IReadOnlyDictionary<string, AbsSdmxSeries> Series { get; init; } =
            new Dictionary<string, AbsSdmxSeries>();
    }

    private sealed class AbsSdmxSeries
    {
        public IReadOnlyList<int?> Attributes { get; init; } = [];

        public IReadOnlyDictionary<string, JsonElement[]> Observations { get; init; } =
            new Dictionary<string, JsonElement[]>();
    }

    private sealed class AbsSdmxStructure
    {
        public AbsSdmxDimensions Dimensions { get; init; } = new();

        public AbsSdmxAttributes Attributes { get; init; } = new();
    }

    private sealed class AbsSdmxDimensions
    {
        public IReadOnlyList<AbsSdmxDimension> Series { get; init; } = [];

        public IReadOnlyList<AbsSdmxDimension> Observation { get; init; } = [];
    }

    private sealed class AbsSdmxDimension
    {
        public string Id { get; init; } = string.Empty;

        public IReadOnlyList<AbsSdmxCodeValue> Values { get; init; } = [];
    }

    private sealed class AbsSdmxAttributes
    {
        public IReadOnlyList<AbsSdmxAttribute> DataSet { get; init; } = [];

        public IReadOnlyList<AbsSdmxAttribute> Series { get; init; } = [];
    }

    private sealed class AbsSdmxAttribute
    {
        public string Id { get; init; } = string.Empty;

        public IReadOnlyList<AbsSdmxCodeValue> Values { get; init; } = [];
    }

    private sealed class AbsSdmxCodeValue
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;
    }
}
