using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Common;

namespace Ssalddel.Tests.Controllers;

public sealed class OfficialFoodIngredientDiscoveryControllerTests
{
    [Fact]
    public void 공식재료가격레시피조회는_공개읽기경로로제공된다()
    {
        var controllerType = typeof(AgriculturalFisheriesInformationController);
        var action = controllerType.GetMethod(
            nameof(AgriculturalFisheriesInformationController.SearchFoodIngredients));

        Assert.NotNull(controllerType.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(action);
        var route = Assert.Single(action!.GetCustomAttributes<HttpGetAttribute>());
        Assert.Equal("food-ingredients", route.Template);
    }
}
