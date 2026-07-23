namespace 살뜰.Services.Options;

public sealed class CommunityActivityBoardContentOptions
{
    public const string SectionName = "CommunityActivityBoards";

    public bool EnsureAnnouncementsAtStartup { get; set; } = true;

    public int StartupDelaySeconds { get; set; } = 5;

    public bool SeedTestActivityPosts { get; set; }

    public int TestPostsPerBoard { get; set; } = 2;

    public string TestScenarioKey { get; set; } = "development-observation-v1";
}
