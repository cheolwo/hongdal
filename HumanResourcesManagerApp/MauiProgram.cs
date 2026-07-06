using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace HumanResourcesManagerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddHongdalUiCommonAppServices();
        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
        builder.Services.AddScoped<PlatformCommunityService>();
        builder.Services.AddScoped<PlatformHomeModeStateService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
