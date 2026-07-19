namespace Ssalddel.Contracts.CommandSettings;

public sealed class Command기능설정목록응답
{
    public IReadOnlyList<Command기능설정항목응답> Items { get; set; } = [];
}

public sealed class Command기능설정항목응답
{
    public string CommandName { get; set; } = string.Empty;

    public string CommandDisplayName { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string VersionDisplayName { get; set; } = string.Empty;

    public int VersionSortOrder { get; set; }

    public bool IsCurrentRelease { get; set; }

    public string FeatureName { get; set; } = string.Empty;

    public string FeatureDisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public bool DefaultEnabled { get; set; }

    public bool HasUserOverride { get; set; }

    public bool IsUserConfigurable { get; set; } = true;
}

public sealed class Command기능설정수정요청
{
    public bool IsEnabled { get; set; }
}
