using Ssalddel.Client.Infrastructure.Security;

namespace WarehouseManagerApp.Services;

/// <summary>창고 앱의 1차 화면 진입 역할만 판단하며, 최종 권한은 서버 HR 정책이 검사합니다.</summary>
public sealed class WarehouseAccessPolicyService
{
    private static readonly string[] WarehouseSecurityRoles = ["창고관리자", "서버관리자"];

    public bool CanAccessWarehouseOperations(ClientAuthSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.IsAuthenticated
               && session.Roles.Any(role => WarehouseSecurityRoles.Contains(
                   role,
                   StringComparer.OrdinalIgnoreCase));
    }
}
