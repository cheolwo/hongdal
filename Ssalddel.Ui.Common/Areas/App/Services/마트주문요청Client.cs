using Ssalddel.Contracts.Mart;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I마트주문요청Service
{
    Task<마트주문요청응답> 등록Async(
        마트주문요청등록요청 request,
        CancellationToken cancellationToken = default);

    Task<마트주문요청응답?> 상세Async(
        Guid orderRequestId,
        CancellationToken cancellationToken = default);
}

public sealed class 마트주문요청Client(ISsalddelJsonApiClient apiClient)
    : I마트주문요청Service
{
    private const string BasePath = "api/v1/orderer/mart/order-requests";

    public async Task<마트주문요청응답> 등록Async(
        마트주문요청등록요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await apiClient.SendAsync<마트주문요청등록요청, 마트주문요청응답>(
                   HttpMethod.Post,
                   BasePath,
                   request,
                   "마트 주문 요청 등록",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("마트 주문 요청 등록 응답이 비어 있습니다.");
    }

    public Task<마트주문요청응답?> 상세Async(
        Guid orderRequestId,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<마트주문요청응답>(
            $"{BasePath}/{orderRequestId:D}",
            "마트 주문 요청 상세 조회",
            allowNotFound: true,
            cancellationToken);
}
