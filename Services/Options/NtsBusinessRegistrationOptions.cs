namespace 홍달.Services.Options
{
    public sealed class NtsBusinessRegistrationOptions
    {
        public const string SectionName = "NtsBusinessRegistration";

        public string BaseUrl { get; set; } = "https://api.odcloud.kr/api/nts-businessman/v1";
        public string StatusPath { get; set; } = "/status";
        public string? ServiceKey { get; set; }
    }
}


