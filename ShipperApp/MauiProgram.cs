using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using ShipperApp.Options;
using ShipperApp.Services.CommonContents;
using ShipperApp.Services;
using ShipperApp.Services.Localization;
using ShipperApp.Services.Samples;
using Hongdal.Ui.Common.Areas.App.Services;

namespace ShipperApp;

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

		builder.Services.AddShipperAppServices(builder.Configuration);
		builder.Services.AddHongdalUiCommonAppServices<IAuthSession>();
		builder.Services.AddHongdalDocumentOutputServices();
		builder.Services.AddHongdalApiHttpClient(HongdalApiEndpoint.ResolveBaseAddress(
			builder.Configuration[HongdalApiEndpoint.ConfigurationKey],
			new Uri(HongdalApiEndpoint.LocalDevelopmentBaseAddress)));
		builder.Services.AddMudServices();
		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
