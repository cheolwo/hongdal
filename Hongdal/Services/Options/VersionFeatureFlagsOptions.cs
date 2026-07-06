namespace 홍달.Services.Options;

public sealed class VersionFeatureFlagsOptions
{
    public const string SectionName = "VersionFeatureFlags";

    public bool CargoYongdalV1 { get; set; } = true;

    public bool WarehouseV15 { get; set; }

    public bool CustomsHsV20 { get; set; }

    public bool ApartmentGroupOrderV25 { get; set; }

    public bool FoodDeliveryV30 { get; set; }

    public bool HongdalMartV35 { get; set; }
}
