using Hongdal.Services.AgriculturalFisheries.Information;
using Hongdal.Services.AgriculturalFisheries.ImportReadiness;
using Microsoft.Extensions.Options;
using 홍달.Services.External.PublicData;
using 홍달.Services.Options;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgriculturalFisheriesInformationModule(
        this IServiceCollection services)
    {
        services.AddHttpClient<IAtDomesticFoodPriceLookupService, AtDomesticFoodPriceLookupService>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                client.BaseAddress = new Uri(options.AtFoodPrices.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            });
        services
            .AddHttpClient<UsdaNassQuickStats가격공급자>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.UsdaNassQuickStats.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<I미국농수산가격공급자>(serviceProvider =>
            serviceProvider.GetRequiredService<UsdaNassQuickStats가격공급자>());
        services.AddSingleton<IFoodPriceCrosswalkCatalog, FoodPriceCrosswalkCatalog>();
        services.AddScoped<IAgriculturalFisheriesInformationService, AgriculturalFisheriesInformationService>();
        services.AddScoped<I미국농수산가격조회Service, 미국농수산가격조회Service>();
        services.AddScoped<IMeatImportReadinessService, MeatImportReadinessService>();
        services.AddScoped<IFoodPriceComparisonService, FoodPriceComparisonService>();

        return services;
    }
}
