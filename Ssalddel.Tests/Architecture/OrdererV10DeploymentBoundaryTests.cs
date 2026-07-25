using System.Text.Json;
using Ssalddel.Controllers;
using Ssalddel.Controllers.Orderer;

namespace Ssalddel.Tests.Architecture;

public sealed class OrdererV10DeploymentBoundaryTests
{
    [Fact]
    public void 공동구매수요Controller는_주문자공통경계를사용한다()
    {
        Assert.True(typeof(OrdererControllerBase).IsAssignableFrom(
            typeof(공동구매자동집단화Controller)));
    }

    [Fact]
    public void Azure기본구성은_후속업무기능을열지않는다()
    {
        var compose = Read("deploy", "azure-vm/compose.yaml");

        Assert.Contains("SsalddelExecution__Mode: Simulation", compose);
        Assert.Contains("VersionFeatureFlags__CommunityTrustWorkflow: \"true\"", compose);
        Assert.Contains("VersionFeatureFlags__GroupPurchasePracticeWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__GroupPurchaseDemandWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__CustomsAndTradeDataWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__DomesticTransportWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__WarehouseFulfillmentWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__SalesChannelFulfillmentWorkflow: \"false\"", compose);
    }

    [Fact]
    public void 기본앱설정은_0_0커뮤니티만연다()
    {
        using var document = JsonDocument.Parse(Read("Ssalddel", "appsettings.json"));
        var flags = document.RootElement.GetProperty("VersionFeatureFlags");

        Assert.True(flags.GetProperty("CommunityTrustWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("GroupPurchasePracticeWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("GroupPurchaseDemandWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("CustomsAndTradeDataWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("DomesticTransportWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("WarehouseFulfillmentWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("SalesChannelFulfillmentWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("FoodDeliveryWorkflow").GetBoolean());
        Assert.False(flags.GetProperty("SsalddelMartWorkflow").GetBoolean());
    }

    [Fact]
    public void 주문자1_0프로필은_비구속수요만추가로연다()
    {
        var compose = Read("deploy", "azure-vm/compose.orderer-v10.override.yaml");

        Assert.Contains("SsalddelExecution__Mode: Simulation", compose);
        Assert.Contains("VersionFeatureFlags__CommunityTrustWorkflow: \"true\"", compose);
        Assert.Contains("VersionFeatureFlags__GroupPurchasePracticeWorkflow: \"true\"", compose);
        Assert.Contains("VersionFeatureFlags__GroupPurchaseDemandWorkflow: \"true\"", compose);
        Assert.Contains("VersionFeatureFlags__CustomsAndTradeDataWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__DomesticTransportWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__WarehouseFulfillmentWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__SalesChannelFulfillmentWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__FoodDeliveryWorkflow: \"false\"", compose);
        Assert.Contains("VersionFeatureFlags__SsalddelMartWorkflow: \"false\"", compose);
        Assert.Contains("SalesChannelOrderSync__Enabled: \"false\"", compose);
        Assert.Contains("WorkRelationshipSnapshots__Enabled: \"false\"", compose);
    }

    [Fact]
    public void 주문자배포스크립트는_공통배포와동일한프로필롤백을사용한다()
    {
        var v10 = Read("deploy", "azure-vm/deploy-orderer-v10.sh");
        var v15 = Read("deploy", "azure-vm/deploy-orderer-v15.sh");
        var shared = Read("deploy", "azure-vm/deploy-preview-profile.sh");

        Assert.Contains("deploy-preview-profile.sh", v10);
        Assert.Contains("orderer-v10", v10);
        Assert.Contains("deploy-preview-profile.sh", v15);
        Assert.Contains("orderer-v15", v15);
        Assert.Contains("orderer-v10|orderer-v15", shared);
        Assert.Contains("-f \"$override_file\"", shared);
        Assert.Contains("rollback()", shared);
    }

    private static string Read(string project, string relativePath)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
