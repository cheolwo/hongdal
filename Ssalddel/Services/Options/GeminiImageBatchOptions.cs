namespace 살뜰.Services.Options;

public sealed class GeminiImageBatchOptions
{
    public const string SectionName = "GeminiImageBatch";

    public bool Enabled { get; set; }

    public string BaseUrl { get; set; } =
        "https://generativelanguage.googleapis.com/v1beta/";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } =
        "gemini-3.1-flash-lite-image";

    public int MaxItemsPerBatch { get; set; } = 50;

    public int TimeoutSeconds { get; set; } = 300;

    public int MaxInputFileBytes { get; set; } =
        200 * 1024 * 1024;

    public int MaxGeneratedImageBytes { get; set; } =
        25 * 1024 * 1024;

    public decimal EstimatedOutputUsdPerImage { get; set; } = 0.0168m;

    public string PricingReferenceDate { get; set; } = "2026-08-01";
}
