namespace Ssalddel.Tests.Architecture;

public sealed class PlatformCommunityPostListCompositionTests
{
    [Fact]
    public void 커뮤니티_글목록_루트는_보기_전환과_화면_영역만_조립한다()
    {
        var componentDirectory = FindComponentDirectory();
        var pagePath = Path.Combine(componentDirectory, "PlatformCommunityPostList.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 130);
        Assert.Contains("<PlatformCommunityPostListToolbar", source);
        Assert.Contains("<PlatformCommunityPostTable", source);
        Assert.Contains("<PlatformCommunityPostCards", source);
        Assert.Contains("<PlatformCommunityPostSearchFooter", source);
        Assert.DoesNotContain("<article", source);
        Assert.DoesNotContain("role=\"row\"", source);
        Assert.DoesNotContain("type=\"search\"", source);
        Assert.DoesNotContain("FormatSalesPrice", source);
    }

    [Theory]
    [InlineData("PlatformCommunityPostListToolbar.razor")]
    [InlineData("PlatformCommunityPostTable.razor")]
    [InlineData("PlatformCommunityPostCards.razor")]
    [InlineData("PlatformCommunityPostSearchFooter.razor")]
    [InlineData("PlatformCommunityPostListPresentation.cs")]
    public void 글목록_표시_책임은_전용_파일로_존재한다(string fileName)
    {
        var componentPath = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(componentPath), $"글 목록 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

    [Fact]
    public void 전통_게시판_목록과_상세는_서버_조회수를_표시한다()
    {
        var componentDirectory = FindComponentDirectory();
        var table = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostTable.razor"));
        var cards = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostCards.razor"));
        var headerMeta = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostHeaderMeta.razor"));

        Assert.Contains("post.ViewCount", table);
        Assert.Contains("post.ViewCount", cards);
        Assert.Contains("Post.ViewCount", headerMeta);
    }

    [Fact]
    public void 글목록과_상세는_서버가_허용한_글에_삭제_행동을_제공한다()
    {
        var componentDirectory = FindComponentDirectory();
        var table = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostTable.razor"));
        var cards = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostCards.razor"));
        var conversation = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostConversationPanel.razor"));
        var dialog = File.ReadAllText(Path.Combine(componentDirectory, "PlatformCommunityPostDeleteDialog.razor"));

        Assert.Contains("post.CanDelete", table);
        Assert.Contains("post.CanDelete", cards);
        Assert.Contains("Post.CanDelete", conversation);
        Assert.Contains("RequiresPassword", dialog);
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
