using Ssalddel.FoodApi.Options;
using Microsoft.Extensions.Options;

namespace Ssalddel.FoodApi.Application.Pricing;

public sealed class FoodDeliveryPricingPolicyMemoryStore : IFoodDeliveryPricingPolicyStore
{
    private readonly object _sync = new();
    private FoodDeliveryPricingOptions _current;

    public FoodDeliveryPricingPolicyMemoryStore(IOptions<FoodDeliveryPricingOptions> options)
    {
        _current = Clone(options.Value);
    }

    public FoodDeliveryPricingOptions Get()
    {
        lock (_sync)
        {
            return Clone(_current);
        }
    }

    public FoodDeliveryPricingOptions Update(FoodDeliveryPricingOptions policy)
    {
        Validate(policy);

        lock (_sync)
        {
            _current = Clone(policy);
            return Clone(_current);
        }
    }

    private static void Validate(FoodDeliveryPricingOptions policy)
    {
        if (policy.BaseFee < 0 || policy.MinimumFee < 0 || policy.DistanceUnitFee < 0)
        {
            throw new ArgumentException("플랫폼 요금은 0 이상이어야 합니다.");
        }

        if (policy.DriverBasePayout < 0 || policy.DriverMinimumPayout < 0 || policy.DriverDistanceUnitPayout < 0)
        {
            throw new ArgumentException("기사 지급액은 0 이상이어야 합니다.");
        }

        if (policy.IncludedDistanceMeters < 0)
        {
            throw new ArgumentException("기본 포함거리는 0 이상이어야 합니다.");
        }

        if (policy.DistanceUnitMeters <= 0)
        {
            throw new ArgumentException("거리 단위는 1m 이상이어야 합니다.");
        }
    }

    private static FoodDeliveryPricingOptions Clone(FoodDeliveryPricingOptions source)
    {
        return new FoodDeliveryPricingOptions
        {
            BaseFee = source.BaseFee,
            IncludedDistanceMeters = source.IncludedDistanceMeters,
            DistanceUnitMeters = source.DistanceUnitMeters,
            DistanceUnitFee = source.DistanceUnitFee,
            MinimumFee = source.MinimumFee,
            DriverBasePayout = source.DriverBasePayout,
            DriverDistanceUnitPayout = source.DriverDistanceUnitPayout,
            DriverMinimumPayout = source.DriverMinimumPayout
        };
    }
}
