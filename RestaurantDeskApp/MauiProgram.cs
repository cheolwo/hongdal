using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hongdal.Ui.Common.Areas.App.Services;
using MudBlazor.Services;
using RestaurantDeskApp.Options;
using RestaurantDeskApp.Services;

namespace RestaurantDeskApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.Configure<RestaurantDeskOptions>(builder.Configuration.GetSection(RestaurantDeskOptions.SectionName));
        builder.Services.Configure<RestaurantOrderAlertOptions>(builder.Configuration.GetSection(RestaurantOrderAlertOptions.SectionName));
        builder.Services.AddSingleton<RestaurantDeskSampleService>();
        builder.Services.AddSingleton<I주문알림Service, 주문알림Service>();
        builder.Services.AddSingleton<I음식점주문SignalRClientService, 음식점주문SignalRClientService>();
        builder.Services.AddHongdalUiCommonAppServices();
        builder.Services.AddHongdalDocumentOutputServices();
        builder.Services.AddHttpClient<I음식주문ApiClient, Hongdal음식주문Client>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RestaurantDeskOptions>>().Value;
            client.BaseAddress = options.GetServerBaseAddress();
        });
        builder.Services.AddSingleton<음식점전표DraftFactory>();
        builder.Services.AddSingleton<I음식점주문DeskService, 음식점주문DeskService>();
        builder.Services.AddScoped<배차주소ApiService>();
        builder.Services.AddHongdalApiHttpClient(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RestaurantDeskOptions>>().Value;
            return options.GetServerBaseAddress();
        });
        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
