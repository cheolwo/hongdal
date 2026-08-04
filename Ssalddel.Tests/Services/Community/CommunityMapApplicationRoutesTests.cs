using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Mart;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityMapApplicationRoutesTests
{
    [Fact]
    public void 지도마커는_세가지기존신청화면으로_안전한문맥을전달한다()
    {
        var options = CommunityMapApplicationRoutes.ForMarker(
            "news-publisher:kr-yonhap",
            "연합뉴스",
            "news-publisher",
            "kr");

        Assert.Equal(3, options.Count);
        Assert.Equal(
            [
                CommunityMapApplicationActionCodes.LogisticsProxy,
                CommunityMapApplicationActionCodes.TransportProxy,
                CommunityMapApplicationActionCodes.IndividualOrder
            ],
            options.Select(option => option.Code));

        var logistics = InboundRequestNavigationContext.Parse(options[0].Href);
        Assert.StartsWith(InboundRequestPageRoutes.Create, options[0].Href, StringComparison.Ordinal);
        Assert.Equal(CommunityMapApplicationRoutes.SourceCode, logistics.Source);
        Assert.Equal("news-publisher:kr-yonhap", logistics.SourceMarkerId);
        Assert.Equal("연합뉴스", logistics.NodeTitle);
        Assert.Equal("news-publisher", logistics.NodeGroup);
        Assert.Equal("KR", logistics.Scope);
        Assert.Equal(CommunityMapApplicationRoutes.ReturnPath, logistics.From);
        Assert.Null(logistics.SupplierName);
        Assert.Null(logistics.WarehouseId);

        var transport = ShipperRequestNavigationContext.Parse(options[1].Href);
        Assert.StartsWith(ShipperRequestPageRoutes.Root, options[1].Href, StringComparison.Ordinal);
        Assert.Equal(CommunityMapApplicationRoutes.SourceCode, transport.Source);
        Assert.Equal("news-publisher:kr-yonhap", transport.SourceMarkerId);
        Assert.Equal("연합뉴스", transport.NodeTitle);
        Assert.Equal("news-publisher", transport.NodeKind);
        Assert.Equal("KR", transport.CountryCode);
        Assert.Equal(CommunityMapApplicationRoutes.ReturnPath, transport.ReturnPath);

        Assert.StartsWith(MartProductPageRoutes.OrderRoot, options[2].Href, StringComparison.Ordinal);
        Assert.Contains("source=community-map", options[2].Href, StringComparison.Ordinal);
        Assert.Contains("sourceMarkerId=news-publisher%3Akr-yonhap", options[2].Href, StringComparison.Ordinal);
        Assert.Contains("markerTitle=%EC%97%B0%ED%95%A9%EB%89%B4%EC%8A%A4", options[2].Href, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", "name", "layer", "KR")]
    [InlineData("marker", "bad\nname", "layer", "KR")]
    [InlineData("marker", "name", "layer", "")]
    public void 비어있거나제어문자가있는_지도문맥은거부한다(
        string markerId,
        string markerName,
        string layerCode,
        string countryCode)
    {
        Assert.Throws<ArgumentException>(() => CommunityMapApplicationRoutes.ForMarker(
            markerId,
            markerName,
            layerCode,
            countryCode));
    }

    [Theory]
    [InlineData("https://localhost/shipper/inbound/requests/new?source=community-map", true)]
    [InlineData("https://localhost/shipper/request?source=community-map", true)]
    [InlineData("https://localhost/shipper/request/transport?source=community-map", true)]
    [InlineData("https://localhost/shipper/request/request-123?created=true&source=community-map", true)]
    [InlineData("https://localhost/shipper/inbound/requests/42?source=community-map", true)]
    [InlineData("https://localhost/orderer/mart/order?source=community-map", true)]
    [InlineData("https://localhost/shipper/request", false)]
    [InlineData("https://localhost/shipper/request?source=community-post", false)]
    [InlineData("https://localhost/login?source=community-map", false)]
    [InlineData("https://localhost/community/home?source=community-map", false)]
    public void 지도출발신청경로만_메뉴없는단독Layout을사용한다(string uri, bool expected)
        => Assert.Equal(expected, CommunityMapApplicationRoutes.UsesStandaloneApplicationLayout(uri));

    [Fact]
    public void 잘못된QueryEncoding은_단독Layout으로오인하지않는다()
        => Assert.False(CommunityMapApplicationRoutes.UsesStandaloneApplicationLayout(
            "https://localhost/shipper/request?source=%ZZcommunity-map"));
}
