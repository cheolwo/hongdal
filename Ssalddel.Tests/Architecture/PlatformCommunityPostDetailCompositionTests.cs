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
