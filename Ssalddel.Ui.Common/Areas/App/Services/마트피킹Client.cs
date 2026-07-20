using Ssalddel.Contracts.Mart;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I마트피킹읽기Service
{
    Task<마트피킹주문목록응답> 목록Async(
        마트피킹주문목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<마트피킹주문상세응답?> 상세Async(
        long orderId,
        CancellationToken cancellationToken = default);
}

public sealed class 마트피킹Client(ISsalddelJsonApiClient apiClient)
    : I마트피킹읽기Service
{
    public async Task<마트피킹주문목록응답> 목록Async(
        마트피킹주문목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            query.Add($"검색어={Uri.EscapeDataString(request.검색어.Trim())}");
        }

        if (request.창고Id is > 0)
        {
            query.Add($"창고Id={request.창고Id.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.작업상태))
        {
            query.Add($"작업상태={Uri.EscapeDataString(request.작업상태.Trim())}");
        }

        return await apiClient.GetAsync<마트피킹주문목록응답>(
                   $"api/v1/warehouse-operations/mart/picking-orders?{string.Join('&', query)}",
                   "마트 피킹 주문 목록 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("마트 피킹 주문 목록 응답이 비어 있습니다.");
    }

    public Task<마트피킹주문상세응답?> 상세Async(
        long orderId,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<마트피킹주문상세응답>(
            $"api/v1/warehouse-operations/mart/picking-orders/{orderId}",
            "마트 피킹 주문 상세 조회",
            allowNotFound: true,
            cancellationToken);
}
