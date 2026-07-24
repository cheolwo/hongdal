using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Controllers.Common;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Controllers;

public sealed class CommunityPostIngredientPriceHintsControllerTests
{
    [Fact]
    public void 가격힌트API는_익명읽기전용_커뮤니티0점0경계를명시한다()
    {
        var type = typeof(CommunityPostIngredientPriceHintsController);
        var version = type.GetCustomAttribute<SsalddelApiVersionAttribute>();
        var route = type.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(type.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(SsalddelProductVersion.V0_0, version?.Version);
        Assert.Equal(
            "api/v1/community/post-authoring/ingredient-price-hints",
            route?.Template);
    }

    [Fact]
    public async Task 잘못된본문은_400문제로변환한다()
    {
        var controller = new CommunityPostIngredientPriceHintsController(
            new ThrowingService());

        var result = await controller.GetHints(
            new CommunityPostIngredientPriceHintRequest("본문"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(400, problem.Status);
        Assert.Contains("가격 힌트", problem.Title, StringComparison.Ordinal);
    }

    private sealed class ThrowingService : ICommunityPostIngredientPriceHintService
    {
        public Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
            CommunityPostIngredientPriceHintRequest request,
            CancellationToken cancellationToken = default)
            => throw new ArgumentException("잘못된 본문");
    }
}
