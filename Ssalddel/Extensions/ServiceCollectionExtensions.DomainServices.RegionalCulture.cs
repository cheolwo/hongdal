using Ssalddel.Services.Content;
using 살뜰.Services.Images;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddSsalddelRegionalCultureDomainServices(
        this IServiceCollection services)
    {
        services.AddSingleton<I이미지프롬프트생성기, 지역문화애니메이션프롬프트생성기>();
        services.AddScoped<I샘플이미지대상Resolver, 지역문화이미지대상Resolver>();
        services.AddScoped<I지역문화이미지순차생성Service, 지역문화이미지순차생성Service>();
        services.AddScoped<I지역문화이미지생성관리UseCase, 지역문화이미지생성관리UseCase>();
        services.AddHostedService<지역문화이미지생성Worker>();

        return services;
    }
}
