using Hongdal.Application.Community;
using Hongdal.Application.Driver.Transport;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Services.Community;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hongdal.Tests.Application.Driver.Transport;

public sealed class 운송경험치EventHandlerTests
{
    [Fact]
    public async Task Handle_상차지도착Event_상차지도착경험치코드를기록한다()
    {
        var service = new FakeCommunityExperienceAwardService();
        var handler = new 운송경험치EventHandler(CreateRecorder(service));
        var occurredAt = new DateTime(2026, 7, 12, 1, 30, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 운송상차지도착됨Event(
                "driver-0",
                100,
                "배차확정",
                "상차지도착",
                occurredAt,
                "trace-arrive-pickup"),
            CancellationToken.None);

        var request = Assert.Single(service.Requests);
        Assert.Equal("driver-0", request.UserId);
        Assert.Equal(App식별자.DriverApp, request.AppKey);
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportPickupArrived, request.EventCode);
        Assert.Equal("DriverTransport", request.SourceKind);
        Assert.Equal("100", request.SourceId);
        Assert.Equal("100", request.SourceDisplayId);
        Assert.Equal("api/v1/driver/transports/100/arrive-pickup", request.Route);
        Assert.Equal("trace-arrive-pickup", request.TraceId);
        Assert.Equal(occurredAt, request.OccurredAtUtc);
    }

    [Fact]
    public async Task Handle_상차완료Event_상차완료경험치코드를기록한다()
    {
        var service = new FakeCommunityExperienceAwardService();
        var handler = new 운송경험치EventHandler(CreateRecorder(service));
        var occurredAt = new DateTime(2026, 7, 12, 2, 0, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 운송상차완료됨Event(
                "driver-1",
                101,
                "HD-101",
                "서울",
                "부산",
                "상차지도착",
                "상차완료",
                occurredAt,
                "trace-pickup",
                null),
            CancellationToken.None);

        var request = Assert.Single(service.Requests);
        Assert.Equal("driver-1", request.UserId);
        Assert.Equal(App식별자.DriverApp, request.AppKey);
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportPickupCompleted, request.EventCode);
        Assert.Equal("DriverTransport", request.SourceKind);
        Assert.Equal("101", request.SourceId);
        Assert.Equal("HD-101", request.SourceDisplayId);
        Assert.Equal("api/v1/driver/transports/101/pickup-complete", request.Route);
        Assert.Equal("trace-pickup", request.TraceId);
        Assert.Equal(occurredAt, request.OccurredAtUtc);
    }

    [Fact]
    public async Task Handle_하차지도착Event_하차지도착경험치코드를기록한다()
    {
        var service = new FakeCommunityExperienceAwardService();
        var handler = new 운송경험치EventHandler(CreateRecorder(service));
        var occurredAt = new DateTime(2026, 7, 12, 2, 30, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 운송하차지도착됨Event(
                "driver-1",
                101,
                "상차완료",
                "하차지도착",
                occurredAt,
                "trace-arrive-dropoff"),
            CancellationToken.None);

        var request = Assert.Single(service.Requests);
        Assert.Equal("driver-1", request.UserId);
        Assert.Equal(App식별자.DriverApp, request.AppKey);
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportDropoffArrived, request.EventCode);
        Assert.Equal("DriverTransport", request.SourceKind);
        Assert.Equal("101", request.SourceId);
        Assert.Equal("101", request.SourceDisplayId);
        Assert.Equal("api/v1/driver/transports/101/arrive-dropoff", request.Route);
        Assert.Equal("trace-arrive-dropoff", request.TraceId);
        Assert.Equal(occurredAt, request.OccurredAtUtc);
    }

    [Fact]
    public async Task Handle_인수완료Event_하차완료경험치코드를기록한다()
    {
        var service = new FakeCommunityExperienceAwardService();
        var handler = new 운송경험치EventHandler(CreateRecorder(service));
        var occurredAt = new DateTime(2026, 7, 12, 3, 0, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 운송인수완료됨Event(
                102,
                "HD-102",
                "driver-2",
                "서울",
                "대전",
                "인수완료",
                occurredAt,
                "trace-dropoff",
                null),
            CancellationToken.None);

        var request = Assert.Single(service.Requests);
        Assert.Equal("driver-2", request.UserId);
        Assert.Equal(App식별자.DriverApp, request.AppKey);
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportDropoffCompleted, request.EventCode);
        Assert.Equal("DriverTransport", request.SourceKind);
        Assert.Equal("102", request.SourceId);
        Assert.Equal("HD-102", request.SourceDisplayId);
        Assert.Equal("api/v1/driver/transports/102/complete", request.Route);
        Assert.Equal("trace-dropoff", request.TraceId);
        Assert.Equal(occurredAt, request.OccurredAtUtc);
    }

    [Fact]
    public async Task Handle_운송문제신고Event_문제신고경험치코드를기록한다()
    {
        var service = new FakeCommunityExperienceAwardService();
        var handler = new 운송경험치EventHandler(CreateRecorder(service));
        var occurredAt = new DateTime(2026, 7, 12, 3, 30, 0, DateTimeKind.Utc);

        await handler.Handle(
            new 운송문제신고됨Event(
                "driver-3",
                103,
                "HD-103",
                "상차",
                "site-delay",
                "상차지 대기",
                "현장 확인 필요",
                "proof/103.jpg",
                "https://example.test/proof/103.jpg",
                true,
                occurredAt,
                "trace-issue"),
            CancellationToken.None);

        var request = Assert.Single(service.Requests);
        Assert.Equal("driver-3", request.UserId);
        Assert.Equal(App식별자.DriverApp, request.AppKey);
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportIssueReported, request.EventCode);
        Assert.Equal("DriverTransportIssue", request.SourceKind);
        Assert.Equal("103", request.SourceId);
        Assert.Equal("HD-103", request.SourceDisplayId);
        Assert.Equal("api/v1/driver/transports/103/report-issue", request.Route);
        Assert.Equal("trace-issue", request.TraceId);
        Assert.Equal(occurredAt, request.OccurredAtUtc);
    }

    private sealed class FakeCommunityExperienceAwardService : ICommunityExperienceAwardService
    {
        public List<CommunityExperienceAwardRequest> Requests { get; } = [];

        public Task<CommunityExperienceAwardResult> RecordAsync(
            CommunityExperienceAwardRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new CommunityExperienceAwardResult(
                true,
                request.EventCode,
                1,
                "ok"));
        }
    }

    private static CommunityExperienceEventRecorder CreateRecorder(
        ICommunityExperienceAwardService service)
        => new(service, NullLogger<CommunityExperienceEventRecorder>.Instance);
}
