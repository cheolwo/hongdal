using DriverApp.Models.Driver;

namespace DriverApp.Services;

public interface IDriverRecommendationDecisionService
{
    event Action? Changed;

    IReadOnlyDictionary<string, RecommendationDecisionState> Decisions { get; }

    RecommendationDecisionState? GetDecision(string requestId);

    RecommendationDecisionState Accept(DriverRequestItem request);

    Task<RecommendationDecisionState> AcceptAsync(DriverRequestItem request, CancellationToken cancellationToken = default);

    RecommendationDecisionState Hold(DriverRequestItem request);

    RecommendationDecisionState CancelAccepted(DriverRequestItem request, string reason);

    Task<RecommendationDecisionState> CancelAcceptedAsync(DriverRequestItem request, string reason, CancellationToken cancellationToken = default);

    RecommendationDecisionState Reject(DriverRequestItem request, string reason);

    Task<RecommendationDecisionState> RejectAsync(DriverRequestItem request, string reason, CancellationToken cancellationToken = default);
}
