namespace Ssalddel.Tests.Architecture;

public sealed class ShipperRequestDetailCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperRequestDetailPreview.razor", "/shipper/request/{RequestId}", "<ShipperRequestSummaryScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperRequestTimelinePage.razor", "/shipper/request/{RequestId}/timeline", "<ShipperRequestTimelineScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperRequestPaymentPage.razor", "/shipper/request/{RequestId}/payment", "<ShipperRequestPaymentScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperRequestProofsPage.razor", "/shipper/request/{RequestId}/proofs", "<ShipperRequestProofsScreen")]
    [InlineData("SsalddelApp", "Components/Pages/ShipperRequestDetail.razor", "/shipper/request/{RequestId}", "<ShipperRequestSummaryScreen")]
    [InlineData("SsalddelApp", "Components/Pages/ShipperRequestTimelinePage.razor", "/shipper/request/{RequestId}/timeline", "<ShipperRequestTimelineScreen")]
    [InlineData("SsalddelApp", "Components/Pages/ShipperRequestPaymentPage.razor", "/shipper/request/{RequestId}/payment", "<ShipperRequestPaymentScreen")]
    [InlineData("SsalddelApp", "Components/Pages/ShipperRequestProofsPage.razor", "/shipper/request/{RequestId}/proofs", "<ShipperRequestProofsScreen")]
    public void Web과모바일상세Route는_같은책임별공용Screen을조립한다(
        string project,
        string relativePath,
        string route,
        string screenMarkup)
    {
        var source = File.ReadAllText(ProjectFile(project, relativePath));

        Assert.Contains($"@page \"{route}\"", source);
        Assert.Contains(screenMarkup, source);
        Assert.Contains("ShipperRequestDetailPageViewModel", source);
        Assert.DoesNotContain("IShipperOperationsService", source);
        Assert.DoesNotContain("화주결제정산Service", source);
        Assert.DoesNotContain("FakeShipperPaymentService", source);
        Assert.DoesNotContain("BuildTimeline", source);
        Assert.DoesNotContain(File.ReadLines(ProjectFile(project, relativePath)), line => line.Trim().Equals("try", StringComparison.Ordinal));
    }

    [Fact]
    public void 공용상세Screen은_route와플랫폼서비스를소유하지않는다()
    {
        foreach (var fileName in new[]
                 {
                     "ShipperRequestSummaryScreen.razor",
                     "ShipperRequestTimelineScreen.razor",
                     "ShipperRequestPaymentScreen.razor",
                     "ShipperRequestProofsScreen.razor"
                 })
        {
            var source = File.ReadAllText(ComponentFile(fileName));
            Assert.DoesNotContain("@page ", source);
            Assert.DoesNotContain("Ssalddel.WebApp", source);
            Assert.DoesNotContain("SsalddelApp", source);
            Assert.DoesNotContain("IShipperOperationsService", source);
            Assert.DoesNotContain("화주결제정산Service", source);
            Assert.DoesNotContain("FakeShipperPaymentService", source);
        }
    }

    [Fact]
    public void 결제Command와결제Panel은_명시적인PaymentRoute에만연결한다()
    {
        var appSummary = File.ReadAllText(ProjectFile("SsalddelApp", "Components/Pages/ShipperRequestDetail.razor"));
        var appTimeline = File.ReadAllText(ProjectFile("SsalddelApp", "Components/Pages/ShipperRequestTimelinePage.razor"));
        var appPayment = File.ReadAllText(ProjectFile("SsalddelApp", "Components/Pages/ShipperRequestPaymentPage.razor"));
        var appProofs = File.ReadAllText(ProjectFile("SsalddelApp", "Components/Pages/ShipperRequestProofsPage.razor"));
        var paymentScreen = File.ReadAllText(ComponentFile("ShipperRequestPaymentScreen.razor"));

        Assert.DoesNotContain("PaymentRequested=", appSummary);
        Assert.DoesNotContain("PaymentRequested=", appTimeline);
        Assert.DoesNotContain("PaymentRequested=", appProofs);
        Assert.Contains("PaymentRequested=\"CompletePaymentAsync\"", appPayment);
        Assert.Contains("<SsalddelPaymentCheckoutPanel", paymentScreen);
        Assert.DoesNotContain("<SsalddelPaymentCheckoutPanel", File.ReadAllText(ComponentFile("ShipperRequestSummaryScreen.razor")));
        Assert.DoesNotContain("<SsalddelPaymentCheckoutPanel", File.ReadAllText(ComponentFile("ShipperRequestProofsScreen.razor")));
    }

    [Fact]
    public void Web과모바일은_같은서버원장Endpoint를_의뢰Id로조회한다()
    {
        var web = File.ReadAllText(ProjectFile("Ssalddel.WebApp", "Services/화주결제정산Service.cs"));
        var app = File.ReadAllText(ProjectFile("SsalddelApp", "Services/ServerBackedShipperOperationsService.cs"));

        const string endpoint = "api/v1/shipper/requests/{Uri.EscapeDataString(requestId.Trim())}";
        Assert.Contains(endpoint, web);
        Assert.Contains(endpoint, app);
    }

    [Fact]
    public void 모바일서버Mapper는_결제와증빙원본필드를_공용표시에전달한다()
    {
        var mapper = File.ReadAllText(ProjectFile("SsalddelApp", "Services/ServerBackedShipperOperationsService.cs"));

        Assert.Contains("인수증번호 = source.인수증번호", mapper);
        Assert.Contains("증빙방식 = source.증빙방식", mapper);
        Assert.Contains("정산시점 = source.정산시점", mapper);
        Assert.Contains("현장수금확인일시 = source.현장수금확인일시", mapper);
    }

    private static string ComponentFile(string fileName)
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Transport",
            fileName);

    private static string ProjectFile(string project, string relativePath)
        => Path.Combine(
            FindRepositoryRoot(),
            project,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

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
