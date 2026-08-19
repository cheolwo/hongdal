using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Web.UnityReviewApp.Services;

public interface ISynty공간조립모바일검토Client
{
    Task<Synty공간조립검토함Response> 검토함조회Async(
        string? reviewStateCode = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<Synty공간조립검토결정전송결과> 결정전송Async(
        string reviewItemStableId,
        Synty공간조립검토결정Request request,
        CancellationToken cancellationToken = default);

    Task<Synty공간조립오프라인동기화결과> 오프라인대기열동기화Async(
        CancellationToken cancellationToken = default);

    Task<int> 오프라인대기수조회Async(CancellationToken cancellationToken = default);
}

public sealed class Synty공간조립모바일검토Client(
    HttpClient httpClient,
    UnityReviewAuthSessionService authSession,
    Synty공간조립오프라인검토Store offlineStore)
    : ISynty공간조립모바일검토Client
{
    public async Task<Synty공간조립검토함Response> 검토함조회Async(
        string? reviewStateCode = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var url = $"{Synty공간조립모바일검토Routes.Base}?take={Math.Clamp(take, 1, 100)}";
        if (!string.IsNullOrWhiteSpace(reviewStateCode))
        {
            url += $"&reviewStateCode={Uri.EscapeDataString(reviewStateCode.Trim())}";
        }

        using var request = CreateAuthorizedRequest(HttpMethod.Get, url);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "공간 조립 검토함을 불러오지 못했습니다.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<Synty공간조립검토함Response>(cancellationToken)
               ?? throw new InvalidOperationException("공간 조립 검토함 응답이 비어 있습니다.");
    }

    public async Task<Synty공간조립검토결정전송결과> 결정전송Async(
        string reviewItemStableId,
        Synty공간조립검토결정Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await SendOnlineAsync(reviewItemStableId, request, cancellationToken);
            return new Synty공간조립검토결정전송결과(item, false, "검토 결과를 서버 원장에 저장했습니다.");
        }
        catch (HttpRequestException)
        {
            await offlineStore.추가또는교체Async(reviewItemStableId, request, cancellationToken);
            return new Synty공간조립검토결정전송결과(
                null,
                true,
                "통신이 없어 휴대폰에 임시 저장했습니다. 연결되면 동기화해 주세요.");
        }
    }

    public async Task<Synty공간조립오프라인동기화결과> 오프라인대기열동기화Async(
        CancellationToken cancellationToken = default)
    {
        var queue = await offlineStore.목록Async(cancellationToken);
        var synchronizedCount = 0;
        string? error = null;
        while (queue.Count > 0)
        {
            var pending = queue[0];
            try
            {
                await SendOnlineAsync(pending.ReviewItemStableId, pending.Request, cancellationToken);
                queue.RemoveAt(0);
                synchronizedCount++;
                await offlineStore.저장Async(queue, cancellationToken);
            }
            catch (HttpRequestException)
            {
                error = "아직 서버에 연결할 수 없습니다.";
                break;
            }
            catch (Synty공간조립검토HttpException exception)
            {
                error = exception.StatusCode == HttpStatusCode.Conflict
                    ? "오프라인 검토 중 원장이 변경되었습니다. 최신 카드를 다시 확인해 주세요."
                    : exception.Message;
                break;
            }
        }

        return new Synty공간조립오프라인동기화결과(
            synchronizedCount,
            queue.Count,
            error);
    }

    public async Task<int> 오프라인대기수조회Async(CancellationToken cancellationToken = default)
        => (await offlineStore.목록Async(cancellationToken)).Count;

    private async Task<Synty공간조립검토항목Dto> SendOnlineAsync(
        string reviewItemStableId,
        Synty공간조립검토결정Request request,
        CancellationToken cancellationToken)
    {
        var url = $"{Synty공간조립모바일검토Routes.Base}/items/" +
                  $"{Uri.EscapeDataString(reviewItemStableId)}/decisions";
        using var httpRequest = CreateAuthorizedRequest(HttpMethod.Post, url);
        httpRequest.Content = JsonContent.Create(request);
        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadFailureAsync(response, cancellationToken);
            throw new Synty공간조립검토HttpException(response.StatusCode, message);
        }
        return await response.Content.ReadFromJsonAsync<Synty공간조립검토항목Dto>(cancellationToken)
               ?? throw new InvalidOperationException("공간 조립 검토 저장 응답이 비어 있습니다.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string fallback,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        throw new Synty공간조립검토HttpException(
            response.StatusCode,
            await ReadFailureAsync(response, cancellationToken, fallback));
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        if (!authSession.IsLoggedIn || string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            throw new InvalidOperationException("Unity 산출물 검토 앱에 서버관리자로 로그인해 주세요.");
        }

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        return request;
    }

    private static async Task<string> ReadFailureAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        string? fallback = null)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(body)
            ? fallback ?? $"HTTP {(int)response.StatusCode}"
            : body;
    }
}

public sealed record Synty공간조립검토결정전송결과(
    Synty공간조립검토항목Dto? Item,
    bool QueuedOffline,
    string Message);

public sealed record Synty공간조립오프라인동기화결과(
    int SynchronizedCount,
    int PendingCount,
    string? ErrorMessage);

public sealed class Synty공간조립검토HttpException(
    HttpStatusCode statusCode,
    string message) : InvalidOperationException(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
