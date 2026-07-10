using DriverApp.Models.Driver;
using Hongdal.Contracts.Common.Drivers;

namespace DriverApp.Services;

public sealed record DriverIncomingRecommendation(
    DriverMapMarkerItem Marker,
    DriverRequestItem Request,
    DateTime ReceivedAt,
    string SourceCode,
    int PendingCount);

public interface IDriverRecommendationNotificationService
{
    event Action<DriverIncomingRecommendation?>? Changed;

    DriverIncomingRecommendation? GetCurrent();

    void Publish(DriverIncomingRecommendation recommendation);

    void MarkDismissed(string requestId);

    void MarkHandled(string requestId);
}
