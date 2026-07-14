using Hongdal.Ui.Common.Areas.App.Services;

namespace RestaurantDeskApp.Options;

public sealed class RestaurantDeskOptions
{
    public const string SectionName = "RestaurantDesk";

    public long RestaurantId { get; set; } = 101;

    public string ServerBaseUrl { get; set; } = HongdalApiEndpoint.LocalDevelopmentBaseAddress;

    public Uri GetServerBaseAddress()
        => HongdalApiEndpoint.ResolveBaseAddress(
            ServerBaseUrl,
            new Uri(HongdalApiEndpoint.LocalDevelopmentBaseAddress));
}
