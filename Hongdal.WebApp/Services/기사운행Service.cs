using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Work;

namespace Hongdal.WebApp.Services;

public sealed class 기사운행Service
{
    private readonly HttpClient _httpClient;
    private readonly WebAuthSessionService _authSession;

    public 기사운행Service(HttpClient httpClient, WebAuthSessionService authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public Task<기사운행시작응답> 운행시작Async(
        기사운행시작요청 payload,
        CancellationToken cancellationToken = default)
        => PostJsonAsync<기사운행시작요청, 기사운행시작응답>(
            "api/v1/driver/work/start",
            payload,
            "운행 시작",
            cancellationToken);

    public async Task 운행종료Async(CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/driver/work/stop", cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "운행 종료", cancellationToken);
    }

    public Task<기사위치갱신응답> 위치갱신Async(
        기사위치갱신요청 payload,
        CancellationToken cancellationToken = default)
        => PostJsonAsync<기사위치갱신요청, 기사위치갱신응답>(
            "api/v1/driver/work/location",
            payload,
            "위치 갱신",
            cancellationToken);

    public Task<기사운행상태응답> 운행상태조회Async(CancellationToken cancellationToken = default)
        => GetAsync<기사운행상태응답>("api/v1/driver/work/status", "운행 상태 조회", cancellationToken);

    public Task<기사현재근무응답> 현재근무조회Async(CancellationToken cancellationToken = default)
        => GetAsync<기사현재근무응답>("api/v1/driver/work/current", "현재 근무 조회", cancellationToken);

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

    private async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        string path,
        TRequest payload,
        string actionName,
        CancellationToken cancellationToken)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, path, cancellationToken);
        request.Content = JsonContent.Create(payload);

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
            throw new InvalidOperationException("이 작업은 서버 인증이 필요합니다. 먼저 웹 로그인에서 기사 계정으로 로그인해 주세요.");
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
