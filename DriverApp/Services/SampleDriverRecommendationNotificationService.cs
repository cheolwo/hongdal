using DriverApp.Models.Driver.Map;
using System.Globalization;

namespace DriverApp.Services;

public sealed class SampleDriverRecommendationNotificationService : IDriverRecommendationNotificationService
{
    private readonly IDriverSampleDataService sampleDataService;
    private readonly IDriverHomeMapService mapService;
    private readonly IDriverRecommendationDecisionService decisionService;
    private readonly HashSet<string> dismissedRequestIds = new(StringComparer.OrdinalIgnoreCase);
    private DriverIncomingRecommendation? publishedRecommendation;

    public SampleDriverRecommendationNotificationService(
        IDriverSampleDataService sampleDataService,
        IDriverHomeMapService mapService,
        IDriverRecommendationDecisionService decisionService)
    {
        this.sampleDataService = sampleDataService;
        this.mapService = mapService;
        this.decisionService = decisionService;
    }

    public event Action<DriverIncomingRecommendation?>? Changed;

    public DriverIncomingRecommendation? GetCurrent()
        => publishedRecommendation ?? BuildSampleRecommendation();

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

    private DriverIncomingRecommendation? BuildSampleRecommendation()
    {
        var marker = mapService
            .BuildMarkers(sampleDataService.추천의뢰목록)
            .FirstOrDefault(IsPendingMarker);
        if (marker is null ||
            !long.TryParse(marker.RequestId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var requestId))
        {
            return null;
        }

        var request = sampleDataService.추천의뢰조회(requestId);
        if (request is null)
        {
            return null;
        }

        var pendingCount = sampleDataService.추천의뢰목록.Count(x =>
            !dismissedRequestIds.Contains(x.의뢰Id) &&
            decisionService.GetDecision(x.의뢰Id) is null);

        return new DriverIncomingRecommendation(
            marker,
            request,
            DateTime.Now,
            "sample",
            pendingCount);
    }

    private bool IsPendingMarker(DriverMapMarkerItem marker)
        => !dismissedRequestIds.Contains(marker.RequestId) &&
            decisionService.GetDecision(marker.RequestId) is null;
}
