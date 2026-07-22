using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Components.Community;

namespace Ssalddel.Tests.Architecture;

public sealed class CommunityGroupPurchaseWorkspaceCompositionTests
{
    [Fact]
    public void 공동구매_조정자는_routeScreen이선택한표면하나만조립한다()
    {
        var componentDirectory = FindComponentDirectory();
        var workspacePath = Path.Combine(componentDirectory, "CommunityGroupPurchaseWorkspace.razor");
        var source = File.ReadAllText(workspacePath);

        Assert.True(File.ReadLines(workspacePath).Count() <= 180);
        Assert.Contains("<CommunityGroupPurchaseHeader", source);
        Assert.Contains("<CommunityGroupPurchaseProposalForm", source);
        Assert.Contains("<CommunityGroupPurchaseCampaignList", source);
        Assert.Contains("<CommunityGroupPurchaseDetailState", source);
        Assert.Contains("<CommunityGroupPurchaseSummary", source);
        Assert.Contains("<CommunityGroupPurchaseProcess", source);
        Assert.Contains("<CommunityGroupPurchaseRouteNavigation", source);
        Assert.Contains("Surface == CommunityGroupPurchaseSurfaceKind.List", source);
        Assert.Contains("Surface == CommunityGroupPurchaseSurfaceKind.Create", source);
        Assert.Contains("Surface == CommunityGroupPurchaseSurfaceKind.Participation", source);
        Assert.Contains("Surface == CommunityGroupPurchaseSurfaceKind.Objections", source);
        Assert.Contains("Surface == CommunityGroupPurchaseSurfaceKind.Resolution", source);
        Assert.Contains("Surface == CommunityGroupPurchaseSurfaceKind.Signature", source);
        Assert.Contains("<CommunityGroupPurchaseObjectionPanel", source);
        Assert.Contains("<CommunityGroupPurchaseNegotiationTimeline", source);
        Assert.DoesNotContain("group-purchase-layout", source);
        Assert.DoesNotContain("group-purchase-action-grid", source);
        Assert.DoesNotContain("<MudTextField", source);
        Assert.DoesNotContain("<MudNumericField", source);
        Assert.DoesNotContain("<MudSelect", source);
        Assert.DoesNotContain("@foreach", source);
        Assert.DoesNotContain("@inject", source);
        Assert.DoesNotContain("@code", source);
    }

    [Theory]
    [InlineData("CommunityGroupPurchaseHeader.razor")]
    [InlineData("CommunityGroupPurchaseHeader.razor.css")]
    [InlineData("CommunityGroupPurchaseProposalForm.razor")]
    [InlineData("CommunityGroupPurchaseProposalForm.razor.css")]
    [InlineData("CommunityGroupPurchaseCampaignList.razor")]
    [InlineData("CommunityGroupPurchaseCampaignList.razor.css")]
    [InlineData("CommunityGroupPurchaseDetailState.razor")]
    [InlineData("CommunityGroupPurchaseDetailState.razor.css")]
    [InlineData("CommunityGroupPurchaseSummary.razor")]
    [InlineData("CommunityGroupPurchaseSummary.razor.css")]
    [InlineData("CommunityGroupPurchaseProcess.razor")]
    [InlineData("CommunityGroupPurchaseProcess.razor.css")]
    [InlineData("CommunityGroupPurchaseStageActionPanel.razor")]
    [InlineData("CommunityGroupPurchaseStageActionPanel.razor.css")]
    [InlineData("CommunityGroupPurchaseRecruitmentStage.razor")]
    [InlineData("CommunityGroupPurchaseResolutionStage.razor")]
    [InlineData("CommunityGroupPurchaseResolutionStage.razor.css")]
    [InlineData("CommunityGroupPurchaseSignatureStage.razor")]
    [InlineData("CommunityGroupPurchaseExecutionStage.razor")]
    [InlineData("CommunityGroupPurchaseExecutionStage.razor.css")]
    [InlineData("CommunityGroupPurchaseProposalStage.razor")]
    [InlineData("CommunityGroupPurchaseObjectionPanel.razor")]
    [InlineData("CommunityGroupPurchaseObjectionPanel.razor.css")]
    [InlineData("CommunityGroupPurchaseModels.cs")]
    [InlineData("CommunityGroupPurchasePresentation.cs")]
    [InlineData("CommunityGroupPurchaseWorkspace.razor.cs")]
    [InlineData("CommunityGroupPurchaseSurfaceKind.cs")]
    [InlineData("CommunityGroupPurchaseRouteNavigation.razor")]
    [InlineData("CommunityGroupPurchaseRouteNavigation.razor.css")]
    [InlineData("CommunityGroupPurchaseListScreen.razor")]
    [InlineData("CommunityGroupPurchaseCreateScreen.razor")]
    [InlineData("CommunityGroupPurchaseDetailScreen.razor")]
    [InlineData("CommunityGroupPurchaseParticipationScreen.razor")]
    [InlineData("CommunityGroupPurchaseSuppliersScreen.razor")]
    [InlineData("CommunityGroupPurchaseNegotiationScreen.razor")]
    [InlineData("CommunityGroupPurchaseObjectionsScreen.razor")]
    [InlineData("CommunityGroupPurchaseResolutionScreen.razor")]
    [InlineData("CommunityGroupPurchaseSignatureScreen.razor")]
    [InlineData("CommunityGroupPurchaseDeliveryOptionsScreen.razor")]
    [InlineData("CommunityGroupPurchaseFulfillmentDraftScreen.razor")]
    public void 공동구매_화면과표현과조정책임은_전용파일로존재한다(string fileName)
    {
        var path = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(path), $"공동구매 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(path));
    }

