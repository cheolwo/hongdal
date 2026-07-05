namespace Hongdal.BackOffice.Client.Configuration;

public sealed class BackOfficeApiOptions
{
    public const string SectionName = "BackOfficeApi";

    public string BaseUrl { get; set; } = string.Empty;
}
