namespace Hongdal.Contracts.CommandSettings;

public sealed class AuxiliaryFeatureSettingsResponse
{
    public IReadOnlyList<AuxiliaryFeatureSettingItem> Items { get; set; } = [];
}

public sealed class AuxiliaryFeatureSettingItem
{
    public string TargetType { get; set; } = "Command";
    public string TargetName { get; set; } = string.Empty;
    public string TargetDisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string VersionDisplayName { get; set; } = string.Empty;
    public int VersionSortOrder { get; set; }
    public bool IsCurrentRelease { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureDisplayName { get; set; } = string.Empty;
    public bool AppDefaultEnabled { get; set; }
    public bool GlobalEnabled { get; set; }
    public bool HasGlobalOverride { get; set; }
    public bool? UserEnabled { get; set; }
    public bool HasUserOverride { get; set; }
    public bool EffectiveEnabled { get; set; }
    public bool IsUserConfigurable { get; set; }
    public bool IsRequired { get; set; }
}

public sealed class AuxiliaryFeatureSettingUpdateRequest
{
    public bool IsEnabled { get; set; }
}

public static class AuxiliaryFeatureTargetTypes
{
    public const string Command = "Command";
    public const string Event = "Event";
    public const string Service = "Service";
}
