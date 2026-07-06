using Hongdal.Contracts.Common.PublicData;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.External.PublicData;

public sealed class ApartmentComplexLookupService : IApartmentComplexLookupService
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;

    public ApartmentComplexLookupService(HttpClient httpClient, IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PublicDataLookupResponse<ApartmentComplexItem>> SearchAsync(
        ApartmentComplexSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            return Fail<ApartmentComplexItem>("PublicData:ApartmentComplex:ServiceKey 또는 PublicData:DataGoKrServiceKey 설정이 필요합니다.", request.Page, request.PageSize);
        }

        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var query = new Dictionary<string, string?>
        {
            ["serviceKey"] = serviceKey,
            ["pageNo"] = page.ToString(),
            ["numOfRows"] = pageSize.ToString(),
            ["sidoCode"] = request.SidoCode,
            ["sigunguCode"] = request.SigunguCode,
            ["emdCode"] = request.EupmyeondongCode,
            ["roadName"] = request.RoadName,
            ["kaptName"] = request.Keyword
        };

        var relative = QueryHelpers.AddQueryString(_options.ApartmentComplex.ListPath.TrimStart('/'), query);
        try
        {
            using var response = await _httpClient.GetAsync(relative, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Fail<ApartmentComplexItem>($"HTTP {(int)response.StatusCode}", page, pageSize);
            }

            var items = PublicDataParsing.ReadItems(body)
                .Select(ToApartmentComplexItem)
                .Where(item => !string.IsNullOrWhiteSpace(item.ComplexCode) || !string.IsNullOrWhiteSpace(item.ComplexName))
                .ToArray();

            return new PublicDataLookupResponse<ApartmentComplexItem>
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
            return Fail<ApartmentComplexItem>($"공동주택 단지 목록 API 호출 실패: {ex.Message}", page, pageSize);
        }
    }

    public async Task<PublicDataLookupResponse<ApartmentComplexBasicItem>> GetBasicInfoAsync(
        ApartmentComplexBasicRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            return Fail<ApartmentComplexBasicItem>("PublicData:ApartmentComplex:ServiceKey 또는 PublicData:DataGoKrServiceKey 설정이 필요합니다.", 1, 1);
        }

        if (string.IsNullOrWhiteSpace(request.ComplexCode))
        {
            return Fail<ApartmentComplexBasicItem>("공동주택 단지 코드가 필요합니다.", 1, 1);
        }

        var query = new Dictionary<string, string?>
        {
            ["serviceKey"] = serviceKey,
            ["kaptCode"] = request.ComplexCode,
            ["pageNo"] = "1",
            ["numOfRows"] = "1"
        };

        var relative = QueryHelpers.AddQueryString(_options.ApartmentComplex.BasicInfoPath.TrimStart('/'), query);
        try
        {
            using var response = await _httpClient.GetAsync(relative, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Fail<ApartmentComplexBasicItem>($"HTTP {(int)response.StatusCode}", 1, 1);
            }

            var items = PublicDataParsing.ReadItems(body)
                .Select(ToApartmentComplexBasicItem)
                .Where(item => !string.IsNullOrWhiteSpace(item.ComplexCode) || !string.IsNullOrWhiteSpace(item.ComplexName))
                .ToArray();

            return new PublicDataLookupResponse<ApartmentComplexBasicItem>
            {
                Success = true,
                Page = 1,
                PageSize = 1,
                TotalCount = PublicDataParsing.ReadTotalCount(body),
                Items = items
            };
        }
        catch (Exception ex)
        {
            return Fail<ApartmentComplexBasicItem>($"공동주택 기본정보 API 호출 실패: {ex.Message}", 1, 1);
        }
    }

    private static ApartmentComplexItem ToApartmentComplexItem(Dictionary<string, string?> item)
    {
        return new ApartmentComplexItem
        {
            ComplexCode = PublicDataParsing.FirstValue(item, "kaptCode", "complexCode", "단지코드") ?? string.Empty,
            ComplexName = PublicDataParsing.FirstValue(item, "kaptName", "complexName", "단지명") ?? string.Empty,
            Sido = PublicDataParsing.FirstValue(item, "as1", "sido", "시도"),
            Sigungu = PublicDataParsing.FirstValue(item, "as2", "sigungu", "시군구"),
            Eupmyeondong = PublicDataParsing.FirstValue(item, "as3", "emd", "읍면동"),
            RoadAddress = PublicDataParsing.FirstValue(item, "roadAddr", "도로명주소"),
            LegalDongAddress = PublicDataParsing.FirstValue(item, "jibunAddr", "법정동주소")
        };
    }

    private static ApartmentComplexBasicItem ToApartmentComplexBasicItem(Dictionary<string, string?> item)
    {
        return new ApartmentComplexBasicItem
        {
            ComplexCode = PublicDataParsing.FirstValue(item, "kaptCode", "complexCode", "단지코드") ?? string.Empty,
            ComplexName = PublicDataParsing.FirstValue(item, "kaptName", "complexName", "단지명") ?? string.Empty,
            HouseholdCount = PublicDataParsing.FirstInt(item, "hoCnt", "kaptdPcnt", "세대수"),
            BuildingCount = PublicDataParsing.FirstInt(item, "dongCnt", "kaptdDcnt", "동수"),
            ManagementType = PublicDataParsing.FirstValue(item, "codeMgr", "관리방식"),
            HeatingType = PublicDataParsing.FirstValue(item, "codeHeat", "난방방식"),
            ApprovalDate = PublicDataParsing.FirstValue(item, "useAprDay", "사용승인일"),
            RoadAddress = PublicDataParsing.FirstValue(item, "roadAddr", "도로명주소"),
            LegalDongAddress = PublicDataParsing.FirstValue(item, "jibunAddr", "법정동주소")
        };
    }

    private static PublicDataLookupResponse<T> Fail<T>(string message, int page, int pageSize)
    {
        return new PublicDataLookupResponse<T>
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
        if (!string.IsNullOrWhiteSpace(_options.ApartmentComplex.ServiceKey))
        {
            return _options.ApartmentComplex.ServiceKey;
        }

        if (!string.IsNullOrWhiteSpace(_options.DataGoKrServiceKey))
        {
            return _options.DataGoKrServiceKey;
        }

        return _options.ServiceKey;
    }
}
