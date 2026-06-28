namespace 홍달.Services.Options
{
    public sealed class NaverCloudDirectionsOptions
    {
        public const string SectionName = "NaverCloudDirections";

        public string BaseUrl { get; set; } = "https://maps.apigw.ntruss.com";
        public string Path { get; set; } = "/map-direction/v1/driving";
        public string ApiKeyId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultOption { get; set; } = "trafast";
    }
}


