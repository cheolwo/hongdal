namespace Ssalddel.Application.CommandProcessing;

public sealed record Command기능정책(string FeatureName, bool IsUserConfigurable, bool IsRequired, string Version = Command기능버전Catalog.CurrentRelease);

public static class Command기능정책Catalog
{
    public static readonly IReadOnlyList<Command기능정책> All =
    [
        new(Command기능명.AuditLog, false, true, "1.0"),
        new(Command기능명.WorkRelationshipSnapshot, true, false, "1.0"),
        new(Command기능명.Sms, true, false, "1.0"),
        new(Command기능명.Sns, true, false, "1.0"),
        new(Command기능명.Push, true, false, "1.0")
    ];

    public static Command기능정책 Get(string featureName)
    {
        return All.FirstOrDefault(x => string.Equals(x.FeatureName, featureName, StringComparison.Ordinal))
            ?? new Command기능정책(featureName, false, false);
    }
}
