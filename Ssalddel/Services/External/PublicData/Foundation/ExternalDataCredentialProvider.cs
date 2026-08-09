using Microsoft.Extensions.Configuration;
using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData;

public sealed class ExternalDataCredential
{
    public ExternalDataCredential(
        ExternalDataCredentialType type,
        string reference,
        string secretValue)
    {
        if (type == ExternalDataCredentialType.None)
            throw new ArgumentException("CredentialTypeNone", nameof(type));
        Type = type;
        Reference = string.IsNullOrWhiteSpace(reference)
            ? throw new ArgumentException("CredentialReferenceMissing", nameof(reference))
            : reference.Trim();
        SecretValue = string.IsNullOrWhiteSpace(secretValue)
            ? throw new ArgumentException("CredentialValueMissing", nameof(secretValue))
            : secretValue;
    }

    public ExternalDataCredentialType Type { get; }
    public string Reference { get; }
    public string SecretValue { get; }
    public override string ToString() => $"{Type}:[REDACTED]";
}

public interface IExternalDataCredentialProvider
{
    ValueTask<ExternalDataCredential?> GetAsync(
        string sourceId,
        string datasetId,
        CancellationToken cancellationToken = default);
}

public sealed class ConfigurationExternalDataCredentialProvider : IExternalDataCredentialProvider
{
    private readonly IConfiguration configuration;
    private readonly IExternalDataSourceCatalog sourceCatalog;

    public ConfigurationExternalDataCredentialProvider(
        IConfiguration configuration,
        IExternalDataSourceCatalog sourceCatalog)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.sourceCatalog = sourceCatalog ?? throw new ArgumentNullException(nameof(sourceCatalog));
    }

    public ValueTask<ExternalDataCredential?> GetAsync(
        string sourceId,
        string datasetId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var source = sourceCatalog.GetRequired(sourceId, datasetId);
        if (!source.RequiresCredential) return ValueTask.FromResult<ExternalDataCredential?>(null);
        foreach (var reference in source.CredentialReferences)
        {
            var value = configuration[reference];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return ValueTask.FromResult<ExternalDataCredential?>(new ExternalDataCredential(
                    source.CredentialType,
                    reference,
                    value));
            }
        }

        return ValueTask.FromResult<ExternalDataCredential?>(null);
    }
}

public interface IExternalDataCollectionPolicy
{
    bool IsEnabled(ExternalDataSourceDefinition source);
}

/// <summary>외부 수집은 source별 명시 설정이 true일 때만 허용합니다.</summary>
public sealed class ConfigurationExternalDataCollectionPolicy : IExternalDataCollectionPolicy
{
    private readonly IConfiguration configuration;

    public ConfigurationExternalDataCollectionPolicy(IConfiguration configuration)
        => this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public bool IsEnabled(ExternalDataSourceDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return configuration.GetValue<bool?>($"ExternalData:Sources:{source.SourceId}:Enabled")
               ?? source.DefaultCollectionEnabled;
    }
}
