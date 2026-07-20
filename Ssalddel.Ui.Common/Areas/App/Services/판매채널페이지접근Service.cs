using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I판매채널페이지접근Service
{
    Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default);
}

/// <summary>판매채널 페이지의 서버 기능 플래그만 조회합니다.</summary>
public sealed class 판매채널페이지접근Service(ISsalddelJsonApiClient apiClient) : I판매채널페이지접근Service
{
    internal const string FeatureKey = "SalesChannelFulfillmentWorkflow";
    private const string WorkflowCode = "SalesChannelFulfillment";
    private const string MetadataPath = "api/v1/version-feature-flags";

    public async Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
    {
        var metadata = await apiClient.GetAsync<VersionFeatureFlagsResponse>(
                           MetadataPath,
                           "판매채널 출고 기능 확인",
                           allowNotFound: false,
                           cancellationToken)
                       ?? throw new InvalidOperationException("버전 기능 메타데이터 응답이 비어 있습니다.");

        var flag = metadata.Flags.FirstOrDefault(pair =>
            string.Equals(pair.Key, FeatureKey, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(flag.Key))
        {
            return flag.Value;
        }

        var workflow = metadata.Workflows.FirstOrDefault(item =>
            string.Equals(item.WorkflowCode, WorkflowCode, StringComparison.OrdinalIgnoreCase));
        return workflow?.IsEnabled
               ?? throw new InvalidOperationException("판매채널 출고 기능 상태를 확인할 수 없습니다.");
    }
}
