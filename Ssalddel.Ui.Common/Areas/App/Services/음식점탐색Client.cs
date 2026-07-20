using System.Globalization;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Restaurants;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I음식배달페이지접근Service
{
    Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default);
}

public sealed class 음식배달페이지접근Service(ISsalddelJsonApiClient apiClient)
    : I음식배달페이지접근Service
{
    internal const string FeatureKey = "FoodDeliveryWorkflow";
    private const string WorkflowCode = "FoodDelivery";

    public async Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
    {
        var metadata = await apiClient.GetAsync<VersionFeatureFlagsResponse>(
                           "api/v1/version-feature-flags",
                           "음식 배달 기능 확인",
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
               ?? throw new InvalidOperationException("음식 배달 기능 상태를 확인할 수 없습니다.");
    }
}

public interface I음식점탐색정책읽기Service
{
    Task<RestaurantSearchPolicyDto> 조회Async(CancellationToken cancellationToken = default);
}

public sealed class 음식점탐색정책Client(ISsalddelJsonApiClient apiClient)
    : I음식점탐색정책읽기Service
{
    public async Task<RestaurantSearchPolicyDto> 조회Async(CancellationToken cancellationToken = default)
        => await apiClient.GetAsync<RestaurantSearchPolicyDto>(
               "api/v1/orderer/restaurant-search-policy",
               "음식점 탐색 정책 조회",
               allowNotFound: false,
               cancellationToken)
           ?? throw new InvalidOperationException("음식점 탐색 정책 응답이 비어 있습니다.");
}

public interface I음식점공개읽기Service
{
    Task<IReadOnlyList<음식점탐색권역응답>> 권역목록Async(
        CancellationToken cancellationToken = default);

    Task<음식점공개목록응답> 목록Async(
        음식점공개목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<음식점공개상세응답?> 상세Async(
        long restaurantId,
        CancellationToken cancellationToken = default);
}

public sealed class 음식점공개Client(ISsalddelJsonApiClient apiClient) : I음식점공개읽기Service
{
    public async Task<IReadOnlyList<음식점탐색권역응답>> 권역목록Async(
        CancellationToken cancellationToken = default)
        => await apiClient.GetAsync<IReadOnlyList<음식점탐색권역응답>>(
               "api/v1/orderer/restaurants/service-areas",
               "음식점 탐색 권역 조회",
               allowNotFound: false,
               cancellationToken)
           ?? throw new InvalidOperationException("음식점 탐색 권역 응답이 비어 있습니다.");

    public async Task<음식점공개목록응답> 목록Async(
        음식점공개목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = new List<string>
        {
            $"배달권키={Uri.EscapeDataString(request.배달권키)}",
            $"반경Km={request.반경Km.ToString(CultureInfo.InvariantCulture)}",
            $"주문가능만={request.주문가능만.ToString().ToLowerInvariant()}",
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            query.Add($"검색어={Uri.EscapeDataString(request.검색어.Trim())}");
        }

        return await apiClient.GetAsync<음식점공개목록응답>(
                   $"api/v1/orderer/restaurants?{string.Join('&', query)}",
                   "공개 음식점 목록 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("공개 음식점 목록 응답이 비어 있습니다.");
    }

    public Task<음식점공개상세응답?> 상세Async(
        long restaurantId,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<음식점공개상세응답>(
            $"api/v1/orderer/restaurants/{restaurantId}",
            "공개 음식점 상세 조회",
            allowNotFound: true,
            cancellationToken);
}
