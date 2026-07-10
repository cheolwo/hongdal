namespace RestaurantDeskApp.Options;

public sealed class RestaurantDeskOptions
{
    public const string SectionName = "RestaurantDesk";

    public long RestaurantId { get; set; } = 101;

    public string ServerBaseUrl { get; set; } = "https://localhost:7117/";
}
