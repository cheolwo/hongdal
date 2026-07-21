namespace Ssalddel.Tests.Architecture;

public sealed class PlatformCommunityPostComposerCompositionTests
{
    [Fact]
    public void 커뮤니티_글쓰기_루트는_화면_영역과_저장_흐름만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentDirectory = Path.Combine(
            repositoryRoot,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community");
        var pagePath = Path.Combine(componentDirectory, "PlatformCommunityPostComposer.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 220);
        Assert.Contains("<PlatformCommunityComposerHeader", source);
        Assert.Contains("<PlatformCommunityComposerFeedback", source);
        Assert.Contains("<PlatformCommunityComposerBodyFields", source);
        Assert.Contains("<PlatformCommunityComposerSalesEditor", source);
        Assert.Contains("<PlatformCommunityComposerAttachmentTools", source);
        Assert.Contains("<PlatformCommunityComposerContextBar", source);
        Assert.Contains("<PlatformCommunityComposerSettings", source);
        Assert.Contains("ViewModel.SaveAsync()", source);
        Assert.DoesNotContain("<MudTextField", source);
        Assert.DoesNotContain("<MudNumericField", source);
        Assert.DoesNotContain("<MudSelect", source);
        Assert.DoesNotContain("<InputFile", source);
    }

    [Theory]
    [InlineData("PlatformCommunityComposerHeader.razor")]
    [InlineData("PlatformCommunityComposerFeedback.razor")]
    [InlineData("PlatformCommunityComposerBodyFields.razor")]
    [InlineData("PlatformCommunityComposerSalesEditor.razor")]
    [InlineData("PlatformCommunityComposerAttachmentTools.razor")]
    [InlineData("PlatformCommunityComposerContextBar.razor")]
    [InlineData("PlatformCommunityComposerSettings.razor")]
    [InlineData("PlatformCommunityComposerPresentation.cs")]
    public void 글쓰기_표시_책임은_각_전용_파일로_존재한다(string fileName)
    {
        var componentPath = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            fileName);

        Assert.True(File.Exists(componentPath), $"글쓰기 전용 파일이 없습니다: {fileName}");
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
