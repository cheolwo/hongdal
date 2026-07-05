using System.Net.Http.Json;
using Hongdal.Contracts.Common.Orderer;

namespace OrdererApp.Services;

public sealed record RestaurantSearchPolicy(
    double DefaultRadiusKm,
    double MinRadiusKm,
    double MaxRadiusKm,
    double RadiusStepKm,
    IReadOnlyList<double> QuickRadiusOptions,
    double RecommendedRadiusKm,
    double DeliveryFeeCautionRadiusKm);

public interface IRestaurantSearchPolicyService
{
    Task<RestaurantSearchPolicy> GetPolicyAsync(CancellationToken cancellationToken = default);
}

public sealed class SampleRestaurantSearchPolicyService : IRestaurantSearchPolicyService
{
    public static readonly RestaurantSearchPolicy Policy = new(
        DefaultRadiusKm: 7,
        MinRadiusKm: 1,
        MaxRadiusKm: 10,
        RadiusStepKm: 0.5,
        QuickRadiusOptions: [3, 5, 7, 10],
        RecommendedRadiusKm: 7,
        DeliveryFeeCautionRadiusKm: 10);

    public Task<RestaurantSearchPolicy> GetPolicyAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Policy);
    }
}

public sealed class HttpRestaurantSearchPolicyService : IRestaurantSearchPolicyService
{
    private readonly HttpClient _httpClient;

    public HttpRestaurantSearchPolicyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RestaurantSearchPolicy> GetPolicyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await _httpClient.GetFromJsonAsync<RestaurantSearchPolicyDto>(
                "api/v1/orderer/restaurant-search-policy",
                cancellationToken);

            return policy is null
                ? SampleRestaurantSearchPolicyService.Policy
                : ToAppPolicy(policy);
        }
        catch (HttpRequestException)
        {
            return SampleRestaurantSearchPolicyService.Policy;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return SampleRestaurantSearchPolicyService.Policy;
        }
    }

    private static RestaurantSearchPolicy ToAppPolicy(RestaurantSearchPolicyDto dto)
    {
        return new RestaurantSearchPolicy(
            dto.DefaultRadiusKm,
            dto.MinRadiusKm,
            dto.MaxRadiusKm,
            dto.RadiusStepKm,
            dto.QuickRadiusOptions,
            dto.RecommendedRadiusKm,
            dto.DeliveryFeeCautionRadiusKm);
    }
}
