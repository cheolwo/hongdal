using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Admin.Audit;

namespace SsalddelAdmin.Services;

public sealed class ActivityLogService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly bool _useMemoryFallback;

    public ActivityLogService(HttpClient httpClient, 관리자인증세션Service session, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _session = session;
        _useMemoryFallback = configuration.GetValue("AdminData:UseMemory", false);
    }

    public async Task<사용자행위로그목록응답> SearchAsync(사용자행위로그검색요청 request, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            var items = BuildMemoryItems();
            return new 사용자행위로그목록응답
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = items.Count
            };
        }

        using var message = CreateRequest(HttpMethod.Get, BuildSearchPath(request));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<사용자행위로그목록응답>(cancellationToken: cancellationToken)
               ?? new 사용자행위로그목록응답();
    }

    public async Task<사용자행위로그상세응답?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            var item = BuildMemoryItems().FirstOrDefault(x => x.Id == id);
            return item is null ? null : BuildMemoryDetail(item);
        }

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
        if (_useMemoryFallback)
        {
            var items = BuildMemoryItems()
                .Where(x => string.Equals(x.TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            return new Trace행위로그묶음응답
            {
                TraceId = traceId,
                Items = items
            };
        }

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

    private static IReadOnlyList<사용자행위로그요약응답> BuildMemoryItems()
    {
        var now = DateTime.UtcNow;
        return
        [
            new()
            {
                Id = 1,
                AppKey = "SsalddelAdmin",
                UserId = "admin-sample",
                UserName = "운영자",
                RoleName = "서버관리자",
                EmailMasked = "op***@ssalddel.local",
                PhoneLast4 = "2222",
                ActionType = "Read",
                ActionName = "운송 관제 조회",
                Route = "/transports/TR-101",
                TraceId = "TRACE-ADMIN-001",
                IsSuccess = true,
                OccurredAtUtc = now.AddMinutes(-12)
            },
            new()
            {
                Id = 2,
                AppKey = "DriverApp",
                UserId = "DRV-001",
                UserName = "홍기사",
                RoleName = "용달기사",
                EmailMasked = "dr***@ssalddel.local",
                PhoneLast4 = "2222",
                ActionType = "Command",
                ActionName = "상차 예외 신고",
                Route = "/driver/transports/current",
                TraceId = "TRACE-ADMIN-001",
                IsSuccess = true,
                OccurredAtUtc = now.AddMinutes(-15)
            },
            new()
            {
                Id = 3,
                AppKey = "SsalddelAdmin",
                UserId = "admin-sample",
                UserName = "운영자",
                RoleName = "서버관리자",
                EmailMasked = "op***@ssalddel.local",
                PhoneLast4 = "2222",
                ActionType = "Update",
                ActionName = "화면 정책 변경",
                Route = "/view-policies",
                TraceId = "TRACE-ADMIN-002",
                IsSuccess = false,
                OccurredAtUtc = now.AddMinutes(-40)
            }
        ];
    }

    private static 사용자행위로그상세응답 BuildMemoryDetail(사용자행위로그요약응답 item)
        => new()
        {
            Id = item.Id,
            AppKey = item.AppKey,
            UserId = item.UserId,
            UserName = item.UserName,
            RoleName = item.RoleName,
            EmailMasked = item.EmailMasked,
            PhoneLast4 = item.PhoneLast4,
            ActionType = item.ActionType,
            ActionName = item.ActionName,
            Route = item.Route,
            TraceId = item.TraceId,
            IsSuccess = item.IsSuccess,
            ErrorCode = item.IsSuccess ? string.Empty : "PolicyValidation",
            ErrorMessage = item.IsSuccess ? string.Empty : "화면 정책 변경 전 승인 범위를 확인해야 합니다.",
            ClientIp = "127.0.0.1",
            UserAgent = "SsalddelAdmin memory capture",
            OccurredAtUtc = item.OccurredAtUtc,
            MetadataJson = "{\"source\":\"memory\",\"purpose\":\"admin-capture\"}"
        };
}
