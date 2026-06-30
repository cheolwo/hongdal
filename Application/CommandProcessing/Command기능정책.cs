namespace Hongdal.Application.CommandProcessing;

public sealed record Command기능정책(string FeatureName, bool IsUserConfigurable, bool IsRequired);

public static class Command기능정책Catalog
{
    public static readonly IReadOnlyList<Command기능정책> All =
    [
        new(Command기능명.AuditLog, false, true),
        new(Command기능명.Sms, true, false),
        new(Command기능명.Sns, true, false),
        new(Command기능명.Push, true, false)
    ];

    public static Command기능정책 Get(string featureName)
    {
        return All.FirstOrDefault(x => string.Equals(x.FeatureName, featureName, StringComparison.Ordinal))
            ?? new Command기능정책(featureName, false, false);
    }
}
