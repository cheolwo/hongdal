using System.Net.Http.Headers;

namespace SsalddelAdmin.Services;

public sealed class 기사운행현황Service
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;

    public 기사운행현황Service(HttpClient httpClient, 관리자인증세션Service session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<IReadOnlyList<현재운행기사응답>> 현재운행기사조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<현재운행기사응답>>("api/v1/admin/drivers/operating", cancellationToken);
        return result ?? [];
    }

    private void ApplyAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            throw new InvalidOperationException("로그인이 필요합니다.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
    }
}

public sealed class 현재운행기사응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public DateTime? 최근근무시작시각 { get; set; }
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
}
