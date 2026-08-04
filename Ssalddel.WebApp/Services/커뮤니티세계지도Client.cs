using System.Net.Http.Json;
using System.Net.Http.Headers;
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

    public async Task<커뮤니티세계지도질문초안Response> 질문초안생성Async(
        string observationStableId,
        커뮤니티세계지도질문초안Request request,
        CancellationToken cancellationToken = default)
    {
        var route = $"{커뮤니티세계지도Routes.ObservationApi}/{Uri.EscapeDataString(observationStableId)}/question-draft";
        using var response = await httpClient.PostAsJsonAsync(route, request, cancellationToken);
        await EnsureSuccessAsync(response, "질문 초안을 만들지 못했습니다.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<커뮤니티세계지도질문초안Response>(cancellationToken)
               ?? throw new InvalidOperationException("질문 초안 응답이 비어 있습니다.");
    }

    public async Task<커뮤니티세계지도뉴스후보Response> 뉴스후보조회Async(
        string observationStableId,
        string? sourceKey = null,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        var route = $"{커뮤니티세계지도Routes.ObservationApi}/{Uri.EscapeDataString(observationStableId)}/news-candidates";
        if (!string.IsNullOrWhiteSpace(sourceKey))
        {
            route += $"?sourceKey={Uri.EscapeDataString(sourceKey.Trim())}&take={Math.Clamp(take, 1, 20)}";
        }

        using var response = await httpClient.GetAsync(route, cancellationToken);
        await EnsureSuccessAsync(response, "뉴스 검토 후보를 조회하지 못했습니다.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<커뮤니티세계지도뉴스후보Response>(cancellationToken)
               ?? throw new InvalidOperationException("뉴스 검토 후보 응답이 비어 있습니다.");
    }

    public async Task<커뮤니티세계지도질문게시Response> 질문게시Async(
        string observationStableId,
        커뮤니티세계지도질문게시Request request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("질문을 게시하려면 먼저 로그인해 주세요.");
        }

        var route = $"{커뮤니티세계지도Routes.ObservationApi}/{Uri.EscapeDataString(observationStableId)}/questions";
        using var message = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        await EnsureSuccessAsync(response, "질문을 게시하지 못했습니다.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<커뮤니티세계지도질문게시Response>(cancellationToken)
               ?? throw new InvalidOperationException("질문 게시 응답이 비어 있습니다.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(detail)
            ? $"{fallbackMessage} HTTP {(int)response.StatusCode}"
            : $"{fallbackMessage} HTTP {(int)response.StatusCode}: {detail}");
    }
}
