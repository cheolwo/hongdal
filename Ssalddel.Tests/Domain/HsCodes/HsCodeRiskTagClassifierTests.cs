using Ssalddel.Domain.HsCodes;

namespace Ssalddel.Tests.Domain.HsCodes;

public sealed class HsCodeRiskTagClassifierTests
{
    [Fact]
    public void Suggest_AddsFoodAndBrokerReviewTagsForFoodChapters()
    {
        var tags = HsCodeRiskTagClassifier.Suggest("2106.90");

        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.Food);
        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.FoodQuarantine);
        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.SupplementOrPreparedFoodReview);
        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.BrokerReviewRecommended);
    }

    [Fact]
    public void Suggest_AddsElectricalTagForChapter85()
    {
        var tags = HsCodeRiskTagClassifier.Suggest("8543.70");

        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.ElectricalCertification);
        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.BrokerReviewRecommended);
    }

    [Fact]
    public void Suggest_AddsBatteryTagForBatteryHeadings()
    {
        var tags = HsCodeRiskTagClassifier.Suggest("8507.60");

        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.BatteryIncludedPossible);
        Assert.Contains(tags, x => x.TagType == HsCodeRiskTagType.ElectricalCertification);
    }

    [Fact]
    public void Suggest_ReturnsNoTagsWhenChapterCannotBeParsed()
    {
        var tags = HsCodeRiskTagClassifier.Suggest("review-required");

        Assert.Empty(tags);
    }
}
