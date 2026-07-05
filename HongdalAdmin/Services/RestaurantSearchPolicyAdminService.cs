using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Common.Orderer;

namespace HongdalAdmin.Services;

public sealed class RestaurantSearchPolicyAdminService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;

    public RestaurantSearchPolicyAdminService(HttpClient httpClient, 관리자인증세션Service session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<RestaurantSearchPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/v1/admin/orderer/restaurant-search-policy");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RestaurantSearchPolicyDto>(cancellationToken: cancellationToken)
               ?? new RestaurantSearchPolicyDto();
    }

    public async Task<RestaurantSearchPolicyDto> UpdateAsync(RestaurantSearchPolicyUpdateRequest updateRequest, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, "api/v1/admin/orderer/restaurant-search-policy");
        request.Content = JsonContent.Create(updateRequest);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RestaurantSearchPolicyDto>(cancellationToken: cancellationToken)
               ?? new RestaurantSearchPolicyDto();
    }

    public async Task<RestaurantSearchPolicyDto> ResetAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/v1/admin/orderer/restaurant-search-policy/reset");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RestaurantSearchPolicyDto>(cancellationToken: cancellationToken)
               ?? new RestaurantSearchPolicyDto();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return request;
    }
}
