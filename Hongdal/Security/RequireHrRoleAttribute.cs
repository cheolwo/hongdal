using Hongdal.Application.CommandProcessing;
using Hongdal.Application.HumanResources;
using Hongdal.Contracts.Common.Hr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hongdal.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireHrRoleAttribute : TypeFilterAttribute
{
    public RequireHrRoleAttribute(params string[] roleCodes)
        : base(typeof(HrRoleAuthorizationFilter))
    {
        RoleCodes = roleCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
        Arguments = [RoleCodes.ToArray()];
    }

    public IReadOnlyCollection<string> RoleCodes { get; }
}

public sealed class HrRoleAuthorizationFilter : IAsyncAuthorizationFilter
{
    private static readonly string[] AdminSecurityRoles = ["서버관리자"];

    private readonly IReadOnlyCollection<string> _roleCodes;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IHrRoleAssignmentStore _roleAssignmentStore;

    public HrRoleAuthorizationFilter(
        string[] roleCodes,
        ICurrentUserAccessor currentUserAccessor,
        IHrRoleAssignmentStore roleAssignmentStore)
    {
        _roleCodes = roleCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
        _currentUserAccessor = currentUserAccessor;
        _roleAssignmentStore = roleAssignmentStore;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (_roleCodes.Count == 0 || IsServerAdmin())
        {
            return;
        }

        var userId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "로그인 사용자 정보를 확인할 수 없습니다." });
            return;
        }

        var (scopeType, scopeId) = ResolveScope(context);
        var decision = await _roleAssignmentStore.AuthorizeAccessAsync(
            userId,
            scopeType,
            scopeId,
            _roleCodes,
            HrRequestClientIpResolver.Resolve(context.HttpContext),
            DateTimeOffset.UtcNow,
            context.HttpContext.RequestAborted);

        if (!decision.IsAllowed)
        {
            context.Result = new ObjectResult(new
            {
                message = ResolveDenyMessage(decision.DenyReason),
                reason = decision.DenyReason,
                requiredRoles = _roleCodes,
                scopeType,
                scopeId
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    private static string ResolveDenyMessage(string reason)
    {
        return reason switch
        {
            "OutsideWorkSchedule" => "현재 근무 요일 또는 근무 시간이 아니어서 해당 API를 실행할 수 없습니다.",
            "OutsideWorksiteIpRange" => "허용된 작업장 IP에서 접속한 요청이 아니어서 해당 API를 실행할 수 없습니다.",
            "HrRoleNotAssigned" => "HR 세부 역할이 없어 해당 API를 실행할 수 없습니다.",
            _ => "HR 접근 조건을 만족하지 않아 해당 API를 실행할 수 없습니다."
        };
    }

    private bool IsServerAdmin()
    {
        return AdminSecurityRoles.Any(role => string.Equals(role, _currentUserAccessor.Role, StringComparison.OrdinalIgnoreCase));
    }

    private static (string ScopeType, string ScopeId) ResolveScope(AuthorizationFilterContext context)
    {
        if (context.RouteData.Values.TryGetValue("warehouseId", out var warehouseId) && warehouseId is not null)
        {
            return (HrScopeTypes.Warehouse, warehouseId.ToString() ?? HrScopeIds.Global);
        }

        return (HrScopeTypes.Platform, HrScopeIds.Global);
    }
}
