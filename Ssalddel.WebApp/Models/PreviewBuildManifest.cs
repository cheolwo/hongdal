namespace Ssalddel.WebApp.Models;

public sealed record PreviewBuildManifest
{
    public string Environment { get; init; } = "Local";

    public string Release { get; init; } = "local";

    public string Commit { get; init; } = "working-tree";

    public string Branch { get; init; } = "local";

    public DateTimeOffset? BuiltAtUtc { get; init; }

    public static PreviewBuildManifest Local { get; } = new();
}
