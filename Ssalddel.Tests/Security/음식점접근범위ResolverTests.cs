using System.Security.Claims;
using Ssalddel.Contracts.Food;
using Ssalddel.Security;

namespace Ssalddel.Tests.Security;

public sealed class 음식점접근범위ResolverTests
{
    [Fact]
    public void 서버발급음식점클레임만_양의음식점Id로해석한다()
    {
        var user = Principal(new Claim(음식점접근ClaimTypes.음식점Id, "101"));

        Assert.Equal(101, 음식점접근범위Resolver.음식점Id조회(user));
        Assert.Null(음식점접근범위Resolver.음식점Id조회(Principal()));
        Assert.Null(음식점접근범위Resolver.음식점Id조회(
            Principal(new Claim(음식점접근ClaimTypes.음식점Id, "-1"))));
    }

    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "test"));
}
