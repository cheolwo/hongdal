using Hongdal.Application.Community;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Services.Community;
using 홍달.Data;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송경험치EventHandler :
    INotificationHandler<운송상차지도착됨Event>,
    INotificationHandler<운송상차완료됨Event>,
    INotificationHandler<운송하차지도착됨Event>,
    INotificationHandler<운송인수완료됨Event>,
    INotificationHandler<운송문제신고됨Event>
{
    private readonly ICommunityExperienceEventRecorder _experienceEventRecorder;

    public 운송경험치EventHandler(ICommunityExperienceEventRecorder experienceEventRecorder)
    {
        _experienceEventRecorder = experienceEventRecorder;
    }

    public Task Handle(운송상차지도착됨Event notification, CancellationToken cancellationToken)
        => RecordAsync(
            notification.기사Id,
            CommunityLedgerExperienceEventCodes.TransportPickupArrived,
            "DriverTransport",
            notification.운송Id.ToString(),
            notification.운송Id.ToString(),
            $"api/v1/driver/transports/{notification.운송Id}/arrive-pickup",
            notification.TraceId,
            notification.발생시각Utc,
            cancellationToken);

    public Task Handle(운송상차완료됨Event notification, CancellationToken cancellationToken)
        => RecordAsync(
            notification.기사Id,
            CommunityLedgerExperienceEventCodes.TransportPickupCompleted,
            "DriverTransport",
            notification.운송Id.ToString(),
            notification.운송번호,
            $"api/v1/driver/transports/{notification.운송Id}/pickup-complete",
            notification.TraceId,
            notification.발생시각Utc,
            cancellationToken);

    public Task Handle(운송하차지도착됨Event notification, CancellationToken cancellationToken)
        => RecordAsync(
            notification.기사Id,
            CommunityLedgerExperienceEventCodes.TransportDropoffArrived,
            "DriverTransport",
            notification.운송Id.ToString(),
            notification.운송Id.ToString(),
            $"api/v1/driver/transports/{notification.운송Id}/arrive-dropoff",
            notification.TraceId,
            notification.발생시각Utc,
            cancellationToken);

    public Task Handle(운송인수완료됨Event notification, CancellationToken cancellationToken)
        => RecordAsync(
            notification.기사Id,
            CommunityLedgerExperienceEventCodes.TransportDropoffCompleted,
            "DriverTransport",
            notification.운송Id.ToString(),
            notification.운송번호,
            $"api/v1/driver/transports/{notification.운송Id}/complete",
            notification.TraceId,
            notification.발생시각Utc,
            cancellationToken);

    public Task Handle(운송문제신고됨Event notification, CancellationToken cancellationToken)
        => RecordAsync(
            notification.기사Id,
            CommunityLedgerExperienceEventCodes.TransportIssueReported,
            "DriverTransportIssue",
            notification.운송Id.ToString(),
            notification.운송번호,
            $"api/v1/driver/transports/{notification.운송Id}/report-issue",
            notification.TraceId,
            notification.발생시각Utc,
            cancellationToken);

    private Task RecordAsync(
        string userId,
        string eventCode,
        string sourceKind,
        string sourceId,
        string sourceDisplayId,
        string route,
        string traceId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
        => _experienceEventRecorder.RecordAsync(
            new CommunityExperienceAwardRequest(
                userId,
                역할명.기사,
                eventCode,
                sourceKind,
                sourceId,
                sourceDisplayId,
                route,
                traceId,
                occurredAtUtc,
                App식별자.DriverApp),
            "운송",
            cancellationToken);
}
