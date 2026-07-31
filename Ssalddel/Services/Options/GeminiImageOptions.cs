namespace 살뜰.Services.Options;

public sealed class GeminiImageOptions
{
    public const string SectionName = "GeminiImage";

    public bool Enabled { get; set; }

    public string BaseUrl { get; set; } =
        "https://generativelanguage.googleapis.com/v1beta/";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gemini-3.1-flash-image";

    public string GeneratePath { get; set; } = "interactions";

    public string DefaultResolution { get; set; } = "1K";

    public string OutputMimeType { get; set; } = "image/jpeg";

    public int TimeoutSeconds { get; set; } = 180;

    public int MaxGeneratedImageBytes { get; set; } =
        25 * 1024 * 1024;
}
