namespace 홍달.Services.Options;

public sealed class CustomsOptions
{
    public const string SectionName = "Customs";

    public string UnipassBaseUrl { get; set; } = "https://unipass.customs.go.kr";
    public string PersonalCodeValidationPath { get; set; } = "/csp/persEcmRdcnt/retrievePersEcmRdcnt.do";
    public string CargoTrackingBaseUrl { get; set; } = "https://unipass.customs.go.kr:38010";
    public string CargoTrackingPath { get; set; } = "/ext/rest/cargCsclPrgsInfoQry/retrieveCargCsclPrgsInfo";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 20;
}
