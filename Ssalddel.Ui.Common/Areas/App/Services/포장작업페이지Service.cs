using System.Globalization;
using Ssalddel.Contracts.Common.Inventory;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I포장작업페이지Service
{
    Task<포장작업목록페이지응답> 목록조회Async(포장작업목록조회요청 request, CancellationToken cancellationToken = default);
    Task<포장작업상세응답?> 상세조회Async(long inboundItemId, CancellationToken cancellationToken = default);
    Task<포장작업결과응답?> 완료Async(long inboundItemId, 포장작업완료요청 request, CancellationToken cancellationToken = default);
}

public sealed class 포장작업페이지Service(ISsalddelJsonApiClient client) : I포장작업페이지Service
{
    private const string BasePath = "api/v1/warehouse-operations/packing-tasks";
    public async Task<포장작업목록페이지응답> 목록조회Async(포장작업목록조회요청 request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = Math.Max(0, request.Page); var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = new List<string>
        {
            $"status={Uri.EscapeDataString(포장작업조회상태코드.Normalize(request.Status))}",
            $"page={page.ToString(CultureInfo.InvariantCulture)}", $"pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}"
        };
        if (request.WarehouseId is > 0) query.Add($"warehouseId={request.WarehouseId.Value.ToString(CultureInfo.InvariantCulture)}");
        if (!string.IsNullOrWhiteSpace(request.Search)) query.Add($"search={Uri.EscapeDataString(request.Search.Trim())}");
        return await client.GetAsync<포장작업목록페이지응답>($"{BasePath}?{string.Join("&", query)}", "포장 작업 목록 조회", allowNotFound: false, cancellationToken: cancellationToken)
            ?? new 포장작업목록페이지응답 { Page = page, PageSize = pageSize };
    }
    public Task<포장작업상세응답?> 상세조회Async(long inboundItemId, CancellationToken cancellationToken = default)
        => client.GetAsync<포장작업상세응답>($"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}", "포장 작업 상세 조회", cancellationToken: cancellationToken);
    public Task<포장작업결과응답?> 완료Async(long inboundItemId, 포장작업완료요청 request, CancellationToken cancellationToken = default)
        => client.SendAsync<포장작업완료요청, 포장작업결과응답>(HttpMethod.Post,
            $"{BasePath}/{inboundItemId.ToString(CultureInfo.InvariantCulture)}/complete", request, "포장 작업 완료", cancellationToken: cancellationToken);
}
