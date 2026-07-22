namespace 살뜰.Services.Options
{
    public sealed class GoogleCloudStorageOptions
    {
        public const string SectionName = "GoogleCloudStorage";

        public string BucketName { get; set; } = string.Empty;
        public string? PublicBucketName { get; set; }
        public string? PrivateBucketName { get; set; }
        public string? ServiceAccountJsonPath { get; set; }
        public string PublicBaseUrl { get; set; } = "https://storage.googleapis.com";
    }
}



