using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DriverApp.Models.Driver;
using Hongdal.Client.Infrastructure.Transport;
using Hongdal.Contracts.Driver.Action;

namespace DriverApp.Services;

public sealed class DriverRecommendationDecisionService : IDriverRecommendationDecisionService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;
    private readonly ITransportRequestLedgerObserver _ledgerObserver;
    private readonly Dictionary<string, RecommendationDecisionState> _decisions = new(StringComparer.OrdinalIgnoreCase);

    public DriverRecommendationDecisionService(
        HttpClient httpClient,
        IAuthSession authSession,
        ITransportRequestLedgerObserver ledgerObserver)
    {
        _httpClient = httpClient;
        _authSession = authSession;
        _ledgerObserver = ledgerObserver;
    }

    public event Action? Changed;

    public IReadOnlyDictionary<string, RecommendationDecisionState> Decisions => _decisions;

    public RecommendationDecisionState? GetDecision(string requestId)
    {
        return _decisions.TryGetValue(requestId, out var decision) ? decision : null;
    }

    public RecommendationDecisionState Accept(DriverRequestItem request)
    {
        return SaveAccepted(request, "기사님이 추천 의뢰를 수락했습니다.");
    }

    public async Task<RecommendationDecisionState> AcceptAsync(DriverRequestItem request, CancellationToken cancellationToken = default)
    {
        await PostServerAsync($"api/v1/driver/dispatch-actions/{Uri.EscapeDataString(request.의뢰Id)}/accept", null, cancellationToken);
        _ledgerObserver.RequestRefresh(request.의뢰Id, "DriverApp.Accepted");
        return SaveAccepted(
            request,
            "홍달 서버에서 배차 수락 처리되었습니다.");
    }

    public RecommendationDecisionState Hold(DriverRequestItem request)
    {
        request.배차상태 = "보류";
        request.상태 = "검토중";
        return Save(
            request,
            DriverRecommendationDecisionCode.Held,
            "잠시 보류했습니다. 추천 목록에서 다시 확인할 수 있습니다.",
            DriverRecommendationDecisionFollowUpPlan.ForHeld(request.의뢰Id));
    }

    public RecommendationDecisionState CancelAccepted(DriverRequestItem request, string reason)
    {
        var memo = string.IsNullOrWhiteSpace(reason)
            ? "기사님이 수락한 추천 의뢰를 취소했습니다."
            : $"기사님이 수락한 추천 의뢰를 취소했습니다. 사유: {reason}";
        return SaveAcceptanceCanceled(request, reason, memo);
    }

    public async Task<RecommendationDecisionState> CancelAcceptedAsync(DriverRequestItem request, string reason, CancellationToken cancellationToken = default)
    {
        await PostServerAsync(
            $"api/v1/driver/dispatch-actions/{Uri.EscapeDataString(request.의뢰Id)}/cancel-acceptance",
            new 기사배차수락취소요청 { 사유 = reason },
            cancellationToken);
        _ledgerObserver.RequestRefresh(request.의뢰Id, "DriverApp.AcceptanceCanceled");
        var memo = "홍달 서버에서 배차 수락 취소가 접수되었습니다.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            memo = $"{memo} 사유: {reason}";
        }

        return SaveAcceptanceCanceled(request, reason, memo);
    }

    public RecommendationDecisionState Reject(DriverRequestItem request, string reason)
    {
        var memo = string.IsNullOrWhiteSpace(reason)
            ? "기사님이 추천 의뢰를 거절했습니다."
            : $"기사님이 추천 의뢰를 거절했습니다. 사유: {reason}";
        return SaveRejected(request, reason, memo);
    }

    public async Task<RecommendationDecisionState> RejectAsync(DriverRequestItem request, string reason, CancellationToken cancellationToken = default)
    {
        await PostServerAsync(
            $"api/v1/driver/dispatch-actions/{Uri.EscapeDataString(request.의뢰Id)}/reject",
            new 기사배차거절요청 { 사유 = reason },
            cancellationToken);
        _ledgerObserver.RequestRefresh(request.의뢰Id, "DriverApp.Rejected");
        var memo = "홍달 서버에서 배차 거절 처리되었습니다.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            memo = $"{memo} 사유: {reason}";
        }

        return SaveRejected(request, reason, memo);
    }

    private RecommendationDecisionState SaveAccepted(DriverRequestItem request, string memo)
    {
        request.배차상태 = "배차확정";
        request.상태 = "수락완료";
        return Save(
            request,
            DriverRecommendationDecisionCode.Accepted,
            memo,
            DriverRecommendationDecisionFollowUpPlan.ForAccepted(request.의뢰Id));
    }

    private RecommendationDecisionState SaveAcceptanceCanceled(DriverRequestItem request, string reason, string memo)
    {
        request.배차상태 = "수락취소";
        request.상태 = "재배차필요";
        return Save(
            request,
            DriverRecommendationDecisionCode.AcceptanceCanceled,
            memo,
            DriverRecommendationDecisionFollowUpPlan.ForAcceptanceCanceled(request.의뢰Id, reason));
    }

    private RecommendationDecisionState SaveRejected(DriverRequestItem request, string reason, string memo)
    {
        request.배차상태 = "거절";
        request.상태 = "추천제외";
        return Save(
            request,
            DriverRecommendationDecisionCode.Rejected,
            memo,
            DriverRecommendationDecisionFollowUpPlan.ForRejected(request.의뢰Id, reason));
    }

    private RecommendationDecisionState Save(
        DriverRequestItem request,
        string decision,
        string memo,
        DriverRecommendationDecisionFollowUpPlan followUpPlan)
    {
        var state = new RecommendationDecisionState(request.의뢰Id, decision, memo, DateTime.Now, followUpPlan);
        _decisions[request.의뢰Id] = state;
        Observe(request, $"DriverApp.Decision.{decision}");
        Changed?.Invoke();
        return state;
    }

    private void Observe(DriverRequestItem request, string source)
    {
        if (string.IsNullOrWhiteSpace(request.의뢰Id))
        {
            return;
        }

        _ledgerObserver.Observe(
            new TransportRequestLedgerSnapshot(
                request.의뢰Id,
                request.상태,
                null,
                request.배차상태,
                null,
                DateTimeOffset.UtcNow,
                source),
            source);
    }

    private async Task PostServerAsync(string path, object? payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            throw new InvalidOperationException("서버 인증 정보가 없어 배차 처리를 요청할 수 없습니다.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("서버 배차 처리 API에 연결할 수 없습니다.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("서버 배차 처리 API 응답이 지연되어 요청이 중단되었습니다.", ex);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            throw new InvalidOperationException(await BuildFailureMessageAsync(response, cancellationToken));
        }
    }

    private static async Task<string> BuildFailureMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"서버 배차 처리에 실패했습니다. HTTP {(int)response.StatusCode}";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var title = ReadString(root, "title");
            var errorCode = ReadString(root, "errorCode");
            var traceId = ReadString(root, "traceId");
            var errors = ReadErrors(root);
            var message = errors.Count > 0
                ? string.Join(" / ", errors)
                : string.IsNullOrWhiteSpace(title)
                    ? body
                    : title;

            var suffix = new List<string>();
            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                suffix.Add(errorCode);
            }

            if (!string.IsNullOrWhiteSpace(traceId))
            {
                suffix.Add($"traceId={traceId}");
            }

            return suffix.Count == 0
                ? $"서버 배차 처리에 실패했습니다. HTTP {(int)response.StatusCode}: {message}"
                : $"서버 배차 처리에 실패했습니다. HTTP {(int)response.StatusCode}: {message} ({string.Join(", ", suffix)})";
        }
        catch (JsonException)
        {
            return $"서버 배차 처리에 실패했습니다. HTTP {(int)response.StatusCode}: {body}";
        }
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static IReadOnlyList<string> ReadErrors(JsonElement root)
    {
        if (!root.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return errors.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToArray();
    }
}

