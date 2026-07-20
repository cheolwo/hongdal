using System.Net.Http.Json;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Contracts.Common;

namespace WarehouseManagerApp.Services;

public sealed record WarehouseAuthResult(bool IsSuccess, string ErrorMessage)
{
    public static WarehouseAuthResult Success { get; } = new(true, string.Empty);
}

/// <summary>창고 앱의 로그인·토큰 갱신 HTTP 통신만 담당합니다.</summary>
public sealed class WarehouseAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly ClientAuthSession _session;

    public WarehouseAuthApiService(HttpClient httpClient, ClientAuthSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public Task<WarehouseAuthResult> LoginAsync(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(new WarehouseAuthResult(
                false,
                "아이디와 비밀번호를 입력해 주세요."));
        }

        return SendTokenRequestAsync(
            "api/v1/auth/login",
            new 로그인요청
            {
                UserNameOrEmail = userNameOrEmail.Trim(),
                Password = password
            },
            "로그인에 실패했습니다. 아이디와 비밀번호를 확인해 주세요.",
            cancellationToken);
    }

    public Task<WarehouseAuthResult> RefreshAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Task.FromResult(new WarehouseAuthResult(
                false,
                "로그인 세션을 갱신할 수 없습니다."));
        }

        return SendTokenRequestAsync(
            "api/v1/auth/refresh",
            new 토큰갱신요청
            {
                UserId = userId,
                RefreshToken = refreshToken
            },
            "로그인 세션이 만료되었습니다. 다시 로그인해 주세요.",
            cancellationToken);
    }

    private async Task<WarehouseAuthResult> SendTokenRequestAsync<TRequest>(
        string path,
        TRequest request,
        string failureMessage,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(path, request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new WarehouseAuthResult(false, failureMessage);
        }

        var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return new WarehouseAuthResult(false, "서버 인증 응답을 읽을 수 없습니다.");
        }

        await _session.ApplyAsync(token.ToClientAuthTokenSnapshot(), cancellationToken);
        return WarehouseAuthResult.Success;
    }
}
