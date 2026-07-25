using System.Reflection;
using Ssalddel.Controllers.Common;
using Microsoft.AspNetCore.Authorization;

namespace Ssalddel.Tests.Controllers;

public sealed class CommunityBoardControllerTests
{
    [Fact]
    public void 공개목록만익명접근을허용한다()
    {
        var list = typeof(커뮤니티게시판Controller).GetMethod(nameof(커뮤니티게시판Controller.목록조회));
        var requests = typeof(커뮤니티게시판Controller).GetMethod(nameof(커뮤니티게시판Controller.요청목록조회));

        Assert.NotNull(list?.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "서버관리자전용",
            requests?.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
    }

    [Fact]
    public void 개설신청은로그인이필요하고_승인과반려는관리자전용이다()
    {
        var create = typeof(커뮤니티게시판Controller).GetMethod(nameof(커뮤니티게시판Controller.생성));
        var approve = typeof(커뮤니티게시판Controller).GetMethod(nameof(커뮤니티게시판Controller.승인));
        var reject = typeof(커뮤니티게시판Controller).GetMethod(nameof(커뮤니티게시판Controller.거절));

        Assert.NotNull(create?.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Null(create?.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal("서버관리자전용", approve?.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal("서버관리자전용", reject?.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
    }
}
