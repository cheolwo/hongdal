using Ssalddel.Ui.Common.Areas.App.Services;

namespace RestaurantDeskApp.Options;

public sealed class RestaurantDeskOptions
{
    public const string SectionName = "RestaurantDesk";

    public long RestaurantId { get; set; } = 101;

    public string ServerBaseUrl { get; set; } = SsalddelApiEndpoint.LocalDevelopmentBaseAddress;

    public Uri GetServerBaseAddress()
        => SsalddelApiEndpoint.ResolveBaseAddress(
            ServerBaseUrl,
            new Uri(SsalddelApiEndpoint.LocalDevelopmentBaseAddress));
}
