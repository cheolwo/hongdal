using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.ViewSettings;
using Ssalddel.Services.Community;
using 살뜰.Services.Audit;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityExperienceAwardServiceTests
{
    [Fact]
    public async Task RecordAsync_RecordsExperienceAwardAuditLog()
    {
        var activityLogService = new Fake사용자행위로그Service();
        var service = new CommunityExperienceAwardService(activityLogService);
        var occurredAt = new DateTime(2026, 7, 12, 1, 2, 3, DateTimeKind.Utc);

        var result = await service.RecordAsync(
            new CommunityExperienceAwardRequest(
                "driver-1",
                "기사",
                CommunityLedgerExperienceEventCodes.TransportPickupCompleted,
                "DriverTransport",
                "100",
                "HD-100",
                "api/v1/driver/transports/100/pickup-complete",
                "trace-1",
                occurredAt,
                App식별자.DriverApp),
            CancellationToken.None);

        Assert.True(result.처리됨);
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportPickupCompleted, result.EventCode);
        Assert.Equal(20, result.BaseExperience);

        var entry = Assert.Single(activityLogService.Entries);
        Assert.Equal(App식별자.DriverApp, entry.AppKey);
        Assert.Equal("driver-1", entry.UserId);
        Assert.Equal("기사", entry.RoleName);
        Assert.Equal(CommunityExperienceActionTypes.ExperienceAward, entry.ActionType);
        Assert.Equal("운송 상차 완료", entry.ActionName);
        Assert.Equal("api/v1/driver/transports/100/pickup-complete", entry.Route);
        Assert.Equal("trace-1", entry.TraceId);
        Assert.Equal(occurredAt, entry.OccurredAtUtc);

        using var document = JsonDocument.Parse(entry.MetadataJson);
        var root = document.RootElement;
        Assert.Equal(CommunityLedgerExperienceEventCodes.TransportPickupCompleted, root.GetProperty("EventCode").GetString());
        Assert.Equal(20, root.GetProperty("BaseExperience").GetInt32());
        Assert.Equal("DriverTransport", root.GetProperty("SourceKind").GetString());
        Assert.Equal("100", root.GetProperty("SourceId").GetString());
        Assert.Equal(nameof(CommunityLedgerExperiencePolicyResponse), root.GetProperty("Policy").GetString());
    }

    [Fact]
    public async Task RecordAsync_SkipsUnknownExperienceEvent()
    {
        var activityLogService = new Fake사용자행위로그Service();
        var service = new CommunityExperienceAwardService(activityLogService);

        var result = await service.RecordAsync(
            new CommunityExperienceAwardRequest(
                "driver-1",
                "기사",
                "UnknownEvent",
                "DriverTransport",
                "100",
                "HD-100",
                "api/v1/driver/transports/100/pickup-complete",
                "trace-1",
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.False(result.처리됨);
        Assert.Empty(activityLogService.Entries);
        Assert.Contains("등록되지 않은", result.사유);
    }

    private sealed class Fake사용자행위로그Service : I사용자행위로그Service
    {
        public List<사용자행위로그기록> Entries { get; } = [];

        public Task 기록Async(사용자행위로그기록 entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }
}
