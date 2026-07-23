using Ssalddel.Application.Driver.Transport;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityActivityPostPublisherTests
{
    [Fact]
    public async Task PublishAsync_UsesOnlyNonIdentifyingSummaryAndStableOccurrenceKey()
    {
        var automatedPublisher = new RecordingAutomatedPostPublisher();
        var service = new CommunityActivityPostPublisher(
            automatedPublisher,
            TimeProvider.System);
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
            new 운송상차인수증증빙(
                true,
                true,
                "전자서명",
                "홍길동",
                "비공개 업체",
                "recipient-signature",
                "driver-signature",
                null,
                "private-photo.jpg",
                "https://private.example/photo"));

        await service.PublishAsync(definition, occurrence);
        await service.PublishAsync(definition, occurrence);

        var first = automatedPublisher.Drafts[0];
        var second = automatedPublisher.Drafts[1];
        Assert.Equal(definition.Board.DisplayName, first.Category);
        Assert.Equal(first.PeriodKey, second.PeriodKey);
        Assert.Contains(nameof(운송상차완료됨Event), first.Body);
        Assert.Contains(definition.PublicActivitySummary, first.Body);
        Assert.Contains(CommunityActivityBoardCatalog.PrivacyBoundary, first.Body);
        Assert.DoesNotContain("driver-secret", first.Body);
        Assert.DoesNotContain("TR-PRIVATE-812", first.Body);
        Assert.DoesNotContain("비공개 출발지", first.Body);
        Assert.DoesNotContain("홍길동", first.Body);
        Assert.DoesNotContain("private-photo.jpg", first.Body);
    }

    private sealed class RecordingAutomatedPostPublisher : ICommunityAutomatedPostPublisher
    {
        public List<CommunityAutomatedPostDraft> Drafts { get; } = [];

        public Task<CommunityAutomatedPostPublishResult> PublishIfMissingAsync(
            CommunityAutomatedPostDraft draft,
            CancellationToken cancellationToken = default)
        {
            Drafts.Add(draft);
            return Task.FromResult(new CommunityAutomatedPostPublishResult(Drafts.Count, true));
        }
    }
}
