using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.PublicData;

public interface I경기데이터드림CatalogClient
{
    Task<경기데이터드림CatalogResponse> GetAgricultureLivestockFisheriesAsync(
        CancellationToken cancellationToken = default);
}

public interface I경기데이터드림ApiClient
{
    Task<경기데이터드림ApiResponse> QueryAsync(
        경기데이터드림ApiRequest request,
        CancellationToken cancellationToken = default);
}

public interface I경기데이터드림가축사육집계Client
{
    Task<경기데이터드림가축사육집계Response> QueryAsync(
        string? regionCode = null,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    "public-data.gyeonggi-data-dream.catalog",
    SsalddelCodeLayer.ExternalAdapter,
    "경기데이터드림 농림해양수산 API 전체 목록을 동적으로 수집하고 제품 경계별로 분류",
    ContractType = typeof(경기데이터드림CatalogResponse),
    FlowOrder = 3,
    Boundary = "포털 카탈로그는 데이터 존재 근거이며 공급 가능성, 재고, 계약 또는 개인 농장 위치를 확정하지 않음")]
public sealed class 경기데이터드림CatalogClient : I경기데이터드림CatalogClient
{
    private const string CategoryCode = "DO50";
    private const int PortalPageSize = 10;
    private readonly HttpClient _httpClient;
    private readonly TimeProvider _timeProvider;

    public 경기데이터드림CatalogClient(HttpClient httpClient, TimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _timeProvider = timeProvider;
    }

    public static IReadOnlyList<경기데이터드림ModuleDefinition> Modules { get; } =
    [
        new(경기데이터드림ModuleKeys.농산스마트팜식물건강, "농산·스마트팜·식물건강", "농가 ID, 정밀 시설 위치, 상담 이미지와 의견은 공개 투영에서 제외"),
        new(경기데이터드림ModuleKeys.농산생산인증유통, "농산 생산·인증·유통", "시설과 인증 현황을 재고, 처리 여력 또는 공급 계약으로 표현하지 않음"),
        new(경기데이터드림ModuleKeys.축산농장사육, "축산 농장·사육", "농장명, 전화, 주소, 좌표와 개체 식별번호를 공개하지 않고 시군 집계만 제공"),
        new(경기데이터드림ModuleKeys.축산사료방역안전, "축산 사료·방역·안전", "검사 결과를 농가 품질 순위나 거래 자격으로 변환하지 않음"),
        new(경기데이터드림ModuleKeys.축산가공물류인허가, "축산 가공·물류·인허가", "영업 인허가를 현재 가용성, 소유권 또는 거래 권한으로 표현하지 않음"),
        new(경기데이터드림ModuleKeys.수산양식안전, "수산·양식·안전", "표본, 검사일, 품목과 단위를 유지하고 생산자 평가로 확장하지 않음"),
        new(경기데이터드림ModuleKeys.수산위판역사, "수산 위판 역사자료", "2016년 이전 역사 가격을 현재 가격으로 표현하지 않음"),
        new(경기데이터드림ModuleKeys.반려동물제외, "반려동물 별도 범위", "농축수산 생산 모듈에서 제외"),
        new(경기데이터드림ModuleKeys.산림별도, "산림 별도 범위", "농축수산 생산 모듈과 분리"),
        new(경기데이터드림ModuleKeys.농촌기타Catalog, "농촌·해양 기타 카탈로그", "typed projection 전까지 카탈로그만 제공")
    ];

