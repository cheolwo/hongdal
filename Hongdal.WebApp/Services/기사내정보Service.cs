using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Profile;

namespace Hongdal.WebApp.Services;

public sealed class 기사내정보Service
{
    private readonly HttpClient _httpClient;
    private readonly WebAuthSessionService _authSession;

    public 기사내정보Service(HttpClient httpClient, WebAuthSessionService authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public async Task<용달기사등록응답> 내프로필조회Async(CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "기사 내 정보 조회", cancellationToken);
        return await response.Content.ReadFromJsonAsync<용달기사등록응답>(cancellationToken)
               ?? throw new InvalidOperationException("기사 내 정보 조회 응답을 읽을 수 없습니다.");
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(CancellationToken cancellationToken)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (!_authSession.IsLoggedIn || string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            throw new InvalidOperationException("기사 내 정보는 서버 인증이 필요합니다. 먼저 웹 로그인에서 기사 계정으로 로그인해 주세요.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/drivers/me");
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
