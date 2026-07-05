namespace CustomsBrokerApp.Options;

public sealed class CustomsBrokerApiOptions
{
    public const string SectionName = "CustomsBrokerApi";

    public string BaseUrl { get; set; } = "https://localhost:7117/";
}
