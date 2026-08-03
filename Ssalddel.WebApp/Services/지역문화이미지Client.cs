using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.WebApp.Services;

public sealed class 지역문화이미지Client(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AppContextImageAssetDto>> 팩조회Async(
        string appPackId,
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        var response = await httpClient.GetFromJsonAsync<AppContextImageAssetListResponse>(
            AppContextImageAssetRoutes.ForPack(appPackId),
            timeout.Token);
        return response?.Items
               ?? throw new InvalidOperationException("지역문화 이미지 팩 응답이 비어 있습니다.");
    }
}
