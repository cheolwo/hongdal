namespace 홍달.Services.Options;

public sealed class CommunityPostStorageOptions
{
    public const string SectionName = "CommunityPostStorage";

    public string Folder { get; set; } = "community/posts";
    public long MaxImageBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxAttachmentsPerPost { get; set; } = 5;
    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    ];
}
