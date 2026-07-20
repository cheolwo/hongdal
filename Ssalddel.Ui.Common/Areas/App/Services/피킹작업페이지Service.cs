using System.Globalization;
using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I피킹작업페이지Service
{
    Task<피킹작업목록페이지응답> 목록조회Async(
        피킹작업목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<피킹작업상세응답?> 상세조회Async(
        string taskKey,
        CancellationToken cancellationToken = default);

    Task<피킹작업결과응답?> 시작Async(
        string taskKey,
        CancellationToken cancellationToken = default);

    Task<피킹작업결과응답?> 완료Async(
        string taskKey,
        피킹작업완료요청 request,
        CancellationToken cancellationToken = default);
}

public sealed class 피킹작업페이지Service(
    ISsalddelJsonApiClient client) : I피킹작업페이지Service
{
    private const string BasePath = "api/v1/warehouse-operations/picking-tasks";

    public async Task<피킹작업목록페이지응답> 목록조회Async(
        피킹작업목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = Math.Max(0, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = new List<string>
        {
            $"status={Uri.EscapeDataString(피킹작업조회상태코드.Normalize(request.Status))}",
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

        return await client.GetAsync<피킹작업목록페이지응답>(
                   $"{BasePath}?{string.Join("&", query)}",
                   "피킹 작업 목록 조회",
                   allowNotFound: false,
                   cancellationToken: cancellationToken)
               ?? new 피킹작업목록페이지응답 { Page = page, PageSize = pageSize };
    }

    public Task<피킹작업상세응답?> 상세조회Async(
        string taskKey,
        CancellationToken cancellationToken = default)
        => client.GetAsync<피킹작업상세응답>(
            $"{BasePath}/{Uri.EscapeDataString(taskKey.Trim())}",
            "피킹 작업 상세 조회",
            cancellationToken: cancellationToken);

    public Task<피킹작업결과응답?> 시작Async(
        string taskKey,
        CancellationToken cancellationToken = default)
        => client.SendAsync<피킹작업결과응답>(
            HttpMethod.Post,
            $"{BasePath}/{Uri.EscapeDataString(taskKey.Trim())}/start",
            "피킹 작업 시작",
            cancellationToken: cancellationToken);

    public Task<피킹작업결과응답?> 완료Async(
        string taskKey,
        피킹작업완료요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<피킹작업완료요청, 피킹작업결과응답>(
            HttpMethod.Post,
            $"{BasePath}/{Uri.EscapeDataString(taskKey.Trim())}/complete",
            request,
            "피킹 작업 완료",
            cancellationToken: cancellationToken);
}
