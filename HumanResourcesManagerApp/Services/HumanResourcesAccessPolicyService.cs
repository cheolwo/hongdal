using Ssalddel.Client.Infrastructure.Security;

namespace HumanResourcesManagerApp.Services;

/// <summary>인사 앱의 1차 화면 진입 역할만 판단하며, 최종 권한은 서버 정책이 검사합니다.</summary>
public sealed class HumanResourcesAccessPolicyService
{
    private const string HumanResourcesManagerRole = "서버관리자";

    public bool CanReviewRoles(ClientAuthSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.IsAuthenticated
               && session.Roles.Contains(HumanResourcesManagerRole, StringComparer.OrdinalIgnoreCase);
    }
}
