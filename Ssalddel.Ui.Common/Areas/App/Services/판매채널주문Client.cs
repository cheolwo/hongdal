using System.Globalization;
using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I판매채널주문읽기Service
{
    Task<판매채널주문목록응답> 목록조회Async(
        판매채널주문목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<판매채널주문상세응답?> 상세조회Async(
        long orderId,
        CancellationToken cancellationToken = default);
}

/// <summary>영속된 판매채널 주문 출고 후보의 목록과 정확한 ID 상세만 읽습니다.</summary>
public sealed class 판매채널주문Client(
    ISsalddelJsonApiClient client) : I판매채널주문읽기Service
{
    private const string BasePath = "api/v1/sales-channels/orders";

    public async Task<판매채널주문목록응답> 목록조회Async(
        판매채널주문목록조회요청 request,
        CancellationToken cancellationToken = default)
        => await client.GetAsync<판매채널주문목록응답>(
               BuildListPath(request),
               "판매채널 주문 출고 후보 목록 조회",
               allowNotFound: false,
               cancellationToken)
           ?? throw new InvalidOperationException("판매채널 주문 출고 후보 목록 응답이 비어 있습니다.");

    public Task<판매채널주문상세응답?> 상세조회Async(
        long orderId,
        CancellationToken cancellationToken = default)
        => client.GetAsync<판매채널주문상세응답>(
            $"{BasePath}/{orderId}",
            "판매채널 주문 출고 후보 상세 조회",
            allowNotFound: true,
            cancellationToken);

    private static string BuildListPath(판매채널주문목록조회요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var values = new List<string>
        {
            $"page={Math.Max(0, request.Page).ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={Math.Clamp(request.PageSize, 1, 100).ToString(CultureInfo.InvariantCulture)}"
        };
        AddValue(values, "search", request.Search);
        AddValue(values, "syncScope", request.SyncScope);
        AddValue(values, "status", request.Status);
        return $"{BasePath}?{string.Join('&', values)}";
    }

    private static void AddValue(ICollection<string> values, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
