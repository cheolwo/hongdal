using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Common;

namespace Ssalddel.Tests.Controllers;

public sealed class 공식음식식재료탐색ControllerTests
{
    [Fact]
    public void 공식재료가격레시피조회는_공개읽기경로로제공된다()
    {
        var controllerType = typeof(농수산정보Controller);
        var action = controllerType.GetMethod(
            nameof(농수산정보Controller.음식식재료검색));

        Assert.NotNull(controllerType.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(action);
        var route = Assert.Single(action!.GetCustomAttributes<HttpGetAttribute>());
        Assert.Equal("food-ingredients", route.Template);
    }

    [Fact]
    public void 국가별공식음식과_구조화재료상세는_공개읽기경로로제공된다()
    {
        var controllerType = typeof(농수산정보Controller);
        var listAction = controllerType.GetMethod(
            nameof(농수산정보Controller.음식목록검색));
        var detailAction = controllerType.GetMethod(
            nameof(농수산정보Controller.음식상세조회));

        Assert.NotNull(controllerType.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "food-dishes",
            Assert.Single(listAction!.GetCustomAttributes<HttpGetAttribute>()).Template);
        Assert.Equal(
            "food-dishes/{dishKey}",
            Assert.Single(detailAction!.GetCustomAttributes<HttpGetAttribute>()).Template);
    }

    [Fact]
    public void 재료관련국내외기업근거는_공개조회경로로제공된다()
    {
        var controllerType = typeof(농수산정보Controller);
        var action = controllerType.GetMethod(
            nameof(농수산정보Controller.음식식재료기업검색));

        Assert.NotNull(controllerType.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "food-ingredients/companies",
            Assert.Single(action!.GetCustomAttributes<HttpGetAttribute>()).Template);
    }

    [Fact]
    public void 재료별HS후보는_공개검토경로로제공된다()
    {
        var controllerType = typeof(농수산정보Controller);
        var action = controllerType.GetMethod(
            nameof(농수산정보Controller.음식식재료HSCode조회));

        Assert.NotNull(controllerType.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "food-ingredients/hs-codes",
            Assert.Single(action!.GetCustomAttributes<HttpGetAttribute>()).Template);
    }

    [Fact]
    public void 재료별기업전산화결과와_조사범위를_공개읽기경로로제공한다()
    {
        var controllerType = typeof(농수산정보Controller);
        var archiveAction = controllerType.GetMethod(
            nameof(농수산정보Controller.음식식재료기업Archive조회));
        var coverageAction = controllerType.GetMethod(
            nameof(농수산정보Controller.음식식재료기업Coverage조회));

        Assert.Equal(
            "food-ingredients/companies/archive",
            Assert.Single(archiveAction!.GetCustomAttributes<HttpGetAttribute>()).Template);
        Assert.Equal(
            "food-ingredients/companies/coverage",
            Assert.Single(coverageAction!.GetCustomAttributes<HttpGetAttribute>()).Template);
    }
}
