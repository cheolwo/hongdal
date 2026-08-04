using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Community;

namespace SsalddelAdmin.Services;

public sealed class 지도신청운송취소검토AdminService(
    HttpClient httpClient,
    관리자인증세션Service session)
{
    private const string Path = "api/v1/admin/community/map-transport-cancellation-reviews";

    public async Task<IReadOnlyList<지도신청가원장Response>> 목록Async(
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, Path);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "운송 취소 검토 목록을 조회하지 못했습니다.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<지도신청가원장Response>>(cancellationToken) ?? [];
    }

    public async Task<지도신청가원장Response> 처리Async(
        string ledgerId,
        bool approve,
        string confirmationRequestId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            $"{Path}/{Uri.EscapeDataString(ledgerId)}/decision",
            JsonContent.Create(new 지도신청운송취소검토처리Request
            {
                승인 = approve,
                확인운영원본Id = confirmationRequestId,
                검토사유 = reason
            }));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "운송 취소 관리자 검토 결과를 기록하지 못했습니다.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<지도신청가원장Response>(cancellationToken)
               ?? throw new InvalidOperationException("운송 취소 관리자 검토 응답이 비어 있습니다.");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, HttpContent? content = null)
    {
        if (!session.서버관리자인가 || string.IsNullOrWhiteSpace(session.AccessToken))
        {
            throw new InvalidOperationException("서버관리자 로그인이 필요합니다.");
        }

        var request = new HttpRequestMessage(method, path) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return request;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string prefix,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(body) ? prefix : $"{prefix} {body}",
            null,
            response.StatusCode);
    }
}
