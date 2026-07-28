using System.Security.Claims;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Security;

public static class 음식점접근범위Resolver
{
    public static long? 음식점Id조회(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var value = user.FindFirstValue(음식점접근ClaimTypes.음식점Id);
        return long.TryParse(value, out var restaurantId) && restaurantId > 0
            ? restaurantId
            : null;
    }
}
