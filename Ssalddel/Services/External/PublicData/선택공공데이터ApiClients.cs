using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.PublicData;

public interface I국문관광정보공공데이터Client
{
    IReadOnlyList<공공데이터포털업무ApiDefinition> Apis { get; }

    Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default);
}

public interface I온라인가격공공데이터Client
{
    IReadOnlyList<공공데이터포털업무ApiDefinition> Apis { get; }

    Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default);
}

public interface IKosis비교자료공공데이터Client
{
    IReadOnlyList<공공데이터포털업무ApiDefinition> Apis { get; }

    Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default);
}

public interface I수산물산지위판가격Client
{
    Task<수산물산지위판가격Response> QueryAsync(
        수산물산지위판가격Request request,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    선택공공데이터Feature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "한국관광공사 국문 관광정보 15개 오퍼레이션의 인증 및 허용 경로 모듈",
    ContractType = typeof(공공데이터포털업무ApiResponse),
    FlowOrder = 3,
    Boundary = "관광정보와 이미지는 지역문화 대표성을 확정하지 않으며 공공누리 유형과 원천 출처를 별도 보존")]
public sealed class 국문관광정보공공데이터Client
    : 공공데이터포털업무ApiClientBase,
      I국문관광정보공공데이터Client
{
    private const string Prefix = "/B551011/KorService2/";

    private static readonly IReadOnlyList<공공데이터포털업무ApiDefinition> Definitions =
    [
        Api("area-code", "지역코드", "areaCode2"),
        Api("legal-dong-code", "법정동코드", "ldongCode2"),
        Api("category-code", "서비스분류코드", "categoryCode2"),
        Api("classification-code", "분류체계코드", "lclsSystmCode2"),
        Api("area-based", "지역기반 관광정보", "areaBasedList2"),
        Api("location-based", "위치기반 관광정보", "locationBasedList2"),
        Api("keyword-search", "키워드 검색", "searchKeyword2"),
        Api("festival-search", "행사정보", "searchFestival2"),
        Api("stay-search", "숙박정보", "searchStay2"),
        Api("detail-common", "공통정보", "detailCommon2"),
        Api("detail-intro", "소개정보", "detailIntro2"),
        Api("detail-info", "반복정보", "detailInfo2"),
        Api("detail-image", "이미지정보", "detailImage2"),
        Api("sync-list", "관광정보 동기화 목록", "areaBasedSyncList2"),
        Api("pet-tour", "반려동물 동반 여행 정보", "detailPetTour2")
    ];

    public 국문관광정보공공데이터Client(HttpClient httpClient, IOptions<PublicDataOptions> options)
        : base(httpClient, options, "한국관광공사", Definitions)
    {
    }

    public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => Definitions;

    public Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default)
        => QueryCoreAsync(request, cancellationToken);

    private static 공공데이터포털업무ApiDefinition Api(string key, string name, string operation)
        => new(key, name, Prefix + operation, [Prefix]);
}

[SsalddelCodeMetadata(
    선택공공데이터Feature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "온라인 수집가격의 품목과 일별 가격을 서로 다른 오퍼레이션으로 제공",
    ContractType = typeof(공공데이터포털업무ApiResponse),
    FlowOrder = 3,
    Boundary = "웹 수집 상품가격은 KAMIS 또는 KOSIS와 단위와 품목이 일치하기 전 순위나 절감액으로 사용하지 않음")]
public sealed class 온라인가격공공데이터Client
    : 공공데이터포털업무ApiClientBase,
      I온라인가격공공데이터Client
{
    private static readonly IReadOnlyList<공공데이터포털업무ApiDefinition> Definitions =
    [
        new(
            "item-list",
            "온라인가격 품목 목록",
            "/1240000/bpp_openapi/getPriceItemList",
            ["/1240000/bpp_openapi/getPriceItemList"]),
        new(
            "price-observation",
            "온라인 일별 가격",
            "/1240000/bpp_openapi/getPriceInfo",
            ["/1240000/bpp_openapi/getPriceInfo"])
        {
            ServiceKeyParameterName = "ServiceKey"
        }
    ];

    public 온라인가격공공데이터Client(HttpClient httpClient, IOptions<PublicDataOptions> options)
        : base(httpClient, options, "국가데이터처", Definitions)
    {
    }

    public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => Definitions;

    public Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default)
        => QueryCoreAsync(request, cancellationToken);
}

