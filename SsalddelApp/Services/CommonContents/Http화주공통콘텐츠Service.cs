using System.Net;
using System.Net.Http.Json;
using Ssalddel.Contracts.CommonContents;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SsalddelApp.Services.CommonContents;

public sealed class Http화주공통콘텐츠Service(
    SsalddelProtectedApiClient protectedApiClient) : I화주공통콘텐츠Service
{
    private const string BasePath = "api/v1/app/common-contents/widget?역할=shipper&위치=";

    public Task<살뜰위젯콘텐츠Dto?> 혜택콘텐츠조회Async(
        CancellationToken cancellationToken = default)
        => 조회Async("payment-benefit", cancellationToken);

    public Task<살뜰위젯콘텐츠Dto?> 공지콘텐츠조회Async(
        CancellationToken cancellationToken = default)
        => 조회Async("notice", cancellationToken);

    private async Task<살뜰위젯콘텐츠Dto?> 조회Async(
        string location,
        CancellationToken cancellationToken)
    {
        using var response = await protectedApiClient.GetAsync(
            $"{BasePath}{Uri.EscapeDataString(location)}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(detail)
                    ? $"화주 공통 콘텐츠 조회에 실패했습니다. HTTP {(int)response.StatusCode}"
                    : $"화주 공통 콘텐츠 조회에 실패했습니다. HTTP {(int)response.StatusCode}: {detail}");
        }

        return await response.Content.ReadFromJsonAsync<살뜰위젯콘텐츠Dto>(
            cancellationToken: cancellationToken);
    }
}
