using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Restaurants;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class RestaurantSearchPolicyDefaultsTests
{
    [Fact]
    public async Task 주문자와기존음식Api는_같은7Km기본값을사용한다()
    {
        var policy = new RestaurantSearchPolicyDto();
        var stored = await new InMemoryRestaurantSearchPolicyStore().GetAsync();

        Assert.Equal(7d, policy.DefaultRadiusKm);
        Assert.Equal(7d, policy.RecommendedRadiusKm);
        Assert.Equal([3d, 5d, 7d, 10d], policy.QuickRadiusOptions);
        Assert.Equal(7m, new 음식점공개목록조회요청().반경Km);
        Assert.Equal(7m, new 음식점가까운조회요청().반경Km);
        Assert.Equal(policy.DefaultRadiusKm, stored.DefaultRadiusKm);
        Assert.Equal(policy.QuickRadiusOptions, stored.QuickRadiusOptions);
    }
}
