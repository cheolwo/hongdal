using Ssalddel.Ui.Common.Areas.App.Services;

namespace RestaurantDeskApp.Options;

public sealed class RestaurantDeskOptions
{
    public const string SectionName = "RestaurantDesk";

    public long RestaurantId { get; set; } = 101;

    public string RestaurantName { get; set; } = "살뜰 식당";

    public string RestaurantAddress { get; set; } = string.Empty;

    public string RestaurantDetailAddress { get; set; } = string.Empty;

    public decimal? RestaurantLatitude { get; set; }

    public decimal? RestaurantLongitude { get; set; }

    public int DefaultPreparationMinutes { get; set; } = 20;

    public string ServerBaseUrl { get; set; } = SsalddelApiEndpoint.LocalDevelopmentBaseAddress;

    public Uri GetServerBaseAddress()
        => SsalddelApiEndpoint.ResolveBaseAddress(
            ServerBaseUrl,
            new Uri(SsalddelApiEndpoint.LocalDevelopmentBaseAddress));
}
