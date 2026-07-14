using System.Net.Http.Json;
using System.Text.Json;
using Hongdal.Client.Infrastructure.Security;
using Hongdal.Contracts.Common;
using Microsoft.JSInterop;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.WebApp.Services;

public sealed class WebAuthSessionService : IHongdalAccessTokenProvider
{
    private const string StorageKey = "hongdal.web.auth.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly IClientSessionGuard _sessionGuard;

    private ClientAuthTokenSnapshot? _snapshot;

    public WebAuthSessionService(
        HttpClient httpClient,
        IJSRuntime jsRuntime,
        IClientSessionGuard sessionGuard)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _sessionGuard = sessionGuard;
    }

    public string? AccessToken => _snapshot?.AccessToken;
    public string? UserId => _snapshot?.UserId;
    public string? UserName => _snapshot?.UserName;
    public IReadOnlyList<string> Roles => _snapshot?.Roles ?? [];
    public string PrimaryRole => WebRoleThemeResolver.ResolvePrimaryRole(Roles);
    public WebRoleTheme CurrentTheme => WebRoleThemeResolver.Resolve(Roles);
    public bool IsLoggedIn => _sessionGuard.IsAccessTokenUsable(_snapshot, DateTime.UtcNow);
    public event Action? Changed;

    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            _snapshot = null;
            NotifyChanged();
            return;
        }

        try
        {
            _snapshot = JsonSerializer.Deserialize<ClientAuthTokenSnapshot>(json, JsonOptions);
            if (!_sessionGuard.IsAccessTokenUsable(_snapshot, DateTime.UtcNow))
            {
                _snapshot = null;
            }
        }
        catch
        {
            _snapshot = null;
        }

        NotifyChanged();
    }

    public async Task LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("아이디와 비밀번호를 입력해 주세요.");
        }

        using var response = await _httpClient.PostAsJsonAsync(
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

        var token = await response.Content.ReadFromJsonAsync<토큰응답>(cancellationToken);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("서버 로그인 응답에서 토큰을 읽을 수 없습니다.");
        }

        _snapshot = token.ToClientAuthTokenSnapshot();

        await SaveSnapshotAsync(cancellationToken);
        NotifyChanged();
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _snapshot = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
        NotifyChanged();
    }

    private async Task SaveSnapshotAsync(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(_snapshot, JsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, json);
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
