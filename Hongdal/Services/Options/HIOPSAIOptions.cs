namespace 홍달.Services.Options;

public sealed class HIOPSAIOptions
{
    public const string SectionName = "HIOPSAI";

    public bool Enabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string ResponsesPath { get; set; } = "/v1/responses";
    public string DefaultModel { get; set; } = "gpt-5.4-mini";
    public int TimeoutSeconds { get; set; } = 20;
    public int MaxInputTokens { get; set; } = 4000;
    public int MaxOutputTokens { get; set; } = 700;
    public decimal MonthlyBudgetUsd { get; set; } = 20.00m;
    public decimal BudgetWarningUsd { get; set; } = 16.00m;
    public decimal MaxEstimatedCostPerCallUsd { get; set; } = 0.03m;
    public bool EnableCache { get; set; } = true;
    public bool UseOnlyForAmbiguousDispatch { get; set; } = true;
    public string UsageLedgerPath { get; set; } = "logs/hiops-ai-usage.json";

    public decimal InputUsdPerMillionTokens { get; set; } = 0.75m;
    public decimal OutputUsdPerMillionTokens { get; set; } = 4.50m;
}
