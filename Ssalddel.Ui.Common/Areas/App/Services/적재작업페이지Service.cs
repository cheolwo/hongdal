using System.Globalization;
using Ssalddel.Contracts.Common.Inventory;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I적재작업페이지Service
{
    Task<적재작업목록페이지응답> 목록조회Async(적재작업목록조회요청 request, CancellationToken cancellationToken = default);
    Task<적재작업상세응답?> 상세조회Async(long inboundItemId, CancellationToken cancellationToken = default);
    Task<적재작업결과응답?> 완료Async(long inboundItemId, 적재작업완료요청 request, CancellationToken cancellationToken = default);
}

public sealed class 적재작업페이지Service(ISsalddelJsonApiClient client) : I적재작업페이지Service
{
    private const string BasePath = "api/v1/warehouse-operations/put-away-tasks";

    public async Task<적재작업목록페이지응답> 목록조회Async(적재작업목록조회요청 request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = Math.Max(0, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = new List<string>
        {
            $"status={Uri.EscapeDataString(적재작업조회상태코드.Normalize(request.Status))}",
            $"page={page.ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}"
        };
        if (request.WarehouseId is > 0) query.Add($"warehouseId={request.WarehouseId.Value.ToString(CultureInfo.InvariantCulture)}");
        if (!string.IsNullOrWhiteSpace(request.Search)) query.Add($"search={Uri.EscapeDataString(request.Search.Trim())}");
        return await client.GetAsync<적재작업목록페이지응답>($"{BasePath}?{string.Join("&", query)}", "적재 작업 목록 조회", allowNotFound: false, cancellationToken: cancellationToken)
            ?? new 적재작업목록페이지응답 { Page = page, PageSize = pageSize };
    }

    public Task<적재작업상세응답?> 상세조회Async(long inboundItemId, CancellationToken cancellationToken = default)
        => client.GetAsync<적재작업상세응답>($"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}", "적재 작업 상세 조회", cancellationToken: cancellationToken);

    public Task<적재작업결과응답?> 완료Async(long inboundItemId, 적재작업완료요청 request, CancellationToken cancellationToken = default)
        => client.SendAsync<적재작업완료요청, 적재작업결과응답>(HttpMethod.Post,
            $"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}/complete", request, "적재 작업 완료", cancellationToken: cancellationToken);
}
