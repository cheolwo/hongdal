namespace 살뜰.Services.Options;

public sealed class CommunityPostEmailNotificationOptions
{
    public const string SectionName = "CommunityPostEmailNotifications";

    public bool Enabled { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string SubjectPrefix { get; set; } = "[살뜰 새 게시글]";
    public int QueueCapacity { get; set; } = 1000;
    public int PollIntervalSeconds { get; set; } = 5;
    public int MaxAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 15;
    public CommunityPostEmailGmailOptions Gmail { get; set; } = new();
}

public sealed class CommunityPostEmailGmailOptions
{
    public string UserName { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "살뜰";
}
