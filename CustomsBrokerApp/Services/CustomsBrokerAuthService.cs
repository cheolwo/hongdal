using System.Net.Http.Json;

namespace CustomsBrokerApp.Services;

public sealed class CustomsBrokerAuthService
{
    private readonly HttpClient _httpClient;

    public CustomsBrokerAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string AccessToken { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public bool IsSignedIn => !string.IsNullOrWhiteSpace(AccessToken);
    public bool CanCorrectHsCodes => Roles.Contains("관세사", StringComparer.Ordinal) ||
                                     Roles.Contains("서버관리자", StringComparer.Ordinal);

    public async Task<AuthResult> LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest
        {
            UserNameOrEmail = userNameOrEmail,
            Password = password
        };

        using var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            return AuthResult.Failure($"로그인 실패 ({(int)response.StatusCode}): {errorText}");
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return AuthResult.Failure("인증 토큰을 받지 못했습니다.");
        }

        AccessToken = token.AccessToken;
        UserName = token.UserName;
        Roles = token.Roles ?? [];

        if (!CanCorrectHsCodes)
        {
            Logout();
            return AuthResult.Failure("관세사 권한이 있는 계정만 사용할 수 있습니다.");
        }

        return AuthResult.Success();
    }

    public void Logout()
    {
        AccessToken = string.Empty;
        UserName = string.Empty;
        Roles = [];
    }

    private sealed class LoginRequest
    {
        public string UserNameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }
}

public sealed class AuthResult
{
    public bool Succeeded { get; private init; }
    public string ErrorMessage { get; private init; } = string.Empty;

    public static AuthResult Success() => new() { Succeeded = true };

    public static AuthResult Failure(string message) => new()
    {
        Succeeded = false,
        ErrorMessage = message
    };
}
