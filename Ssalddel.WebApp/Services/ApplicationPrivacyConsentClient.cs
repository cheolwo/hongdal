using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.WebApp.Services;

public sealed class ApplicationPrivacyConsentClient(
    HttpClient httpClient,
    WebAuthSessionService authSession)
{
    private const string BasePath = "api/v1/common/application-privacy-consents";

    public async Task<신청개인정보동의증적Response> RecordAsync(
        신청개인정보동의기록Request request,
        CancellationToken cancellationToken = default)
    {
        await authSession.RestoreAsync(cancellationToken);
        if (!authSession.IsLoggedIn || string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            throw new InvalidOperationException("개인정보 동의 증적을 기록하려면 먼저 로그인해 주세요.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, BasePath)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"개인정보 동의 증적을 기록하지 못했습니다. HTTP {(int)response.StatusCode}"
                : $"개인정보 동의 증적을 기록하지 못했습니다. HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<신청개인정보동의증적Response>(cancellationToken)
               ?? throw new InvalidOperationException("개인정보 동의 증적 응답이 비어 있습니다.");
    }

    public async Task<신청개인정보동의증적Response> WithdrawAsync(
        Guid evidenceId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        await authSession.RestoreAsync(cancellationToken);
        if (!authSession.IsLoggedIn || string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            throw new InvalidOperationException("개인정보 동의를 철회하려면 먼저 로그인해 주세요.");
        }

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BasePath}/{evidenceId:D}/withdrawal")
        {
            Content = JsonContent.Create(new 신청개인정보동의철회Request { 철회사유 = reason?.Trim() ?? string.Empty })
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"개인정보 동의를 철회하지 못했습니다. HTTP {(int)response.StatusCode}"
                : $"개인정보 동의를 철회하지 못했습니다. HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<신청개인정보동의증적Response>(cancellationToken)
               ?? throw new InvalidOperationException("개인정보 동의 철회 응답이 비어 있습니다.");
    }
}
