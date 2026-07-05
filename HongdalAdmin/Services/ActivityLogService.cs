using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Admin.Audit;

namespace HongdalAdmin.Services;

public sealed class ActivityLogService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;

    public ActivityLogService(HttpClient httpClient, 관리자인증세션Service session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<사용자행위로그목록응답> SearchAsync(사용자행위로그검색요청 request, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, BuildSearchPath(request));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<사용자행위로그목록응답>(cancellationToken: cancellationToken)
               ?? new 사용자행위로그목록응답();
    }

    public async Task<사용자행위로그상세응답?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, $"api/v1/admin/activity-logs/{id}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<사용자행위로그상세응답>(cancellationToken: cancellationToken);
    }

    public async Task<Trace행위로그묶음응답?> GetTraceAsync(string traceId, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, $"api/v1/admin/activity-logs/trace/{Uri.EscapeDataString(traceId)}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Trace행위로그묶음응답>(cancellationToken: cancellationToken);
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

    private static string BuildSearchPath(사용자행위로그검색요청 request)
    {
        var query = new List<string>();
        Add(query, "appKey", request.AppKey);
        Add(query, "userId", request.UserId);
        Add(query, "email", request.Email);
        Add(query, "phoneLast4", request.PhoneLast4);
        Add(query, "actionType", request.ActionType);
        Add(query, "actionName", request.ActionName);
        Add(query, "traceId", request.TraceId);
        if (request.IsSuccess.HasValue)
        {
            query.Add($"isSuccess={request.IsSuccess.Value.ToString().ToLowerInvariant()}");
        }

        if (request.FromUtc.HasValue)
        {
            query.Add($"fromUtc={Uri.EscapeDataString(request.FromUtc.Value.ToString("O"))}");
        }

        if (request.ToUtc.HasValue)
        {
            query.Add($"toUtc={Uri.EscapeDataString(request.ToUtc.Value.ToString("O"))}");
        }

        query.Add($"page={request.Page}");
        query.Add($"pageSize={request.PageSize}");

        return query.Count == 0
            ? "api/v1/admin/activity-logs"
            : $"api/v1/admin/activity-logs?{string.Join("&", query)}";
    }

    private static void Add(ICollection<string> query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
