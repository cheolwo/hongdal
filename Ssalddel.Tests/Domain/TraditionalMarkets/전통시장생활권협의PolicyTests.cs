using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Domain.TraditionalMarkets;

namespace Ssalddel.Tests.Domain.TraditionalMarkets;

public sealed class 전통시장생활권협의PolicyTests
{
    [Fact]
    public void 양측대표가_모두동의해야_안건이합의된다()
    {
        Assert.Equal(
            전통시장교역안건상태Codes.검토중,
            전통시장생활권협의Policy.안건상태(
                전통시장협의결정Codes.동의,
                전통시장협의결정Codes.대기));

        Assert.Equal(
            전통시장교역안건상태Codes.합의,
            전통시장생활권협의Policy.안건상태(
                전통시장협의결정Codes.동의,
                전통시장협의결정Codes.동의));
    }

    [Theory]
    [InlineData("보완요청", "동의", "보완요청")]
    [InlineData("동의", "보완요청", "보완요청")]
    [InlineData("반대", "보완요청", "반려")]
    [InlineData("동의", "반대", "반려")]
    public void 보완요청과반대는_합의보다우선한다(
        string apartmentDecision,
        string merchantDecision,
        string expectedStatus)
    {
        Assert.Equal(
            expectedStatus,
            전통시장생활권협의Policy.안건상태(apartmentDecision, merchantDecision));
    }

    [Fact]
    public void 사용자Id로_협의체내대표역할을판정한다()
    {
        var council = new 전통시장생활권협의체
        {
            아파트대표UserId = "apartment-representative",
            상인회대표UserId = "merchant-representative"
        };

        Assert.Equal(
            전통시장협의체역할Codes.아파트대표,
            전통시장생활권협의Policy.참여역할(council, "apartment-representative"));
        Assert.Equal(
            전통시장협의체역할Codes.상인회대표,
            전통시장생활권협의Policy.참여역할(council, "merchant-representative"));
        Assert.Empty(전통시장생활권협의Policy.참여역할(council, "other-user"));
    }

    [Fact]
    public void 협의체와안건_참조Key는안정적으로생성된다()
    {
        var id = Guid.Parse("8f732b15-6134-4db2-a78f-a7da66c3ab44");

        Assert.Equal(
            "traditional-market-council:8f732b1561344db2a78fa7da66c3ab44",
            전통시장생활권협의참조.협의체(id));
        Assert.Equal(
            "traditional-market-trade-agenda:8f732b1561344db2a78fa7da66c3ab44",
            전통시장생활권협의참조.안건(id));
    }
}
