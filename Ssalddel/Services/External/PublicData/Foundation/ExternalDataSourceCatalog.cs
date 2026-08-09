using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData;

public interface IExternalDataSourceRegistration
{
    IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions();
}

public interface IExternalDataSourceCatalog
{
    ExternalDataSourceCatalogResponse GetCatalog();
    ExternalDataSourceDefinition GetRequired(string sourceId, string datasetId);
}

/// <summary>
/// 기존 PublicDataApiMetadataCatalog를 일반 API·파일·수동 import Source 계약으로 확장합니다.
/// </summary>
public sealed class ExternalDataSourceCatalog : IExternalDataSourceCatalog
{
    private readonly IReadOnlyDictionary<string, ExternalDataSourceDefinition> definitions;

    public ExternalDataSourceCatalog(
        IPublicDataApiMetadataCatalog apiCatalog,
        IEnumerable<IExternalDataSourceRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(apiCatalog);
        var items = apiCatalog.GetCatalog(new PublicDataApiMetadataQuery()).Items
            .Select(MapExistingApi)
            .Concat((registrations ?? []).SelectMany(registration => registration.GetDefinitions()))
            .Select(Validate)
            .ToArray();
        var duplicate = items
            .GroupBy(Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"ExternalDataSourceDuplicate:{duplicate.Key}");
        definitions = items.ToDictionary(Key, StringComparer.OrdinalIgnoreCase);
    }

    public ExternalDataSourceCatalogResponse GetCatalog() => new()
    {
        Items = definitions.Values
            .OrderBy(item => item.DataDomain, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Provider, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.SourceId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.DatasetId, StringComparer.OrdinalIgnoreCase)
            .ToArray()
    };

    public ExternalDataSourceDefinition GetRequired(string sourceId, string datasetId)
    {
        var key = Key(sourceId, datasetId);
        return definitions.TryGetValue(key, out var definition)
            ? definition
            : throw new KeyNotFoundException($"ExternalDataSourceNotRegistered:{key}");
    }

    private static ExternalDataSourceDefinition MapExistingApi(PublicDataApiMetadataItem item)
    {
        var fileBased = item.ApiType.Contains("file", StringComparison.OrdinalIgnoreCase)
                        || item.ApiType.Contains("download", StringComparison.OrdinalIgnoreCase);
        return new ExternalDataSourceDefinition
        {
            SourceId = item.Key,
            DatasetId = item.Key,
            Name = item.DisplayName,
            Provider = item.Provider,
            DataDomain = item.Domain,
            OfficialSourceUrl = item.BaseUrl,
            DocumentationUrl = item.DocumentationUrl,
            AccessMethod = fileBased
                ? ExternalDataAccessMethod.DownloadFile
                : ExternalDataAccessMethod.HttpApi,
            CredentialType = item.RequiresServiceKey
                ? ExternalDataCredentialType.ApiKeyQuery
                : ExternalDataCredentialType.None,
            RequiresCredential = item.RequiresServiceKey,
            DefaultCollectionEnabled = false,
            ApiAvailable = !fileBased,
            DataFormat = item.DataFormat,
            RefreshCadence = item.FreshnessPolicy,
            RedistributionAllowed = false,
            UsageLimitations = string.Join(" ", item.UsageNotes),
            CredentialReferences = item.ConfigurationPaths,
        };
    }

    private static ExternalDataSourceDefinition Validate(ExternalDataSourceDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Require(item.SourceId, nameof(item.SourceId));
        Require(item.DatasetId, nameof(item.DatasetId));
        Require(item.Name, nameof(item.Name));
        Require(item.Provider, nameof(item.Provider));
        if (item.RequiresCredential && item.CredentialType == ExternalDataCredentialType.None)
            throw new InvalidOperationException($"ExternalDataCredentialTypeMissing:{Key(item)}");
        if (!item.RequiresCredential && item.CredentialType != ExternalDataCredentialType.None)
            throw new InvalidOperationException($"ExternalDataCredentialRequirementMismatch:{Key(item)}");
        // Reference-only catalog entries may establish that a provider needs a key before
        // an executable adapter and its server configuration path have been selected.
        // Enabling such a source still fails closed with MissingCredential.
        return item;
    }

    private static string Key(ExternalDataSourceDefinition item) => Key(item.SourceId, item.DatasetId);

    private static string Key(string sourceId, string datasetId)
    {
        Require(sourceId, nameof(sourceId));
        Require(datasetId, nameof(datasetId));
        return $"{sourceId.Trim()}::{datasetId.Trim()}";
    }

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", name);
    }
}
