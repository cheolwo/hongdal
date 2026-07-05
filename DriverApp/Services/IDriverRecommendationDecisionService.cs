using DriverApp.Models.Driver;

namespace DriverApp.Services;

public interface IDriverRecommendationDecisionService
{
    event Action? Changed;

    IReadOnlyDictionary<string, RecommendationDecisionState> Decisions { get; }

    RecommendationDecisionState? GetDecision(string requestId);

    RecommendationDecisionState Accept(DriverRequestItem request);

    RecommendationDecisionState Hold(DriverRequestItem request);

    RecommendationDecisionState Reject(DriverRequestItem request, string reason);
}
