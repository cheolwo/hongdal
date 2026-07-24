namespace 살뜰.Services.Options;

public sealed class ApifyYouTubeCommentsOptions
{
    public const string SectionName = "ApifyYouTubeComments";

    /// <summary>
    /// 비용이 발생할 수 있는 YouTube 댓글 Actor 실행을 명시적으로 허용합니다.
    /// </summary>
    public bool Enabled { get; set; }

    public string ActorId { get; set; } = "streamers~youtube-comments-scraper";

    public int ActorTimeoutSeconds { get; set; } = 180;

    public int MemoryMegabytes { get; set; } = 1024;

    public int MaxDatasetItems { get; set; } = 100;

    public decimal MaxTotalChargeUsd { get; set; } = 0.25m;

    public int DefaultMaxComments { get; set; } = 50;

    public int MaxCommentsPerRequest { get; set; } = 300;

    public int MaxCommentTextCharacters { get; set; } = 5_000;

    public int MaxAuthorCharacters { get; set; } = 200;
}
