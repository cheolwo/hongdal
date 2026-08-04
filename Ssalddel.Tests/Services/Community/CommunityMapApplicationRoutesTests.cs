using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Mart;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityMapApplicationRoutesTests
{
    [Fact]
    public void 吏?꾨쭏而ㅻ뒗_?멸?吏湲곗〈?좎껌?붾㈃?쇰줈_?덉쟾?쒕Ц留μ쓣?꾨떖?쒕떎()
    {
        var options = CommunityMapApplicationRoutes.ForMarker(
            "news-publisher:kr-yonhap",
            "?고빀?댁뒪",
            "news-publisher",
            "kr");

        Assert.Equal(3, options.Count);
        var expectedReturnPath = CommunityMapApplicationRoutes.ReturnToMarker(
            "news-publisher:kr-yonhap",
            "news-publisher",
            "KR");
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
        Assert.Equal("?고빀?댁뒪", logistics.NodeTitle);
        Assert.Equal("news-publisher", logistics.NodeGroup);
        Assert.Equal("KR", logistics.Scope);
        Assert.Equal(expectedReturnPath, logistics.From);
        Assert.Null(logistics.SupplierName);
        Assert.Null(logistics.WarehouseId);

        var transport = ShipperRequestNavigationContext.Parse(options[1].Href);
        Assert.StartsWith(ShipperRequestPageRoutes.Root, options[1].Href, StringComparison.Ordinal);
        Assert.Equal(CommunityMapApplicationRoutes.SourceCode, transport.Source);
        Assert.Equal("news-publisher:kr-yonhap", transport.SourceMarkerId);
        Assert.Equal("?고빀?댁뒪", transport.NodeTitle);
        Assert.Equal("news-publisher", transport.NodeKind);
        Assert.Equal("KR", transport.CountryCode);
        Assert.Equal(expectedReturnPath, transport.ReturnPath);

        Assert.StartsWith(MartProductPageRoutes.OrderRoot, options[2].Href, StringComparison.Ordinal);
        Assert.Contains("source=community-map", options[2].Href, StringComparison.Ordinal);
        Assert.Contains("sourceMarkerId=news-publisher%3Akr-yonhap", options[2].Href, StringComparison.Ordinal);
        Assert.Contains("markerTitle=%EC%97%B0%ED%95%A9%EB%89%B4%EC%8A%A4", options[2].Href, StringComparison.Ordinal);
        Assert.Equal(expectedReturnPath, MartProductNavigationContext.Parse(options[2].Href).From);
    }

    [Fact]
    public void ?쒖텧?깃났蹂듦?寃쎈줈??媛숈?援???덉씠?대쭏而ㅼ??먯옣Id瑜쇰낫議댄븳??)
    {
        var path = CommunityMapApplicationRoutes.ReturnToMarker(
            "news-publisher:kr-yonhap",
            "news-publisher",
            "kr",
            "map-application:ledger-1");

        Assert.Equal(
            "/community/home?country=KR"
            + "&layers=news-publisher"
            + "&marker=news-publisher%3Akr-yonhap"
            + "&ledger=map-application%3Aledger-1",
            path);
    }

    [Fact]
    public void BuildChooserPath_IncludesCommunityMapApplicationContext()
    {
        var path = CommunityMapApplicationRoutes.BuildChooserPath(
            "news-publisher:kr-yonhap",
            "연합뉴스",
            "news-publisher",
            "kr",
            action: CommunityMapApplicationActionCodes.TransportProxy,
            returnTo: "/community/home?country=KR");

        Assert.StartsWith("/community/map-application-chooser?", path, StringComparison.Ordinal);
        Assert.Contains("source=community-map", path, StringComparison.Ordinal);
        Assert.Contains("sourceMarkerId=news-publisher%3Akr-yonhap", path, StringComparison.Ordinal);
        Assert.Contains("markerTitle=%EC%97%B0%ED%95%A9%EB%89%B4%EC%8A%A4", path, StringComparison.Ordinal);
        Assert.Contains("markerLayer=news-publisher", path, StringComparison.Ordinal);
        Assert.Contains("country=KR", path, StringComparison.Ordinal);
        Assert.Contains("action=transport-proxy", path, StringComparison.Ordinal);
        Assert.Contains("returnTo=%2Fcommunity%2Fhome%3Fcountry%3DKR", path, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://localhost/community/map-application-chooser?source=community-map", true)]
    public void UsesStandaloneApplicationLayout_IncludesChooserRoute(string uri, bool expected)
        => Assert.Equal(expected, CommunityMapApplicationRoutes.UsesStandaloneApplicationLayout(uri));


    [Theory]
    [InlineData("", "name", "layer", "KR")]
    [InlineData("marker", "bad\nname", "layer", "KR")]
    [InlineData("marker", "name", "layer", "")]
    public void 鍮꾩뼱?덇굅?섏젣?대Ц?먭??덈뒗_吏?꾨Ц留μ?嫄곕??쒕떎(
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
    public void 吏?꾩텧諛쒖떊泥?꼍濡쒕쭔_硫붾돱?녿뒗?⑤룆Layout?꾩궗?⑺븳??string uri, bool expected)
        => Assert.Equal(expected, CommunityMapApplicationRoutes.UsesStandaloneApplicationLayout(uri));

    [Fact]
    public void ?섎せ?쏲ueryEncoding?_?⑤룆Layout?쇰줈?ㅼ씤?섏??딅뒗??)
        => Assert.False(CommunityMapApplicationRoutes.UsesStandaloneApplicationLayout(
            "https://localhost/shipper/request?source=%ZZcommunity-map"));
}
