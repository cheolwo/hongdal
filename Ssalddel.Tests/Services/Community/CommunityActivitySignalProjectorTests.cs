using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using 살뜰.도메인.설정;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityActivitySignalProjectorTests
{
    [Fact]
    public void TryProject_DriverTransportLog_ReturnsPrivacySafeCommunitySignal()
    {
        var log = new 사용자행위로그
        {
            Id = 10,
            AppKey = "DriverApp",
            UserId = "driver-user-1",
            UserName = "real-driver-name",
            RoleName = "기사",
            EmailMasked = "d***@example.com",
            PhoneLast4 = "1234",
            ActionType = "Create",
            ActionName = "Ssalddel.Controllers.Driver.기사운송진행Controller.PickupComplete",
            Route = "/api/v1/driver/transports/123/pickup-complete",
            TraceId = "trace-private",
            ClientIp = "127.0.0.1",
            UserAgent = "test-agent",
            MetadataJson = "{\"Url\":\"https://localhost/api/v1/driver/transports/123/pickup-complete?phone=01012345678\"}",
            IsSuccess = true,
            OccurredAtUtc = DateTime.UtcNow
        };

        var signal = CommunityActivitySignalProjector.TryProject(log);

        Assert.NotNull(signal);
        Assert.Equal("activity-10", signal.SignalId);
        Assert.Equal(CommunityActivityScopes.DriverWork, signal.CommunityScope);
        Assert.Equal("익명 기사", signal.ActorRoleLabel);
        Assert.Contains("transport", signal.TopicTags);
        Assert.DoesNotContain("real-driver-name", Flatten(signal));
        Assert.DoesNotContain("driver-user-1", Flatten(signal));
        Assert.DoesNotContain("01012345678", Flatten(signal));
        Assert.DoesNotContain("trace-private", Flatten(signal));
        Assert.DoesNotContain("127.0.0.1", Flatten(signal));
    }

    [Fact]
    public void TryProject_AdminLog_ReturnsNull()
    {
        var log = new 사용자행위로그
        {
            Id = 11,
            AppKey = "SsalddelAdmin",
            RoleName = "서버관리자",
            ActionType = "Read",
            Route = "/api/v1/admin/activity-logs",
            IsSuccess = true,
            OccurredAtUtc = DateTime.UtcNow
        };

        var signal = CommunityActivitySignalProjector.TryProject(log);

        Assert.Null(signal);
    }

    private static string Flatten(CommunityActivitySignalResponse signal)
    {
        return string.Join(
            "|",
            signal.SignalId,
            signal.AppKey,
            signal.CommunityScope,
            signal.ActivityKind,
            signal.Title,
            signal.Summary,
            signal.ActorRoleLabel,
            signal.TimeBucketLabel,
            string.Join(",", signal.TopicTags));
    }
}
