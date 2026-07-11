using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Common.Orderer;

namespace HongdalAdmin.Services;

public sealed class RestaurantSearchPolicyAdminService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly bool _useMemoryFallback;

    public RestaurantSearchPolicyAdminService(HttpClient httpClient, 관리자인증세션Service session, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _session = session;
        _useMemoryFallback = configuration.GetValue("AdminData:UseMemory", false);
    }

    public async Task<RestaurantSearchPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return BuildMemoryPolicy();
        }

        using var request = CreateRequest(HttpMethod.Get, "api/v1/admin/orderer/restaurant-search-policy");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RestaurantSearchPolicyDto>(cancellationToken: cancellationToken)
               ?? new RestaurantSearchPolicyDto();
    }

    public async Task<RestaurantSearchPolicyDto> UpdateAsync(RestaurantSearchPolicyUpdateRequest updateRequest, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return new RestaurantSearchPolicyDto
            {
                DefaultRadiusKm = updateRequest.DefaultRadiusKm,
                MinRadiusKm = updateRequest.MinRadiusKm,
                MaxRadiusKm = updateRequest.MaxRadiusKm,
                RadiusStepKm = updateRequest.RadiusStepKm,
                QuickRadiusOptions = updateRequest.QuickRadiusOptions,
                RecommendedRadiusKm = updateRequest.RecommendedRadiusKm,
                DeliveryFeeCautionRadiusKm = updateRequest.DeliveryFeeCautionRadiusKm,
                UpdatedBy = _session.UserName,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        using var request = CreateRequest(HttpMethod.Put, "api/v1/admin/orderer/restaurant-search-policy");
        request.Content = JsonContent.Create(updateRequest);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RestaurantSearchPolicyDto>(cancellationToken: cancellationToken)
               ?? new RestaurantSearchPolicyDto();
    }

    public async Task<RestaurantSearchPolicyDto> ResetAsync(CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return BuildMemoryPolicy();
        }

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

    private static RestaurantSearchPolicyDto BuildMemoryPolicy()
        => new()
        {
            DefaultRadiusKm = 5,
            MinRadiusKm = 1,
            MaxRadiusKm = 12,
            RadiusStepKm = 0.5,
            QuickRadiusOptions = [3, 5, 7, 10],
            RecommendedRadiusKm = 5,
            DeliveryFeeCautionRadiusKm = 9,
            UpdatedBy = "memory",
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
