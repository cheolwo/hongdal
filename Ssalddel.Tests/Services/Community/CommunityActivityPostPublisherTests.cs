using Ssalddel.Application.Driver.Transport;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityActivityPostPublisherTests
{
    [Fact]
    public async Task PublishAsync_DelegatesOnlyToPrivacySafeProjectionRecorder()
    {
        var recorder = new RecordingProjectionRecorder();
        var service = new CommunityActivityPostPublisher(recorder);
        var definition = CommunityActivityBoardCatalog.FindSource(
            CommunityActivitySourceKinds.Event,
            nameof(운송상차완료됨Event))!;
        var occurrence = new 운송상차완료됨Event(
            "driver-secret",
            812,
            "TR-PRIVATE-812",
            "서울시 비공개 출발지",
            "부산시 비공개 도착지",
            "배차완료",
            "상차완료",
            new DateTime(2026, 7, 23, 1, 2, 0, DateTimeKind.Utc),
            "trace-secret",
            null);

        await service.PublishAsync(definition, occurrence);

        var recorded = Assert.Single(recorder.Records);
        Assert.Same(definition, recorded.Definition);
        Assert.Same(occurrence, recorded.Occurrence);
    }

    private sealed class RecordingProjectionRecorder : I커뮤니티활동공개ProjectionRecorder
    {
        public List<(CommunityActivityBoardDefinition Definition, object Occurrence)> Records { get; } = [];

        public Task RecordAsync(
            CommunityActivityBoardDefinition definition,
            object occurrence,
            CancellationToken cancellationToken = default)
        {
            Records.Add((definition, occurrence));
            return Task.CompletedTask;
        }
    }
}