public static class DriverRecommendationDecisionCode
{
    public const string Accepted = "수락";
    public const string Held = "보류";
    public const string AcceptanceCanceled = "수락취소";
    public const string Rejected = "거절";
}

public static class DriverRecommendationFollowUpActionCode
{
    public const string LockRecommendation = "LockRecommendation";
    public const string CreateAssignmentCandidate = "CreateAssignmentCandidate";
    public const string NotifyShipperAccepted = "NotifyShipperAccepted";
    public const string KeepRecommendationPending = "KeepRecommendationPending";
    public const string ReleaseRecommendationLock = "ReleaseRecommendationLock";
    public const string ReopenDispatch = "ReopenDispatch";
    public const string NotifyShipperCancellation = "NotifyShipperCancellation";
    public const string AuditCancellationReason = "AuditCancellationReason";
    public const string ExcludeDriverFromRecommendation = "ExcludeDriverFromRecommendation";
    public const string RecalculateCandidateDrivers = "RecalculateCandidateDrivers";
    public const string AuditRejectionReason = "AuditRejectionReason";
}

public sealed record RecommendationDecisionState(
    string RequestId,
    string Decision,
    string Memo,
    DateTime DecidedAt,
    DriverRecommendationDecisionFollowUpPlan FollowUpPlan);

