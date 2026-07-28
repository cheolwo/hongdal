using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Shipper.Request;
using 살뜰.도메인.화주;

namespace Ssalddel.Tests.Application.Shipper.Request;

public sealed class 주문자권한검사Tests
{
    [Fact]
    public void ResolveShipperId_일반사용자는_요청본문화주Id를사용할수없다()
    {
        var accessor = new TestCurrentUserAccessor("user-1", "화주");

        var shipperId = 주문자권한검사.ResolveShipperId(accessor, "user-1", "other-shipper");

        Assert.Equal("user-1", shipperId);
    }

    [Fact]
    public void ResolveShipperId_서버관리자는_위임화주Id를지정할수있다()
    {
        var accessor = new TestCurrentUserAccessor("admin-1", "서버관리자");

        var shipperId = 주문자권한검사.ResolveShipperId(accessor, "admin-1", " shipper-2 ");

        Assert.Equal("shipper-2", shipperId);
    }

    [Fact]
    public void IsOwner_주문자UserId가있으면_화주Id만같은사용자는소유자가아니다()
    {
        var request = new 화주운송의뢰
        {
            주문자UserId = "creator-1",
            화주Id = "spoofed-shipper"
        };

        Assert.False(주문자권한검사.IsOwner(request, "spoofed-shipper"));
        Assert.True(주문자권한검사.IsOwner(request, "creator-1"));
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;
}
