using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Application.CommandProcessing;

public sealed record Command기능버전정보(
    string Version,
    string DisplayName,
    int SortOrder,
    bool IsCurrentRelease);

public static class Command기능버전Catalog
{
    public const string CurrentRelease = SsalddelProductRoadmapCatalog.CurrentVersion;
    public const string DeploymentTarget = SsalddelProductRoadmapCatalog.DeploymentTargetVersion;

    private static readonly IReadOnlyDictionary<string, Command기능버전정보> Versions =
        SsalddelProductRoadmapCatalog.All.ToDictionary(
            stage => stage.Version,
            stage => new Command기능버전정보(
                stage.Version,
                stage.FullDisplayName,
                stage.SortOrder,
                stage.IsCurrent),
            StringComparer.OrdinalIgnoreCase);

    public static Command기능버전정보 Get(string? version)
    {
        var key = string.IsNullOrWhiteSpace(version) ? CurrentRelease : version.Trim();
        return Versions.TryGetValue(key, out var info)
            ? info
            : new Command기능버전정보(key, $"{key} 확장 기능", 900, false);
    }
}
