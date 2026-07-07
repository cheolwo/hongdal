using Microsoft.Extensions.Logging;
using Hongdal.Ui.Common.Areas.App.Services;
using MudBlazor.Services;
using OrdererApp.Services;

namespace OrdererApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddHongdalUiCommonAppServices();
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7117/")
        });
        builder.Services.AddScoped<PlatformCommunityService>();
        builder.Services.AddSingleton<IRestaurantSearchPolicyService, HttpRestaurantSearchPolicyService>();
        builder.Services.AddSingleton<IGroupPurchaseShipmentTrackingService, HttpGroupPurchaseShipmentTrackingService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
