using Ssalddel.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityActivitySignalPolicyTests
{
    [Fact]
    public void 공개가능한가_RequiresMinimumAggregateCount()
    {
        Assert.False(커뮤니티활동공개Policy.공개가능한가(
            커뮤니티활동공개Policy.최소공개활동수 - 1));
        Assert.True(커뮤니티활동공개Policy.공개가능한가(
            커뮤니티활동공개Policy.최소공개활동수));
    }

    [Fact]
    public void 주간시작Utc_RemovesExactOccurrenceTime()
    {
        var exactOccurrence = new DateTime(
            2026,
            7,
            23,
            18,
            42,
            17,
            DateTimeKind.Utc);

        var bucketStart = 커뮤니티활동공개Policy.주간시작Utc(exactOccurrence);

        Assert.Equal(
            new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc),
            bucketStart);
        Assert.NotEqual(exactOccurrence, bucketStart);
    }
}
