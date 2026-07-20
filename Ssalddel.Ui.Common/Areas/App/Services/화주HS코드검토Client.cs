using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I화주HS코드검토Client
{
    Task<화주HS코드검토목록응답> 목록조회Async(
        string? query,
        int? businessCategory,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<화주HS코드검토상세응답?> 상세조회Async(
        long reviewId,
        CancellationToken cancellationToken = default);
}

public sealed class 화주HS코드검토Client(ISsalddelJsonApiClient apiClient) : I화주HS코드검토Client
{
    private const string BasePath = "api/v1/shipper/customs/hs-reviews";

    public async Task<화주HS코드검토목록응답> 목록조회Async(
        string? query,
        int? businessCategory,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };
        if (!string.IsNullOrWhiteSpace(query))
        {
            parameters.Add($"query={Uri.EscapeDataString(query.Trim())}");
        }

        if (businessCategory.HasValue)
        {
            parameters.Add($"businessCategory={businessCategory.Value}");
        }

        return await apiClient.GetAsync<화주HS코드검토목록응답>(
                   $"{BasePath}?{string.Join('&', parameters)}",
                   "화주 HS 코드 검토 목록 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("HS 코드 검토 목록 응답이 비어 있습니다.");
    }

    public Task<화주HS코드검토상세응답?> 상세조회Async(
        long reviewId,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<화주HS코드검토상세응답>(
            $"{BasePath}/{reviewId}",
            "화주 HS 코드 검토 상세 조회",
            allowNotFound: true,
            cancellationToken);
}

public interface I화주HS코드검토접근Service
{
    Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default);
}

/// <summary>화면 진입 전에 통관·무역 데이터 기능 플래그만 판정합니다.</summary>
public sealed class 화주HS코드검토접근Service(ISsalddelJsonApiClient apiClient) : I화주HS코드검토접근Service
{
    internal const string FeatureKey = "CustomsAndTradeDataWorkflow";
    private const string WorkflowCode = "CustomsAndTradeData";
    private const string MetadataPath = "api/v1/version-feature-flags";

    public async Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
    {
        var metadata = await apiClient.GetAsync<VersionFeatureFlagsResponse>(
                           MetadataPath,
                           "통관·무역 데이터 기능 확인",
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
               ?? throw new InvalidOperationException("통관·무역 데이터 기능 상태를 확인할 수 없습니다.");
    }
}
