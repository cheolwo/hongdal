using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum Community피드MediaKind
{
    Image,
    Video
}

public sealed record Community피드MediaItem(
    long AttachmentId,
    string Url,
    string ContentType,
    string OriginalFileName,
    Community피드MediaKind Kind);

public static class Community피드MediaPresentation
{
    public const int MaximumImageCount = 4;

    public static IReadOnlyList<Community피드MediaItem> Select(
        IReadOnlyList<PlatformCommunityPostAttachmentResponse> attachments)
    {
        var available = attachments
            .Where(attachment => !string.IsNullOrWhiteSpace(attachment.Url))
            .Select(ToMediaItem)
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();

        var video = available.FirstOrDefault(item => item.Kind == Community피드MediaKind.Video);
        if (video is not null)
        {
            return [video];
        }

        return available
            .Where(item => item.Kind == Community피드MediaKind.Image)
            .Take(MaximumImageCount)
            .ToArray();
    }

    private static Community피드MediaItem? ToMediaItem(
        PlatformCommunityPostAttachmentResponse attachment)
    {
        var contentType = attachment.ContentType.Trim();
        var kind = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? Community피드MediaKind.Video
            : contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                ? Community피드MediaKind.Image
                : (Community피드MediaKind?)null;

        return kind is null
            ? null
            : new Community피드MediaItem(
                attachment.Id,
                attachment.Url.Trim(),
                contentType,
                attachment.OriginalFileName.Trim(),
                kind.Value);
    }
}
