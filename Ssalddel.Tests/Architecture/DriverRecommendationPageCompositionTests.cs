namespace Ssalddel.Tests.Architecture;

public sealed class DriverRecommendationPageCompositionTests
{
    [Fact]
    public void 추천_목록_상세_판단은_서로_다른_route와_책임을_가진다()
    {
        var root = FindRepositoryRoot();
        var list = File.ReadAllText(
            Path.Combine(root, "Ssalddel.WebApp", "Pages", "DriverRecommendations.razor"));
        var detail = File.ReadAllText(
            Path.Combine(root, "Ssalddel.WebApp", "Pages", "DriverRecommendationMapPage.razor"));
        var decision = File.ReadAllText(
            Path.Combine(root, "Ssalddel.WebApp", "Pages", "DriverRecommendationDecisionPage.razor"));
        var decisionPanel = File.ReadAllText(
            Path.Combine(
                root,
                "Ssalddel.WebApp",
                "Pages",
                "DriverRecommendationComponents",
                "DriverRecommendationDecisionPanel.razor"));
        var decisionViewModel = File.ReadAllText(
            Path.Combine(
                root,
                "Ssalddel.WebApp",
                "ViewModels",
                "DriverRecommendationDecisionPageViewModel.cs"));

        Assert.Contains("@page \"/driver/recommendations\"", list);
        Assert.DoesNotContain("추천Service.수락Async", list);
        Assert.DoesNotContain("추천Service.거절Async", list);

        Assert.Contains("@page \"/driver/recommendations/{RequestId}\"", detail);
        Assert.Contains("DriverRecommendationDetailPanel", detail);
        Assert.DoesNotContain("추천Service.수락Async", detail);
        Assert.DoesNotContain("추천Service.거절Async", detail);

        Assert.Contains("@page \"/driver/dispatch-decisions/{RequestId}\"", decision);
        Assert.Contains("MvvmComponentBase<DriverRecommendationDecisionPageViewModel>", decision);
        Assert.Contains("DriverRecommendationDecisionPanel", decision);
        Assert.DoesNotContain("I기사추천수신Service", decision);
        Assert.DoesNotContain("수락Async", decision);
        Assert.DoesNotContain("거절Async", decision);
        Assert.DoesNotContain("자동 거절을 서버에 전송", decision);

        Assert.Contains("Model.AcceptAsync", decisionPanel);
        Assert.Contains("Model.RejectAsync", decisionPanel);
        Assert.Contains("recommendationService.수락Async", decisionViewModel);
        Assert.Contains("recommendationService.거절Async", decisionViewModel);
        Assert.DoesNotContain("_operations.Reject", decisionViewModel[
            decisionViewModel.IndexOf("RunCountdownAsync", StringComparison.Ordinal)..]);
    }

    [Fact]
    public void 화주_레거시_결제상태_route는_결제_Command를_직접_실행하지_않는다()
    {
        var root = FindRepositoryRoot();
        var page = File.ReadAllText(
            Path.Combine(root, "Ssalddel.WebApp", "Pages", "ShipperSettlementStatusPage.razor"));

        Assert.Contains("@page \"/shipper/request/payment-status\"", page);
        Assert.Contains("ShipperRoutes.RequestPaymentFor", page);
        Assert.DoesNotContain("PreparePaymentAsync", page);
        Assert.DoesNotContain("ConfirmPaymentAsync", page);
        Assert.DoesNotContain("ApprovePostpayAsync", page);
        Assert.DoesNotContain("MarkOfflinePaidAsync", page);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Ssalddel.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Ssalddel repository root를 찾을 수 없습니다.");
    }
}
