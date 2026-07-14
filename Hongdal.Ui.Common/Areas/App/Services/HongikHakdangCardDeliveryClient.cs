using System.Net.Http.Json;
using Hongdal.Contracts.Common.Content;
using Hongdal.Contracts.Common.Notifications;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class HongikHakdangCardDeliveryClient
{
    private const string CardBasePath = "api/v1/content/hongik-hakdang/cards";
    private const string InstallationPath = "api/v1/mobile/push/installations";
    private readonly HongdalProtectedApiClient _apiClient;

    public HongikHakdangCardDeliveryClient(HongdalProtectedApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<HongikHakdangCardCatalogDto> GetCatalogAsync(
        string? collectionKey = null,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(collectionKey)
            ? $"{CardBasePath}/catalog"
            : $"{CardBasePath}/catalog?collectionKey={Uri.EscapeDataString(collectionKey.Trim())}";
        using var response = await _apiClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HongikHakdangCardCatalogDto>(cancellationToken)
               ?? throw new InvalidOperationException("카드 카탈로그 응답이 비어 있습니다.");
    }

    public async Task<HongikHakdangTodayCardDto> GetTodayAsync(
        string timeZoneId = "Asia/Seoul",
        CancellationToken cancellationToken = default)
    {
        using var response = await _apiClient.GetAsync(
            $"{CardBasePath}/today?timeZoneId={Uri.EscapeDataString(timeZoneId)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HongikHakdangTodayCardDto>(cancellationToken)
               ?? throw new InvalidOperationException("오늘의 카드 응답이 비어 있습니다.");
    }

    public async Task<HongikHakdangCardDeliveryPreferenceDto> GetPreferenceAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _apiClient.GetAsync($"{CardBasePath}/preferences", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HongikHakdangCardDeliveryPreferenceDto>(cancellationToken)
               ?? throw new InvalidOperationException("카드 설정 응답이 비어 있습니다.");
    }

    public async Task<HongikHakdangCardDeliveryPreferenceDto> UpdatePreferenceAsync(
        HongikHakdangCardDeliveryPreferenceUpdateRequest request,
        CancellationToken cancellationToken = default)
        => await _apiClient.PutAsProtectedJsonAsync<
               HongikHakdangCardDeliveryPreferenceUpdateRequest,
               HongikHakdangCardDeliveryPreferenceDto>(
               $"{CardBasePath}/preferences",
               request,
               cancellationToken)
           ?? throw new InvalidOperationException("카드 설정 저장 응답이 비어 있습니다.");

    public async Task<HongdalMobilePushInstallationResponse> RegisterInstallationAsync(
        HongdalMobilePushInstallationUpsertRequest request,
        CancellationToken cancellationToken = default)
        => await _apiClient.PutAsProtectedJsonAsync<
               HongdalMobilePushInstallationUpsertRequest,
               HongdalMobilePushInstallationResponse>(
               InstallationPath,
               request,
               cancellationToken)
           ?? throw new InvalidOperationException("모바일 설치 등록 응답이 비어 있습니다.");

    public async Task DeactivateInstallationAsync(
        string installationId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _apiClient.DeleteAsync(
            $"{InstallationPath}/{Uri.EscapeDataString(installationId)}",
            cancellationToken);
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}
