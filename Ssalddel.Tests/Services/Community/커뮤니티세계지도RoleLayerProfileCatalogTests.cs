using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도RoleLayerProfileCatalogTests
{
    [Theory]
    [InlineData(null, 커뮤니티세계지도RolePerspectiveCodes.Community)]
    [InlineData("주문자", 커뮤니티세계지도RolePerspectiveCodes.Orderer)]
    [InlineData("구매 담당", 커뮤니티세계지도RolePerspectiveCodes.Orderer)]
    [InlineData("판매자", 커뮤니티세계지도RolePerspectiveCodes.SellerAndShipper)]
    [InlineData("Shipper", 커뮤니티세계지도RolePerspectiveCodes.SellerAndShipper)]
    [InlineData("용달 기사", 커뮤니티세계지도RolePerspectiveCodes.TransportOperator)]
    [InlineData("WarehouseManager", 커뮤니티세계지도RolePerspectiveCodes.WarehouseManager)]
    [InlineData("관세사", 커뮤니티세계지도RolePerspectiveCodes.CustomsSpecialist)]
    [InlineData("서버 관리자", 커뮤니티세계지도RolePerspectiveCodes.PlatformOperator)]
    public void 계정역할은_공개지도관점으로정규화된다(string? role, string expectedCode)
    {
        var profile = 커뮤니티세계지도RoleLayerProfileCatalog.Resolve(role);

        Assert.Equal(expectedCode, profile.Code);
        Assert.NotEmpty(profile.RecommendedLayerCodes);
        Assert.Equal(
            profile.RecommendedLayerCodes.Count,
            profile.RecommendedLayerCodes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void 역할별기본Layer는_업무에필요한공개근거만우선한다()
    {
        var community = 커뮤니티세계지도RoleLayerProfileCatalog.Resolve(null);
        var orderer = 커뮤니티세계지도RoleLayerProfileCatalog.Resolve("Orderer");
        var driver = 커뮤니티세계지도RoleLayerProfileCatalog.Resolve("Driver");
        var customs = 커뮤니티세계지도RoleLayerProfileCatalog.Resolve("CustomsBroker");

        Assert.Equal(
            [
                커뮤니티세계지도LayerCodes.RegionalCulture,
                커뮤니티세계지도LayerCodes.TourismPublicEvidence,
                커뮤니티세계지도LayerCodes.PublicPrice,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext,
                커뮤니티세계지도LayerCodes.NewsPublisher
            ],
            community.RecommendedLayerCodes);
        Assert.Contains(커뮤니티세계지도LayerCodes.ProcurementHandoff, orderer.RecommendedLayerCodes);
        Assert.Contains(커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence, orderer.RecommendedLayerCodes);
        Assert.Contains(커뮤니티세계지도LayerCodes.KosisStatisticalContext, orderer.RecommendedLayerCodes);
        Assert.Equal(
            [커뮤니티세계지도LayerCodes.TransportHandoff],
            driver.RecommendedLayerCodes);
        Assert.Contains(커뮤니티세계지도LayerCodes.ImportReadiness, customs.RecommendedLayerCodes);
        Assert.DoesNotContain(커뮤니티세계지도LayerCodes.RegionalCulture, customs.RecommendedLayerCodes);
        Assert.Contains(
            커뮤니티세계지도LayerCodes.NewsPublisher,
            커뮤니티세계지도RoleLayerProfileCatalog.Resolve("서버 관리자").RecommendedLayerCodes);
    }

    [Fact]
    public void 원장관점Layer는_공개관측Layer와실제원장Template을연결한다()
    {
        var ledgerLayers = 커뮤니티세계지도LayerCatalog.ForDataset(
                CommunityPageRoutes.WorldMapDayWorkDataset)
            .Where(layer => layer.LedgerTemplateKey is not null)
            .ToArray();

        Assert.Equal(4, ledgerLayers.Length);
        Assert.Equal(
            [
                커뮤니티세계지도LayerCodes.ProcurementHandoff,
                커뮤니티세계지도LayerCodes.ImportReadiness,
                커뮤니티세계지도LayerCodes.TransportHandoff,
                커뮤니티세계지도LayerCodes.WarehouseInboundHandoff
            ],
            ledgerLayers.Select(layer => layer.Code));

        Assert.All(ledgerLayers, layer =>
        {
            Assert.NotNull(CommunityLedgerTemplateCatalog.Find(layer.LedgerTemplateKey!));
            Assert.NotNull(layer.ObservationSourceLayerCodes);
            Assert.NotEmpty(layer.ObservationSourceLayerCodes!);
            Assert.All(layer.ObservationSourceLayerCodes!, sourceLayerCode =>
            {
                var sourceLayer = Assert.Single(
                    커뮤니티세계지도LayerCatalog.All,
                    candidate => candidate.Code == sourceLayerCode);
                Assert.Null(sourceLayer.LedgerTemplateKey);
            });
        });
    }

    [Fact]
    public void 개인실행좌표가필요한원장은_공개지도Layer로직접노출하지않는다()
    {
        var ledgerTemplateKeys = 커뮤니티세계지도LayerCatalog.All
            .Select(layer => layer.LedgerTemplateKey)
            .Where(key => key is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain(CommunityLedgerTemplateKeys.FoodDelivery, ledgerTemplateKeys);
        Assert.DoesNotContain(CommunityLedgerTemplateKeys.SsalddelMart, ledgerTemplateKeys);
        Assert.DoesNotContain(CommunityLedgerTemplateKeys.WarehouseOutbound, ledgerTemplateKeys);
        Assert.DoesNotContain(CommunityLedgerTemplateKeys.Order, ledgerTemplateKeys);
    }

    [Fact]
    public void 운영자외역할은_모든Layer를자동활성화하지않는다()
    {
        var publicLayerCount = 커뮤니티세계지도LayerCatalog.ForDataset(
            CommunityPageRoutes.WorldMapDayWorkDataset).Count;

        Assert.All(
            커뮤니티세계지도RoleLayerProfileCatalog.All.Where(profile =>
                profile.Code != 커뮤니티세계지도RolePerspectiveCodes.PlatformOperator),
            profile => Assert.True(profile.RecommendedLayerCodes.Count < publicLayerCount));
    }
}
