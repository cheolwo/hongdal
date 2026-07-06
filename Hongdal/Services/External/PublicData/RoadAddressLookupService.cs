using Hongdal.Contracts.Common.PublicData;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.External.PublicData;

public sealed class RoadAddressLookupService : IRoadAddressLookupService
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;

    public RoadAddressLookupService(HttpClient httpClient, IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PublicDataLookupResponse<RoadAddressItem>> SearchAsync(
        RoadAddressSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            return Fail("PublicData:RoadAddress:ConfirmKey 또는 PublicData:ServiceKey 설정이 필요합니다.", request.Page, request.PageSize);
        }

        if (string.IsNullOrWhiteSpace(request.Keyword))
        {
            return Fail("주소 검색어가 필요합니다.", request.Page, request.PageSize);
        }

        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var query = new Dictionary<string, string?>
        {
            ["confmKey"] = serviceKey,
            ["currentPage"] = page.ToString(),
            ["countPerPage"] = pageSize.ToString(),
            ["keyword"] = request.Keyword,
            ["resultType"] = "json",
            ["hstryYn"] = "Y",
            ["relJibun"] = "Y"
        };

        var relative = QueryHelpers.AddQueryString(_options.RoadAddress.SearchPath.TrimStart('/'), query);

        try
        {
            using var response = await _httpClient.GetAsync(relative, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Fail($"HTTP {(int)response.StatusCode}", page, pageSize);
            }

            var items = PublicDataParsing.ReadItems(body)
                .Select(ToRoadAddressItem)
                .Where(item => !string.IsNullOrWhiteSpace(item.RoadAddress) || !string.IsNullOrWhiteSpace(item.JibunAddress))
                .ToArray();

            return new PublicDataLookupResponse<RoadAddressItem>
            {
                Success = true,
                Page = page,
                PageSize = pageSize,
                TotalCount = PublicDataParsing.ReadTotalCount(body),
                Items = items
            };
        }
        catch (Exception ex)
        {
            return Fail($"주소 API 호출 실패: {ex.Message}", page, pageSize);
        }
    }

    private static RoadAddressItem ToRoadAddressItem(Dictionary<string, string?> item)
    {
        return new RoadAddressItem
        {
            RoadAddress = PublicDataParsing.FirstValue(item, "roadAddr", "roadAddrPart1") ?? string.Empty,
            JibunAddress = PublicDataParsing.FirstValue(item, "jibunAddr") ?? string.Empty,
            ZipCode = PublicDataParsing.FirstValue(item, "zipNo") ?? string.Empty,
            AdministrativeCode = PublicDataParsing.FirstValue(item, "admCd") ?? string.Empty,
            RoadNameManagementNo = PublicDataParsing.FirstValue(item, "rnMgtSn") ?? string.Empty,
            BuildingManagementNo = PublicDataParsing.FirstValue(item, "bdMgtSn") ?? string.Empty,
            RelatedJibun = PublicDataParsing.FirstValue(item, "relJibun"),
            EnglishAddress = PublicDataParsing.FirstValue(item, "engAddr")
        };
    }

    private static PublicDataLookupResponse<RoadAddressItem> Fail(string message, int page, int pageSize)
    {
        return new PublicDataLookupResponse<RoadAddressItem>
        {
            Success = false,
            ErrorMessage = message,
            Page = NormalizePage(page),
            PageSize = NormalizePageSize(pageSize)
        };
    }

    private static int NormalizePage(int value) => Math.Max(1, value);

    private static int NormalizePageSize(int value) => Math.Clamp(value, 1, 30);

    private string ResolveServiceKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.RoadAddress.ConfirmKey))
        {
            return _options.RoadAddress.ConfirmKey;
        }

        return _options.ServiceKey;
    }
}
