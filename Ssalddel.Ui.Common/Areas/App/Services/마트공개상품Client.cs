using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Mart;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I마트페이지접근Service
{
    Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default);
}

public sealed class 마트페이지접근Service(ISsalddelJsonApiClient apiClient)
    : I마트페이지접근Service
{
    internal const string FeatureKey = "SsalddelMartWorkflow";
    private const string WorkflowCode = "SsalddelMart";

    public async Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
    {
        var metadata = await apiClient.GetAsync<VersionFeatureFlagsResponse>(
                           "api/v1/version-feature-flags",
                           "알뜰살뜰 마트 기능 확인",
                           allowNotFound: false,
                           cancellationToken)
                       ?? throw new InvalidOperationException("버전 기능 메타데이터 응답이 비어 있습니다.");
        var flag = metadata.Flags.FirstOrDefault(item =>
            string.Equals(item.Key, FeatureKey, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(flag.Key))
        {
            return flag.Value;
        }

        var workflow = metadata.Workflows.FirstOrDefault(item =>
            string.Equals(item.WorkflowCode, WorkflowCode, StringComparison.OrdinalIgnoreCase));
        return workflow?.IsEnabled
               ?? throw new InvalidOperationException("알뜰살뜰 마트 기능 상태를 확인할 수 없습니다.");
    }
}

public interface I마트공개상품읽기Service
{
    Task<마트공개상품목록응답> 목록Async(
        마트공개상품목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<마트공개상품상세응답?> 상세Async(
        long productId,
        CancellationToken cancellationToken = default);
}

public sealed class 마트공개상품Client(ISsalddelJsonApiClient apiClient)
    : I마트공개상품읽기Service
{
    public async Task<마트공개상품목록응답> 목록Async(
        마트공개상품목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = new List<string>
        {
            $"판매가능만={request.판매가능만.ToString().ToLowerInvariant()}",
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            query.Add($"검색어={Uri.EscapeDataString(request.검색어.Trim())}");
        }

        return await apiClient.GetAsync<마트공개상품목록응답>(
                   $"api/v1/orderer/mart/products?{string.Join('&', query)}",
                   "마트 공개 상품 목록 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("마트 공개 상품 목록 응답이 비어 있습니다.");
    }

    public Task<마트공개상품상세응답?> 상세Async(
        long productId,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<마트공개상품상세응답>(
            $"api/v1/orderer/mart/products/{productId}",
            "마트 공개 상품 상세 조회",
            allowNotFound: true,
            cancellationToken);
}

public interface I마트공개상품후기작성Service
{
    Task<마트공개상품구매후기응답> 작성Async(
        long productId,
        마트공개상품구매후기작성요청 request,
        CancellationToken cancellationToken = default);
}

public sealed class 마트공개상품후기Client(ISsalddelJsonApiClient apiClient)
    : I마트공개상품후기작성Service
{
    public async Task<마트공개상품구매후기응답> 작성Async(
        long productId,
        마트공개상품구매후기작성요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (productId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productId));
        }

        return await apiClient.SendAsync<마트공개상품구매후기작성요청, 마트공개상품구매후기응답>(
                   HttpMethod.Post,
                   $"api/v1/orderer/mart/products/{productId}/reviews",
                   request,
                   "마트 공개 상품 구매후기 작성",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("마트 공개 상품 구매후기 응답이 비어 있습니다.");
    }
}
