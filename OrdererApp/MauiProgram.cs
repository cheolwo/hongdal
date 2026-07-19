using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using MudBlazor.Services;
using OrdererApp.Services;
using OrdererApp.ViewModels;

namespace OrdererApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSsalddelUiCommonAppServices();
        builder.Services.AddTransient<주문자Controller기능모음ViewModel>();
        builder.Services.AddTransient<음식Controller기능모음ViewModel>();
        builder.Services.AddTransient<주문자공동구매기능ViewModel>();
        builder.Services.AddTransient<주문자음식점탐색기능ViewModel>();
        builder.Services.AddTransient<주문자Api기능모음ViewModel>();
        builder.Services.AddSsalddelApiHttpClient(
            SsalddelApiEndpoint.ResolveBaseAddress(
                builder.Configuration[SsalddelApiEndpoint.ConfigurationKey],
                new Uri(SsalddelApiEndpoint.LocalDevelopmentBaseAddress)),
            ServiceLifetime.Singleton);
        builder.Services.AddSingleton<IRestaurantSearchPolicyService, HttpRestaurantSearchPolicyService>();
        builder.Services.AddSingleton<IGroupPurchaseShipmentTrackingService, HttpGroupPurchaseShipmentTrackingService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
