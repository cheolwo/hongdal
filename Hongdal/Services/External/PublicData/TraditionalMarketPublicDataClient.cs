using Hongdal.Contracts.Common.TraditionalMarkets;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.External.PublicData;

public interface ITraditionalMarketPublicDataClient
{
    Task<TraditionalMarketPublicDataPage> FetchPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed class TraditionalMarketPublicDataPage
{
    public IReadOnlyList<TraditionalMarketPublicDataItem> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed class TraditionalMarketPublicDataItem
{
    public string MarketCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string MarketType { get; init; } = string.Empty;
    public string LotNumberAddress { get; init; } = string.Empty;
    public string RoadAddress { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;
    public string CityCounty { get; init; } = string.Empty;
    public TraditionalMarketFacilityResponse Facilities { get; init; } = new();
}

public sealed class TraditionalMarketPublicDataClient : ITraditionalMarketPublicDataClient
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;

    public TraditionalMarketPublicDataClient(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<TraditionalMarketPublicDataPage> FetchPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            throw new InvalidOperationException(
                "PublicData:TraditionalMarket:ServiceKey 또는 PublicData:DataGoKrServiceKey 설정이 필요합니다.");
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 1000);
        var query = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["perPage"] = pageSize.ToString(),
            ["returnType"] = "JSON",
            ["serviceKey"] = serviceKey
        };
        var relativePath = QueryHelpers.AddQueryString(
            _options.TraditionalMarket.ApiPath.TrimStart('/'),
            query);

        using var response = await _httpClient.GetAsync(relativePath, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"전통시장 공공데이터 호출 실패: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var items = PublicDataParsing.ReadItems(body)
            .Select(ToItem)
            .Where(x => !string.IsNullOrWhiteSpace(x.MarketCode) && !string.IsNullOrWhiteSpace(x.Name))
            .ToArray();

        return new TraditionalMarketPublicDataPage
        {
            Items = items,
            TotalCount = PublicDataParsing.ReadTotalCount(body) ?? items.Length,
            Page = page,
            PageSize = pageSize
        };
    }

    private static TraditionalMarketPublicDataItem ToItem(Dictionary<string, string?> values)
        => new()
        {
            MarketCode = Value(values, "시장코드"),
            Name = Value(values, "시장명"),
            MarketType = Value(values, "시장 유형", "시장유형"),
            LotNumberAddress = Value(values, "지번주소"),
            RoadAddress = Value(values, "도로명주소"),
            Province = Value(values, "시도"),
            CityCounty = Value(values, "시군구"),
            Facilities = new TraditionalMarketFacilityResponse
            {
                HasArcade = Flag(values, "아케이드 보유 여부"),
                HasElevatorOrEscalator = Flag(values, "엘리베이터_에스컬레이터_보유여부"),
                HasCustomerSupportCenter = Flag(values, "고객지원센터 보유 여부"),
                HasSprinkler = Flag(values, "스프링쿨러 보유 여부"),
                HasFireDetector = Flag(values, "화재감지기 보유여부"),
                HasChildrenPlayroom = Flag(values, "유아놀이방_보유여부"),
                HasCallCenter = Flag(values, "종합콜센터_보유여부"),
                HasCustomerLounge = Flag(values, "고객휴게실_보유여부"),
                HasNursingCenter = Flag(values, "수유센터_보유여부"),
                HasLocker = Flag(values, "물품보관함_보유여부"),
                HasBicycleStorage = Flag(values, "자전거보관함_보유여부"),
                HasSportsFacility = Flag(values, "체육시설_보유여부"),
                HasLibrary = Flag(values, "간이 도서관_보유여부"),
                HasShoppingCart = Flag(values, "쇼핑카트_보유여부"),
                HasForeignVisitorCenter = Flag(values, "외국인 안내센터_보유여부"),
                HasCustomerPath = Flag(values, "고객동선통로_보유여부"),
                HasBroadcastCenter = Flag(values, "방송센터_보유여부"),
                HasCultureClassroom = Flag(values, "문화교실_보유여부"),
                HasSharedLogisticsWarehouse = Flag(values, "공동물류창고_보유여부"),
                HasDedicatedParking = Flag(values, "시장전용 고객주차장_보유여부"),
                HasTrainingRoom = Flag(values, "교육장_보유여부"),
                HasMeetingRoom = Flag(values, "회의실_보유여부"),
                HasAed = Flag(values, "자동심장충격기_보유여부")
            }
        };

    private static string Value(Dictionary<string, string?> values, params string[] keys)
        => PublicDataParsing.FirstValue(values, keys)?.Trim() ?? string.Empty;

    private static bool? Flag(Dictionary<string, string?> values, params string[] keys)
    {
        var value = PublicDataParsing.FirstValue(values, keys)?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.ToUpperInvariant() switch
        {
            "Y" or "YES" or "TRUE" or "1" or "유" or "있음" => true,
            "N" or "NO" or "FALSE" or "0" or "무" or "없음" => false,
            _ => null
        };
    }

    private string ResolveServiceKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.TraditionalMarket.ServiceKey))
        {
            return _options.TraditionalMarket.ServiceKey;
        }

        if (!string.IsNullOrWhiteSpace(_options.DataGoKrServiceKey))
        {
            return _options.DataGoKrServiceKey;
        }

        return _options.ServiceKey;
    }
}
