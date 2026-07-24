using System.Net;
using System.Net.Http.Json;
using Ssalddel.Contracts.Food;

namespace RestaurantDeskApp.Services;

public sealed class Ssalddel음식주문Client(HttpClient httpClient) : I음식주문ApiClient
{
    private const string BasePath = "api/v1/food-orders";

    public async Task<IReadOnlyList<음식주문응답>> 주문목록조회Async(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(BasePath, cancellationToken);
        await EnsureSuccessAsync(response, "음식 주문 목록 조회", cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<음식주문목록응답>(cancellationToken);
        return payload?.Items ?? [];
    }

    public async Task<음식주문응답?> 주문상세조회Async(
        string 주문번호,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(주문번호))
        {
            return null;
        }

        using var response = await httpClient.GetAsync(
            $"{BasePath}/{Uri.EscapeDataString(주문번호.Trim())}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, "음식 주문 상세 조회", cancellationToken);
        return await response.Content.ReadFromJsonAsync<음식주문응답>(cancellationToken);
    }

    public async Task<음식주문응답?> 음식점수락Async(
        string 주문번호,
        음식점주문수락요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(주문번호);
        ArgumentNullException.ThrowIfNull(request);

        using var response = await httpClient.PostAsJsonAsync(
            $"{BasePath}/{Uri.EscapeDataString(주문번호.Trim())}/restaurant-acceptance",
            request,
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, "음식점 주문 수락", cancellationToken);
        return await response.Content.ReadFromJsonAsync<음식주문응답>(cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"{operation} API 실패: HTTP {(int)response.StatusCode}"
            + (string.IsNullOrWhiteSpace(detail) ? string.Empty : $" · {detail}"),
            null,
            response.StatusCode);
    }
}
