using DriverApp.Models.Driver.Map;
using Hongdal.Client.Infrastructure;
using Microsoft.Extensions.Options;

namespace DriverApp.Services;

public sealed class SampleDriverRecommendationNotificationService : IDriverRecommendationNotificationService
{
    private readonly IDriverSampleDataService sampleDataService;
    private readonly IDriverHomeMapService mapService;
    private readonly IDriverRecommendationDecisionService decisionService;
    private readonly ClientDataModeOptions dataModeOptions;
    private readonly HashSet<string> dismissedRequestIds = new(StringComparer.OrdinalIgnoreCase);
    private DriverIncomingRecommendation? publishedRecommendation;

    public SampleDriverRecommendationNotificationService(
        IDriverSampleDataService sampleDataService,
        IDriverHomeMapService mapService,
        IDriverRecommendationDecisionService decisionService,
        IOptions<ClientDataModeOptions> dataModeOptions)
    {
        this.sampleDataService = sampleDataService;
        this.mapService = mapService;
        this.decisionService = decisionService;
        this.dataModeOptions = dataModeOptions.Value;
    }

    public event Action<DriverIncomingRecommendation?>? Changed;

    public DriverIncomingRecommendation? GetCurrent()
        => publishedRecommendation ?? (dataModeOptions.AllowSampleFallback ? BuildSampleRecommendation() : null);

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
        if (marker is null)
        {
            return null;
        }

        var request = sampleDataService.추천의뢰조회(marker.RequestId);
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
