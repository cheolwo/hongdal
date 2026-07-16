using Microsoft.Extensions.Logging;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using MudBlazor.Services;
using WarehouseManagerApp.Services;
using WarehouseManagerApp.ViewModels;

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
        builder.Services.AddTransient<창고Controller기능모음ViewModel>();
        builder.Services.AddTransient<창고기준정보업무ViewModel>();
        builder.Services.AddTransient<창고입고업무ViewModel>();
        builder.Services.AddTransient<창고재고출고업무ViewModel>();
        builder.Services.AddTransient<창고운송연계업무ViewModel>();
        builder.Services.AddTransient<창고작업기능ViewModel>();
        builder.Services.AddTransient<창고Api기능모음ViewModel>();
        builder.Services.AddHongdalApiHttpClient(HongdalApiEndpoint.ResolveBaseAddress(
            builder.Configuration[HongdalApiEndpoint.ConfigurationKey],
            new Uri(HongdalApiEndpoint.LocalDevelopmentBaseAddress)));
        builder.Services.AddSingleton<IWarehouseWorkEntryGateService, SampleWarehouseWorkEntryGateService>();
        builder.Services.AddSingleton<IInboundReceivingWorkflowService, SampleInboundReceivingWorkflowService>();
        builder.Services.AddSingleton<IWarehousePickingBatchWorkspaceService, SampleWarehousePickingBatchWorkspaceService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
