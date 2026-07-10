using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.Food;

public sealed class Kakao좌표변환Service : IKakao좌표변환Service
{
    private readonly HttpClient _httpClient;

    public Kakao좌표변환Service(HttpClient httpClient, IOptions<KakaoLocalOptions> options)
    {
        _httpClient = httpClient;
        var setting = options.Value;

        _httpClient.BaseAddress = new Uri(setting.BaseUrl);
        if (!string.IsNullOrWhiteSpace(setting.RestApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("KakaoAK", setting.RestApiKey);
        }
    }

    public async Task<(double 위도, double 경도)?> 도로명주소좌표변환Async(string 주소, CancellationToken cancellationToken = default)
    {
        var info = await 주소정보조회Async(주소, cancellationToken);
        if (info?.위도 is decimal latitude && info.경도 is decimal longitude)
        {
            return ((double)latitude, (double)longitude);
        }

        return null;
    }

    public async Task<Kakao주소정보?> 주소정보조회Async(string 주소, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(주소))
        {
            return null;
        }

        var requestUri = $"v2/local/search/address.json?query={Uri.EscapeDataString(주소)}";
        var response = await _httpClient.GetFromJsonAsync<KakaoAddressSearchResponse>(requestUri, cancellationToken);
        var first = response?.documents?.FirstOrDefault();
        return first?.To주소정보();
    }

    public async Task<Kakao지역정보?> 좌표지역정보조회Async(decimal 위도, decimal 경도, CancellationToken cancellationToken = default)
    {
        var requestUri = $"v2/local/geo/coord2regioncode.json?x={경도}&y={위도}";
        var response = await _httpClient.GetFromJsonAsync<KakaoRegionCodeResponse>(requestUri, cancellationToken);
        var first = response?.documents?
            .OrderByDescending(x => string.Equals(x.region_type, "H", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        return first is null
            ? null
            : new Kakao지역정보(
                first.region_1depth_name,
                first.region_2depth_name,
                first.region_3depth_name,
                first.address_name);
    }

    private sealed class KakaoAddressSearchResponse
    {
        public List<KakaoAddressDocument>? documents { get; set; }
    }

    private sealed class KakaoRegionCodeResponse
    {
        public List<KakaoRegionCodeDocument>? documents { get; set; }
    }

    private sealed class KakaoRegionCodeDocument
    {
        public string region_type { get; set; } = string.Empty;
        public string address_name { get; set; } = string.Empty;
        public string region_1depth_name { get; set; } = string.Empty;
        public string region_2depth_name { get; set; } = string.Empty;
        public string region_3depth_name { get; set; } = string.Empty;
    }

    private sealed class KakaoAddressDocument
    {
        public string address_name { get; set; } = string.Empty;
        public string x { get; set; } = string.Empty;
        public string y { get; set; } = string.Empty;
        public KakaoAddress? address { get; set; }
        public KakaoRoadAddress? road_address { get; set; }

        public Kakao주소정보 To주소정보()
        {
            var source = road_address as IKakaoRegionSource ?? address;
            _ = decimal.TryParse(y, out var latitude);
            _ = decimal.TryParse(x, out var longitude);

            return new Kakao주소정보(
                address_name,
                road_address?.address_name ?? string.Empty,
                source?.region_1depth_name ?? string.Empty,
                source?.region_2depth_name ?? string.Empty,
                source?.region_3depth_name ?? string.Empty,
                latitude == 0m ? null : latitude,
                longitude == 0m ? null : longitude);
        }
    }

    private interface IKakaoRegionSource
    {
        string region_1depth_name { get; }
        string region_2depth_name { get; }
        string region_3depth_name { get; }
    }

    private sealed class KakaoAddress : IKakaoRegionSource
    {
        public string address_name { get; set; } = string.Empty;
        public string region_1depth_name { get; set; } = string.Empty;
        public string region_2depth_name { get; set; } = string.Empty;
        public string region_3depth_name { get; set; } = string.Empty;
    }

    private sealed class KakaoRoadAddress : IKakaoRegionSource
    {
        public string address_name { get; set; } = string.Empty;
        public string region_1depth_name { get; set; } = string.Empty;
        public string region_2depth_name { get; set; } = string.Empty;
        public string region_3depth_name { get; set; } = string.Empty;
        public string road_name { get; set; } = string.Empty;
    }
}
