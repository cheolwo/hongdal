namespace Ssalddel.Tests.Architecture;

public sealed class CommunityGroupPurchaseRouteCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchasePage.razor", "/community/group-purchase", "<CommunityGroupPurchaseListScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseCreatePage.razor", "/community/group-purchase/new", "<CommunityGroupPurchaseCreateScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseDetailPage.razor", "/community/group-purchase/{CampaignId:guid}", "<CommunityGroupPurchaseDetailScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseParticipationPage.razor", "/community/group-purchase/{CampaignId:guid}/participation", "<CommunityGroupPurchaseParticipationScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseSuppliersPage.razor", "/community/group-purchase/{CampaignId:guid}/suppliers", "<CommunityGroupPurchaseSuppliersScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseNegotiationPage.razor", "/community/group-purchase/{CampaignId:guid}/negotiation", "<CommunityGroupPurchaseNegotiationScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseObjectionsPage.razor", "/community/group-purchase/{CampaignId:guid}/objections", "<CommunityGroupPurchaseObjectionsScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseResolutionPage.razor", "/community/group-purchase/{CampaignId:guid}/resolution", "<CommunityGroupPurchaseResolutionScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseSignaturePage.razor", "/community/group-purchase/{CampaignId:guid}/signature", "<CommunityGroupPurchaseSignatureScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseDeliveryOptionsPage.razor", "/community/group-purchase/{CampaignId:guid}/delivery-options", "<CommunityGroupPurchaseDeliveryOptionsScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseFulfillmentDraftPage.razor", "/community/group-purchase/{CampaignId:guid}/fulfillment-draft", "<CommunityGroupPurchaseFulfillmentDraftScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchase.razor", "/community/group-purchase", "<CommunityGroupPurchaseListScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseCreatePage.razor", "/community/group-purchase/new", "<CommunityGroupPurchaseCreateScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseDetailPage.razor", "/community/group-purchase/{CampaignId:guid}", "<CommunityGroupPurchaseDetailScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseParticipationPage.razor", "/community/group-purchase/{CampaignId:guid}/participation", "<CommunityGroupPurchaseParticipationScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseSuppliersPage.razor", "/community/group-purchase/{CampaignId:guid}/suppliers", "<CommunityGroupPurchaseSuppliersScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseNegotiationPage.razor", "/community/group-purchase/{CampaignId:guid}/negotiation", "<CommunityGroupPurchaseNegotiationScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseObjectionsPage.razor", "/community/group-purchase/{CampaignId:guid}/objections", "<CommunityGroupPurchaseObjectionsScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseResolutionPage.razor", "/community/group-purchase/{CampaignId:guid}/resolution", "<CommunityGroupPurchaseResolutionScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseSignaturePage.razor", "/community/group-purchase/{CampaignId:guid}/signature", "<CommunityGroupPurchaseSignatureScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseDeliveryOptionsPage.razor", "/community/group-purchase/{CampaignId:guid}/delivery-options", "<CommunityGroupPurchaseDeliveryOptionsScreen")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseFulfillmentDraftPage.razor", "/community/group-purchase/{CampaignId:guid}/fulfillment-draft", "<CommunityGroupPurchaseFulfillmentDraftScreen")]
    public void Web과모바일route는_같은사용자목표Screen하나를조립한다(
        string project,
        string relativePath,
        string route,
        string screenMarkup)
    {
        var pagePath = Path.Combine(
            FindRepositoryRoot(),
            project,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var source = File.ReadAllText(pagePath);
        var routeDirectives = File.ReadLines(pagePath)
            .Where(line => line.TrimStart().StartsWith("@page ", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(routeDirectives);
        Assert.Equal($"@page \"{route}\"", routeDirectives[0].Trim());
        Assert.Contains(screenMarkup, source);
        Assert.DoesNotContain("<CommunityGroupPurchaseWorkspace", source);
    }

    [Theory]
    [InlineData("CommunityGroupPurchaseListScreen.razor", "CommunityGroupPurchaseSurfaceKind.List")]
    [InlineData("CommunityGroupPurchaseCreateScreen.razor", "CommunityGroupPurchaseSurfaceKind.Create")]
    [InlineData("CommunityGroupPurchaseDetailScreen.razor", "CommunityGroupPurchaseSurfaceKind.Overview")]
    [InlineData("CommunityGroupPurchaseParticipationScreen.razor", "CommunityGroupPurchaseSurfaceKind.Participation")]
    [InlineData("CommunityGroupPurchaseSuppliersScreen.razor", "CommunityGroupPurchaseSurfaceKind.Suppliers")]
    [InlineData("CommunityGroupPurchaseNegotiationScreen.razor", "CommunityGroupPurchaseSurfaceKind.Negotiation")]
    [InlineData("CommunityGroupPurchaseObjectionsScreen.razor", "CommunityGroupPurchaseSurfaceKind.Objections")]
    [InlineData("CommunityGroupPurchaseResolutionScreen.razor", "CommunityGroupPurchaseSurfaceKind.Resolution")]
    [InlineData("CommunityGroupPurchaseSignatureScreen.razor", "CommunityGroupPurchaseSurfaceKind.Signature")]
    [InlineData("CommunityGroupPurchaseDeliveryOptionsScreen.razor", "CommunityGroupPurchaseSurfaceKind.DeliveryOptions")]
    [InlineData("CommunityGroupPurchaseFulfillmentDraftScreen.razor", "CommunityGroupPurchaseSurfaceKind.FulfillmentDraft")]
    public void 공용Screen은_route없이_한표면을고정한다(string fileName, string surfaceMarker)
    {
        var source = File.ReadAllText(Path.Combine(FindComponentDirectory(), fileName));

        Assert.DoesNotContain("@page ", source);
        Assert.Contains("<CommunityGroupPurchaseWorkspace", source);
        Assert.Contains(surfaceMarker, source);
        Assert.DoesNotContain("Ssalddel.WebApp", source);
        Assert.DoesNotContain("SsalddelApp", source);
    }

    [Fact]
    public void 과거CampaignQuery는_Web과모바일모두_stable상세route로호환이동한다()
    {
        var web = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.WebApp",
            "Pages",
            "CommunityGroupPurchasePage.razor"));
        var app = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "Components",
            "Pages",
            "CommunityGroupPurchase.razor"));

        foreach (var source in new[] { web, app })
        {
            Assert.Contains("CommunityPageRoutes.GroupPurchaseDetailFor", source);
            Assert.Contains("replace: true", source);
        }
    }

    [Fact]
    public void 단계다이어그램은_local선택상태가아닌_stableRoute로이동한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindComponentDirectory(),
            "CommunityGroupPurchaseProcess.razor"));

        Assert.Contains("href=\"@StageHref(stage.Code)\"", source);
        Assert.Contains("CommunityPageRoutes.GroupPurchaseParticipationFor", source);
        Assert.Contains("CommunityPageRoutes.GroupPurchaseObjectionsFor", source);
        Assert.Contains("CommunityPageRoutes.GroupPurchaseSignatureFor", source);
        Assert.DoesNotContain("StageSelected", source);
    }

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community");

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
