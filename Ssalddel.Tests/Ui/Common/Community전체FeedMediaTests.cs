using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class Community전체FeedMediaTests
{
    [Fact]
    public void 동영상이있으면_이미지보다_피드대표Media로선택한다()
    {
        var media = Community피드MediaPresentation.Select(
        [
            Attachment(1, "image/jpeg", "image.jpg"),
            Attachment(2, "video/mp4", "video.mp4"),
            Attachment(3, "image/webp", "image.webp")
        ]);

        var selected = Assert.Single(media);
        Assert.Equal(2, selected.AttachmentId);
        Assert.Equal(Community피드MediaKind.Video, selected.Kind);
    }

    [Fact]
    public void 이미지는_원래순서대로_최대네개를선택한다()
    {
        var media = Community피드MediaPresentation.Select(
            Enumerable.Range(1, 6)
                .Select(index => Attachment(index, "image/jpeg", $"{index}.jpg"))
                .ToArray());

        Assert.Equal([1L, 2L, 3L, 4L], media.Select(item => item.AttachmentId));
        Assert.All(media, item => Assert.Equal(Community피드MediaKind.Image, item.Kind));
    }

    [Fact]
    public void 알수없는형식과빈주소는_피드Media에서제외한다()
    {
        var media = Community피드MediaPresentation.Select(
        [
            Attachment(1, "application/pdf", "document.pdf"),
            Attachment(2, "video/mp4", "video.mp4", url: " "),
            Attachment(3, "image/png", "image.png")
        ]);

        Assert.Equal(3, Assert.Single(media).AttachmentId);
    }

    [Fact]
    public void 자동재생은_중앙영상하나와_접근성및데이터절약경계를사용한다()
    {
        var component = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            "CommunityFeedMedia.razor");
        var script = Read(
            "Ssalddel.Ui.Common",
            "wwwroot",
            "js",
            "community-feed-scroll.js");

        Assert.Contains("data-community-feed-video", component);
        Assert.Contains("muted", component);
        Assert.Contains("playsinline", component);
        Assert.Contains("preload=\"metadata\"", component);
        Assert.Contains("IntersectionObserver", script);
        Assert.Contains("intersectionRatio < 0.55", script);
        Assert.Contains("candidate.muted = true", script);
        Assert.Contains("connection?.saveData === true", script);
        Assert.Contains("prefers-reduced-motion: reduce", script);
        Assert.Contains("video !== candidate", script);
    }

    private static PlatformCommunityPostAttachmentResponse Attachment(
        long id,
        string contentType,
        string fileName,
        string? url = null)
        => new()
        {
            Id = id,
            Url = url ?? $"https://media.example.test/{fileName}",
            ContentType = contentType,
            OriginalFileName = fileName
        };

    private static string Read(params string[] path)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), Path.Combine(path)));

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
