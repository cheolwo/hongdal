using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Components.Mart;

namespace Ssalddel.Tests.Architecture;

public sealed class OrdererMartOrderRequestWorkspaceCompositionTests
{
    [Fact]
    public void 주문자_마트주문요청_루트는_상태영역과_workflow만_조립한다()
    {
        var componentDirectory = FindComponentDirectory();
        var pagePath = Path.Combine(componentDirectory, "OrdererMartOrderRequestWorkspace.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 70);
        Assert.Contains("<OrdererMartOrderRequestAccessState", source);
        Assert.Contains("<OrdererMartOrderSelectionPrompt", source);
        Assert.Contains("<OrdererMartOrderProductPanel", source);
        Assert.Contains("<OrdererMartOrderAuthenticationPanel", source);
        Assert.Contains("<OrdererMartOrderRequestDetailPanel", source);
        Assert.Contains("<OrdererMartOrderRequestForm", source);
        Assert.DoesNotContain("<MudNumericField", source);
        Assert.DoesNotContain("<Ssalddel공통로그인Panel", source);
        Assert.DoesNotContain("mart-order-receipt", source);
        Assert.DoesNotContain("@foreach", source);
    }

    [Theory]
    [InlineData("OrdererMartOrderRequestAccessState.razor")]
    [InlineData("OrdererMartOrderRequestAccessState.razor.css")]
    [InlineData("OrdererMartOrderSelectionPrompt.razor")]
    [InlineData("OrdererMartOrderSelectionPrompt.razor.css")]
    [InlineData("OrdererMartOrderProductPanel.razor")]
    [InlineData("OrdererMartOrderProductPanel.razor.css")]
    [InlineData("OrdererMartOrderAuthenticationPanel.razor")]
    [InlineData("OrdererMartOrderAuthenticationPanel.razor.css")]
    [InlineData("OrdererMartOrderRequestDetailPanel.razor")]
    [InlineData("OrdererMartOrderRequestDetailPanel.razor.css")]
    [InlineData("OrdererMartOrderRequestForm.razor")]
    [InlineData("OrdererMartOrderRequestForm.razor.css")]
    [InlineData("OrdererMartOrderRequestPresentation.cs")]
    public void 마트주문요청_화면과_표현책임은_전용파일로_존재한다(string fileName)
    {
        var componentPath = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(componentPath), $"마트 주문 요청 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

    [Fact]
    public void 화면예상합계는_표시범위에서만_계산하고_서버합계와_구분한다()
    {
        var product = new 마트공개상품상세응답 { 판매가 = 12_500m };

        Assert.Equal(37_500m, OrdererMartOrderRequestPresentation.EstimatedTotal(product, 3));
        Assert.Equal(0m, OrdererMartOrderRequestPresentation.EstimatedTotal(product, -1));
        Assert.Equal(1_250_000m, OrdererMartOrderRequestPresentation.EstimatedTotal(product, 101));
    }

    [Fact]
    public void 마트주문요청_화면은_좁은폭에서_인증과_입력과_영수증을_단일열로_전환한다()
    {
        var componentDirectory = FindComponentDirectory();
        var authenticationCss = File.ReadAllText(Path.Combine(componentDirectory, "OrdererMartOrderAuthenticationPanel.razor.css"));
        var formCss = File.ReadAllText(Path.Combine(componentDirectory, "OrdererMartOrderRequestForm.razor.css"));
        var detailCss = File.ReadAllText(Path.Combine(componentDirectory, "OrdererMartOrderRequestDetailPanel.razor.css"));

        Assert.Contains("@media (max-width: 720px)", authenticationCss);
        Assert.Contains("grid-template-columns: 1fr", authenticationCss);
        Assert.Contains("@media (max-width: 720px)", formCss);
        Assert.Contains("grid-template-columns: 1fr", formCss);
        Assert.Contains("@media (max-width: 720px)", detailCss);
        Assert.Contains("grid-template-columns: 1fr", detailCss);
    }

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Mart");

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
