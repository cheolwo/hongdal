namespace 살뜰.Services.Options;

public sealed class OfficialNewsRssOptions
{
    public const string SectionName = "OfficialNewsRss";

    public bool Enabled { get; set; }

    public int TimeoutSeconds { get; set; } = 20;

    public int MaxItemsPerFeed { get; set; } = 50;

    public int MaxResponseCharacters { get; set; } = 2_000_000;

    public string UserAgent { get; set; } = "SsalddelOfficialNewsResearch/1.0";
}
