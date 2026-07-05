using DriverApp.Services.CommonContents;
using DriverApp.Services.Samples;
using DriverApp.Services.Security;
using Hongdal.Client.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace DriverApp.Services;

public static class DriverServiceCollectionExtensions
{
    public static IServiceCollection AddDriverAppServices(this IServiceCollection services)
    {
        services.AddSingleton<DriverAppProfile>();
        services.AddSingleton<IClientSecureTokenStore, MauiSecureTokenStore>();
        services.AddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.AddSingleton<IAuthSession, AuthSession>();
        services.AddSingleton<DriverAccessPolicyService>();
        services.AddSingleton<DriverHomeDisplayPreferenceService>();
        services.AddSingleton<IDriverHomeMapService, DriverHomeMapService>();
        services.AddSingleton<추천카드표시설정Service>();
        services.AddSingleton<DriverRecommendationDecisionService>();
        services.AddSingleton<IDriverRecommendationDecisionService>(sp => sp.GetRequiredService<DriverRecommendationDecisionService>());
        services.AddSingleton<기사샘플데이터Service>();
        services.AddSingleton<IDriverSampleDataService>(sp => sp.GetRequiredService<기사샘플데이터Service>());
        services.AddSingleton<탐색캠페인샘플Service>();
        services.AddSingleton<IDriverExplorationCampaignService>(sp => sp.GetRequiredService<탐색캠페인샘플Service>());
        services.AddSingleton<I공통콘텐츠Service, 샘플공통콘텐츠Service>();
        services.AddSingleton<DriverViewVisibilityService>();
        services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = ApiEnvironment.CreateBaseAddress()
        });
        services.AddSingleton<HttpDriverTransportCompletionPhotoService>();
        services.AddSingleton<IDriverTransportCompletionPhotoService, SampleDriverTransportCompletionPhotoService>();
        services.AddScoped<IApiClient, ApiClient>();

        return services;
    }
}
