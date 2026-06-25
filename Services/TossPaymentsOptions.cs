namespace 홍달.Services
{
    public sealed class TossPaymentsOptions
    {
        public const string SectionName = "TossPayments";

        public string ClientKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.tosspayments.com";
    }
}
