namespace Deliver.Services;

public sealed class DeliveryDriverAppProfile
{
    public string AppKey { get; } = "DeliveryDriverApp";
    public string DisplayName { get; } = "홍달 배달기사";
    public string DriverRole { get; } = "배달기사";
    public string PrimaryWorkType { get; } = "FoodDelivery";
}
