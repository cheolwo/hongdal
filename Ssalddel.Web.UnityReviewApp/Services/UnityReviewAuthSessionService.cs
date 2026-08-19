using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Contracts.Common;

namespace Ssalddel.Web.UnityReviewApp.Services;

public sealed class UnityReviewAuthSessionService(
    HttpClient httpClient,
    IJSRuntime jsRuntime,
    IClientSessionGuard sessionGuard)
{
    private const string StorageKey = "ssalddel.unity-review.auth.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private ClientAuthTokenSnapshot? _snapshot;
    private Task? _restoreTask;

    public string? AccessToken => _snapshot?.AccessToken;
    public string? UserId => _snapshot?.UserId;
    public string? UserName => _snapshot?.UserName;
    public IReadOnlyList<string> Roles => _snapshot?.Roles ?? [];
    public bool IsInitialized { get; private set; }
    public bool IsLoggedIn => sessionGuard.IsAccessTokenUsable(_snapshot, DateTime.UtcNow);
    public bool IsServerAdministrator => Roles.Any(role =>
        string.Equals(role, "서버관리자", StringComparison.OrdinalIgnoreCase));

    public event Action? Changed;

    public Task EnsureRestoredAsync()
        => _restoreTask ??= RestoreCoreAsync();

    public async Task LoginAsync(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("아이디와 비밀번호를 입력해 주세요.");
        }

        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/auth/login",
            new 로그인요청
            {
                UserNameOrEmail = userNameOrEmail.Trim(),
                Password = password
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"서버 로그인에 실패했습니다. HTTP {(int)response.StatusCode}"
                : $"서버 로그인에 실패했습니다. HTTP {(int)response.StatusCode}: {body}");
        }

        var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken)
                    ?? throw new InvalidOperationException("서버 로그인 응답이 비어 있습니다.");
        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("서버 로그인 응답에서 토큰을 읽을 수 없습니다.");
        }

        _snapshot = token.ToClientAuthTokenSnapshot();
        IsInitialized = true;
        await SaveSnapshotAsync(cancellationToken);
        Changed?.Invoke();
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _snapshot = null;
        IsInitialized = true;
        await jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            cancellationToken,
            StorageKey);
        Changed?.Invoke();
    }

    private async Task RestoreCoreAsync()
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                _snapshot = JsonSerializer.Deserialize<ClientAuthTokenSnapshot>(json, JsonOptions);
                if (!sessionGuard.IsAccessTokenUsable(_snapshot, DateTime.UtcNow))
                {
                    _snapshot = null;
                }
            }
        }
        catch (JsonException)
        {
            _snapshot = null;
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
        finally
        {
            IsInitialized = true;
            Changed?.Invoke();
        }
    }

    private async Task SaveSnapshotAsync(CancellationToken cancellationToken)
    {
        await jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            cancellationToken,
            StorageKey,
            JsonSerializer.Serialize(_snapshot, JsonOptions));
    }
}
