using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Food;
using Ssalddel.Security;

namespace Ssalddel.Tests.Security;

public sealed class 공급조직접근AccessorTests
{
    [Fact]
    public void 음식점과살들마트는_서버발급Claim에서조직범위를조회한다()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(음식점접근ClaimTypes.음식점Id, "101"),
                new Claim(공급조직접근ClaimTypes.살들마트Id, "mart-seoul-01")
            ], "test"))
        };
        var accessor = new 공급조직접근Accessor(new HttpContextAccessor
        {
            HttpContext = context
        });

        Assert.Equal(
            "101",
            accessor.조직참조Key조회(공급이용조직유형코드.음식점));
        Assert.Equal(
            "mart-seoul-01",
            accessor.조직참조Key조회(공급이용조직유형코드.살들마트));
        Assert.Null(accessor.조직참조Key조회("Unknown"));
    }
}
