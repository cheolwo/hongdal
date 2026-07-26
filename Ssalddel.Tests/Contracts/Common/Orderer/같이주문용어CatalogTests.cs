using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 같이주문용어CatalogTests
{
    [Theory]
    [InlineData(DisplayLanguageCodes.Korean, "같이 주문")]
    [InlineData(DisplayLanguageCodes.English, "Order Together")]
    [InlineData(DisplayLanguageCodes.Japanese, "一緒に注文")]
    public void 표시언어에_맞는_같이주문_용어를_반환한다(
        string languageCode,
        string expected)
    {
        Assert.Equal(expected, 같이주문용어Catalog.표시명(languageCode));
    }

    [Theory]
    [InlineData("공동주문")]
    [InlineData("공동 주문")]
    [InlineData("group order")]
    [InlineData("Order Together")]
    public void 이전_용어는_검색_호환용으로_유지한다(string legacyTerm)
    {
        Assert.Contains(legacyTerm, 같이주문용어Catalog.검색호환용어);
    }
}
