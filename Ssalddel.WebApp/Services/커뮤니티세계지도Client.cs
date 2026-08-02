using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.WebApp.Services;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.ClientAdapter,
    "현재 dataset의 공개 지도 snapshot을 주기적으로 다시 조회",
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.UiStateMutation,
    Boundary = "조회 실패를 내장 자료로 위장하지 않고 화면에 연결 상태를 알립니다.")]
public sealed class 커뮤니티세계지도Client(HttpClient httpClient)
{
    public async Task<커뮤니티세계지도SnapshotDto> 조회Async(
        string datasetCode,
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        var route = $"{커뮤니티세계지도Routes.ObservationApi}?dataset={Uri.EscapeDataString(datasetCode)}";
        return await httpClient.GetFromJsonAsync<커뮤니티세계지도SnapshotDto>(route, timeout.Token)
               ?? throw new InvalidOperationException("세계 지도 snapshot 응답이 비어 있습니다.");
    }
}
