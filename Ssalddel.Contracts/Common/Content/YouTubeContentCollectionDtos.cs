namespace Ssalddel.Contracts.Common.Content;

public static class YouTubeContentCollectionSourceKeys
{
    public const string Video = "youtube-video";
    public const string Transcript = "youtube-transcript";
    public const string Comments = "youtube-comments";
}

public static class YouTubeCommentSortCodes
{
    public const string Top = "top";
    public const string Newest = "newest";

    public static IReadOnlyList<string> All { get; } = [Top, Newest];
}

public sealed class YouTubeContentCollectionRequest
{
    public string? TargetLanguage { get; init; }

    public int? MaxComments { get; init; }

    public string CommentSort { get; init; } = YouTubeCommentSortCodes.Top;
}

public sealed record YouTubeCommentDto(
    string CommentId,
    string? ParentCommentId,
    string? AuthorDisplayName,
    string Text,
    DateTime? PublishedAtUtc,
    string? PublishedTimeText,
    long? LikeCount,
    int ReplyCount,
    bool IsReply,
    bool IsChannelOwner,
    bool HasCreatorHeart,
    bool IsPinned);

public sealed record YouTubeCommentCollectionResponse(
    string VideoId,
    string VideoUrl,
    string Provider,
    DateTime CollectedAtUtc,
    IReadOnlyList<YouTubeCommentDto> Comments);

public sealed record YouTubeContentCollectionSourceStatusDto(
    string SourceKey,
    string Provider,
    bool Enabled,
    bool Succeeded,
    int ItemCount,
    string? Message);

public sealed record YouTubeContentCollectionResponse(
    DateTime CollectedAtUtc,
    YouTubeSocialContextVideoDto Video,
    YouTubeTranscriptResponse? Transcript,
    YouTubeCommentCollectionResponse? Comments,
    IReadOnlyList<YouTubeContentCollectionSourceStatusDto> Sources)
{
    public bool IsComplete => Sources.Count > 0 && Sources.All(source => source.Enabled && source.Succeeded);

    public bool HasPartialData => Transcript is not null || Comments?.Comments.Count > 0;
}