    public async Task<경기데이터드림CatalogResponse> GetAgricultureLivestockFisheriesAsync(
        CancellationToken cancellationToken = default)
    {
        var unique = new Dictionary<string, 경기데이터드림CatalogItem>(StringComparer.Ordinal);
        var page = 1;
        var totalPages = 1;

        do
        {
            var url = QueryHelpers.AddQueryString(
                "/portal/data/dataset/searchDataset.do",
                new Dictionary<string, string?>
                {
                    ["sort"] = "reg",
                    ["page"] = page.ToString(CultureInfo.InvariantCulture),
                    ["size"] = PortalPageSize.ToString(CultureInfo.InvariantCulture),
                    ["cateId"] = CategoryCode
                });
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var result = document.RootElement.GetProperty("result");
            totalPages = result.GetProperty("pageInfo").GetProperty("totalPages").GetInt32();

            foreach (var row in result.GetProperty("contents").EnumerateArray())
            {
                if (!TryReadInt(row, "acolInfSeq", out var apiInfSeq))
                {
                    continue;
                }

                var infId = Read(row, "infId");
                if (string.IsNullOrWhiteSpace(infId))
                {
                    continue;
                }

                var displayName = Read(row, "infNm");
                var description = Read(row, "infExp");
                unique.TryAdd(infId, new 경기데이터드림CatalogItem
                {
                    InfId = infId,
                    InfSeq = ReadInt(row, "infSeq") ?? 1,
                    ApiInfSeq = apiInfSeq,
                    DisplayName = displayName,
                    Description = description,
                    CategoryName = WebUtility.HtmlDecode(Read(row, "topCateNm2")),
                    ModuleKey = Classify(displayName, description),
                    DetailUrl = $"https://data.gg.go.kr/portal/data/service/selectServicePage.do?infId={Uri.EscapeDataString(infId)}&infSeq={apiInfSeq}",
                    RegisteredOn = ReadDate(row, "regDttm"),
                    UpdatedOn = ReadDate(row, "updDttm")
                });
            }

            page++;
        }
        while (page <= totalPages);

        return new 경기데이터드림CatalogResponse
        {
            ObservedAt = _timeProvider.GetUtcNow(),
            Modules = Modules,
            Items = unique.Values
                .OrderBy(item => item.ModuleKey, StringComparer.Ordinal)
                .ThenBy(item => item.DisplayName, StringComparer.Ordinal)
                .ToArray()
        };
    }

    internal static string Classify(string displayName, string description)
    {
        var text = $"{displayName} {description}";
        if (ContainsAny(text, "반려", "유기동물", "동물보호", "동물등록", "길고양이"))
        {
            return 경기데이터드림ModuleKeys.반려동물제외;
        }

        if (ContainsAny(text, "산림", "임업", "나무", "목재", "산촌"))
        {
            return 경기데이터드림ModuleKeys.산림별도;
        }

        if (ContainsAny(text, "위판", "위판장 가격") && ContainsAny(text, "수산", "수협"))
        {
            return 경기데이터드림ModuleKeys.수산위판역사;
        }

        if (ContainsAny(text, "수산물", "수산업", "수산자원", "수산 자원", "어업", "어촌", "어선", "어항", "양식", "치어", "갯벌"))
        {
            return 경기데이터드림ModuleKeys.수산양식안전;
        }

        if (ContainsAny(text, "도축", "집유", "축산물가공", "축산물 가공", "축산물보관", "축산물 보관", "식육", "운반업", "판매업"))
        {
            return 경기데이터드림ModuleKeys.축산가공물류인허가;
        }

        if (ContainsAny(text, "사료", "동물약", "가축전염", "방역", "잔류물질", "분뇨", "폐사축", "수의"))
        {
            return 경기데이터드림ModuleKeys.축산사료방역안전;
        }

        if (ContainsAny(text, "가축", "축산", "우제류", "가금", "종축", "부화", "인공수정", "낙농", "사육"))
        {
            return 경기데이터드림ModuleKeys.축산농장사육;
        }

        if (ContainsAny(text, "스마트팜", "식물병원", "병해충", "생육", "재배환경", "토양"))
        {
            return 경기데이터드림ModuleKeys.농산스마트팜식물건강;
        }

        if (ContainsAny(text, "농산", "농업", "농가", "농작물", "로컬푸드", "친환경", "농식품", "산지유통", "종자", "과실", "채소"))
        {
            return 경기데이터드림ModuleKeys.농산생산인증유통;
        }

        return 경기데이터드림ModuleKeys.농촌기타Catalog;
    }

    private static bool ContainsAny(string text, params string[] values)
        => values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));

    private static string Read(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int? ReadInt(JsonElement element, string propertyName)
        => TryReadInt(element, propertyName, out var value) ? value : null;

    private static bool TryReadInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind != JsonValueKind.Null
               && (property.TryGetInt32(out value)
                   || int.TryParse(property.ToString(), CultureInfo.InvariantCulture, out value));
    }

    private static DateOnly? ReadDate(JsonElement element, string propertyName)
        => DateOnly.TryParse(Read(element, propertyName), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
}

