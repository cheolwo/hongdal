using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Security;

namespace Ssalddel.Middleware;

public sealed class HrRoleAccessMiddleware : IMiddleware
{
    private static readonly string[] AdminSecurityRoles = ["서버관리자"];

    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IHrRoleAssignmentStore _roleAssignmentStore;

    public HrRoleAccessMiddleware(
        ICurrentUserAccessor currentUserAccessor,
        IHrRoleAssignmentStore roleAssignmentStore)
    {
        _currentUserAccessor = currentUserAccessor;
        _roleAssignmentStore = roleAssignmentStore;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var roleCodes = context.GetEndpoint()?.Metadata.GetOrderedMetadata<RequireHrRoleAttribute>()
            .SelectMany(x => x.RoleCodes)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        if (roleCodes.Length == 0 || IsServerAdmin())
        {
            await next(context);
            return;
        }

        var userId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "로그인 사용자 정보를 확인할 수 없습니다." });
            return;
        }

        var (scopeType, scopeId) = ResolveScope(context);
        var decision = await _roleAssignmentStore.AuthorizeAccessAsync(
            userId,
            scopeType,
            scopeId,
            roleCodes,
            HrRequestClientIpResolver.Resolve(context),
            DateTimeOffset.UtcNow,
            context.RequestAborted);

        if (!decision.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                message = ResolveDenyMessage(decision.DenyReason),
                reason = decision.DenyReason,
                requiredRoles = roleCodes,
                scopeType,
                scopeId
            });
            return;
        }

        await next(context);
    }

    private bool IsServerAdmin()
    {
        return AdminSecurityRoles.Any(role => string.Equals(role, _currentUserAccessor.Role, StringComparison.OrdinalIgnoreCase));
    }

    private static (string ScopeType, string ScopeId) ResolveScope(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("warehouseId", out var warehouseId) && warehouseId is not null)
        {
            return (HrScopeTypes.Warehouse, warehouseId.ToString() ?? HrScopeIds.Global);
        }

        return (HrScopeTypes.Platform, HrScopeIds.Global);
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
}
