using DriverApp.Models.Driver;

namespace DriverApp.Services;

public sealed class DriverRecommendationDecisionService : IDriverRecommendationDecisionService
{
    private readonly Dictionary<string, RecommendationDecisionState> _decisions = new(StringComparer.OrdinalIgnoreCase);

    public event Action? Changed;

    public IReadOnlyDictionary<string, RecommendationDecisionState> Decisions => _decisions;

    public RecommendationDecisionState? GetDecision(string requestId)
    {
        return _decisions.TryGetValue(requestId, out var decision) ? decision : null;
    }

    public RecommendationDecisionState Accept(DriverRequestItem request)
    {
        request.배차상태 = "수락";
        request.상태 = "수락완료";
        return Save(request, "수락", "기사님이 추천 의뢰를 수락했습니다.");
    }

    public RecommendationDecisionState Hold(DriverRequestItem request)
    {
        request.배차상태 = "보류";
        request.상태 = "검토중";
        return Save(request, "보류", "잠시 보류했습니다. 추천 목록에서 다시 확인할 수 있습니다.");
    }

    public RecommendationDecisionState Reject(DriverRequestItem request, string reason)
    {
        request.배차상태 = "거절";
        request.상태 = "추천제외";
        var memo = string.IsNullOrWhiteSpace(reason)
            ? "기사님이 추천 의뢰를 거절했습니다."
            : $"기사님이 추천 의뢰를 거절했습니다. 사유: {reason}";
        return Save(request, "거절", memo);
    }

    private RecommendationDecisionState Save(DriverRequestItem request, string decision, string memo)
    {
        var state = new RecommendationDecisionState(request.의뢰Id, decision, memo, DateTime.Now);
        _decisions[request.의뢰Id] = state;
        Changed?.Invoke();
        return state;
    }
}

public sealed record RecommendationDecisionState(
    string RequestId,
    string Decision,
    string Memo,
    DateTime DecidedAt);