    [Fact]
    public void 단계동작_루트는_선택한단계의전용화면만조립한다()
    {
        var stagePath = Path.Combine(
            FindComponentDirectory(),
            "CommunityGroupPurchaseStageActionPanel.razor");
        var source = File.ReadAllText(stagePath);

        Assert.True(File.ReadLines(stagePath).Count() <= 85);
        Assert.Contains("<CommunityGroupPurchaseRecruitmentStage", source);
        Assert.Contains("<CommunityGroupPurchaseResolutionStage", source);
        Assert.Contains("<CommunityGroupPurchaseSignatureStage", source);
        Assert.Contains("<CommunityGroupPurchaseExecutionStage", source);
        Assert.Contains("<CommunityGroupPurchaseProposalStage", source);
        Assert.DoesNotContain("<MudTextField", source);
        Assert.DoesNotContain("<MudNumericField", source);
        Assert.DoesNotContain("<MudSelect", source);
        Assert.DoesNotContain("@foreach", source);
    }

    [Fact]
    public void 공동구매_단계표현은_서버응답상태만반영한다()
    {
        var openCampaign = new CommunityVoteResponse
        {
            Status = CommunityVoteStatusCodes.Open
        };
        var closedCampaign = new CommunityVoteResponse
        {
            Status = CommunityVoteStatusCodes.Closed
        };
        var signedCampaign = new CommunityVoteResponse
        {
            Status = CommunityVoteStatusCodes.Closed,
            ResolutionDocument = new CommunityVoteResolutionDocumentResponse
            {
                Status = CommunityVoteResolutionStatusCodes.Signed
            }
        };

        Assert.Equal(
            "active",
            CommunityGroupPurchasePresentation.StageState(
                CommunityGroupPurchasePresentation.StageRecruitment,
                openCampaign));
        Assert.Equal(
            "waiting",
            CommunityGroupPurchasePresentation.StageState(
                CommunityGroupPurchasePresentation.StageResolution,
                openCampaign));
        Assert.Equal(
            "active",
            CommunityGroupPurchasePresentation.StageState(
                CommunityGroupPurchasePresentation.StageResolution,
                closedCampaign));
        Assert.Equal(
            "complete",
            CommunityGroupPurchasePresentation.StageState(
                CommunityGroupPurchasePresentation.StageSignature,
                signedCampaign));
        Assert.Equal(
            "active",
            CommunityGroupPurchasePresentation.StageState(
                CommunityGroupPurchasePresentation.StageExecution,
                signedCampaign));
    }

    [Fact]
    public void 이의제기는_선택단계접두어만표시하고본문에서는접두어를숨긴다()
    {
        var comments = new[]
        {
            new PlatformCommunityPostCommentResponse { Body = "[이의제기:proposal] 제안 근거를 확인해 주세요." },
            new PlatformCommunityPostCommentResponse { Body = "[이의제기:recruitment] 수량 단위를 확인해 주세요." },
            new PlatformCommunityPostCommentResponse { Body = "일반 의견" }
        };

        var proposalObjections = CommunityGroupPurchasePresentation.StageObjections(
            comments,
            CommunityGroupPurchasePresentation.StageProposal).ToArray();

        Assert.Single(proposalObjections);
        Assert.Equal(2, CommunityGroupPurchasePresentation.ObjectionCount(comments));
        Assert.Equal(
            "제안 근거를 확인해 주세요.",
            CommunityGroupPurchasePresentation.StripObjectionPrefix(proposalObjections[0].Body));
    }

    [Fact]
    public void 저장과선택은_성공한정확한CampaignId를다시조회한다()
    {
        var coordinator = File.ReadAllText(Path.Combine(
            FindComponentDirectory(),
            "CommunityGroupPurchaseWorkspace.razor.cs"));

        Assert.Contains("await 상세ViewModel.조회Async(campaignId)", coordinator);
        Assert.Contains("CampaignCreated.InvokeAsync(vote.Id)", coordinator);
        Assert.Contains("var campaignId = SelectedCampaign.Id", coordinator);
        Assert.Contains("목록ViewModel.공동구매갱신(refreshed)", coordinator);
        Assert.Contains("I공동구매업무Service GroupPurchaseService", coordinator);
        Assert.Contains("EnsureAuthenticatedCommand(\"수요 참여\")", coordinator);
        Assert.Contains("EnsureAuthenticatedCommand(\"모집 마감\")", coordinator);
        Assert.Contains("EnsureAuthenticatedCommand(\"전자서명\")", coordinator);
    }

    [Theory]
    [InlineData("CommunityGroupPurchaseHeader.razor.css")]
    [InlineData("CommunityGroupPurchaseProposalForm.razor.css")]
    [InlineData("CommunityGroupPurchaseCampaignList.razor.css")]
    [InlineData("CommunityGroupPurchaseDetailState.razor.css")]
    [InlineData("CommunityGroupPurchaseProcess.razor.css")]
    [InlineData("CommunityGroupPurchaseStageActionPanel.razor.css")]
    [InlineData("CommunityGroupPurchaseObjectionPanel.razor.css")]
    public void 공동구매_상호작용영역은_좁은폭에서터치크기를보장한다(string fileName)
    {
        var css = File.ReadAllText(Path.Combine(FindComponentDirectory(), fileName));

        Assert.Contains("@media (max-width: 720px)", css);
        Assert.Contains("min-height: 44px", css);
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
