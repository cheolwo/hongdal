using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.WebApp.Services;

public sealed record CommunityMapApplicationLedgerAttempt(
    지도신청가원장Response? Ledger,
    string? Error)
{
    public bool Succeeded => Ledger is not null;
}

public sealed class CommunityMapApplicationLedgerClient(
    HttpClient httpClient,
    WebAuthSessionService authSession)
{
    private const string Path = "api/v1/community/map-applications/provisional-ledger";

    public async Task<지도신청가원장Response?> FindByOperationalSourceAsync(
        string workCode,
        string operationalSourceType,
        string operationalSourceId,
        CancellationToken cancellationToken = default)
    {
        await authSession.RestoreAsync(cancellationToken);
        if (!authSession.IsLoggedIn || string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            return null;
        }

        var query = $"workCode={Uri.EscapeDataString(workCode)}"
                    + $"&operationalSourceType={Uri.EscapeDataString(operationalSourceType)}"
                    + $"&operationalSourceId={Uri.EscapeDataString(operationalSourceId)}";
        using var message = new HttpRequestMessage(HttpMethod.Get, $"{Path}/by-operational-source?{query}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"지도 신청 원장을 조회하지 못했습니다. HTTP {(int)response.StatusCode}"
                : $"지도 신청 원장을 조회하지 못했습니다. HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<지도신청가원장Response>(cancellationToken);
    }

    public async Task<지도신청가원장Response> CreateAsync(
        지도신청가원장생성Request request,
        CancellationToken cancellationToken = default)
    {
        await authSession.RestoreAsync(cancellationToken);
        if (!authSession.IsLoggedIn || string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            throw new InvalidOperationException("지도 신청 가원장을 만들려면 먼저 로그인해 주세요.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, Path)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"지도 신청 가원장을 만들지 못했습니다. HTTP {(int)response.StatusCode}"
                : $"지도 신청 가원장을 만들지 못했습니다. HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<지도신청가원장Response>(cancellationToken)
               ?? throw new InvalidOperationException("지도 신청 가원장 응답이 비어 있습니다.");
    }

    public async Task<CommunityMapApplicationLedgerAttempt> TryCreateAsync(
        지도신청가원장생성Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return new(await CreateAsync(request, cancellationToken), null);
        }
        catch (Exception ex)
        {
            return new(null, ex.Message);
        }
    }

    public Task<지도신청가원장Response> MarkSubmittedAsync(
        string ledgerId,
        지도신청실원장전환Request request,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"{Path}/{Uri.EscapeDataString(ledgerId)}/application-submission",
            request,
            "지도 신청 원장에 제출 결과를 연결하지 못했습니다.",
            cancellationToken);

    public async Task<CommunityMapApplicationLedgerAttempt> TryMarkSubmittedAsync(
        string ledgerId,
        지도신청실원장전환Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return new(await MarkSubmittedAsync(ledgerId, request, cancellationToken), null);
        }
        catch (Exception ex)
        {
            return new(null, ex.Message);
        }
    }

    public Task<지도신청가원장Response> MarkConsentWithdrawnAsync(
        string ledgerId,
        Guid evidenceId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"{Path}/{Uri.EscapeDataString(ledgerId)}/privacy-consent-withdrawal",
            new 지도신청동의철회반영Request { 신청개인정보동의증적Id = evidenceId },
            "개인정보 동의 철회를 지도 신청 원장에 반영하지 못했습니다.",
            cancellationToken);

    public Task<지도신청가원장Response> MarkOperationalCancelledAsync(
        string ledgerId,
        string operationalSourceType,
        string operationalSourceId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"{Path}/{Uri.EscapeDataString(ledgerId)}/operational-cancellation",
            new 지도신청운영취소반영Request
            {
                운영원본종류 = operationalSourceType,
                운영원본Id = operationalSourceId
            },
            "운영 신청 취소 결과를 지도 신청 원장에 반영하지 못했습니다.",
            cancellationToken);

    public Task<지도신청가원장Response> RequestTransportCancellationReviewAsync(
        string ledgerId,
        string operationalSourceId,
        string reason,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"{Path}/{Uri.EscapeDataString(ledgerId)}/transport-cancellation-review",
            new 지도신청운송취소검토요청Request
            {
                운영원본Id = operationalSourceId,
                사유 = reason
            },
            "운송 취소 검토 요청을 원장에 기록하지 못했습니다.",
            cancellationToken);

    private async Task<지도신청가원장Response> PostAsync<TRequest>(
        string path,
        TRequest request,
        string errorPrefix,
        CancellationToken cancellationToken)
    {
        await authSession.RestoreAsync(cancellationToken);
        if (!authSession.IsLoggedIn || string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            throw new InvalidOperationException("지도 신청 원장을 변경하려면 먼저 로그인해 주세요.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"{errorPrefix} HTTP {(int)response.StatusCode}"
                : $"{errorPrefix} HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<지도신청가원장Response>(cancellationToken)
               ?? throw new InvalidOperationException("지도 신청 원장 응답이 비어 있습니다.");
    }

}
