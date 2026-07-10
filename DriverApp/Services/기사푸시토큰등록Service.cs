using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Notification;
using Microsoft.Maui.Storage;

namespace DriverApp.Services;

public sealed class 기사푸시토큰등록Service : I기사푸시토큰등록Service
{
    private const string 저장키 = "hongdal.driver.fcmToken.v1";

    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;

    public 기사푸시토큰등록Service(HttpClient httpClient, IAuthSession authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public async Task 수신토큰저장및등록Async(string? pushToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pushToken))
        {
            return;
        }

        var trimmedToken = pushToken.Trim();
        Preferences.Set(저장키, trimmedToken);
        await 서버등록Async(trimmedToken, cancellationToken);
    }

    public async Task 저장토큰등록Async(CancellationToken cancellationToken = default)
    {
        var token = Preferences.Get(저장키, string.Empty);
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        await 서버등록Async(token, cancellationToken);
    }

    private async Task 서버등록Async(string pushToken, CancellationToken cancellationToken)
    {
        await _authSession.RestoreAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, "api/v1/driver/notifications/push-token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        request.Content = JsonContent.Create(new 기사푸시토큰등록요청
        {
            PushToken = pushToken
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
