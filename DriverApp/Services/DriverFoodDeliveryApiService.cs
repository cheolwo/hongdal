using Ssalddel.Contracts.Common.Drivers;
using Ssalddel.Contracts.Driver.Food;

namespace DriverApp.Services;

public interface IDriverFoodDeliveryApiService
{
    Task<FoodDeliveryDriverWorkspaceDto> 업무공간조회Async(CancellationToken cancellationToken = default);

    Task<FoodDeliveryDriverActionResponse?> 제안수락Async(
        string offerId,
        CancellationToken cancellationToken = default);

    Task<FoodDeliveryDriverActionResponse?> 제안거절Async(
        string offerId,
        CancellationToken cancellationToken = default);

    Task<FoodDeliveryDriverActionResponse?> 묶음수락Async(
        IReadOnlyList<string> offerIds,
        CancellationToken cancellationToken = default);

    Task<FoodDeliveryDriverActionResponse?> 픽업완료Async(
        string offerId,
        CancellationToken cancellationToken = default);

    Task<FoodDeliveryDriverActionResponse?> 전달완료Async(
        string offerId,
        CancellationToken cancellationToken = default);
}

public sealed class DriverFoodDeliveryApiService(IDriverApiClient apiClient) : IDriverFoodDeliveryApiService
{
    private const string BasePath = "api/v1/driver/food-deliveries";

    public async Task<FoodDeliveryDriverWorkspaceDto> 업무공간조회Async(
        CancellationToken cancellationToken = default)
        => await apiClient.GetAsync<FoodDeliveryDriverWorkspaceDto>(
               $"{BasePath}/workspace",
               "음식 배달 업무공간 조회",
               cancellationToken)
           ?? new FoodDeliveryDriverWorkspaceDto();

    public Task<FoodDeliveryDriverActionResponse?> 제안수락Async(
        string offerId,
        CancellationToken cancellationToken = default)
        => PostOfferActionAsync(offerId, "accept", "음식 배달 제안 수락", cancellationToken);

    public Task<FoodDeliveryDriverActionResponse?> 제안거절Async(
        string offerId,
        CancellationToken cancellationToken = default)
        => PostOfferActionAsync(offerId, "reject", "음식 배달 제안 거절", cancellationToken);

    public Task<FoodDeliveryDriverActionResponse?> 묶음수락Async(
        IReadOnlyList<string> offerIds,
        CancellationToken cancellationToken = default)
        => apiClient.PostAsync<FoodDeliveryBundleAcceptRequest, FoodDeliveryDriverActionResponse>(
            $"{BasePath}/bundles/accept",
            new FoodDeliveryBundleAcceptRequest { OfferIds = offerIds },
            "음식 배달 묶음 제안 수락",
            cancellationToken);

    public Task<FoodDeliveryDriverActionResponse?> 픽업완료Async(
        string offerId,
        CancellationToken cancellationToken = default)
        => PostOfferActionAsync(offerId, "pickup-complete", "음식 배달 픽업 완료", cancellationToken);

    public Task<FoodDeliveryDriverActionResponse?> 전달완료Async(
        string offerId,
        CancellationToken cancellationToken = default)
        => PostOfferActionAsync(offerId, "delivery-complete", "음식 배달 전달 완료", cancellationToken);

    private Task<FoodDeliveryDriverActionResponse?> PostOfferActionAsync(
        string offerId,
        string action,
        string operationName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(offerId);
        var encodedOfferId = Uri.EscapeDataString(offerId.Trim());
        return apiClient.PostAsync<FoodDeliveryDriverActionResponse>(
            $"{BasePath}/offers/{encodedOfferId}/{action}",
            operationName,
            cancellationToken);
    }
}
