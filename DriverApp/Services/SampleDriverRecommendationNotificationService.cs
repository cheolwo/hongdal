using DriverApp.Models.Driver.Map;

namespace DriverApp.Services;

public sealed class SampleDriverRecommendationNotificationService : IDriverRecommendationNotificationService
{
    private readonly HashSet<string> dismissedRequestIds = new(StringComparer.OrdinalIgnoreCase);
    private DriverIncomingRecommendation? publishedRecommendation;

    public event Action<DriverIncomingRecommendation?>? Changed;

    public DriverIncomingRecommendation? GetCurrent()
        => publishedRecommendation;

    public void Publish(DriverIncomingRecommendation recommendation)
    {
        publishedRecommendation = recommendation;
        dismissedRequestIds.Remove(recommendation.Request.의뢰Id);
        Changed?.Invoke(recommendation);
    }

    public void MarkDismissed(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return;
        }

        dismissedRequestIds.Add(requestId);
        if (publishedRecommendation?.Request.의뢰Id == requestId)
        {
            publishedRecommendation = null;
        }

        Changed?.Invoke(GetCurrent());
    }

    public void MarkHandled(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return;
        }

        dismissedRequestIds.Add(requestId);
        if (publishedRecommendation?.Request.의뢰Id == requestId)
        {
            publishedRecommendation = null;
        }

        Changed?.Invoke(GetCurrent());
    }
}
