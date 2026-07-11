using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.CommandSettings;

namespace HongdalAdmin.Services;

public sealed class AuxiliaryFeatureSettingsService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly bool _useMemoryFallback;

    public AuxiliaryFeatureSettingsService(HttpClient httpClient, 관리자인증세션Service session, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _session = session;
        _useMemoryFallback = configuration.GetValue("AdminData:UseMemory", false);
    }

    public async Task<AuxiliaryFeatureSettingsResponse> GetAsync(string? userId, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return BuildMemoryResponse(userId);
        }

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
    {
        if (_useMemoryFallback)
        {
            return Task.CompletedTask;
        }

        return SendUpdateAsync($"api/v1/admin/auxiliary-feature-settings/global/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", isEnabled, cancellationToken);
    }

    public Task ResetGlobalAsync(string targetType, string targetName, string featureName, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return Task.CompletedTask;
        }

        return SendDeleteAsync($"api/v1/admin/auxiliary-feature-settings/global/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", cancellationToken);
    }

    public Task SetUserAsync(string userId, string targetType, string targetName, string featureName, bool isEnabled, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return Task.CompletedTask;
        }

        return SendUpdateAsync($"api/v1/admin/auxiliary-feature-settings/users/{Escape(userId)}/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", isEnabled, cancellationToken);
    }

    public Task ResetUserAsync(string userId, string targetType, string targetName, string featureName, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return Task.CompletedTask;
        }

        return SendDeleteAsync($"api/v1/admin/auxiliary-feature-settings/users/{Escape(userId)}/{Escape(targetType)}/{Escape(targetName)}/{Escape(featureName)}", cancellationToken);
    }

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

    private static AuxiliaryFeatureSettingsResponse BuildMemoryResponse(string? userId)
    {
        var hasUser = !string.IsNullOrWhiteSpace(userId);
        return new AuxiliaryFeatureSettingsResponse
        {
            Items =
            [
                new()
                {
                    TargetType = AuxiliaryFeatureTargetTypes.Command,
                    TargetName = "운송예외신고Command",
                    TargetDisplayName = "운송 예외 신고",
                    Category = "필수 업무",
                    Version = "v1.0",
                    VersionDisplayName = "홍달 1.0",
                    VersionSortOrder = 10,
                    IsCurrentRelease = true,
                    FeatureName = "상태전환",
                    FeatureDisplayName = "예외 상태 전환",
                    AppDefaultEnabled = true,
                    GlobalEnabled = true,
                    EffectiveEnabled = true,
                    IsRequired = true,
                    IsUserConfigurable = false
                },
                new()
                {
                    TargetType = AuxiliaryFeatureTargetTypes.Command,
                    TargetName = "운송예외신고Command",
                    TargetDisplayName = "운송 예외 신고",
                    Category = "운영 보조",
                    Version = "v1.0",
                    VersionDisplayName = "홍달 1.0",
                    VersionSortOrder = 10,
                    IsCurrentRelease = true,
                    FeatureName = "관리자알림",
                    FeatureDisplayName = "관리자 알림",
                    AppDefaultEnabled = true,
                    GlobalEnabled = true,
                    HasGlobalOverride = false,
                    UserEnabled = hasUser ? true : null,
                    HasUserOverride = hasUser,
                    EffectiveEnabled = true,
                    IsRequired = false,
                    IsUserConfigurable = true
                },
                new()
                {
                    TargetType = AuxiliaryFeatureTargetTypes.Service,
                    TargetName = "WorkRelationshipSnapshotService",
                    TargetDisplayName = "업무 인연 스냅샷",
                    Category = "관계 보조",
                    Version = "v1.5",
                    VersionDisplayName = "홍달 1.5",
                    VersionSortOrder = 15,
                    IsCurrentRelease = false,
                    FeatureName = "인연스냅샷",
                    FeatureDisplayName = "인연 스냅샷 기록",
                    AppDefaultEnabled = true,
                    GlobalEnabled = false,
                    HasGlobalOverride = true,
                    UserEnabled = null,
                    HasUserOverride = false,
                    EffectiveEnabled = false,
                    IsRequired = false,
                    IsUserConfigurable = true
                }
            ]
        };
    }
}
