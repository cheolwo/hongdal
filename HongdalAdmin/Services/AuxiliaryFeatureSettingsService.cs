using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.CommandSettings;

namespace HongdalAdmin.Services;

public sealed class AuxiliaryFeatureSettingsService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;

    public AuxiliaryFeatureSettingsService(HttpClient httpClient, 관리자인증세션Service session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<AuxiliaryFeatureSettingsResponse> GetAsync(string? userId, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(userId)
            ? "api/v1/admin/auxiliary-feature-settings"
            : $"api/v1/admin/auxiliary-feature-settings?userId={Uri.EscapeDataString(userId.Trim())}";

        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuxiliaryFeatureSettingsResponse>(cancellationToken: cancellationToken)
               ?? new AuxiliaryFeatureSettingsResponse();
    }

    public Task SetGlobalAsync(string targetType, string targetName, string featureName, bool isEnabled, CancellationToken cancellationToken = default)
        => SendUpdateAsync($"api/v1/admin/auxiliary-feature-settings/global/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", isEnabled, cancellationToken);

    public Task ResetGlobalAsync(string targetType, string targetName, string featureName, CancellationToken cancellationToken = default)
        => SendDeleteAsync($"api/v1/admin/auxiliary-feature-settings/global/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", cancellationToken);

    public Task SetUserAsync(string userId, string targetType, string targetName, string featureName, bool isEnabled, CancellationToken cancellationToken = default)
        => SendUpdateAsync($"api/v1/admin/auxiliary-feature-settings/users/{Escape(userId)}/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", isEnabled, cancellationToken);

    public Task ResetUserAsync(string userId, string targetType, string targetName, string featureName, CancellationToken cancellationToken = default)
        => SendDeleteAsync($"api/v1/admin/auxiliary-feature-settings/users/{Escape(userId)}/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", cancellationToken);

    private async Task SendUpdateAsync(string path, bool isEnabled, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Put, path);
        request.Content = JsonContent.Create(new AuxiliaryFeatureSettingUpdateRequest
        {
            IsEnabled = isEnabled
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendDeleteAsync(string path, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Delete, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
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

    private static string Escape(string value) => Uri.EscapeDataString(value.Trim());
}
