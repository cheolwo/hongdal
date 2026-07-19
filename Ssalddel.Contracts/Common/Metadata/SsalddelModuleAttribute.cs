using System.Reflection;

namespace Ssalddel.Contracts.Common.Metadata;

public static class SsalddelProductVersionCodes
{
    public const string V0_0 = "0.0";
}

public static class SsalddelCommunityV0ModuleKeys
{
    public const string Ui = "community-v0-ui";
    public const string Content = "community-v0-content";
    public const string Participation = "community-v0-participation";
    public const string Ledger = "community-v0-ledger";
    public const string Safety = "community-v0-safety";
    public const string Authoring = "community-v0-authoring";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        Ui,
        Content,
        Participation,
        Ledger,
        Safety,
        Authoring
    };
}

public static class SsalddelCommunityV0ReleaseStages
{
    public const string IndependentExecution = "0.0-A";
    public const string Persistence = "0.0-B";
    public const string DomesticGroupPurchasePilot = "0.0-C";
    public const string ClosedLoop = "0.0-D";
    public const string SafetyAndOperations = "0.0-E";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        IndependentExecution,
        Persistence,
        DomesticGroupPurchasePilot,
        ClosedLoop,
        SafetyAndOperations
    };
}

public static class SsalddelCommunityV0Metadata
{
    public const string FeatureFlag = "CommunityTrustWorkflow";
    public const string WorkflowKey = "CommunityTrust";
}

public enum SsalddelModuleKind
{
    ClientComposition,
    ClientFeature,
    Api,
    Application,
    Persistence,
    BackgroundProcessing
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = false)]
public class SsalddelModuleAttribute : Attribute
{
    public SsalddelModuleAttribute(
        string moduleKey,
        string productVersion,
        SsalddelModuleKind kind,
        string responsibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(productVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(responsibility);

        ModuleKey = moduleKey.Trim();
        ProductVersion = productVersion.Trim();
        Kind = kind;
        Responsibility = responsibility.Trim();
    }

    public string ModuleKey { get; }

    public string ProductVersion { get; }

    public SsalddelModuleKind Kind { get; }

    public string Responsibility { get; }

    public string ReleaseStage { get; set; } = string.Empty;

    public string FeatureFlag { get; set; } = string.Empty;

    public string WorkflowKey { get; set; } = string.Empty;

    public bool DefaultEnabled { get; set; }

    public string Boundary { get; set; } = string.Empty;
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = false)]
public sealed class SsalddelCommunityV0ModuleAttribute : SsalddelModuleAttribute
{
    public SsalddelCommunityV0ModuleAttribute(
        string moduleKey,
        SsalddelModuleKind kind,
        string responsibility)
        : base(moduleKey, SsalddelProductVersionCodes.V0_0, kind, responsibility)
    {
        FeatureFlag = SsalddelCommunityV0Metadata.FeatureFlag;
        WorkflowKey = SsalddelCommunityV0Metadata.WorkflowKey;
        DefaultEnabled = true;
    }
}

public sealed record SsalddelModuleDescriptor(
    MemberInfo Component,
    string ModuleKey,
    string ProductVersion,
    SsalddelModuleKind Kind,
    string Responsibility,
    string ReleaseStage,
    string FeatureFlag,
    string WorkflowKey,
    bool DefaultEnabled,
    string Boundary)
{
    public string ComponentName
        => Component is Type type
            ? type.FullName ?? type.Name
            : $"{Component.DeclaringType?.FullName}.{Component.Name}";
}

public static class SsalddelModuleMetadataReader
{
    public static IReadOnlyList<SsalddelModuleDescriptor> Read(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return assemblies
            .Where(assembly => assembly is not null)
            .Distinct()
            .SelectMany(GetLoadableTypes)
            .SelectMany(ReadTypeAndMethods)
            .OrderBy(module => module.ProductVersion, StringComparer.Ordinal)
            .ThenBy(module => module.ModuleKey, StringComparer.Ordinal)
            .ThenBy(module => module.ComponentName, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<SsalddelModuleDescriptor> ReadVersion(
        string productVersion,
        params Assembly[] assemblies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productVersion);

        return Read(assemblies)
            .Where(module => string.Equals(
                module.ProductVersion,
                productVersion.Trim(),
                StringComparison.Ordinal))
            .ToArray();
    }

    public static IReadOnlyList<SsalddelModuleDescriptor> Read(MemberInfo component)
    {
        ArgumentNullException.ThrowIfNull(component);
        return ReadMember(component).ToArray();
    }

    private static IEnumerable<SsalddelModuleDescriptor> ReadTypeAndMethods(Type type)
    {
        foreach (var descriptor in ReadMember(type))
        {
            yield return descriptor;
        }

        foreach (var method in type.GetMethods(
                     BindingFlags.Public
                     | BindingFlags.NonPublic
                     | BindingFlags.Instance
                     | BindingFlags.Static
                     | BindingFlags.DeclaredOnly))
        {
            foreach (var descriptor in ReadMember(method))
            {
                yield return descriptor;
            }
        }
    }

    private static IEnumerable<SsalddelModuleDescriptor> ReadMember(MemberInfo component)
        => component
            .GetCustomAttributes<SsalddelModuleAttribute>(inherit: false)
            .Select(attribute => new SsalddelModuleDescriptor(
                component,
                attribute.ModuleKey,
                attribute.ProductVersion,
                attribute.Kind,
                attribute.Responsibility,
                attribute.ReleaseStage,
                attribute.FeatureFlag,
                attribute.WorkflowKey,
                attribute.DefaultEnabled,
                attribute.Boundary));

    private static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>().ToArray();
        }
    }
}
