namespace HongdalAdmin.Options;

public sealed class FoodApiOptions
{
    public const string SectionName = "FoodApi";

    public string BaseUrl { get; set; } = "https://localhost:7264/";
}
