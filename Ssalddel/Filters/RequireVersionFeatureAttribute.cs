using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireVersionFeatureAttribute : TypeFilterAttribute
{
    public RequireVersionFeatureAttribute(string featureKey)
        : base(typeof(RequireVersionFeatureFilter))
    {
        Arguments = [featureKey];
    }
}

/// <summary>
/// 제품 버전과 무관하게 실행 Feature가 활성화되어야 함을 표시합니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireFeatureAttribute : RequireVersionFeatureAttribute
{
    public RequireFeatureAttribute(string featureKey)
        : base(featureKey)
    {
    }
}
