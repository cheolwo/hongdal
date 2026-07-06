using Microsoft.Extensions.Logging;
using Hongdal.Ui.Common.Areas.App.Services;
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
        builder.Services.AddHongdalUiCommonAppServices();
        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
        builder.Services.AddScoped<PlatformCommunityService>();
        builder.Services.AddScoped<PlatformHomeModeStateService>();
        builder.Services.AddSingleton<IWarehouseWorkEntryGateService, SampleWarehouseWorkEntryGateService>();
        builder.Services.AddSingleton<IInboundReceivingWorkflowService, SampleInboundReceivingWorkflowService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
