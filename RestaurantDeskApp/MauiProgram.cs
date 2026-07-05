using Microsoft.Extensions.Logging;
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

        builder.Services.Configure<FoodApiOptions>(builder.Configuration.GetSection(FoodApiOptions.SectionName));
        builder.Services.AddSingleton<RestaurantDeskSampleService>();
        builder.Services.AddSingleton<I주문알림Service, 주문알림Service>();
        builder.Services.AddHttpClient<배차주소ApiService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FoodApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
        builder.Services.AddScoped<PlatformCommunityService>();
        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
