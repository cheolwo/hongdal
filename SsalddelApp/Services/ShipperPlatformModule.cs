using Ssalddel.Client.Infrastructure.Notifications;
using Ssalddel.Client.Infrastructure.Security;
using SsalddelApp.Services.Application;
using SsalddelApp.Services.CommonContents;
using SsalddelApp.Services.Localization;
using SsalddelApp.Services.Samples;
using SsalddelApp.Services.Security;
using Ssalddel.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SsalddelApp.Services;

internal static class ShipperPlatformModule
{
    internal static IServiceCollection AddShipperPlatformModule(this IServiceCollection services)
    {
        services.AddSingleton<IClientSecureTokenStore, MauiSecureTokenStore>();
        services.AddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.AddSingleton<IAuthSession, AuthSession>();
        services.AddSingleton<ISsalddelMobilePushTokenProvider, NullSsalddelMobilePushTokenProvider>();
        services.AddSingleton(provider =>
            new SsalddelMobilePushInstallationClient(
                provider.GetRequiredService<HttpClient>(),
                provider.GetRequiredService<ISsalddelMobilePushTokenProvider>(),
                () => provider.GetRequiredService<IAuthSession>().AccessToken));
        services.AddScoped<AuthApiService>();
        services.AddScoped<LogisticsServiceContractClient>();
        services.AddSingleton<I꾸미기보유권LocalStore, Maui꾸미기보유권LocalStore>();
        services.AddScoped<꾸미기보유권동기화Service>();
        services.AddScoped<ICommunityDecorationPurchaseClient>(provider =>
            provider.GetRequiredService<꾸미기보유권동기화Service>());
        services.AddSingleton<SsalddelClientRoleService>();
        services.AddSingleton<ShipperOperatingProfileService>();
        services.AddSingleton<IWarehouseWorkEntryGateService, SampleWarehouseWorkEntryGateService>();
        services.AddSingleton<ShipperViewVisibilityService>();
        services.AddSingleton<ShipperLocalizationService>();
        services.AddScoped<IShipperExplorationInquiryService, HttpShipperExplorationInquiryService>();
        services.AddScoped<I화주공통콘텐츠Service, Http화주공통콘텐츠Service>();
        services.AddSingleton<IAppEventPublisher, AppEventPublisher>();
        return services;
    }
}
