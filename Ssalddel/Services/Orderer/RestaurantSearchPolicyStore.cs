using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface IRestaurantSearchPolicyStore
{
    Task<RestaurantSearchPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<RestaurantSearchPolicyDto> UpdateAsync(RestaurantSearchPolicyUpdateRequest request, string? updatedBy, CancellationToken cancellationToken = default);

    Task<RestaurantSearchPolicyDto> ResetAsync(string? updatedBy, CancellationToken cancellationToken = default);
}

public sealed class InMemoryRestaurantSearchPolicyStore : IRestaurantSearchPolicyStore
{
    private readonly object _sync = new();
    private RestaurantSearchPolicyDto _policy = CreateDefaultPolicy(null);

    public Task<RestaurantSearchPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(Clone(_policy));
        }
    }

    public Task<RestaurantSearchPolicyDto> UpdateAsync(RestaurantSearchPolicyUpdateRequest request, string? updatedBy, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request);

        var next = new RestaurantSearchPolicyDto
        {
            DefaultRadiusKm = request.DefaultRadiusKm,
            MinRadiusKm = request.MinRadiusKm,
            MaxRadiusKm = request.MaxRadiusKm,
            RadiusStepKm = request.RadiusStepKm,
            QuickRadiusOptions = NormalizeQuickOptions(request.QuickRadiusOptions),
            RecommendedRadiusKm = request.RecommendedRadiusKm,
            DeliveryFeeCautionRadiusKm = request.DeliveryFeeCautionRadiusKm,
            UpdatedBy = NormalizeUpdatedBy(updatedBy),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        lock (_sync)
        {
            _policy = next;
            return Task.FromResult(Clone(_policy));
        }
    }

    public Task<RestaurantSearchPolicyDto> ResetAsync(string? updatedBy, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _policy = CreateDefaultPolicy(updatedBy);
            return Task.FromResult(Clone(_policy));
        }
    }

    private static RestaurantSearchPolicyDto CreateDefaultPolicy(string? updatedBy)
    {
        return new RestaurantSearchPolicyDto
        {
            DefaultRadiusKm = RestaurantSearchPolicyDefaults.DefaultRadiusKm,
            MinRadiusKm = RestaurantSearchPolicyDefaults.MinRadiusKm,
            MaxRadiusKm = RestaurantSearchPolicyDefaults.MaxRadiusKm,
            RadiusStepKm = RestaurantSearchPolicyDefaults.RadiusStepKm,
            QuickRadiusOptions = RestaurantSearchPolicyDefaults.CreateQuickRadiusOptions(),
            RecommendedRadiusKm = RestaurantSearchPolicyDefaults.RecommendedRadiusKm,
            DeliveryFeeCautionRadiusKm = RestaurantSearchPolicyDefaults.DeliveryFeeCautionRadiusKm,
            UpdatedBy = NormalizeUpdatedBy(updatedBy),
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static RestaurantSearchPolicyDto Clone(RestaurantSearchPolicyDto source)
    {
        return new RestaurantSearchPolicyDto
        {
            DefaultRadiusKm = source.DefaultRadiusKm,
            MinRadiusKm = source.MinRadiusKm,
            MaxRadiusKm = source.MaxRadiusKm,
            RadiusStepKm = source.RadiusStepKm,
            QuickRadiusOptions = source.QuickRadiusOptions.ToList(),
            RecommendedRadiusKm = source.RecommendedRadiusKm,
            DeliveryFeeCautionRadiusKm = source.DeliveryFeeCautionRadiusKm,
            UpdatedBy = source.UpdatedBy,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static void Validate(RestaurantSearchPolicyUpdateRequest request)
    {
        if (request.MinRadiusKm <= 0)
        {
            throw new ArgumentException("Minimum radius must be greater than 0.");
        }

        if (request.MaxRadiusKm < request.MinRadiusKm)
        {
            throw new ArgumentException("Maximum radius must be greater than or equal to minimum radius.");
        }

        if (request.DefaultRadiusKm < request.MinRadiusKm || request.DefaultRadiusKm > request.MaxRadiusKm)
        {
            throw new ArgumentException("Default radius must be within the configured range.");
        }

        if (request.RecommendedRadiusKm < request.MinRadiusKm || request.RecommendedRadiusKm > request.MaxRadiusKm)
        {
            throw new ArgumentException("Recommended radius must be within the configured range.");
        }

        if (request.DeliveryFeeCautionRadiusKm < request.MinRadiusKm || request.DeliveryFeeCautionRadiusKm > request.MaxRadiusKm)
        {
            throw new ArgumentException("Delivery fee caution radius must be within the configured range.");
        }

        if (request.RadiusStepKm <= 0)
        {
            throw new ArgumentException("Radius step must be greater than 0.");
        }

        var quickOptions = NormalizeQuickOptions(request.QuickRadiusOptions);
        if (quickOptions.Count == 0)
        {
            throw new ArgumentException("At least one quick radius option is required.");
        }

        if (quickOptions.Any(x => x < request.MinRadiusKm || x > request.MaxRadiusKm))
        {
            throw new ArgumentException("Quick radius options must be within the configured range.");
        }
    }

    private static List<double> NormalizeQuickOptions(IEnumerable<double> values)
    {
        return values
            .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            .Select(x => Math.Round(x, 2, MidpointRounding.AwayFromZero))
            .Distinct()
            .Order()
            .ToList();
    }

    private static string? NormalizeUpdatedBy(string? updatedBy)
    {
        return string.IsNullOrWhiteSpace(updatedBy) ? null : updatedBy.Trim();
    }
}
