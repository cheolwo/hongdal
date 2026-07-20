using System.Globalization;
using Ssalddel.Contracts.Common.Inventory;

namespace Ssalddel.Ui.Common.Areas.App.Services;

/// <summary>입고 검수 페이지가 사용하는 최소 조회와 검수 Command API 경계입니다.</summary>
public interface I입고검수페이지Service
{
    Task<입고검수대상페이지응답> 목록조회Async(
        입고검수대상목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<입고검수대상상세응답?> 상세조회Async(
        long inboundItemId,
        CancellationToken cancellationToken = default);

    Task<창고작업결과응답?> 검수Async(
        long inboundItemId,
        입고검수요청 request,
        CancellationToken cancellationToken = default);
}

public sealed class 입고검수페이지Service(
    ISsalddelJsonApiClient client) : I입고검수페이지Service
{
    private const string BasePath = "api/v1/warehouse-operations/inventory";

    public async Task<입고검수대상페이지응답> 목록조회Async(
        입고검수대상목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = Math.Max(0, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = new List<string>
        {
            $"inspectionStatus={Uri.EscapeDataString(입고검수조회상태코드.Normalize(request.InspectionStatus))}",
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

        return await client.GetAsync<입고검수대상페이지응답>(
                   $"{BasePath}/inspection-targets?{string.Join("&", query)}",
                   "입고 검수 대상 목록 조회",
                   allowNotFound: false,
                   cancellationToken: cancellationToken)
               ?? new 입고검수대상페이지응답 { Page = page, PageSize = pageSize };
    }

    public Task<입고검수대상상세응답?> 상세조회Async(
        long inboundItemId,
        CancellationToken cancellationToken = default)
        => client.GetAsync<입고검수대상상세응답>(
            $"{BasePath}/{inboundItemId}/inspection-target",
            "입고 검수 대상 상세 조회",
            cancellationToken: cancellationToken);

    public Task<창고작업결과응답?> 검수Async(
        long inboundItemId,
        입고검수요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<입고검수요청, 창고작업결과응답>(
            HttpMethod.Post,
            $"{BasePath}/{inboundItemId}/inspect",
            request,
            "입고 검수",
            cancellationToken: cancellationToken);
}
