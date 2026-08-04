using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Tests.Contracts.Shipper.Request;

public sealed class ShipperRequestPageRoutesTests
{
    [Theory]
    [InlineData(ShipperRequestAuthoringStep.Cargo, "/shipper/request/cargo")]
    [InlineData(ShipperRequestAuthoringStep.Transport, "/shipper/request/transport")]
    [InlineData(ShipperRequestAuthoringStep.Procedure, "/shipper/request/procedure")]
    [InlineData(ShipperRequestAuthoringStep.Review, "/shipper/request/review")]
    public void 작성단계는_Web과모바일이공유하는StableRoute를가진다(
        ShipperRequestAuthoringStep step,
        string expected)
    {
        Assert.Equal(expected, ShipperRequestPageRoutes.PathFor(step));
    }

    [Fact]
    public void 작성단계를이동해도_커뮤니티글과Diagram복귀문맥을보존한다()
    {
        var source =
            "https://localhost/shipper/request?sourcePostId=73&source=diagram-node&ledgerTemplateKey=cargo-transport" +
            "&sourceMarkerId=marker-73&nodeTitle=%EC%9A%B4%EC%86%A1%20%EC%9D%98%EB%A2%B0&nodeKind=delivery&country=KR" +
            "&from=%2Fdiagram%3FledgerTemplate%3Dcargo-transport%26node%3D%25EC%259A%25B4%25EC%2586%25A1%2520%25EC%259D%2598%25EB%25A2%25B0%26zoom%3D120";

        var context = ShipperRequestNavigationContext.Parse(source);
        var next = context.PathFor(ShipperRequestAuthoringStep.Transport);

        Assert.Equal(73, context.SourcePostId);
        Assert.Equal("diagram-node", context.Source);
        Assert.Equal("marker-73", context.SourceMarkerId);
        Assert.Equal("cargo-transport", context.LedgerTemplateKey);
        Assert.Equal("운송 의뢰", context.NodeTitle);
        Assert.Equal("delivery", context.NodeKind);
        Assert.Equal("KR", context.CountryCode);
        Assert.StartsWith("/shipper/request/transport?", next);
        Assert.Contains("sourcePostId=73", next);
        Assert.Contains("nodeTitle=%EC%9A%B4%EC%86%A1%20%EC%9D%98%EB%A2%B0", next);
        Assert.Contains("from=%2Fdiagram%3FledgerTemplate%3Dcargo-transport", next);

        var restored = ShipperRequestNavigationContext.Parse(next);
        Assert.Equal(context, restored);
    }

    [Theory]
    [InlineData("https://evil.example/diagram")]
    [InlineData("//evil.example/diagram")]
    [InlineData("javascript:alert(1)")]
    public void 외부또는과도하게넓은복귀경로는_전달하지않는다(string returnPath)
    {
        var context = ShipperRequestNavigationContext.Parse(
            $"/shipper/request?from={Uri.EscapeDataString(returnPath)}");

        Assert.Null(context.ReturnPath);
        Assert.Equal(ShipperRequestPageRoutes.Cargo, context.PathFor(ShipperRequestAuthoringStep.Cargo));
    }

    [Fact]
    public void 손상된Query는_예외없이빈문맥으로정규화한다()
    {
        var context = ShipperRequestNavigationContext.Parse("/shipper/request?nodeTitle=%ZZ&sourcePostId=-4");

        Assert.Null(context.SourcePostId);
        Assert.Null(context.NodeTitle);
        Assert.Equal(ShipperRequestPageRoutes.Root, context.RootPath);
    }
}
