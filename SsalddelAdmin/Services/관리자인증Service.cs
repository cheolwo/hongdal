using System.Net.Http.Json;

namespace SsalddelAdmin.Services;

public sealed class 관리자인증Service
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;

    public 관리자인증Service(HttpClient httpClient, 관리자인증세션Service session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<로그인처리결과> 로그인Async(string userNameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var request = new 로그인요청
        {
            UserNameOrEmail = userNameOrEmail,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            return 로그인처리결과.실패($"로그인 실패 ({(int)response.StatusCode}): {errorText}");
        }

        var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken: cancellationToken);
        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return 로그인처리결과.실패("토큰 정보를 받지 못했습니다.");
        }

        if (token.Roles is null ||
            (!token.Roles.Contains("서버관리자", StringComparer.Ordinal) &&
             !token.Roles.Contains("관세사", StringComparer.Ordinal)))
        {
            return 로그인처리결과.실패("서버관리자 또는 관세사 권한이 없는 계정입니다.");
        }

        _session.로그인적용(token);
        return 로그인처리결과.성공();
    }

    public void 로그아웃()
    {
        _session.로그아웃();
    }
}

public sealed class 로그인요청
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class 토큰응답
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public sealed class 로그인처리결과
{
    public bool 성공여부 { get; private set; }
    public string 오류메시지 { get; private set; } = string.Empty;

    public static 로그인처리결과 성공() => new 로그인처리결과 { 성공여부 = true };

    public static 로그인처리결과 실패(string message) => new 로그인처리결과
    {
        성공여부 = false,
        오류메시지 = message
    };
}
