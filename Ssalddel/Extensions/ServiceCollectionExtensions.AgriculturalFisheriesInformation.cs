using System.Net;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Ssalddel.Services.AgriculturalFisheries.ImportReadiness;
using Ssalddel.Services.FoodCulture;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgriculturalFisheriesInformationModule(
        this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
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
        services
            .AddHttpClient<AbsConsumerPriceIndex식품가격공급자>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.AbsConsumerPriceIndex.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<I호주농수산식품가격공급자>(serviceProvider =>
            serviceProvider.GetRequiredService<AbsConsumerPriceIndex식품가격공급자>());
        services
            .AddHttpClient<UsdaNassPriceArchiveService>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.UsdaNassQuickStats.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddScoped<IUsdaNassPriceArchiveService>(serviceProvider =>
            serviceProvider.GetRequiredService<UsdaNassPriceArchiveService>());
        services
            .AddHttpClient<IKamisJsonClient, KamisJsonClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.Kamis.BaseUrl);
                    client.DefaultRequestVersion = HttpVersion.Version11;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-KAMIS-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(90, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddScoped<IKamisPriceArchiveService, KamisPriceArchiveService>();
        services
            .AddHttpClient<MfdsCookRecipeRemoteSource>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.MfdsCookRecipe.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Official-Food-Recipe-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<IOfficialFoodRecipeRemoteSource>(serviceProvider =>
            serviceProvider.GetRequiredService<MfdsCookRecipeRemoteSource>());
        services
            .AddHttpClient<IOfficialFoodIngredientDomesticCompanySource,
                MfdsIngredientProductCompanySource>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.MfdsIngredientCompanies.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Ingredient-Company-Research/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(20, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services
            .AddHttpClient<RdaLocalFoodRemoteSource>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.RdaLocalFood.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Official-Food-Recipe-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(90, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<IOfficialFoodRecipeRemoteSource>(serviceProvider =>
            serviceProvider.GetRequiredService<RdaLocalFoodRemoteSource>());
        services
            .AddHttpClient<MaffRegionalCuisineRemoteSource>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.MaffRegionalCuisine.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Official-Food-Recipe-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<IOfficialFoodRecipeRemoteSource>(serviceProvider =>
            serviceProvider.GetRequiredService<MaffRegionalCuisineRemoteSource>());
        services
            .AddHttpClient<NhsHealthierFamiliesRecipeRemoteSource>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(options.NhsHealthierFamiliesRecipes.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Official-Food-Recipe-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<IOfficialFoodRecipeRemoteSource>(serviceProvider =>
            serviceProvider.GetRequiredService<NhsHealthierFamiliesRecipeRemoteSource>());
        services.AddSingleton<OfficialFoodRecipeIngredientParser>();
        services.AddSingleton<IOfficialFoodIngredientPriceMatchCatalog,
            OfficialFoodIngredientPriceMatchCatalog>();
        services.AddScoped<IOfficialFoodIngredientPublicPriceService,
            OfficialFoodIngredientPublicPriceService>();
        services.AddScoped<IOfficialFoodRecipeIngredientIndexService,
            OfficialFoodRecipeIngredientIndexService>();
        services.AddScoped<IOfficialFoodRecipeArchiveService, OfficialFoodRecipeArchiveService>();
        services.AddScoped<IOfficialFoodIngredientImportedCompanySource,
            MfdsImportedFoodIngredientCompanySource>();
        services.AddScoped<IOfficialFoodIngredientCompanyResearchService,
            OfficialFoodIngredientCompanyResearchService>();
        services.AddScoped<IOfficialFoodIngredientCompanyArchiveService,
            OfficialFoodIngredientCompanyArchiveService>();
        services.AddScoped<IChinaImportedFoodRegionCommunityPostSource,
            ChinaImportedFoodRegionCommunityPostSource>();
        services.AddScoped<IUnitedStatesImportedFoodStateCommunityPostSource,
            UnitedStatesImportedFoodStateCommunityPostSource>();
        services.AddScoped<IOfficialFoodIngredientHsMappingService,
            OfficialFoodIngredientHsMappingService>();
        services.AddSingleton<IFoodPriceCrosswalkCatalog, FoodPriceCrosswalkCatalog>();
        services.AddScoped<IAgriculturalFisheriesInformationService, AgriculturalFisheriesInformationService>();
        services.AddScoped<I미국농수산가격조회Service, 미국농수산가격조회Service>();
        services.AddScoped<I호주농수산식품가격조회Service, 호주농수산식품가격조회Service>();
        services.AddSingleton<I미국농어업경영체정보원천Service,
            미국농어업경영체정보원천Service>();
        services.AddScoped<IMeatImportReadinessService, MeatImportReadinessService>();
        services.AddScoped<IFoodPriceComparisonService, FoodPriceComparisonService>();

        return services;
    }
}
