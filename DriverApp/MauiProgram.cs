using Microsoft.Extensions.Logging;
using DriverApp.Services.CommonContents;
using DriverApp.Services;
using DriverApp.Services.Samples;
using DriverApp.Handlers;
using Hongdal.Ui.Common.Areas.App.Services;
using MudBlazor.Services;

namespace DriverApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<Controls.DriverNativeMapView, DriverNativeMapViewHandler>();
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddDriverAppServices(builder.Configuration);
		builder.Services.AddHongdalUiCommonAppServices();
		builder.Services.AddHongdalDocumentOutputServices();
		builder.Services.AddTransient<NativeDriverHomePage>();
		builder.Services.AddMudServices();
		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
