namespace 살뜰.Services.Options;

public sealed class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    public string ServiceUri { get; set; } = string.Empty;
    public string PublicContainerName { get; set; } = "community-public";
    public string PrivateContainerName { get; set; } = "platform-private";
    public string? ManagedIdentityClientId { get; set; }
}
