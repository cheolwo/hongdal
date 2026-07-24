using System.Reflection;
using Ssalddel.ApiMetadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Filters;

/// <summary>
/// Enforces the feature key declared by <see cref="SsalddelApiVersionAttribute"/>
/// for every MVC controller action.
/// </summary>
public sealed class SsalddelApiVersionFeatureFilter : IAsyncActionFilter, IOrderedFilter
{
    private readonly IVersionFeatureFlagService _featureFlagService;

    public SsalddelApiVersionFeatureFilter(IVersionFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public int Order => int.MinValue;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureKey = ResolveFeatureKey(context.ActionDescriptor);
        if (string.IsNullOrWhiteSpace(featureKey))
        {
            var version = ResolveVersion(context.ActionDescriptor);
            if (version is null || version > SsalddelProductVersion.V0_0)
            {
                context.Result = CreateUnclassifiedBoundaryResult(context, version);
                return;
            }

            await next();
            return;
        }

        var featureFilter = new RequireVersionFeatureFilter(featureKey, _featureFlagService);
        await featureFilter.OnActionExecutionAsync(context, next);
    }

    internal static string? ResolveFeatureKey(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return null;
        }

        var actionFeatureKey = controllerAction.MethodInfo
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?
            .FeatureKey;
        if (!string.IsNullOrWhiteSpace(actionFeatureKey))
        {
            return actionFeatureKey;
        }

        return controllerAction.ControllerTypeInfo
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?
            .FeatureKey;
    }

    internal static SsalddelProductVersion? ResolveVersion(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return null;
        }

        return controllerAction.MethodInfo
                   .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?
                   .Version
               ?? controllerAction.ControllerTypeInfo
                   .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?
                   .Version;
    }

    private static ObjectResult CreateUnclassifiedBoundaryResult(
        ActionExecutingContext context,
        SsalddelProductVersion? version)
    {
        var versionLabel = version.HasValue
            ? SsalddelProductVersionLabels.GetLabel(version.Value)
            : "unclassified";
        var problem = new ProblemDetails
        {
            Title = "배포 범위가 명시되지 않은 기능입니다.",
            Status = StatusCodes.Status404NotFound,
            Type = "https://httpstatuses.com/404",
            Detail = $"{versionLabel} API must declare an execution feature key before it can be exposed.",
            Instance = context.HttpContext.Request.Path.Value
        };
        problem.Extensions["errors"] = new[] { problem.Title };
        problem.Extensions["errorCode"] = "FeatureBoundaryUnclassified";
        problem.Extensions["productVersion"] = versionLabel;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status404NotFound
        };
    }
}
