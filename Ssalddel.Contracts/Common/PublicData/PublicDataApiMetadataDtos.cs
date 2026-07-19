namespace Ssalddel.Contracts.Common.PublicData;

public sealed class PublicDataApiMetadataResponse
{
    public IReadOnlyList<PublicDataApiMetadataItem> Items { get; init; } = [];
}

public sealed class PublicDataApiMetadataItem
{
    public string Key { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Purpose { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;

    public string VersionScope { get; init; } = string.Empty;

    public string ApiType { get; init; } = string.Empty;

    public string DataFormat { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public string DocumentationUrl { get; init; } = string.Empty;

    public bool RequiresServiceKey { get; init; }

    public bool ContainsResidentialData { get; init; }

    public bool ContainsPersonalData { get; init; }

    public IReadOnlyList<string> MainParameters { get; init; } = [];

    public IReadOnlyList<string> MainResponseFields { get; init; } = [];

    public IReadOnlyList<string> UsageNotes { get; init; } = [];
}

public sealed class PublicDataApiMetadataQuery
{
    public string? Domain { get; init; }

    public string? VersionScope { get; init; }

    public bool? ContainsResidentialData { get; init; }
}
