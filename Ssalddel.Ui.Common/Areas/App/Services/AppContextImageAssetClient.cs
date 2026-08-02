using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class AppContextImageAssetClient(
    ISsalddelJsonApiClient apiClient)
{
    public async Task<IReadOnlyList<AppContextImageAssetDto>> GetPackAsync(
        string appPackId,
        CancellationToken cancellationToken = default)
    {
        var response = await apiClient.GetAsync<AppContextImageAssetListResponse>(
            AppContextImageAssetRoutes.ForPack(appPackId),
            "앱 문맥 이미지 조회",
            allowNotFound: false,
            cancellationToken);
        return response?.Items ?? [];
    }
}
