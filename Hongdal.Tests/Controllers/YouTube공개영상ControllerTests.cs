using System.Reflection;
using Hongdal.Controllers.Admin.Content07;
using Hongdal.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Tests.Controllers;

public sealed class YouTube공개영상ControllerTests
{
    [Fact]
    public void 최신영상조회는_인증없이호출할수있다()
    {
        var action = typeof(YouTube공개영상Controller)
            .GetMethod(nameof(YouTube공개영상Controller.최신영상조회), BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(action);
        Assert.NotNull(action.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(action.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void 공개영상경로는_관리자경로와분리되어있다()
    {
        var route = typeof(YouTube공개영상Controller).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(route);
        Assert.Equal("api/v1/content/youtube/videos", route.Template);
    }

    [Fact]
    public void 공개재생목록조회는_인증없이호출할수있다()
    {
        var route = typeof(YouTube공개재생목록Controller).GetCustomAttribute<RouteAttribute>();
        var listAction = typeof(YouTube공개재생목록Controller)
            .GetMethod(nameof(YouTube공개재생목록Controller.재생목록조회), BindingFlags.Instance | BindingFlags.Public);
        var videosAction = typeof(YouTube공개재생목록Controller)
            .GetMethod(nameof(YouTube공개재생목록Controller.재생목록영상조회), BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(route);
        Assert.Equal("api/v1/content/youtube/playlists", route.Template);
        Assert.NotNull(listAction);
        Assert.NotNull(listAction.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(videosAction);
        Assert.NotNull(videosAction.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "{playlistId}/videos",
            videosAction.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }

    [Fact]
    public void 영상공개설정은_관리자전용쓰기API다()
    {
        var authorize = typeof(YouTube채널감시Controller).GetCustomAttribute<AuthorizeAttribute>();
        var action = typeof(YouTube채널감시Controller)
            .GetMethod(nameof(YouTube채널감시Controller.영상공개설정), BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(authorize);
        Assert.Equal("서버관리자전용", authorize.Policy);
        Assert.NotNull(action);
        var httpPut = action.GetCustomAttribute<HttpPutAttribute>();
        Assert.NotNull(httpPut);
        Assert.Equal("videos/{videoId}/publication", httpPut.Template);
    }
}
