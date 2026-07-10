using Hongdal.ApiMetadata;
using Microsoft.AspNetCore.SignalR;
using 홍달.Services.Versioning;

namespace Hongdal.Hubs;

[HongdalApiVersion(HongdalProductVersion.V3_0, FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow, WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.FoodDelivery)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.FoodDelivery)]
public sealed class RestaurantOrderHub : Hub
{
    public const string ReceiveRestaurantOrderNotificationMethod = "ReceiveRestaurantOrderNotification";

    public Task JoinRestaurantOrders(long restaurantId)
    {
        if (restaurantId <= 0)
        {
            throw new HubException("음식점Id가 필요합니다.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, BuildRestaurantGroup(restaurantId));
    }

    public Task LeaveRestaurantOrders(long restaurantId)
    {
        if (restaurantId <= 0)
        {
            throw new HubException("음식점Id가 필요합니다.");
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildRestaurantGroup(restaurantId));
    }

    public static string BuildRestaurantGroup(long restaurantId) => $"restaurant-orders-{restaurantId}";
}
