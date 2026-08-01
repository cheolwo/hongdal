using System.Reflection;

namespace Ssalddel.Contracts.Common.Metadata;

public static class SsalddelCodeFeatureKeys
{
    public const string ApifyActorIntegration = "apify-actor-integration";
    public const string CommunityAuthoringImage = "community-authoring-image";
    public const string GroupPurchaseDemandProcessManager = "group-purchase-demand-process-manager";
    public const string GroupImportTradeReadiness = "group-import-trade-readiness";
    public const string ImportedFoodKoreanLabelIntegration = "imported-food-korean-label-integration";
    public const string PlatformDeliveryZoneLedger = "platform-delivery-zone-ledger";
    public const string PlatformSupplyBrokerage = "platform-supply-brokerage";
    public const string RegionalCultureImagePrompt = "regional-culture-image-prompt";
    public const string RegionalCulturePublicInstitution = "regional-culture-public-institution";
    public const string RegionalAgriculturalMap = "regional-agricultural-map";
    public const string TransportExecutionProfile = "transport-execution-profile";
    public const string TradeLedgerExtensions = "trade-ledger-extensions";
}

public enum SsalddelCodeLayer
{
    Contract,
    Api,
    Application,
    Domain,
    Infrastructure,
    ExternalAdapter,
    ClientAdapter,
    ViewModel,
    View
}

[Flags]
public enum SsalddelCodeEffect
{
    None = 0,
    NetworkCall = 1 << 0,
    ThirdPartyApiCall = 1 << 1,
    PersistentRead = 1 << 2,
    PersistentWrite = 1 << 3,
    ObjectStorageRead = 1 << 4,
    ObjectStorageWrite = 1 << 5,
    UiStateMutation = 1 << 6,
    MayIncurExternalCost = 1 << 7
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    AllowMultiple = true,
    Inherited = false)]
public sealed class SsalddelCodeMetadataAttribute : Attribute
{
    public SsalddelCodeMetadataAttribute(
        string featureKey,
        SsalddelCodeLayer layer,
        string responsibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(responsibility);

        FeatureKey = featureKey.Trim();
        Layer = layer;
        Responsibility = responsibility.Trim();
    }

    public string FeatureKey { get; }

    public SsalddelCodeLayer Layer { get; }

    public string Responsibility { get; }

    public SsalddelCodeEffect Effects { get; set; }

    public Type? ContractType { get; set; }

    public int FlowOrder { get; set; }

    public string Boundary { get; set; } = string.Empty;
}

public sealed record SsalddelCodeMetadataDescriptor(
    Type ComponentType,
    string FeatureKey,
    SsalddelCodeLayer Layer,
    string Responsibility,
    SsalddelCodeEffect Effects,
    Type? ContractType,
    int FlowOrder,
    string Boundary);

public static class SsalddelCodeMetadataReader
{
    public static IReadOnlyList<SsalddelCodeMetadataDescriptor> Read(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return assemblies
            .Where(assembly => assembly is not null)
            .Distinct()
            .SelectMany(GetLoadableTypes)
            .SelectMany(ReadType)
            .OrderBy(metadata => metadata.FeatureKey, StringComparer.Ordinal)
            .ThenBy(metadata => metadata.FlowOrder)
            .ThenBy(metadata => metadata.ComponentType.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<SsalddelCodeMetadataDescriptor> ReadFeature(
        string featureKey,
        params Assembly[] assemblies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureKey);

        return Read(assemblies)
            .Where(metadata => string.Equals(
                metadata.FeatureKey,
                featureKey.Trim(),
                StringComparison.Ordinal))
            .ToArray();
    }

    public static IReadOnlyList<SsalddelCodeMetadataDescriptor> Read(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        return ReadType(componentType).ToArray();
    }

    private static IEnumerable<SsalddelCodeMetadataDescriptor> ReadType(Type componentType)
        => componentType
            .GetCustomAttributes<SsalddelCodeMetadataAttribute>(inherit: false)
            .Select(attribute => new SsalddelCodeMetadataDescriptor(
                componentType,
                attribute.FeatureKey,
                attribute.Layer,
                attribute.Responsibility,
                attribute.Effects,
                attribute.ContractType,
                attribute.FlowOrder,
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
