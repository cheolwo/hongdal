using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Admin.Food;

namespace SsalddelAdmin.Services;

public sealed class FoodOrderOperationsTraceAdminService(
    HttpClient httpClient,
    관리자인증세션Service session)
{
    public async Task<음식주문운영추적응답?> 조회Async(
        string orderNo,
        CancellationToken cancellationToken = default)
    {
        var normalized = orderNo?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("주문번호를 입력해 주세요.", nameof(orderNo));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/v1/admin/food-orders/{Uri.EscapeDataString(normalized)}/operations-trace");
        if (!string.IsNullOrWhiteSpace(session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<음식주문운영추적응답>(
            cancellationToken: cancellationToken);
    }
}
