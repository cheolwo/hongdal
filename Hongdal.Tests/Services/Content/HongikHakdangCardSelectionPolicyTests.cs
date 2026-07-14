using Hongdal.Services.Content;

namespace Hongdal.Tests.Services.Content;

public sealed class HongikHakdangCardSelectionPolicyTests
{
    private readonly HongikHakdangCardSelectionPolicy _policy = new();

    [Fact]
    public void Select_IsDeterministic_ForTheSameDateAndTimeZone()
    {
        var activeCardIds = Enumerable.Range(1, 20).Select(value => (long)value).ToArray();
        var date = new DateOnly(2026, 7, 14);

        var first = _policy.Select(date, "Asia/Seoul", activeCardIds, []);
        var second = _policy.Select(date, "Asia/Seoul", activeCardIds.Reverse().ToArray(), []);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Select_ExcludesRecentlySelectedCards_WhenUnusedCardsRemain()
    {
        long[] activeCardIds = [10, 20, 30, 40];
        long[] recentlySelectedCardIds = [10, 20, 30];

        var selected = _policy.Select(
            new DateOnly(2026, 7, 14),
            "Asia/Seoul",
            activeCardIds,
            recentlySelectedCardIds);

        Assert.Equal(40, selected);
    }

    [Fact]
    public void Select_ReusesTheActiveDeck_AfterEveryCardHasBeenSeen()
    {
        long[] activeCardIds = [10, 20, 30];

        var selected = _policy.Select(
            new DateOnly(2026, 7, 14),
            "Asia/Seoul",
            activeCardIds,
            activeCardIds);

        Assert.Contains(selected, activeCardIds);
    }
}
