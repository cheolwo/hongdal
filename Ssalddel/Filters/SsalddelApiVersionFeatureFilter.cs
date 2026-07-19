using System.Reflection;
using Ssalddel.ApiMetadata;
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
}
