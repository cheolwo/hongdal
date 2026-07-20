using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace HumanResourcesManagerApp.Services;

public sealed record HumanResourcesPageAvailability(bool IsEnabled, string Notice);

/// <summary>서버 capability 메타데이터에서 인사 앱 페이지의 노출 가능성만 해석합니다.</summary>
public sealed class HumanResourcesPageAvailabilityService(ICommunityProcurementClient client)
{
    private const string FeatureKey = "HrParticipationWorkflow";

    public async Task<HumanResourcesPageAvailability> GetRoleReviewsAsync(
        CancellationToken cancellationToken = default)
    {
        var metadata = await client.GetVersionWorkflowMetadataAsync(cancellationToken);
        var capability = metadata.PageCapabilities.FirstOrDefault(item =>
            string.Equals(item.AppCode, SsalddelPageAppCodes.HumanResources, StringComparison.Ordinal)
            && string.Equals(item.RoutePattern, HumanResourcesManagerRoutes.RoleReviews, StringComparison.OrdinalIgnoreCase));

        if (capability is not null)
        {
            return new HumanResourcesPageAvailability(capability.IsFeatureEnabled, capability.Notice);
        }

        var enabled = metadata.Flags.TryGetValue(FeatureKey, out var flag) && flag;
        return new HumanResourcesPageAvailability(
            enabled,
            enabled
                ? "HR 역할 검토 조회 기능이 활성화되어 있습니다."
                : "현재 환경에서는 HR 역할 검토 조회 기능이 비활성입니다.");
    }
}
