using Ssalddel.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Filters;

public sealed class RequireVersionFeatureFilterTests
{
    [Fact]
    public async Task OnActionExecutionAsync_ReturnsNotFound_WhenFeatureIsDisabled()
    {
        var actionContext = CreateActionContext("/api/v1/orderer/group-purchase-overseas-shipments/lookup");
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            new object());
        var nextCalled = false;
        var filter = new RequireVersionFeatureFilter(
            VersionFeatureFlagKeys.OrdererGroupOrderV25,
            new FakeVersionFeatureFlagService(false));

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        });

        Assert.False(nextCalled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("FeatureDisabled", problem.Extensions["errorCode"]);
        Assert.Equal(VersionFeatureFlagKeys.OrdererGroupOrderV25, problem.Extensions["featureKey"]);
        Assert.Equal("test-trace", problem.Extensions["traceId"]);
    }

    [Fact]
    public async Task OnActionExecutionAsync_CallsNext_WhenFeatureIsEnabled()
    {
        var actionContext = CreateActionContext("/api/v1/orderer/group-purchase-overseas-shipments/lookup");
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            new object());
        var nextCalled = false;
        var filter = new RequireVersionFeatureFilter(
            VersionFeatureFlagKeys.OrdererGroupOrderV25,
            new FakeVersionFeatureFlagService(true));

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        });

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    private static ActionContext CreateActionContext(string path)
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "test-trace"
        };
        httpContext.Request.Path = path;

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    private sealed class FakeVersionFeatureFlagService : IVersionFeatureFlagService
    {
        private readonly bool _enabled;

        public FakeVersionFeatureFlagService(bool enabled)
        {
            _enabled = enabled;
        }

        public bool IsEnabled(string featureKey)
        {
            return _enabled;
        }

        public IReadOnlyDictionary<string, bool> GetAll()
        {
            return new Dictionary<string, bool>
            {
                [VersionFeatureFlagKeys.OrdererGroupOrderV25] = _enabled
            };
        }
    }
}
