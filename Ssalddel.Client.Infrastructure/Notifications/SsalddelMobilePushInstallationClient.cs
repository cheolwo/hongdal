using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Notifications;

namespace Ssalddel.Client.Infrastructure.Notifications;

public enum SsalddelMobilePushRegistrationState
{
    Registered,
    TokenNotAvailable,
    AuthenticationRequired,
    Failed
}

public sealed record SsalddelMobilePushRegistrationResult(
    SsalddelMobilePushRegistrationState State,
    string? ErrorMessage = null);

public sealed class SsalddelMobilePushInstallationClient
{
    private const string InstallationsPath = "api/v1/mobile/push/installations";

    private readonly HttpClient _httpClient;
    private readonly ISsalddelMobilePushTokenProvider _pushTokenProvider;
    private readonly Func<string?> _accessTokenProvider;

    public SsalddelMobilePushInstallationClient(
        HttpClient httpClient,
        ISsalddelMobilePushTokenProvider pushTokenProvider,
        Func<string?> accessTokenProvider)
    {
        _httpClient = httpClient;
        _pushTokenProvider = pushTokenProvider;
        _accessTokenProvider = accessTokenProvider;
    }

    public async Task<SsalddelMobilePushRegistrationResult> EnsureRegisteredAsync(
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _pushTokenProvider.GetCurrentAsync(cancellationToken);
        if (snapshot is null || string.IsNullOrWhiteSpace(snapshot.PushToken))
        {
            return new SsalddelMobilePushRegistrationResult(
                SsalddelMobilePushRegistrationState.TokenNotAvailable);
        }

        var accessToken = _accessTokenProvider();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new SsalddelMobilePushRegistrationResult(
                SsalddelMobilePushRegistrationState.AuthenticationRequired);
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, InstallationsPath)
        {
            Content = JsonContent.Create(new SsalddelMobilePushInstallationUpsertRequest(
                snapshot.InstallationId,
                snapshot.AppKey,
                snapshot.Platform,
                snapshot.PushToken,
                snapshot.AppVersion,
                snapshot.DeviceModel))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new SsalddelMobilePushRegistrationResult(
                    SsalddelMobilePushRegistrationState.Registered);
            }

            return new SsalddelMobilePushRegistrationResult(
                SsalddelMobilePushRegistrationState.Failed,
                $"모바일 Push 설치 등록 실패: HTTP {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return new SsalddelMobilePushRegistrationResult(
                SsalddelMobilePushRegistrationState.Failed,
                ex.Message);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SsalddelMobilePushRegistrationResult(
                SsalddelMobilePushRegistrationState.Failed,
                "모바일 Push 설치 등록 시간이 초과되었습니다.");
        }
    }
}