public sealed record DriverRecommendationDecisionFollowUpPlan(
    string RequestId,
    string DecisionCode,
    IReadOnlyList<string> ServerActionCodes,
    bool ShouldReopenDispatch,
    bool ShouldNotifyShipper,
    bool ShouldRecalculateRecommendations,
    bool RequiresCancellationReason,
    string OperationalMemo)
{
    public static DriverRecommendationDecisionFollowUpPlan ForAccepted(string requestId)
        => new(
            requestId,
            DriverRecommendationDecisionCode.Accepted,
            [
                DriverRecommendationFollowUpActionCode.LockRecommendation,
                DriverRecommendationFollowUpActionCode.CreateAssignmentCandidate,
                DriverRecommendationFollowUpActionCode.NotifyShipperAccepted
            ],
            ShouldReopenDispatch: false,
            ShouldNotifyShipper: true,
            ShouldRecalculateRecommendations: false,
            RequiresCancellationReason: false,
            "수락 직후에는 추천 잠금과 배차 후보 생성을 우선 처리한다.");

    public static DriverRecommendationDecisionFollowUpPlan ForHeld(string requestId)
        => new(
            requestId,
            DriverRecommendationDecisionCode.Held,
            [DriverRecommendationFollowUpActionCode.KeepRecommendationPending],
            ShouldReopenDispatch: false,
            ShouldNotifyShipper: false,
            ShouldRecalculateRecommendations: false,
            RequiresCancellationReason: false,
            "보류는 기사 개인의 검토 상태이며 서버 배차 상태를 확정하지 않는다.");

    public static DriverRecommendationDecisionFollowUpPlan ForAcceptanceCanceled(string requestId, string reason)
        => new(
            requestId,
            DriverRecommendationDecisionCode.AcceptanceCanceled,
            [
                DriverRecommendationFollowUpActionCode.ReleaseRecommendationLock,
                DriverRecommendationFollowUpActionCode.ReopenDispatch,
                DriverRecommendationFollowUpActionCode.NotifyShipperCancellation,
                DriverRecommendationFollowUpActionCode.AuditCancellationReason
            ],
            ShouldReopenDispatch: true,
            ShouldNotifyShipper: true,
            ShouldRecalculateRecommendations: true,
            RequiresCancellationReason: string.IsNullOrWhiteSpace(reason),
            "수락 취소는 재배차가 필요하므로 잠금 해제, 화주 알림, 사유 감사 기록을 남긴다.");

    public static DriverRecommendationDecisionFollowUpPlan ForRejected(string requestId, string reason)
        => new(
            requestId,
            DriverRecommendationDecisionCode.Rejected,
            [
                DriverRecommendationFollowUpActionCode.ExcludeDriverFromRecommendation,
                DriverRecommendationFollowUpActionCode.RecalculateCandidateDrivers,
                DriverRecommendationFollowUpActionCode.AuditRejectionReason
            ],
            ShouldReopenDispatch: false,
            ShouldNotifyShipper: false,
            ShouldRecalculateRecommendations: true,
            RequiresCancellationReason: false,
            string.IsNullOrWhiteSpace(reason)
                ? "거절 사유가 없으면 후보 제외와 재추천 계산만 수행한다."
                : "거절 사유를 추천 품질 보정과 감사 로그에 반영한다.");
}
