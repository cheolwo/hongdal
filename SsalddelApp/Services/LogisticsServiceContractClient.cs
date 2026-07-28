using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.WarehouseBilling;

namespace SsalddelApp.Services;

public sealed class LogisticsServiceContractClient(
    HttpClient httpClient,
    IAuthSession authSession,
    AuthApiService authApiService)
{
    private const string CostPreviewPath =
        "api/v1/logistics-service-contracts/cost-preview";

    public async Task<물류대행비용미리보기응답> CreateCostPreviewAsync(
        물류대행비용미리보기요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var refreshError = await authApiService.EnsureAccessTokenAsync(
            cancellationToken: cancellationToken);
        if (!string.IsNullOrWhiteSpace(refreshError))
        {
            throw new InvalidOperationException(refreshError);
        }

        using var response = await SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var retryRefreshError = await authApiService.EnsureAccessTokenAsync(
                forceRefresh: true,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(retryRefreshError))
            {
                throw new InvalidOperationException(retryRefreshError);
            }

            using var retryResponse = await SendAsync(request, cancellationToken);
            return await ReadResponseAsync(retryResponse, cancellationToken);
        }

        return await ReadResponseAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        물류대행비용미리보기요청 request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, CostPreviewPath)
        {
            Content = JsonContent.Create(request)
        };
        if (!string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            message.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        }

        return await httpClient.SendAsync(message, cancellationToken);
    }

    private static async Task<물류대행비용미리보기응답> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(detail)
                    ? "물류대행 계약 비용 검토안을 만들지 못했습니다."
                    : $"물류대행 계약 비용 검토안을 만들지 못했습니다. {detail}");
        }

        return await response.Content
                   .ReadFromJsonAsync<물류대행비용미리보기응답>(cancellationToken)
               ?? throw new InvalidOperationException(
                   "물류대행 계약 비용 검토 응답이 비어 있습니다.");
    }
}
