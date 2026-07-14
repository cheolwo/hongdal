namespace 홍달.Services.Options;

public sealed class YouTubeOptions
{
    public const string SectionName = "YouTube";

    public bool Enabled { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3/";

    public int TimeoutSeconds { get; set; } = 20;

    public int SyncIntervalSeconds { get; set; } = 300;

    public int MaxResultsPerChannel { get; set; } = 20;

    public List<YouTube기본감시채널Options> DefaultChannels { get; set; } = [];
}

public sealed class YouTube기본감시채널Options
{
    public string ChannelId { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
}
