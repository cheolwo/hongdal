using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using HongdalAdminApp.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace HongdalAdminApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<AdminAuthSession>();
        builder.Services.AddHongdalUiCommonAppServices<AdminAuthSession>();
        builder.Services.AddTransient<관리자Controller기능모음ViewModel>();
        builder.Services.AddTransient<관리자전체Api기능모음ViewModel>();
        builder.Services.AddHongdalApiHttpClient(HongdalApiEndpoint.ResolveBaseAddress(
            builder.Configuration[HongdalApiEndpoint.ConfigurationKey]));
        builder.Services.AddScoped<AdminAuthService>();
        builder.Services.AddScoped<CommunityManagementAdminService>();
        builder.Services.AddScoped<HongikHakdangAdminService>();
        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
