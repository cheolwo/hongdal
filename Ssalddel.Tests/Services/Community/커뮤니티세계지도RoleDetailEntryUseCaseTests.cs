using Microsoft.AspNetCore.Authorization;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.ViewSettings;
using Ssalddel.Controllers.Common;
using Ssalddel.Services.Community;
using 살뜰.Data;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도RoleDetailEntryUseCaseTests
{
    private readonly 커뮤니티세계지도RoleDetailEntryUseCase _useCase = new();

    [Fact]
    public void 공개LayerCatalog는_출처유형과공개범위와실행경계를분리한다()
    {
        var dayLayers = 커뮤니티세계지도LayerCatalog.ForDataset("day-work");

        Assert.Equal(13, dayLayers.Count);
        Assert.All(dayLayers, layer => Assert.False(string.IsNullOrWhiteSpace(layer.SourceTypeCode)));
        Assert.All(dayLayers, layer => Assert.False(string.IsNullOrWhiteSpace(layer.DisclosureScopeCode)));

        var traditionalMarket = Assert.Single(dayLayers,
            layer => layer.Code == 커뮤니티세계지도LayerCodes.TraditionalMarketHub);
        Assert.Equal(
            커뮤니티세계지도SourceTypeCodes.PublicOperationalAggregate,
            traditionalMarket.SourceTypeCode);
        Assert.Equal(
            커뮤니티세계지도DisclosureScopeCodes.PublicAggregated,
            traditionalMarket.DisclosureScopeCode);

        var handoffLayers = dayLayers.Where(layer => layer.RoleDetailEntryCode is not null).ToArray();
        Assert.Equal(4, handoffLayers.Length);
        Assert.All(handoffLayers, layer =>
        {
            Assert.Equal(
                커뮤니티세계지도SourceTypeCodes.PublicEvidenceComposite,
                layer.SourceTypeCode);
            Assert.Equal(
                커뮤니티세계지도ExecutionBoundaryCodes.RoleAppAuthorizedDetail,
                layer.ExecutionBoundaryCode);
        });
    }

    [Theory]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.GroupPurchase, 역할명.커뮤니티회원, App식별자.OrdererApp, "/group-purchase/groups")]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.Transport, 역할명.기사, App식별자.DriverApp, "/driver/transports/current")]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.Transport, 역할명.화주, App식별자.SsalddelApp, "/shipper/transport")]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.WarehouseInbound, 역할명.창고관리자, App식별자.WarehouseManagerApp, "/work/inbound/inspection")]
    public void 인증Role에맞는_역할App작업대만반환한다(
        string entryCode,
        string role,
        string expectedApp,
        string expectedRoute)
    {
        var response = _useCase.Resolve(entryCode, role);

        Assert.NotNull(response);
        Assert.Equal(expectedApp, response.AppKey);
        Assert.Equal(expectedRoute, response.Route);
        Assert.Equal(
            커뮤니티세계지도ExecutionBoundaryCodes.RoleAppAuthorizedDetail,
            response.ExecutionBoundaryCode);
        Assert.Contains("다시 검증", response.AuthorizationNotice);
    }

    [Theory]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.Transport, 역할명.커뮤니티회원)]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.WarehouseInbound, 역할명.기사)]
    [InlineData("unknown", 역할명.서버관리자)]
    [InlineData(커뮤니티세계지도RoleDetailEntryCodes.GroupPurchase, "")]
    public void 권한없는Role이나알수없는진입은_경로를반환하지않는다(string entryCode, string role)
        => Assert.Null(_useCase.Resolve(entryCode, role));

    [Fact]
    public void 상세진입Controller는_익명공개Controller와분리해인증을요구한다()
    {
        var controllerType = typeof(커뮤니티세계지도RoleDetailEntryController);

        Assert.NotNull(controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault());
        Assert.Empty(controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }

    [Fact]
    public void 상세진입응답은_개별업무식별자나운영상태를계약에포함하지않는다()
    {
        var propertyNames = typeof(커뮤니티세계지도RoleDetailEntryResponse)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(propertyNames, name => name.Contains("User", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("Location", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("Contact", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("Inventory", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("Assignment", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("ResourceId", StringComparison.OrdinalIgnoreCase));
    }
}
