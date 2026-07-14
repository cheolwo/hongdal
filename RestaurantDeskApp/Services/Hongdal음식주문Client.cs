using System.Net.Http.Json;
using Hongdal.Contracts.Food;
using Microsoft.Extensions.Logging;

namespace RestaurantDeskApp.Services;

public sealed class Hongdal음식주문Client(
    HttpClient httpClient,
    RestaurantDeskSampleService sampleService,
    ILogger<Hongdal음식주문Client> logger) : I음식주문ApiClient
{
    public async Task<IReadOnlyList<음식주문응답>> 주문목록조회Async(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<음식주문목록응답>(
                "api/v1/food-orders",
                cancellationToken);

            if (response?.Items.Count > 0)
            {
                return response.Items;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogDebug(ex, "Hongdal 서버 주문 목록 조회에 실패해 데스크 샘플 주문으로 대체합니다.");
        }

        return sampleService.Get음식주문목록();
    }

    public async Task<음식주문응답?> 주문상세조회Async(string 주문번호, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(주문번호))
        {
            return null;
        }

        try
        {
            return await httpClient.GetFromJsonAsync<음식주문응답>(
                $"api/v1/food-orders/{Uri.EscapeDataString(주문번호.Trim())}",
                cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogDebug(ex, "Hongdal 서버 주문 상세 조회에 실패해 목록 조회 fallback을 사용합니다. 주문번호={OrderNo}", 주문번호);
        }

        var orders = await 주문목록조회Async(cancellationToken);
        return orders.FirstOrDefault(x => string.Equals(x.주문번호, 주문번호, StringComparison.OrdinalIgnoreCase));
    }
}
