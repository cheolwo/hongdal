namespace 살뜰.Services.Options;

public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    public string Provider { get; set; } = ObjectStorageProviderNames.GoogleCloud;
}

public static class ObjectStorageProviderNames
{
    public const string AzureBlob = "AzureBlob";
    public const string GoogleCloud = "GoogleCloud";
    public const string Local = "Local";
}
