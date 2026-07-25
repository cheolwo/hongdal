using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Exploration;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SsalddelApp.Services;

public sealed class HttpShipperExplorationInquiryService(
    SsalddelProtectedApiClient protectedApiClient) : IShipperExplorationInquiryService
{
    private const string BasePath = "api/v1/shipper/exploration-inbox";

    public async Task<IReadOnlyList<탐색문의목록항목응답>> 목록조회Async(
        CancellationToken cancellationToken = default)
    {
        using var response = await protectedApiClient.GetAsync(BasePath, cancellationToken);
        await 응답확인Async(response, "받은 탐색 문의 목록 조회", cancellationToken);
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<탐색문의목록항목응답>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<탐색문의상세응답?> 상세조회Async(
        long campaignId,
        CancellationToken cancellationToken = default)
    {
        using var response = await protectedApiClient.GetAsync(
            $"{BasePath}/{campaignId}",
            cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await 응답확인Async(response, "받은 탐색 문의 상세 조회", cancellationToken);
        return await response.Content.ReadFromJsonAsync<탐색문의상세응답>(
            cancellationToken: cancellationToken);
    }

    public async Task 응답Async(
        long campaignId,
        탐색문의응답요청 request,
        CancellationToken cancellationToken = default)
    {
        using var response = await protectedApiClient.PostAsProtectedJsonAsync(
            $"{BasePath}/{campaignId}/reply",
            request,
            cancellationToken);
        await 응답확인Async(response, "탐색 문의 응답 저장", cancellationToken);
    }

    private static async Task 응답확인Async(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(detail)
                ? $"{operation}에 실패했습니다. HTTP {(int)response.StatusCode}"
                : $"{operation}에 실패했습니다. HTTP {(int)response.StatusCode}: {detail}");
    }
}
