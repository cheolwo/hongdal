using System.Collections.Generic;

namespace 홍달.Services.Options;

public sealed class CommandFileStorageOptions
{
    public const string SectionName = "CommandFileStorage";

    public string RootFolder { get; set; } = "uploads";

    public CommandFileStorageRule DefaultCommand { get; set; } = new()
    {
        Folder = "commands"
    };

    public CommandFileStorageRule AdminFilePod { get; set; } = new()
    {
        Folder = "admin/files-pod"
    };

    public Dictionary<string, CommandFileStorageRule> Commands { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class CommandFileStorageRule
{
    public string Folder { get; set; } = string.Empty;

    public bool IncludeRequestIdSegment { get; set; } = true;
}
