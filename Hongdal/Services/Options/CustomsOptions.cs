namespace 홍달.Services.Options;

public sealed class CustomsOptions
{
    public const string SectionName = "Customs";

    public string UnipassBaseUrl { get; set; } = "https://unipass.customs.go.kr";
    public string PersonalCodeValidationPath { get; set; } = "/csp/persEcmRdcnt/retrievePersEcmRdcnt.do";
    public string CargoTrackingBaseUrl { get; set; } = "https://apis.data.go.kr";
    public string CargoTrackingPath { get; set; } = "/1220000/CargoTracingService/getCargoInfo";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 20;
}
