using Ssalddel.Contracts.Common.Mart;

namespace Ssalddel.Tests.Contracts.Common.Mart;

public sealed class MartProductPageRoutesTests
{
    [Fact]
    public void StableIdRoutes_RequirePositiveProductId()
    {
        Assert.Equal("/food/mart/products/41", MartProductPageRoutes.DetailFor(41));
        Assert.Equal("/food/mart/reviews/41", MartProductPageRoutes.ReviewFor(41));
        Assert.Equal("/food/mart/order/41", MartProductPageRoutes.OrderFor(41));
        Assert.Throws<ArgumentOutOfRangeException>(() => MartProductPageRoutes.DetailFor(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => MartProductPageRoutes.ReviewFor(-1));
    }

    [Fact]
    public void Context_RoundTripsListStateAcrossDetailReviewAndOrder()
    {
        var context = new MartProductNavigationContext
        {
            From = "/diagram?node=mart-41",
            Search = "제철 감자",
            AvailableOnly = true,
            Page = 3
        };

        var detail = context.PathFor(MartProductScreenKind.Detail, 41);
        var parsed = MartProductNavigationContext.Parse(detail);

        Assert.StartsWith("/food/mart/products/41?", detail, StringComparison.Ordinal);
        Assert.Equal(context.From, parsed.From);
        Assert.Equal(context.Search, parsed.Search);
        Assert.True(parsed.AvailableOnly);
        Assert.Equal(3, parsed.Page);
        Assert.StartsWith("/food/mart/reviews/41?", parsed.PathFor(MartProductScreenKind.Review, 41), StringComparison.Ordinal);
        Assert.StartsWith("/food/mart/order/41?", parsed.PathFor(MartProductScreenKind.Order, 41), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/safe\\redirect")]
    [InlineData("/%2f%2fevil.example")]
    public void Context_DropsUnsafeReturnPath(string from)
    {
        var parsed = MartProductNavigationContext.Parse(
            $"/food/mart?from={Uri.EscapeDataString(from)}");

        Assert.Null(parsed.From);
        Assert.Equal("/food", parsed.ResolveReturnPath("/food"));
    }

    [Fact]
    public void Context_NormalizesInvalidQueryAndSupportsLegacySearchName()
    {
        var parsed = MartProductNavigationContext.Parse(
            "/food/mart?search=%EC%83%9D%EC%88%98&available=1&page=-3");

        Assert.Equal("생수", parsed.Search);
        Assert.True(parsed.AvailableOnly);
        Assert.Equal(1, parsed.Page);
        Assert.Equal(MartProductPageRoutes.Root, new MartProductNavigationContext().PathFor(MartProductScreenKind.List));
    }
}
