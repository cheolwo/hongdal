using System.Net;
using System.Net.Http.Json;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Contracts.Common;

namespace SsalddelAdminApp.Services;

public sealed class AdminAuthService
{
    private readonly HttpClient httpClient;
    private readonly AdminAuthSession session;
    private readonly SemaphoreSlim refreshGate = new(1, 1);

    public AdminAuthService(HttpClient httpClient, AdminAuthSession session)
    {
        this.httpClient = httpClient;
        this.session = session;
    }

    public async Task<AdminLoginResult> LoginAsync(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = await SendTokenRequestAsync(
            "api/v1/auth/login",
            new 로그인요청
            {
                UserNameOrEmail = userNameOrEmail,
                Password = password
            },
            "로그인에 실패했습니다.",
            cancellationToken);
        return result.Succeeded
            ? AdminLoginResult.Success()
            : AdminLoginResult.Fail(result.ErrorMessage ?? "로그인에 실패했습니다.");
    }

    public async Task<string?> EnsureAccessTokenAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        await refreshGate.WaitAsync(cancellationToken);
        try
        {
            var state = await session.RestoreAsync(cancellationToken);
            if (!forceRefresh && state == ClientAuthSessionRestoreState.Authenticated)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(session.UserId)
                || string.IsNullOrWhiteSpace(session.RefreshToken)
                || session.RefreshTokenExpiresAtUtc <= DateTime.UtcNow)
            {
                return "관리자 로그인 세션이 만료되었습니다. 다시 로그인해 주세요.";
            }

            var result = await SendTokenRequestAsync(
                "api/v1/auth/refresh",
                new 토큰갱신요청
                {
                    UserId = session.UserId,
                    RefreshToken = session.RefreshToken
                },
                "관리자 로그인 세션을 갱신하지 못했습니다.",
                cancellationToken);
            if (!result.Succeeded
                && result.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                await session.LogoutAsync();
            }

            return result.ErrorMessage;
        }
        finally
        {
            refreshGate.Release();
        }
    }

    private async Task<AdminTokenRequestResult> SendTokenRequestAsync<TRequest>(
        string path,
        TRequest request,
        string failureMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new AdminTokenRequestResult(
                    false,
                    $"{failureMessage} ({(int)response.StatusCode})",
                    response.StatusCode);
            }

            var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return new AdminTokenRequestResult(false, "서버에서 로그인 토큰을 받지 못했습니다.");
            }

            if (!token.Roles.Contains("서버관리자", StringComparer.Ordinal))
            {
                return new AdminTokenRequestResult(
                    false,
                    "서버관리자 권한이 있는 계정만 사용할 수 있습니다.",
                    HttpStatusCode.Unauthorized);
            }

            await session.ApplyAsync(token);
            return AdminTokenRequestResult.Success;
        }
        catch (HttpRequestException)
        {
            return new AdminTokenRequestResult(false, "살뜰 서비스에 연결할 수 없습니다.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AdminTokenRequestResult(false, "관리자 인증 응답 시간이 초과되었습니다.");
        }
    }

    private sealed record AdminTokenRequestResult(
        bool Succeeded,
        string? ErrorMessage = null,
        HttpStatusCode? StatusCode = null)
    {
        public static AdminTokenRequestResult Success { get; } = new(true);
    }
}

public sealed record AdminLoginResult(bool Succeeded, string? ErrorMessage)
{
    public static AdminLoginResult Success() => new(true, null);
    public static AdminLoginResult Fail(string message) => new(false, message);
}