[SsalddelCodeMetadata(
    "public-data.gyeonggi-data-dream.query",
    SsalddelCodeLayer.ExternalAdapter,
    "경기데이터드림 API의 공통 인증, 페이지 및 오류 응답 경계",
    ContractType = typeof(경기데이터드림ApiResponse),
    FlowOrder = 3,
    Boundary = "원문 응답은 서버 내부 adapter 경계이며 공개 Controller에서 직접 반환하지 않음")]
public sealed partial class 경기데이터드림ApiClient : I경기데이터드림ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;
    private readonly TimeProvider _timeProvider;

    public 경기데이터드림ApiClient(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        TimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async Task<경기데이터드림ApiResponse> QueryAsync(
        경기데이터드림ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var observedAt = _timeProvider.GetUtcNow();
        var options = _options.GyeonggiDataDream;
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return Fail("PublicData:GyeonggiDataDream:ApiKey 설정이 필요합니다.", observedAt);
        }

        var datasetPath = request.DatasetPath.Trim().Trim('/');
        if (!DatasetPathPattern().IsMatch(datasetPath))
        {
            return Fail("경기데이터드림 DatasetPath 형식이 올바르지 않습니다.", observedAt);
        }

        var parameters = request.Parameters
            .Where(item => !string.Equals(item.Key, "KEY", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(item.Key, "Type", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(item.Key, "pIndex", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(item.Key, "pSize", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        parameters["KEY"] = options.ApiKey.Trim();
        parameters["Type"] = "json";
        parameters["pIndex"] = Math.Max(1, request.Page).ToString(CultureInfo.InvariantCulture);
        parameters["pSize"] = Math.Clamp(request.PageSize, 1, Math.Max(1, options.PageSize))
            .ToString(CultureInfo.InvariantCulture);
        var url = QueryHelpers.AddQueryString(datasetPath, parameters);

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new 경기데이터드림ApiResponse
            {
                Success = response.IsSuccessStatusCode && !HasProviderError(body),
                HttpStatusCode = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty,
                Body = body,
                ErrorMessage = response.IsSuccessStatusCode && !HasProviderError(body)
                    ? string.Empty
                    : "경기데이터드림 원천이 오류 응답을 반환했습니다.",
                ObservedAt = observedAt
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Fail("경기데이터드림 원천을 조회하지 못했습니다.", observedAt);
        }
    }

    private static bool HasProviderError(string body)
        => body.Contains("INFO-200", StringComparison.OrdinalIgnoreCase)
           || body.Contains("ERROR-", StringComparison.OrdinalIgnoreCase);

    private static 경기데이터드림ApiResponse Fail(string message, DateTimeOffset observedAt)
        => new()
        {
            ErrorMessage = message,
            ObservedAt = observedAt
        };

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{1,99}$", RegexOptions.CultureInvariant)]
    private static partial Regex DatasetPathPattern();
}

[SsalddelCodeMetadata(
    "public-data.gyeonggi-data-dream.livestock-summary",
    SsalddelCodeLayer.ExternalAdapter,
    "가축 사육업체 원문 전체 페이지를 경기도 범위·영업상태별 비식별 집계로 투영",
    ContractType = typeof(경기데이터드림가축사육집계Response),
    FlowOrder = 4,
    Boundary = "농장명, 전화, 주소, 좌표, 가축 및 권리자 식별번호를 반환하지 않음")]
public sealed class 경기데이터드림가축사육집계Client : I경기데이터드림가축사육집계Client
{
    public const string DatasetScopeRegionCode = "KR-41";
    public const string DatasetScopeRegionName = "경기도";
    private const int PageSize = 1000;
    private const int MaxPages = 100;
    private readonly I경기데이터드림ApiClient _client;

    public 경기데이터드림가축사육집계Client(I경기데이터드림ApiClient client)
    {
        _client = client;
    }

    public async Task<경기데이터드림가축사육집계Response> QueryAsync(
        string? regionCode = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = string.IsNullOrWhiteSpace(regionCode)
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?> { ["SIGUN_CD"] = regionCode.Trim() };
        var aggregates = new Dictionary<(string RegionCode, string RegionName, string Status), int>();
        var observedAt = DateTimeOffset.MinValue;
        var page = 1;
        var processedRowCount = 0;
        try
        {
            while (page <= MaxPages)
            {
                var response = await _client.QueryAsync(
                    new 경기데이터드림ApiRequest
                    {
                        DatasetPath = "LivestockBreeding",
                        Page = page,
                        PageSize = PageSize,
                        Parameters = parameters
                    },
                    cancellationToken);
                observedAt = response.ObservedAt > observedAt
                    ? response.ObservedAt
                    : observedAt;
                if (!response.Success)
                {
                    return Failed(response.ErrorMessage, observedAt);
                }

                using var document = JsonDocument.Parse(response.Body);
                var rows = FindRows(document.RootElement);
                foreach (var row in rows)
                {
                    var providedRegionCode = ReadFirst(row, "SIGUN_CD").Trim();
                    var providedRegionName = ReadFirst(row, "SIGUN_NM", "SIGUN_NM_INFO").Trim();
                    var aggregateRegionCode = providedRegionCode.Length == 0
                        ? DatasetScopeRegionCode
                        : providedRegionCode;
                    var aggregateRegionName = providedRegionName.Length == 0
                        ? DatasetScopeRegionName
                        : providedRegionName;
                    var status = ReadFirst(row, "BSN_STATE_NM", "BIZCOND_DIV_NM_INFO").Trim();
                    if (status.Length == 0)
                    {
                        status = "상태 미제공";
                    }

                    var key = (aggregateRegionCode, aggregateRegionName, status);
                    aggregates[key] = aggregates.GetValueOrDefault(key) + 1;
                }
                processedRowCount += rows.Count;

                var totalCount = FindTotalCount(document.RootElement);
                var reachedEnd = totalCount.HasValue
                    ? processedRowCount >= totalCount.Value
                    : rows.Count < PageSize;
                if (reachedEnd)
                {
                    break;
                }

                if (rows.Count == 0)
                {
                    return Failed("가축 사육업체 응답이 전체 건수보다 먼저 종료되었습니다.", observedAt);
                }

                page++;
            }

            if (page > MaxPages)
            {
                return Failed("가축 사육업체 응답 페이지 수가 안전 한도를 초과했습니다.", observedAt);
            }

            var items = aggregates
                .Select(pair => new 경기데이터드림가축사육지역집계
                {
                    RegionCode = pair.Key.RegionCode,
                    RegionName = pair.Key.RegionName,
                    BusinessStatus = pair.Key.Status,
                    BusinessCount = pair.Value
                })
                .OrderBy(item => item.RegionName, StringComparer.Ordinal)
                .ThenBy(item => item.BusinessStatus, StringComparer.Ordinal)
                .ToArray();

            return new 경기데이터드림가축사육집계Response
            {
                Success = true,
                ObservedAt = observedAt,
                Items = items
            };
        }
        catch (JsonException)
        {
            return new 경기데이터드림가축사육집계Response
            {
                ErrorMessage = "가축 사육업체 응답 구조를 확인할 수 없습니다.",
                ObservedAt = observedAt,
                Items = []
            };
        }
    }

    private static 경기데이터드림가축사육집계Response Failed(
        string errorMessage,
        DateTimeOffset observedAt)
        => new()
        {
            ErrorMessage = errorMessage,
            ObservedAt = observedAt,
            Items = []
        };

    private static IReadOnlyList<JsonElement> FindRows(JsonElement root)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in property.Value.EnumerateArray())
            {
                if (part.ValueKind == JsonValueKind.Object
                    && part.TryGetProperty("row", out var rows)
                    && rows.ValueKind == JsonValueKind.Array)
                {
                    return rows.EnumerateArray().Select(row => row.Clone()).ToArray();
                }
            }
        }

        return [];
    }

    private static int? FindTotalCount(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "list_total_count", StringComparison.OrdinalIgnoreCase)
                    && property.Value.TryGetInt32(out var count))
                {
                    return count;
                }

                var nested = FindTotalCount(property.Value);
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
                var nested = FindTotalCount(item);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static string ReadFirst(JsonElement row, params string[] names)
    {
        foreach (var name in names)
        {
            if (row.TryGetProperty(name, out var value))
            {
                return value.ToString();
            }
        }

        return string.Empty;
    }
}