[SsalddelCodeMetadata(
    선택공공데이터Feature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "KOSIS 지표정보와 통계표 수치자료를 승인된 서로 다른 서비스 경계로 조회",
    ContractType = typeof(공공데이터포털업무ApiResponse),
    FlowOrder = 3,
    Boundary = "집계 통계는 지역 배경과 가격지수 비교용이며 개인, 가구 또는 참여자 평가에 사용하지 않음")]
public sealed class Kosis비교자료공공데이터Client
    : 공공데이터포털업무ApiClientBase,
      IKosis비교자료공공데이터Client
{
    private static readonly IReadOnlyList<공공데이터포털업무ApiDefinition> Definitions =
    [
        new(
            "indicator-by-name",
            "KOSIS 지표명별 목록",
            "/1240000/IndicatorService/IndListSearchRequest",
            ["/1240000/IndicatorService/IndListSearchRequest"]),
        new(
            "indicator-by-id",
            "KOSIS 고유번호별 목록",
            "/1240000/IndicatorService/IndIdListSearchRequest",
            ["/1240000/IndicatorService/IndIdListSearchRequest"]),
        new(
            "indicator-detail-by-id",
            "KOSIS 고유번호별 지표 상세",
            "/1240000/IndicatorService/IndIdDetailSearchRequest",
            ["/1240000/IndicatorService/IndIdDetailSearchRequest"]),
        new(
            "indicator-by-period",
            "KOSIS 수록주기별 목록",
            "/1240000/IndicatorService/PrListSearchRequest",
            ["/1240000/IndicatorService/PrListSearchRequest"]),
        new(
            "indicator-detail-by-name",
            "KOSIS 지표명별 상세",
            "/1240000/IndicatorService/IndDetailSearchRequest",
            ["/1240000/IndicatorService/IndDetailSearchRequest"]),
        new(
            "statistics-data",
            "KOSIS 통계표 수치자료",
            "/1240000/statisticsData/getStatisticsData",
            ["/1240000/statisticsData/getStatisticsData"])
    ];

    public Kosis비교자료공공데이터Client(HttpClient httpClient, IOptions<PublicDataOptions> options)
        : base(httpClient, options, "국가데이터처 KOSIS", Definitions)
    {
    }

    public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => Definitions;

    public Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default)
        => QueryCoreAsync(request, cancellationToken);
}

[SsalddelCodeMetadata(
    선택공공데이터Feature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "MAFRA 역사 수산물 산지 위판가격을 HTTPS 및 별도키 경계에서 조회",
    ContractType = typeof(수산물산지위판가격Response),
    FlowOrder = 3,
    Boundary = "수록 종료일 2016-01-19를 현재 가격으로 표현하지 않고 운송주체나 거래상대 식별정보를 저장하지 않음")]
