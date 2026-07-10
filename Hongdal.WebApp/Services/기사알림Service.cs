using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Notification;

namespace Hongdal.WebApp.Services;

public sealed class 기사알림Service
{
    private readonly HttpClient _httpClient;
    private readonly WebAuthSessionService _authSession;

    public 기사알림Service(HttpClient httpClient, WebAuthSessionService authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public Task<기사알림설정응답> 설정조회Async(CancellationToken cancellationToken = default)
        => GetAsync<기사알림설정응답>("api/v1/driver/notifications/settings", "알림 설정 조회", cancellationToken);

    public Task<기사푸시토큰응답> 푸시토큰조회Async(CancellationToken cancellationToken = default)
        => GetAsync<기사푸시토큰응답>("api/v1/driver/notifications/push-token", "푸시 토큰 조회", cancellationToken);

    public async Task<기사알림설정응답> 설정저장Async(
        기사알림설정수정요청 payload,
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, "api/v1/driver/notifications/settings", cancellationToken);
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "알림 설정 저장", cancellationToken);
        return await response.Content.ReadFromJsonAsync<기사알림설정응답>(cancellationToken)
               ?? throw new InvalidOperationException("알림 설정 저장 응답을 읽을 수 없습니다.");
    }

    private async Task<TResponse> GetAsync<TResponse>(
        string path,
        string actionName,
        CancellationToken cancellationToken)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, path, cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, actionName, cancellationToken);
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
               ?? throw new InvalidOperationException($"{actionName} 응답을 읽을 수 없습니다.");
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (!_authSession.IsLoggedIn || string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            throw new InvalidOperationException("알림 설정은 서버 인증이 필요합니다. 먼저 웹 로그인에서 기사 계정으로 로그인해 주세요.");
        }

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string actionName, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
            ? $"{actionName} 실패: HTTP {(int)response.StatusCode}"
            : $"{actionName} 실패: HTTP {(int)response.StatusCode}: {body}");
    }
}
