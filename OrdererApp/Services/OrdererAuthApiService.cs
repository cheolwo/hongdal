using System.Net.Http.Json;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Contracts.Common;

namespace OrdererApp.Services;

public sealed record OrdererAuthResult(bool 성공, string? 오류메시지 = null);

/// <summary>주문자 앱 로그인과 토큰 갱신 HTTP 통신만 담당합니다.</summary>
public sealed class OrdererAuthApiService(
    HttpClient httpClient,
    ClientAuthSession session)
{
    public Task<OrdererAuthResult> 로그인Async(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(new OrdererAuthResult(false, "아이디와 비밀번호를 입력해 주세요."));
        }

        return 토큰요청Async(
            "api/v1/auth/login",
            new 로그인요청
            {
                UserNameOrEmail = userNameOrEmail.Trim(),
                Password = password
            },
            "로그인에 실패했습니다. 아이디와 비밀번호를 확인해 주세요.",
            cancellationToken);
    }

    public Task<OrdererAuthResult> 갱신Async(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Task.FromResult(new OrdererAuthResult(false, "로그인 세션을 갱신할 수 없습니다."));
        }

        return 토큰요청Async(
            "api/v1/auth/refresh",
            new 토큰갱신요청
            {
                UserId = userId,
                RefreshToken = refreshToken
            },
            "로그인 세션이 만료되었습니다. 다시 로그인해 주세요.",
            cancellationToken);
    }

    private async Task<OrdererAuthResult> 토큰요청Async<TRequest>(
        string path,
        TRequest request,
        string failureMessage,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new OrdererAuthResult(false, failureMessage);
        }

        var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return new OrdererAuthResult(false, "서버 인증 응답을 읽을 수 없습니다.");
        }

        await session.ApplyAsync(token.ToClientAuthTokenSnapshot(), cancellationToken);
        return new OrdererAuthResult(true);
    }
}