public sealed class Mafra수산물산지위판가격Client : I수산물산지위판가격Client
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;
    private readonly TimeProvider _timeProvider;

    public Mafra수산물산지위판가격Client(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        TimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async Task<수산물산지위판가격Response> QueryAsync(
        수산물산지위판가격Request request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var options = _options.FisheriesAuction;
        if (request.CollectionDate < new DateOnly(1999, 1, 1)
            || request.CollectionDate > new DateOnly(2016, 1, 19))
        {
            return Fail("산지 위판가격 수록기간은 1999-01-01부터 2016-01-19까지입니다.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return Fail("PublicData:FisheriesAuction:ApiKey 설정이 필요합니다.");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttps && !options.AllowInsecureHttp))
        {
            return Fail("산지 위판가격 원천이 HTTPS를 제공하지 않습니다. 보호된 중계 URL 또는 명시적인 AllowInsecureHttp 검토가 필요합니다.");
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, Math.Max(1, options.MaxPageSize));
        var start = checked(((page - 1) * pageSize) + 1);
        var end = checked(start + pageSize - 1);
        var path = $"/openapi/{Uri.EscapeDataString(options.ApiKey.Trim())}/json/"
                   + $"{Uri.EscapeDataString(options.DatasetName.Trim())}/{start}/{end}"
                   + $"?COLCT_DE={request.CollectionDate:yyyyMMdd}";
        if (!string.IsNullOrWhiteSpace(request.MarketName))
        {
            path += $"&MTC_NM={Uri.EscapeDataString(request.MarketName.Trim())}";
        }

        try
        {
            using var response = await _httpClient.GetAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return Parse(document.RootElement, options.DatasetName);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Fail("역사 수산물 산지 위판가격 원천을 조회하지 못했습니다.");
        }
    }

    internal 수산물산지위판가격Response Parse(JsonElement root, string datasetName)
    {
        if (!TryGet(root, datasetName, out var payload)
            || !TryGet(payload, "row", out var rows))
        {
            return Fail("산지 위판가격 응답 구조를 확인할 수 없습니다.");
        }

        var items = (rows.ValueKind == JsonValueKind.Array
                ? rows.EnumerateArray().ToArray()
                : rows.ValueKind == JsonValueKind.Object ? [rows] : [])
            .Select(Map)
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();

        return new 수산물산지위판가격Response
        {
            Success = true,
            TotalCount = ReadInt(payload, "totalCnt") ?? items.Length,
            Items = items,
            ObservedAt = _timeProvider.GetUtcNow()
        };
    }

    private 수산물산지위판가격Response Fail(string message)
        => new()
        {
            ErrorMessage = message,
            ObservedAt = _timeProvider.GetUtcNow()
        };

    private static 수산물산지위판가격Item? Map(JsonElement row)
    {
        if (!DateOnly.TryParseExact(
                Read(row, "COLCT_DE"),
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return null;
        }

        return new 수산물산지위판가격Item
        {
            CollectionDate = date,
            AuctionCooperativeCode = Read(row, "CNSGSLE_ASSC_CODE"),
            StatisticsFishSpeciesCode = Read(row, "NSO_KDFSH_CODE"),
            FishCooperativeItemCode = Read(row, "SUHYUP_PRDLST_CODE"),
            FishingMethodCode = Read(row, "SBID_TIME"),
            FishSpeciesName = Read(row, "KDFSH_NM"),
            TotalQuantity = ReadDecimal(row, "TOT_QY"),
            TotalTransactionVolume = ReadDecimal(row, "TOT_DLAMT"),
            TotalAmountKrw = ReadDecimal(row, "TOT_PRIC"),
            HighestPriceKrw = ReadDecimal(row, "TOP_PRIC"),
            LowestPriceKrw = ReadDecimal(row, "LWET_PRIC"),
            AveragePriceKrw = ReadDecimal(row, "AVRG_PRIC"),
            UnitName = Read(row, "UNIT_NM"),
            PackageName = Read(row, "FRMLC_NM"),
            SizeName = Read(row, "MG_NM"),
            QualityName = Read(row, "QLITY_NM"),
            MarketName = Read(row, "MTC_NM")
        };
    }

    private static bool TryGet(JsonElement element, string name, out JsonElement value)
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

    private static string Read(JsonElement element, string name)
        => TryGet(element, name, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() ?? string.Empty : value.GetRawText()
            : string.Empty;

    private static decimal? ReadDecimal(JsonElement element, string name)
        => decimal.TryParse(
            Read(element, name).Replace(",", string.Empty),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;

    private static int? ReadInt(JsonElement element, string name)
        => int.TryParse(Read(element, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}
