using DriverApp.Services.CommonContents;
using DriverApp.Services.Samples;
using DriverApp.Services.Security;
using Hongdal.Client.Infrastructure;
using Hongdal.Client.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriverApp.Services;

public static class DriverServiceCollectionExtensions
{
    public static IServiceCollection AddDriverAppServices(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var hasClientDataModeConfig = configuration?.GetSection(ClientDataModeOptions.SectionName).Exists() == true;
        if (configuration is not null)
        {
            services.Configure<ClientDataModeOptions>(configuration.GetSection(ClientDataModeOptions.SectionName));
        }
        else
        {
            services.Configure<ClientDataModeOptions>(_ => { });
        }

        services.PostConfigure<ClientDataModeOptions>(options =>
        {
#if DEBUG
            if (!hasClientDataModeConfig)
            {
                options.AllowSampleFallback = true;
                options.AllowDevelopmentSnapshotFallback = true;
            }
#endif
        });

        services.AddSingleton<DriverAppProfile>();
        services.AddSingleton<IClientSecureTokenStore, MauiSecureTokenStore>();
        services.AddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.AddSingleton<IAuthSession, AuthSession>();
        services.AddSingleton<DriverAccessPolicyService>();
        services.AddSingleton<DriverHomeDisplayPreferenceService>();
        services.AddSingleton<IDriverHomeMapService, DriverHomeMapService>();
        services.AddSingleton<DriverHomeRoutePlanningService>();
        services.AddSingleton<IDriverWorkApiService, DriverWorkApiService>();
#if ANDROID
        services.AddSingleton<I기사위치송신Service, Android기사위치송신Service>();
#else
        services.AddSingleton<I기사위치송신Service, Noop기사위치송신Service>();
#endif
        services.AddSingleton<추천카드표시설정Service>();
        services.AddSingleton<DriverRecommendationDecisionService>();
        services.AddSingleton<IDriverRecommendationDecisionService>(sp => sp.GetRequiredService<DriverRecommendationDecisionService>());
        services.AddSingleton<IDriverRecommendationNotificationService, SampleDriverRecommendationNotificationService>();
        services.AddSingleton<기사샘플데이터Service>();
        services.AddSingleton<ServerBackedDriverSampleDataService>();
        services.AddSingleton<IDriverSampleDataService>(sp => sp.GetRequiredService<ServerBackedDriverSampleDataService>());
        services.AddSingleton<탐색캠페인샘플Service>();
        services.AddSingleton<IDriverExplorationCampaignService>(sp => sp.GetRequiredService<탐색캠페인샘플Service>());
        services.AddSingleton<I공통콘텐츠Service, 샘플공통콘텐츠Service>();
        services.AddSingleton<DriverViewVisibilityService>();
        services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = ApiEnvironment.CreateBaseAddress()
        });
        services.AddScoped<AuthApiService>();
        services.AddSingleton<HttpDriverTransportCompletionPhotoService>();
        services.AddSingleton<IDriverTransportCompletionPhotoService>(sp => sp.GetRequiredService<HttpDriverTransportCompletionPhotoService>());
        services.AddScoped<IApiClient, ApiClient>();

        return services;
    }
}
