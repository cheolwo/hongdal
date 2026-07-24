using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class PlatformCommunityNodeNavigationResolverTests
{
    private readonly WebPlatformCommunityNodeNavigationResolver _resolver = new();

    [Fact]
    public void DefaultResolver_DoesNotGuessAPlatformRoute()
    {
        var resolver = new UnsupportedPlatformCommunityNodeNavigationResolver();

        var target = resolver.Resolve(Request(
            CommunityLedgerTemplateKeys.CargoTransport,
            "운송 의뢰",
            "work"));

        Assert.Null(target);
    }

    [Theory]
    [InlineData(CommunityLedgerTemplateKeys.CargoTransport, "운송 의뢰", "work", null, "/shipper/request", PlatformCommunityNodeNavigationArea.Shipper)]
    [InlineData(CommunityLedgerTemplateKeys.CargoTransport, "운송 구간", "delivery", null, "/driver/transports/current", PlatformCommunityNodeNavigationArea.Driver)]
    [InlineData(CommunityLedgerTemplateKeys.CargoTransport, "상차 증빙", "work", null, "/driver/transports/current", PlatformCommunityNodeNavigationArea.Driver)]
    [InlineData(CommunityLedgerTemplateKeys.CargoTransport, "상차 확인", "form", PlatformDiagramFormKinds.TransportPickupConfirmation, "/driver/transports/current", PlatformCommunityNodeNavigationArea.Driver)]
    [InlineData(CommunityLedgerTemplateKeys.CargoTransport, "정산", "confirm", null, "/shipper/request/payment-status", PlatformCommunityNodeNavigationArea.Shipper)]
    [InlineData(CommunityLedgerTemplateKeys.WarehouseInbound, "입고 검수", "work", null, "/work/inbound/inspection", PlatformCommunityNodeNavigationArea.Warehouse)]
    [InlineData(CommunityLedgerTemplateKeys.WarehouseOutbound, "피킹", "work", null, "/work/picking-batch", PlatformCommunityNodeNavigationArea.Warehouse)]
    [InlineData(CommunityLedgerTemplateKeys.GroupImport, "통관", "work", null, "/community/group-import", PlatformCommunityNodeNavigationArea.Community)]
    [InlineData(CommunityLedgerTemplateKeys.WarehouseInbound, "입고 신청", "form", PlatformDiagramFormKinds.WarehouseInbound, "/shipper/inbound/requests/new", PlatformCommunityNodeNavigationArea.Shipper)]
    public void WebResolver_ReturnsOnlyRoutesProvidedByTheWebHost(
        string templateKey,
        string nodeTitle,
        string nodeKind,
        string? formKind,
        string expectedPath,
        PlatformCommunityNodeNavigationArea expectedArea)
    {
        var target = _resolver.Resolve(Request(templateKey, nodeTitle, nodeKind, formKind));

        Assert.NotNull(target);
        Assert.Equal(expectedPath, target.Path);
        Assert.Equal(expectedArea, target.Area);
        Assert.StartsWith("/", target.Path);
    }

    [Fact]
    public void CargoFallback_UsesTheListInsteadOfInventingARequestId()
    {
        var target = _resolver.Resolve(Request(
            CommunityLedgerTemplateKeys.CargoTransport,
            "기타 운송 단계",
            "work"));

        Assert.NotNull(target);
        Assert.Equal(ShipperRoutes.Request, target.Path);
        Assert.DoesNotContain("HD-WEB-001", target.Path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestMatching_IsCaseInsensitiveAndDoesNotGuessBlankTokens()
    {
        var request = Request(
            CommunityLedgerTemplateKeys.CargoTransport,
            "PICKUP ROUTE",
            "Delivery");

        Assert.True(request.IsNodeKind("delivery"));
        Assert.True(request.TitleContainsAny("route"));
        Assert.False(request.TitleContainsAny("", "  ", "정산"));
    }

    private static PlatformCommunityNodeNavigationRequest Request(
        string templateKey,
        string nodeTitle,
        string nodeKind,
        string? formKind = null)
        => new(templateKey, nodeTitle, nodeKind, formKind);
}
