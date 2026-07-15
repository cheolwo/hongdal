using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Work;
using Hongdal.Contracts.Common.Community;

namespace DriverApp.Services;

public sealed class DriverWorkApiService : IDriverWorkApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;

    public DriverWorkApiService(HttpClient httpClient, IAuthSession authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public async Task<기사운행시작응답?> 운행시작Async(기사운행시작요청 request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, "api/v1/driver/work/start");
        httpRequest.Content = JsonContent.Create(request);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<기사운행시작응답>(cancellationToken);
    }

    public async Task 운행종료Async(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/v1/driver/work/stop");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<기사위치갱신응답?> 위치갱신Async(기사위치갱신요청 request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, "api/v1/driver/work/location");
        httpRequest.Content = JsonContent.Create(request);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<기사위치갱신응답>(cancellationToken);
    }

    public async Task<IReadOnlyList<CommunityDriverInquiryResponse>> 커뮤니티의뢰목록Async(
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/v1/driver/community-inquiries");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CommunityDriverInquiryResponse>>(cancellationToken)
               ?? [];
    }

    public async Task<CommunityDriverInquiryResponse?> 커뮤니티의뢰답변Async(
        Guid inquiryId,
        CommunityDriverInquiryDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, $"api/v1/driver/community-inquiries/{inquiryId}/decision");
        httpRequest.Content = JsonContent.Create(request);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityDriverInquiryResponse>(cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        }

        return request;
    }
}
