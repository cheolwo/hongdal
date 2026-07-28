using Ssalddel.ApiMetadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Ssalddel.Security;
using 살뜰.Services.Versioning;

namespace Ssalddel.Hubs;

[SsalddelApiVersion(SsalddelProductVersion.V3_0, FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow, WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.FoodDelivery)]
[Authorize(Policy = "음식점운영자전용")]
public sealed class RestaurantOrderHub : Hub
{
    public const string ReceiveRestaurantOrderNotificationMethod = "ReceiveRestaurantOrderNotification";
    public const string ReceiveRestaurantOrderStatusChangedMethod = "ReceiveRestaurantOrderStatusChanged";

    public Task JoinRestaurantOrders()
    {
        var restaurantId = ResolveRestaurantId();
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildRestaurantGroup(restaurantId));
    }

    public Task LeaveRestaurantOrders()
    {
        var restaurantId = ResolveRestaurantId();
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildRestaurantGroup(restaurantId));
    }

    public static string BuildRestaurantGroup(long restaurantId) => $"restaurant-orders-{restaurantId}";

    private long ResolveRestaurantId()
        => 음식점접근범위Resolver.음식점Id조회(Context.User!)
           ?? throw new HubException("로그인 계정에 음식점 접근 범위가 없습니다.");
}
