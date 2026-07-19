using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Driver.Settlement;

namespace Ssalddel.WebApp.Services;

public sealed class 기사정산Service
{
    private readonly HttpClient _httpClient;
    private readonly WebAuthSessionService _authSession;

    public 기사정산Service(HttpClient httpClient, WebAuthSessionService authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public Task<기사정산응답> 현재월조회Async(CancellationToken cancellationToken = default)
        => GetAsync<기사정산응답>("api/v1/driver/settlements/current-month", "현재 월 정산 조회", cancellationToken);

    public Task<IReadOnlyList<기사정산월요약응답>> 목록조회Async(CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<기사정산월요약응답>>("api/v1/driver/settlements", "정산 목록 조회", cancellationToken);

    private async Task<TResponse> GetAsync<TResponse>(
        string path,
        string actionName,
        CancellationToken cancellationToken)
    {
        using var request = await CreateAuthorizedRequestAsync(path, cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, actionName, cancellationToken);
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
               ?? throw new InvalidOperationException($"{actionName} 응답을 읽을 수 없습니다.");
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(string path, CancellationToken cancellationToken)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (!_authSession.IsLoggedIn || string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            throw new InvalidOperationException("정산 정보는 서버 인증이 필요합니다. 먼저 웹 로그인에서 기사 계정으로 로그인해 주세요.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, path);
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
