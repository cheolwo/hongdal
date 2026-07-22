namespace Ssalddel.Tests.Architecture;

public sealed class CommunityPersonalPageCompositionTests
{
    [Fact]
    public void 개인_라우트_페이지는_섹션과_ViewModel만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var pagePath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "CommunityPersonalPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 150);
        Assert.Contains("<CommunityPersonalOverviewSection", source);
        Assert.Contains("<CommunityPersonalPostsSection", source);
        Assert.Contains("<CommunityPersonalDecorationsSection", source);
        Assert.DoesNotContain("PlatformCommunityService", source);
        Assert.DoesNotContain("ICommunityDecorationSelectionStore", source);
        Assert.DoesNotContain("CommunityPersonalPreferenceService", source);
        Assert.DoesNotContain("GetPostsAsync", source);
        Assert.DoesNotContain("SaveDecorationSelectionAsync", source);
        Assert.DoesNotContain("@page \"/community/decorations", source);
    }

    [Fact]
    public void 개인꾸미기관리는_상점과FakePG를직접실행하지않는다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "CommunityPersonal",
            "CommunityPersonalDecorationsSection.razor"));

        Assert.Contains("CommunityPageRoutes.Decorations", source);
        Assert.Contains("CommunityPageRoutes.DecorationProductFor", source);
        Assert.DoesNotContain("OwnAsync", source);
        Assert.DoesNotContain("Purchase(", source);
        Assert.DoesNotContain("FakePG", source);
    }

    [Theory]
    [InlineData("CommunityPersonalOverviewSection.razor")]
    [InlineData("CommunityPersonalPostsSection.razor")]
    [InlineData("CommunityPersonalActionsSection.razor")]
    [InlineData("CommunityPersonalLedgersSection.razor")]
    [InlineData("CommunityPersonalNotificationsSection.razor")]
    [InlineData("CommunityPersonalDecorationsSection.razor")]
    [InlineData("CommunityPersonalSettingsSection.razor")]
    public void 개인_하위_책임은_별도_컴포넌트로_존재한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "CommunityPersonal",
            fileName);

        Assert.True(File.Exists(componentPath), $"개인 페이지 하위 컴포넌트가 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

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
