namespace Hongdal.Contracts.Common.Versioning;

public sealed class VersionFeatureFlagsResponse
{
    public IReadOnlyDictionary<string, bool> Flags { get; init; } = new Dictionary<string, bool>();
}
