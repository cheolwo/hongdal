using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.Versioning;

public sealed class VersionFeatureFlagService : IVersionFeatureFlagService
{
    private readonly IOptionsMonitor<VersionFeatureFlagsOptions> _options;

    public VersionFeatureFlagService(IOptionsMonitor<VersionFeatureFlagsOptions> options)
    {
        _options = options;
    }

    public bool IsEnabled(string featureKey)
    {
        var flags = _options.CurrentValue;
        return featureKey switch
        {
            VersionFeatureFlagKeys.CargoYongdalV1 => flags.CargoYongdalV1,
            VersionFeatureFlagKeys.WarehouseV15 => flags.WarehouseV15,
            VersionFeatureFlagKeys.CustomsHsV20 => flags.CustomsHsV20,
            VersionFeatureFlagKeys.ApartmentGroupOrderV25 => flags.ApartmentGroupOrderV25,
            VersionFeatureFlagKeys.FoodDeliveryV30 => flags.FoodDeliveryV30,
            VersionFeatureFlagKeys.HongdalMartV35 => flags.HongdalMartV35,
            _ => false
        };
    }

    public IReadOnlyDictionary<string, bool> GetAll()
    {
        var flags = _options.CurrentValue;
        return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            [VersionFeatureFlagKeys.CargoYongdalV1] = flags.CargoYongdalV1,
            [VersionFeatureFlagKeys.WarehouseV15] = flags.WarehouseV15,
            [VersionFeatureFlagKeys.CustomsHsV20] = flags.CustomsHsV20,
            [VersionFeatureFlagKeys.ApartmentGroupOrderV25] = flags.ApartmentGroupOrderV25,
            [VersionFeatureFlagKeys.FoodDeliveryV30] = flags.FoodDeliveryV30,
            [VersionFeatureFlagKeys.HongdalMartV35] = flags.HongdalMartV35
        };
    }
}

public static class VersionFeatureFlagKeys
{
    public const string CargoYongdalV1 = nameof(CargoYongdalV1);

    public const string WarehouseV15 = nameof(WarehouseV15);

    public const string CustomsHsV20 = nameof(CustomsHsV20);

    public const string ApartmentGroupOrderV25 = nameof(ApartmentGroupOrderV25);

    public const string FoodDeliveryV30 = nameof(FoodDeliveryV30);

    public const string HongdalMartV35 = nameof(HongdalMartV35);
}
