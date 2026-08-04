namespace Ssalddel.Tests.Architecture;

public sealed class PlatformCommunityPostDetailCompositionTests
{
    [Fact]
    public void 첨부이미지는_본문뒤이면서_함께하는일보다_먼저_조립한다()
    {
        var directory = FindComponentDirectory();
        var detail = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostDetail.razor"));
        var galleryIndex = detail.IndexOf("<PlatformCommunityPostAttachmentGallery", StringComparison.Ordinal);
        var participationIndex = detail.IndexOf("<PlatformCommunityPostParticipationPanel", StringComparison.Ordinal);
        var conversation = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostConversationPanel.razor"));

        Assert.True(galleryIndex >= 0);
        Assert.True(participationIndex > galleryIndex);
        Assert.DoesNotContain("Post.Attachments", conversation);
        Assert.True(File.Exists(Path.Combine(directory, "PlatformCommunityPostAttachmentGallery.razor")));
    }

    [Fact]
    public void 조회_추천_댓글_작성일자는_상세제목의_우측정보로_조립한다()
    {
        var directory = FindComponentDirectory();
        var detail = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostDetail.razor"));
        var headerMeta = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostHeaderMeta.razor"));
        var conversation = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostConversationPanel.razor"));

        Assert.Contains("platform-community-post-header__aside", detail);
        Assert.Contains("<PlatformCommunityPostHeaderMeta", detail);
        Assert.Contains("Post.ViewCount", headerMeta);
        Assert.Contains("Post.RecommendationCount", headerMeta);
        Assert.Contains("Post.CommentCount", headerMeta);
        Assert.Contains("Post.CreatedAtUtc", headerMeta);
        Assert.DoesNotContain("Post.ViewCount", conversation);
        Assert.DoesNotContain("FormatDate(Post.CreatedAtUtc)", conversation);
    }

    [Fact]
    public void 첨부이미지는_한장씩_세로로_배치하고_클릭할때만_댓글입력을_연다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var directory = FindComponentDirectory();
        var gallery = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostAttachmentGallery.razor"));
        var style = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "Ssalddel.Ui.Common",
            "wwwroot",
            "css",
            "platform-community-home.css"));

        Assert.Contains("ToggleAttachmentComment", gallery);
        Assert.Contains("aria-expanded", gallery);
        Assert.Contains("@if (isCommentPanelExpanded)", gallery);
        Assert.Contains("platform-community-attachment-comment-panel", gallery);
        Assert.Contains(".platform-community-attachments", style);
        Assert.Contains("flex-direction: column", style);
        Assert.DoesNotContain("grid-template-columns: repeat(auto-fill", style);
    }

    [Fact]
    public void 마음모으기는_공동구매정책과_작성자선택값을_함께_확인한다()
    {
        var directory = FindComponentDirectory();
        var participation = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostParticipationPanel.razor"));
        var composer = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostComposer.razor"));
        var option = File.ReadAllText(Path.Combine(directory, "PlatformCommunityComposerInterestGatheringOption.razor"));

        Assert.Contains("CommunityPostInterestGatheringPolicy.IsEnabledFor", participation);
        Assert.Contains("PlatformCommunityComposerInterestGatheringOption", composer);
        Assert.Contains("IsInterestGatheringEnabled", option);
    }

    [Fact]
    public void 지도출발글은_구조화된공개근거를_참여보다먼저표시한다()
    {
        var directory = FindComponentDirectory();
        var detail = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostDetail.razor"));
        var evidencePanel = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostSourceEvidencePanel.razor"));
        var evidenceIndex = detail.IndexOf("<PlatformCommunityPostSourceEvidencePanel", StringComparison.Ordinal);
        var participationIndex = detail.IndexOf("<PlatformCommunityPostParticipationPanel", StringComparison.Ordinal);

        Assert.True(evidenceIndex >= 0);
        Assert.True(participationIndex > evidenceIndex);
        Assert.Contains("Post.SourceEvidence", evidencePanel);
        Assert.Contains("ObservationStableId", evidencePanel);
        Assert.Contains("SnapshotRevision", evidencePanel);
        Assert.Contains("SourceVersion", evidencePanel);
        Assert.Contains("evidence.MapHref", evidencePanel);
        Assert.Contains("지도 근거 다시 보기", evidencePanel);
        Assert.Contains("연결 자료 상세", evidencePanel);
        Assert.Contains("공식 원문 열기", evidencePanel);
        Assert.Contains("참여·주문·계약·배차가 생성되지 않습니다", evidencePanel);
        Assert.Contains("rel=\"noopener noreferrer\"", evidencePanel);
    }

    [Fact]
    public void 관심모집시작은_두필수확인을_사용자에게받고_그값을서버요청에전달한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var directory = FindComponentDirectory();
        var panel = File.ReadAllText(Path.Combine(directory, "PlatformCommunityPostParticipationPanel.razor"));
        var dialog = File.ReadAllText(Path.Combine(directory, "CommunityParticipationStartConsentDialog.razor"));
        var viewModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "ViewModels",
            "PlatformCommunityPostEngagementViewModel.cs"));

        Assert.Contains("ShowAsync<CommunityParticipationStartConsentDialog>", panel);
        Assert.Contains("dialogResult.Data is not StartCommunityPostParticipationRequest request", panel);
        Assert.Equal(2, CountOccurrences(dialog, "<MudCheckBox"));
        Assert.Contains("Disabled=\"@(!CanSubmit)\"", dialog);
        Assert.Contains("ConfirmExplicitStart = explicitStartConfirmed", dialog);
        Assert.Contains("ConfirmNonBindingParticipation = nonBindingConfirmed", dialog);
        Assert.Contains("StartParticipationAsync(Post.Id, request)", panel);
        Assert.Contains("ConfirmExplicitStart = request.ConfirmExplicitStart", viewModel);
        Assert.Contains("ConfirmNonBindingParticipation = request.ConfirmNonBindingParticipation", viewModel);
        Assert.DoesNotContain("ConfirmExplicitStart = true", viewModel);
        Assert.DoesNotContain("ConfirmNonBindingParticipation = true", viewModel);
        Assert.Contains("RefreshPosts: true", viewModel);
    }

    [Fact]
    public void 가원장생성은_세확인값을직접받고_마지막관심역할은철회요청으로처리한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var directory = FindComponentDirectory();
        var dialog = File.ReadAllText(Path.Combine(directory, "CommunityProvisionalLedgerDialog.razor"));
        var viewModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "ViewModels",
            "PlatformCommunityPostEngagementViewModel.cs"));

        Assert.Equal(3, CountOccurrences(dialog, "<MudCheckBox"));
        Assert.Contains("Disabled=\"@(!CanSubmit)\"", dialog);
        Assert.Contains("ConfirmProvisionalLedger = provisionalLedgerConfirmed", dialog);
        Assert.Contains("ConfirmNonBindingEvidence = nonBindingEvidenceConfirmed", dialog);
        Assert.Contains("ConfirmParticipantNotifications = participantNotificationsConfirmed", dialog);
        Assert.DoesNotContain("ConfirmProvisionalLedger = true", dialog);
        Assert.DoesNotContain("ConfirmNonBindingEvidence = true", dialog);
        Assert.DoesNotContain("ConfirmParticipantNotifications = true", dialog);
        Assert.Contains("WithdrawCommunityVoteAsync", viewModel);
        Assert.Contains("selected.Count == 0", viewModel);
    }

    private static int CountOccurrences(string source, string value)
        => source.Split(value, StringSplitOptions.None).Length - 1;

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
