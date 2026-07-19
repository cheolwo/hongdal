namespace 살뜰.Services.Options;

public sealed class TypecastOptions
{
    public const string SectionName = "Typecast";

    public bool Enabled { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.typecast.ai";

    public string VoicesPath { get; set; } = "/v2/voices";

    public string TextToSpeechPath { get; set; } = "/v1/text-to-speech";

    public int TimeoutSeconds { get; set; } = 30;
}
