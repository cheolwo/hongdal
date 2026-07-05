namespace 홍달.Services.Options
{
    public sealed class OpinetOptions
    {
        public const string SectionName = "Opinet";

        public string BaseUrl { get; set; } = "https://www.opinet.co.kr";
        public string AveragePricePath { get; set; } = "/api/avgAllPrice.do";
        public string CertKey { get; set; } = string.Empty;
        public string OutputFormat { get; set; } = "xml";
    }
}


