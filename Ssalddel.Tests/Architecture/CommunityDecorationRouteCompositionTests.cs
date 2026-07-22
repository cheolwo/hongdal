using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Architecture;

public sealed class CommunityDecorationRouteCompositionTests
{
    [Theory]
    [InlineData("CommunityDecorationStoreScreen.razor", "<main class=\"decoration-market\"")]
    [InlineData("CommunityDecorationProductScreen.razor", "<main class=\"decoration-detail")]
    [InlineData("CommunityDecorationCheckoutScreen.razor", "<main class=\"decoration-checkout")]
    public void 꾸미기목표는_공용Screen으로분리된다(string fileName, string expectedMarkup)
    {
        var source = Read("Ssalddel.Ui.Common", "Areas", "App", "Components", "Community", fileName);

        Assert.Contains(expectedMarkup, source);
        Assert.DoesNotContain("@page", source);
        Assert.DoesNotContain("SsalddelApp", source);
        Assert.DoesNotContain("ShipperRoutes", source);
    }

    [Theory]
    [InlineData("SsalddelApp", "CommunityDecorationStorePage.razor", "/community/decorations", "<CommunityDecorationStoreScreen")]
    [InlineData("Ssalddel.WebApp", "CommunityDecorationStorePage.razor", "/community/decorations", "<CommunityDecorationStoreScreen")]
    [InlineData("SsalddelApp", "CommunityDecorationDetailPage.razor", "/community/decorations/products/{ProductKey}", "<CommunityDecorationProductScreen")]
    [InlineData("Ssalddel.WebApp", "CommunityDecorationProductPage.razor", "/community/decorations/products/{ProductKey}", "<CommunityDecorationProductScreen")]
    [InlineData("SsalddelApp", "CommunityDecorationCheckoutPage.razor", "/community/decorations/checkout/{ProductKey}", "<CommunityDecorationCheckoutScreen")]
    [InlineData("Ssalddel.WebApp", "CommunityDecorationCheckoutPage.razor", "/community/decorations/checkout/{ProductKey}", "<CommunityDecorationCheckoutScreen")]
    public void Web과모바일은_같은canonicalRoute와Screen의미를사용한다(
        string project,
        string fileName,
        string route,
        string screen)
    {
        var source = project == "SsalddelApp"
            ? Read(project, "Components", "Pages", fileName)
            : Read(project, "Pages", fileName);

        Assert.Contains($"@page \"{route}\"", source);
        Assert.Contains(screen, source);
    }

    [Theory]
    [InlineData("SsalddelApp", "CommunityDecorationLegacyProductPage.razor", "CommunityPageRoutes.DecorationProductFor")]
    [InlineData("Ssalddel.WebApp", "CommunityDecorationLegacyProductPage.razor", "CommunityPageRoutes.DecorationProductFor")]
    [InlineData("SsalddelApp", "CommunityDecorationLegacyCheckoutPage.razor", "CommunityPageRoutes.DecorationCheckoutFor")]
    [InlineData("Ssalddel.WebApp", "CommunityDecorationLegacyCheckoutPage.razor", "CommunityPageRoutes.DecorationCheckoutFor")]
    public void 기존주소는_canonicalRoute로교체이동한다(string project, string fileName, string builder)
    {
        var segments = project == "SsalddelApp"
            ? new[] { project, "Components", "Pages", fileName }
            : new[] { project, "Pages", fileName };
        var source = Read(segments);

        Assert.Contains(builder, source);
        Assert.Contains("replace: true", source);
        Assert.DoesNotContain("Screen", source);
    }

    [Fact]
    public void FakePGCommand는_checkoutScreen과플랫폼adapter에만있다()
    {
        var store = Read("Ssalddel.Ui.Common", "Areas", "App", "Components", "Community", "CommunityDecorationStoreScreen.razor");
        var product = Read("Ssalddel.Ui.Common", "Areas", "App", "Components", "Community", "CommunityDecorationProductScreen.razor");
        var checkout = Read("Ssalddel.Ui.Common", "Areas", "App", "Components", "Community", "CommunityDecorationCheckoutScreen.razor");

        Assert.DoesNotContain("ICommunityDecorationPurchaseClient", store);
        Assert.DoesNotContain("ICommunityDecorationPurchaseClient", product);
        Assert.Contains("ICommunityDecorationPurchaseClient", checkout);
        Assert.Contains("PurchaseClient.ConfirmAsync", checkout);
        Assert.Contains("!agreed || isProcessing", checkout);
        Assert.DoesNotContain("꾸미기보유권동기화Service", checkout);
        Assert.DoesNotContain("Task.Delay", checkout);
    }

    [Theory]
    [InlineData("CommunityDecorationStoreScreen.razor.css")]
    [InlineData("CommunityDecorationProductScreen.razor.css")]
    [InlineData("CommunityDecorationCheckoutScreen.razor.css")]
    public void 공용꾸미기Screen은_모바일단일열과48px행동을가진다(string fileName)
    {
        var css = Read("Ssalddel.Ui.Common", "Areas", "App", "Components", "Community", fileName);

        Assert.Contains("@media (max-width:", css);
        Assert.Contains("min-height: 48px", css);
        Assert.Contains("grid-template-columns: 1fr", css);
    }

    [Fact]
    public void 공용routeBuilder는_상품과checkout경계를구분한다()
    {
        Assert.Equal(
            "/community/decorations/products/theme-pack",
            CommunityPageRoutes.DecorationProductFor("theme-pack"));
        Assert.Equal(
            "/community/decorations/checkout/theme-pack",
            CommunityPageRoutes.DecorationCheckoutFor("theme-pack"));
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine(new[] { FindRepositoryRoot() }.Concat(segments).ToArray()));

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
