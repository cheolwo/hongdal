using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using 홍달.Services.Versioning;

namespace Hongdal.Filters;

public sealed class RequireVersionFeatureFilter : IAsyncActionFilter
{
    private readonly string _featureKey;
    private readonly IVersionFeatureFlagService _featureFlagService;

    public RequireVersionFeatureFilter(
        string featureKey,
        IVersionFeatureFlagService featureFlagService)
    {
        _featureKey = featureKey;
        _featureFlagService = featureFlagService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_featureFlagService.IsEnabled(_featureKey))
        {
            await next();
            return;
        }

        var problem = new ProblemDetails
        {
            Title = "현재 버전에서 비활성화된 기능입니다.",
            Status = StatusCodes.Status404NotFound,
            Type = "https://httpstatuses.com/404",
            Detail = $"{_featureKey} feature flag is disabled.",
            Instance = context.HttpContext.Request.Path.Value
        };
        problem.Extensions["errors"] = new[] { problem.Title };
        problem.Extensions["errorCode"] = "FeatureDisabled";
        problem.Extensions["featureKey"] = _featureKey;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status404NotFound
        };
    }
}
