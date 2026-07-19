using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireVersionFeatureAttribute : TypeFilterAttribute
{
    public RequireVersionFeatureAttribute(string featureKey)
        : base(typeof(RequireVersionFeatureFilter))
    {
        Arguments = [featureKey];
    }
}
