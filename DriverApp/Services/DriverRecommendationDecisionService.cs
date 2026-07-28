using DriverApp.Models.Driver;
using Ssalddel.Client.Infrastructure.Transport;
using Ssalddel.Contracts.Driver.Action;

namespace DriverApp.Services;

public sealed class DriverRecommendationDecisionService : IDriverRecommendationDecisionService
{
    private readonly IDriverDispatchActionApiService _dispatchActionApi;
    private readonly IDriverSampleDataService _driverData;
    private readonly ITransportRequestLedgerObserver _ledgerObserver;
    private readonly Dictionary<string, RecommendationDecisionState> _decisions = new(StringComparer.OrdinalIgnoreCase);

    public DriverRecommendationDecisionService(
        IDriverDispatchActionApiService dispatchActionApi,
        IDriverSampleDataService driverData,
        ITransportRequestLedgerObserver ledgerObserver)
    {
        _dispatchActionApi = dispatchActionApi;
        _driverData = driverData;
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
        var response = await _dispatchActionApi.수락Async(request.의뢰Id, cancellationToken);
        var verified = await RefreshServerLedgerAsync(
            request.의뢰Id,
            "DriverApp.Accepted",
            cancellationToken);
        return SaveAccepted(
            request,
            verified
                ? response?.Message ?? "살뜰 서비스의 최신 원장에서 배차 수락을 확인했습니다."
                : "배차 수락은 처리됐지만 최신 원장 재조회에 실패했습니다. 새로고침해 상태를 확인해 주세요.",
            updateRequest: false,
            observeRequest: false);
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
        await _dispatchActionApi.수락취소Async(
            request.의뢰Id,
            new 기사배차수락취소요청 { 사유 = reason },
            cancellationToken);
        var verified = await RefreshServerLedgerAsync(
            request.의뢰Id,
            "DriverApp.AcceptanceCanceled",
            cancellationToken);
        var memo = verified
            ? "살뜰 서비스의 최신 원장에서 배차 수락 취소를 확인했습니다."
            : "배차 수락 취소는 처리됐지만 최신 원장 재조회에 실패했습니다. 새로고침해 상태를 확인해 주세요.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            memo = $"{memo} 사유: {reason}";
        }

        return SaveAcceptanceCanceled(
            request,
            reason,
            memo,
            updateRequest: false,
            observeRequest: false);
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
        await _dispatchActionApi.거절Async(
            request.의뢰Id,
            new 기사배차거절요청 { 사유 = reason },
            cancellationToken);
        var verified = await RefreshServerLedgerAsync(
            request.의뢰Id,
            "DriverApp.Rejected",
            cancellationToken);
        var memo = verified
            ? "살뜰 서비스의 최신 원장에서 배차 거절을 확인했습니다."
            : "배차 거절은 처리됐지만 최신 원장 재조회에 실패했습니다. 새로고침해 상태를 확인해 주세요.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            memo = $"{memo} 사유: {reason}";
        }

        return SaveRejected(
            request,
            reason,
            memo,
            updateRequest: false,
            observeRequest: false);
    }

    private async Task<bool> RefreshServerLedgerAsync(
        string requestId,
        string source,
        CancellationToken cancellationToken)
    {
        _ledgerObserver.RequestRefresh(requestId, source);
        try
        {
            await _driverData.RefreshAsync(cancellationToken, force: true);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private RecommendationDecisionState SaveAccepted(
        DriverRequestItem request,
        string memo,
        bool updateRequest = true,
        bool observeRequest = true)
    {
        if (updateRequest)
        {
            request.배차상태 = "배차확정";
            request.상태 = "수락완료";
        }

        return Save(
            request,
            DriverRecommendationDecisionCode.Accepted,
            memo,
            DriverRecommendationDecisionFollowUpPlan.ForAccepted(request.의뢰Id),
            observeRequest);
    }

    private RecommendationDecisionState SaveAcceptanceCanceled(
        DriverRequestItem request,
        string reason,
        string memo,
        bool updateRequest = true,
        bool observeRequest = true)
    {
        if (updateRequest)
        {
            request.배차상태 = "수락취소";
            request.상태 = "재배차필요";
        }

        return Save(
            request,
            DriverRecommendationDecisionCode.AcceptanceCanceled,
            memo,
            DriverRecommendationDecisionFollowUpPlan.ForAcceptanceCanceled(request.의뢰Id, reason),
            observeRequest);
    }

    private RecommendationDecisionState SaveRejected(
        DriverRequestItem request,
        string reason,
        string memo,
        bool updateRequest = true,
        bool observeRequest = true)
    {
        if (updateRequest)
        {
            request.배차상태 = "거절";
            request.상태 = "추천제외";
        }

        return Save(
            request,
            DriverRecommendationDecisionCode.Rejected,
            memo,
            DriverRecommendationDecisionFollowUpPlan.ForRejected(request.의뢰Id, reason),
            observeRequest);
    }

    private RecommendationDecisionState Save(
        DriverRequestItem request,
        string decision,
        string memo,
        DriverRecommendationDecisionFollowUpPlan followUpPlan,
        bool observeRequest = true)
    {
        var state = new RecommendationDecisionState(request.의뢰Id, decision, memo, DateTime.Now, followUpPlan);
        _decisions[request.의뢰Id] = state;
        if (observeRequest)
        {
            Observe(request, $"DriverApp.Decision.{decision}");
        }

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
