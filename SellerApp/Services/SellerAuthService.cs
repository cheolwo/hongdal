using System.Net;
using System.Net.Http.Json;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Contracts.Common;

namespace SellerApp.Services;

public sealed class SellerAuthService
{
    private static readonly string[] SellerRoles = ["판매자", "화주", "서버관리자"];
    private readonly HttpClient httpClient;
    private readonly SellerAuthSession session;
    private readonly SemaphoreSlim refreshGate = new(1, 1);

    public SellerAuthService(HttpClient httpClient, SellerAuthSession session)
    {
        this.httpClient = httpClient;
        this.session = session;
    }

    public static bool IsSellerRole(string role)
        => SellerRoles.Contains(role, StringComparer.Ordinal);

    public async Task<SellerLoginResult> LoginAsync(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail)
            || string.IsNullOrWhiteSpace(password))
        {
            return SellerLoginResult.Fail("아이디와 비밀번호를 입력해 주세요.");
        }

        var result = await SendTokenRequestAsync(
            "api/v1/auth/login",
            new 로그인요청
            {
                UserNameOrEmail = userNameOrEmail.Trim(),
                Password = password
            },
            "판매자 로그인에 실패했습니다.",
            cancellationToken);
        return result.Succeeded
            ? SellerLoginResult.Success()
            : SellerLoginResult.Fail(result.ErrorMessage ?? "판매자 로그인에 실패했습니다.");
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
                || string.IsNullOrWhiteSpace(session.RefreshToken))
            {
                return "판매자 로그인 세션이 만료되었습니다. 다시 로그인해 주세요.";
            }

            var result = await SendTokenRequestAsync(
                "api/v1/auth/refresh",
                new 토큰갱신요청
                {
                    UserId = session.UserId,
                    RefreshToken = session.RefreshToken
                },
                "판매자 로그인 세션을 갱신하지 못했습니다.",
                cancellationToken);
            if (!result.Succeeded
                && result.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                await session.ClearAsync(cancellationToken);
            }

            return result.ErrorMessage;
        }
        finally
        {
            refreshGate.Release();
        }
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
        => session.ClearAsync(cancellationToken);

    private async Task<TokenRequestResult> SendTokenRequestAsync<TRequest>(
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
                return new(false, $"{failureMessage} ({(int)response.StatusCode})", response.StatusCode);
            }

            var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return new(false, "서버에서 로그인 토큰을 받지 못했습니다.");
            }

            if (!token.Roles.Any(IsSellerRole))
            {
                return new(
                    false,
                    "판매자 또는 화주 역할이 있는 계정만 사용할 수 있습니다.",
                    HttpStatusCode.Unauthorized);
            }

            await session.ApplyAsync(token.ToClientAuthTokenSnapshot(), cancellationToken);
            return new(true);
        }
        catch (HttpRequestException)
        {
            return new(false, "살뜰 서버에 연결할 수 없습니다.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "판매자 인증 응답 시간이 초과되었습니다.");
        }
    }

    private sealed record TokenRequestResult(
        bool Succeeded,
        string? ErrorMessage = null,
        HttpStatusCode? StatusCode = null);
}

public sealed record SellerLoginResult(bool Succeeded, string? ErrorMessage)
{
    public static SellerLoginResult Success() => new(true, null);
    public static SellerLoginResult Fail(string message) => new(false, message);
}
