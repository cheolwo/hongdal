using Hongdal.Domain.HsCodes;

namespace Hongdal.Tests.Domain.HsCodes;

public sealed class HsCodeBusinessCategoryClassifierTests
{
    [Theory]
    [InlineData("01", HsCodeBusinessCategory.Food)]
    [InlineData("2106.90", HsCodeBusinessCategory.Food)]
    [InlineData("24.02", HsCodeBusinessCategory.Food)]
    [InlineData("2501.00", HsCodeBusinessCategory.GeneralCargo)]
    [InlineData("8543.70", HsCodeBusinessCategory.GeneralCargo)]
    [InlineData("9401.69", HsCodeBusinessCategory.GeneralCargo)]
    public void Classify_UsesHsChapterForInitialBusinessCategory(
        string hsCode,
        HsCodeBusinessCategory expectedCategory)
    {
        var decision = HsCodeBusinessCategoryClassifier.Classify(hsCode);

        Assert.Equal(expectedCategory, decision.Category);
        Assert.NotEmpty(decision.Reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("review-required")]
    public void Classify_ReturnsUnknownWhenChapterCannotBeParsed(string? hsCode)
    {
        var decision = HsCodeBusinessCategoryClassifier.Classify(hsCode);

        Assert.Equal(HsCodeBusinessCategory.Unknown, decision.Category);
        Assert.Equal(HsCodeBusinessCategoryClassifier.UnknownReason, decision.Reason);
    }
}
