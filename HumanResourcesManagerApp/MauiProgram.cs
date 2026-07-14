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
        builder.Services.AddHongdalApiHttpClient(HongdalApiEndpoint.ResolveBaseAddress(
            builder.Configuration[HongdalApiEndpoint.ConfigurationKey],
            new Uri(HongdalApiEndpoint.LocalDevelopmentBaseAddress)));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
