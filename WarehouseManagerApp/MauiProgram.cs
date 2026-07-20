using Microsoft.Extensions.Logging;
using Ssalddel.Ui.Common.Areas.App.Services;
using MudBlazor.Services;
using WarehouseManagerApp.Services;

namespace WarehouseManagerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddWarehouseManagerApplication();
        builder.Services.AddSsalddelUiCommonAppServices<WarehouseAccessTokenProvider>();
        builder.Services.AddSsalddelApiHttpClient(SsalddelApiEndpoint.ResolveBaseAddress(
            builder.Configuration[SsalddelApiEndpoint.ConfigurationKey],
            new Uri(SsalddelApiEndpoint.LocalDevelopmentBaseAddress)));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
