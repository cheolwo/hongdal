using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I홍익학당철학영상MapLayerClient
{
    Task<HongikAcademyContentMapLayerResponse> 레이어조회Async(
        CancellationToken cancellationToken = default);
}

public sealed class 홍익학당철학영상MapLayerClient(
    ISsalddelJsonApiClient apiClient) : I홍익학당철학영상MapLayerClient
{
    public async Task<HongikAcademyContentMapLayerResponse> 레이어조회Async(
        CancellationToken cancellationToken = default)
        => await apiClient.GetAsync<HongikAcademyContentMapLayerResponse>(
               HongikAcademyContentMapRoutes.LayerApi,
               "홍익학당 철학·영상 지도 레이어 조회",
               allowNotFound: false,
               cancellationToken)
           ?? throw new InvalidOperationException("홍익학당 철학·영상 지도 레이어 응답이 비어 있습니다.");
}
