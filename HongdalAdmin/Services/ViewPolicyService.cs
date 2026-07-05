using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Common.ViewSettings;

namespace HongdalAdmin.Services;

public sealed class ViewPolicyService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private IReadOnlyList<View가시성항목응답> _visibleItems = [];

    public ViewPolicyService(HttpClient httpClient, 관리자인증세션Service session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public event Action? Changed;

    public IReadOnlyList<View가시성항목응답> VisibleItems => _visibleItems;
    public bool IsLoaded { get; private set; }

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoaded)
        {
            return;
        }

        await ReloadVisibleViewsAsync(cancellationToken);
    }

    public async Task ReloadVisibleViewsAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"api/v1/view-settings/effective?appKey={Uri.EscapeDataString(App식별자.HongdalAdmin)}");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<View가시성목록응답>(cancellationToken: cancellationToken);
        _visibleItems = payload?.Items?.OrderBy(x => x.SortOrder).ToArray() ?? [];
        IsLoaded = true;
        Changed?.Invoke();
    }

    public async Task<관리자View정책목록응답> GetPoliciesAsync(string? appKey = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(appKey)
            ? "api/v1/admin/view-policies"
            : $"api/v1/admin/view-policies?appKey={Uri.EscapeDataString(appKey)}";

        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<관리자View정책목록응답>(cancellationToken: cancellationToken)
               ?? new 관리자View정책목록응답();
    }

    public async Task<관리자View정책항목응답?> UpdatePolicyAsync(long id, bool policyEnabled, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, $"api/v1/admin/view-policies/{id}");
        request.Content = JsonContent.Create(new 관리자View정책수정요청
        {
            PolicyEnabled = policyEnabled
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<관리자View정책항목응답>(cancellationToken: cancellationToken);
        await ReloadVisibleViewsAsync(cancellationToken);
        return updated;
    }

    public View가시성항목응답? GetBlockingItem(string relativePath)
    {
        if (!IsLoaded)
        {
            return null;
        }

        var normalizedPath = NormalizePath(relativePath);
        var matched = _visibleItems
            .Where(x => IsMatchingRoute(x.Route, normalizedPath))
            .OrderByDescending(x => x.Route.Length)
            .FirstOrDefault();

        return matched is { EffectiveVisible: false } ? matched : null;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return request;
    }

    private static bool IsMatchingRoute(string route, string path)
    {
        var normalizedRoute = NormalizePath(route);
        if (normalizedRoute == "/")
        {
            return path == "/";
        }

        if (path.Equals(normalizedRoute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return path.StartsWith(normalizedRoute + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var normalized = path.StartsWith('/') ? path : "/" + path;
        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }
}
