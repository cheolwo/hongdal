namespace 홍달.Services.Options
{
    public sealed class GoogleCloudStorageOptions
    {
        public const string SectionName = "GoogleCloudStorage";

        public string BucketName { get; set; } = string.Empty;
        public string? ServiceAccountJsonPath { get; set; }
        public string PublicBaseUrl { get; set; } = "https://storage.googleapis.com";
    }
}



