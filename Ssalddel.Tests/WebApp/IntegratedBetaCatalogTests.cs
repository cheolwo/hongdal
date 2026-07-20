using Microsoft.AspNetCore.Components;
using Ssalddel.WebApp.Models;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class IntegratedBetaCatalogTests
{
    [Theory]
    [InlineData("/community")]
    [InlineData("/community/write")]
    [InlineData("/community/posts/42")]
    public void 익명_커뮤니티_핵심_경로는_운영_상태이며_로그인이_선택이다(string href)
    {
        var state = IntegratedBetaCatalog.Resolve(href);

        Assert.True(state.IsCataloged);
        Assert.Equal(IntegratedBetaStage.Live, state.Stage);
        Assert.False(state.RequiresAuthentication);
        Assert.Equal(WebInteractionBoundary.PlatformPersistence, state.Boundary);
    }

    [Theory]
    [InlineData("/community/group-purchase", IntegratedBetaStage.Beta)]
    [InlineData("/community/group-import", IntegratedBetaStage.Experience)]
    [InlineData("/shipper/request", IntegratedBetaStage.Beta)]
    [InlineData("/driver/recommendations", IntegratedBetaStage.Preparing)]
    [InlineData("/driver/transports/current", IntegratedBetaStage.Preparing)]
    [InlineData("/warehouse/work-board", IntegratedBetaStage.Beta)]
    public void 외부_업무_효과가_있는_대표_경로는_Simulation_경계를_유지한다(
        string href,
        IntegratedBetaStage expectedStage)
    {
        var state = IntegratedBetaCatalog.Resolve(href);

        Assert.True(state.IsCataloged);
        Assert.Equal(expectedStage, state.Stage);
        Assert.Equal(WebInteractionBoundary.Simulation, state.Boundary);
        Assert.NotEqual(IntegratedBetaStage.Live, state.Stage);
    }

    [Fact]
    public void 역할별_통합_홈의_모든_대표_화면은_카탈로그에_등록되어_있다()
    {
        var screens = WebExperienceCatalog.Roles.SelectMany(role => role.Screens);

        Assert.All(screens, screen =>
        {
            var isCataloged = IntegratedBetaCatalog.TryResolve(screen.Href, out var state);

            Assert.True(isCataloged, $"통합 상태가 없는 대표 화면: {screen.Href}");
            Assert.True(state.IsCataloged);
        });
    }

    [Fact]
    public void 역할별_내비게이션의_모든_경로는_카탈로그에_등록되어_있다()
    {
        var themeCodes = new[] { "guest", "driver", "shipper", "warehouse", "orderer", "customs", "member" };
        var items = themeCodes
            .SelectMany(WebNavigationCatalog.GetBusinessItems)
            .Concat(WebNavigationCatalog.CommunityItems)
            .Concat(WebNavigationCatalog.IntegratedItems)
            .DistinctBy(item => item.Href);

        Assert.All(items, item =>
            Assert.True(
                IntegratedBetaCatalog.TryResolve(item.Href, out _),
                $"통합 상태가 없는 내비게이션: {item.Href}"));
    }

    [Fact]
    public void 통합_WebApp의_모든_컴파일된_라우트는_capability_규칙으로_분류된다()
    {
        var routes = typeof(Ssalddel.WebApp.App).Assembly
            .GetTypes()
            .SelectMany(type => type
                .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                .Cast<RouteAttribute>()
                .Select(attribute => new { Component = type.FullName, attribute.Template }))
            .OrderBy(item => item.Template, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(routes);
        Assert.All(routes, route =>
            Assert.True(
                IntegratedBetaCatalog.TryResolve(route.Template, out _),
                $"capability 규칙이 없는 WebApp 라우트: {route.Template} ({route.Component})"));
    }

    [Fact]
    public void 알_수_없는_경로는_안전하게_준비_중으로_분류한다()
    {
        var state = IntegratedBetaCatalog.Resolve("/future/external-operation?preview=true");

        Assert.False(state.IsCataloged);
        Assert.Equal(IntegratedBetaStage.Preparing, state.Stage);
        Assert.Equal(WebInteractionBoundary.Simulation, state.Boundary);
    }

    [Fact]
    public void 쿼리와_후행_슬래시는_같은_경로_상태로_해석한다()
    {
        var state = IntegratedBetaCatalog.Resolve("/community/?board=free#latest");

        Assert.Equal(IntegratedBetaStage.Live, state.Stage);
        Assert.False(state.RequiresAuthentication);
    }
}
