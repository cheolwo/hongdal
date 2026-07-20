using System.Globalization;
using Ssalddel.Contracts.Common.Inventory;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I재고현황페이지Service
{
    Task<창고재고현황목록페이지응답> 목록조회Async(
        창고재고현황목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<창고재고현황상세응답?> 상세조회Async(
        long inboundItemId,
        CancellationToken cancellationToken = default);
}

public sealed class 재고현황페이지Service(
    ISsalddelJsonApiClient client) : I재고현황페이지Service
{
    private const string BasePath = "api/v1/warehouse-operations/inventory-overview";

    public async Task<창고재고현황목록페이지응답> 목록조회Async(
        창고재고현황목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = Math.Max(0, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = new List<string>
        {
            $"status={Uri.EscapeDataString(창고재고조회상태코드.Normalize(request.Status))}",
            $"page={page.ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}"
        };

        if (request.WarehouseId is > 0)
        {
            query.Add($"warehouseId={request.WarehouseId.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query.Add($"search={Uri.EscapeDataString(request.Search.Trim())}");
        }

        return await client.GetAsync<창고재고현황목록페이지응답>(
                   $"{BasePath}?{string.Join("&", query)}",
                   "창고 재고 현황 목록 조회",
                   allowNotFound: false,
                   cancellationToken: cancellationToken)
               ?? new 창고재고현황목록페이지응답 { Page = page, PageSize = pageSize };
    }

    public Task<창고재고현황상세응답?> 상세조회Async(
        long inboundItemId,
        CancellationToken cancellationToken = default)
        => client.GetAsync<창고재고현황상세응답>(
            $"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}",
            "창고 재고 현황 상세 조회",
            cancellationToken: cancellationToken);
}
