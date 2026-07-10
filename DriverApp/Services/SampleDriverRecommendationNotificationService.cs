using DriverApp.Services.Samples;
using Hongdal.Client.Infrastructure;
using Hongdal.Contracts.Common.Drivers;
using Microsoft.Extensions.Options;

namespace DriverApp.Services;

public sealed class SampleDriverRecommendationNotificationService(
    기사샘플데이터Service sampleDataService,
    IDriverHomeMapService mapService,
    IOptions<ClientDataModeOptions> dataModeOptions) : IDriverRecommendationNotificationService
{
    private readonly HashSet<string> dismissedRequestIds = new(StringComparer.OrdinalIgnoreCase);
    private DriverIncomingRecommendation? publishedRecommendation;
    private bool sampleRecommendationInitialized;

    public event Action<DriverIncomingRecommendation?>? Changed;

    public DriverIncomingRecommendation? GetCurrent()
    {
#if DEBUG
        if (publishedRecommendation is null
            && !sampleRecommendationInitialized
            && dataModeOptions.Value.AllowSampleFallback)
        {
            publishedRecommendation = CreateSampleRecommendation();
            sampleRecommendationInitialized = true;
        }
#endif

        return publishedRecommendation;
    }

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

    private DriverIncomingRecommendation? CreateSampleRecommendation()
    {
        var request = sampleDataService.추천의뢰목록
            .FirstOrDefault(x => !dismissedRequestIds.Contains(x.의뢰Id));
        if (request is null)
        {
            return null;
        }

        var marker = mapService.BuildMarkers([request]).FirstOrDefault();
        if (marker is null)
        {
            return null;
        }

        return new DriverIncomingRecommendation(
            marker,
            request,
            DateTime.Now,
            "DEBUG_SAMPLE",
            sampleDataService.추천의뢰목록.Count);
    }
}
