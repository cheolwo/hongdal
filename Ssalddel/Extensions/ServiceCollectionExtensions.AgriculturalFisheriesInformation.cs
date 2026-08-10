using System.Net;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Ssalddel.Services.AgriculturalFisheries.ImportReadiness;
using Ssalddel.Services.Community;
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
            .AddHttpClient<Mafra공영도매시장경락가격공급자>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                    client.BaseAddress = new Uri(
                        options.DomesticAgriculturalAuctionPrices.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Ssalddel-Domestic-Auction-Price-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<I국내농산물경락가격공급자>(serviceProvider =>
            serviceProvider.GetRequiredService<Mafra공영도매시장경락가격공급자>());
        services.AddScoped<I국내농산물경락가격조회Service,
            국내농산물경락가격조회Service>();
        services.AddScoped<I국내농산물경락가격ArchiveService,
            국내농산물경락가격ArchiveService>();
        services.AddScoped<I농산물지역가격비교QueryService,
            농산물지역가격비교QueryService>();
        services.AddScoped<지역농수산Map가격관측Reader>();
        services.AddScoped<지역농수산Map지역Resolver>();
        services.AddScoped<I지역농수산MapMarker조회UseCase,
            지역농수산MapMarker조회UseCase>();
        services
            .AddHttpClient<INongsaroOpenApiClient, NongsaroOpenApiClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<PublicDataOptions>>()
                        .Value;
                    client.BaseAddress = new Uri(options.Nongsaro.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Ssalddel-Nongsaro-Public-Data-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(
                        Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<I농사로작목기술Module, 농사로작목기술Module>();
        services.AddScoped<I작물기준정보분류조회UseCase,
            작물기준정보분류조회UseCase>();
        services.AddScoped<I공통식품품목Identity조회UseCase,
            공통식품품목Identity조회UseCase>();
        services.AddScoped<I공통식품품목기존Data대조UseCase,
            공통식품품목기존Data대조UseCase>();
        services.AddTransient<I농사로농작업일정Module, 농사로농작업일정Module>();
        services.AddTransient<I농사로농작물재해예방Module,
            농사로농작물재해예방Module>();
        services.AddTransient<I농사로품종정보Module, 농사로품종정보Module>();
        services.AddTransient<I농사로지역문화Module, 농사로지역문화Module>();
        services.AddTransient<I농사로표준사료Module, 농사로표준사료Module>();
        services.TryAddSingleton<I경기데이터드림가축사육MapSnapshotStore,
            경기데이터드림가축사육MapSnapshotStore>();
        services.AddScoped<I경기데이터드림가축사육MapProjectionRefresher,
            경기데이터드림가축사육MapProjectionRefresher>();
        services.AddScoped<I경기데이터드림가축사육MapMarkerReader,
            경기데이터드림가축사육MapMarkerReader>();
        services.AddHostedService<경기데이터드림가축사육MapProjectionRefreshService>();
        services.TryAddSingleton<I선택공공데이터MapSnapshotStore,
            선택공공데이터MapSnapshotStore>();
        services.AddScoped<I선택공공데이터MapProjectionRefresher,
            선택공공데이터MapProjectionRefresher>();
        services.AddScoped<I선택공공데이터MapMarkerReader,
            선택공공데이터MapMarkerReader>();
        services.AddHostedService<선택공공데이터MapProjectionRefreshService>();
        services
            .AddHttpClient<Mof어획구역CatalogSource>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<PublicDataOptions>>()
                        .Value;
                    client.BaseAddress = new Uri(options.MofFishingAreas.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Ssalddel-MOF-Fishing-Area-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(
                        Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddTransient<IMof어획구역CatalogSource>(serviceProvider =>
            serviceProvider.GetRequiredService<Mof어획구역CatalogSource>());
        services.AddScoped<I해양수산Map바다Tile조회UseCase,
            해양수산Map바다Tile조회UseCase>();
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
            .AddHttpClient<IUsdaAmsMarketNewsClient, UsdaAmsMarketNewsClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<PublicDataOptions>>()
                        .Value
                        .UsdaAmsMarketNews;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Ssalddel-USDA-AMS-Market-News-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(
                        Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddScoped<IUsdaAms시장가격ArchiveService,
            UsdaAms시장가격ArchiveService>();
        services
            .AddHttpClient<IUsdaAms공개사업체DirectoryClient,
                UsdaAms공개사업체DirectoryClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<PublicDataOptions>>()
                        .Value
                        .UsdaAmsLocalFoodDirectory;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                        + "AppleWebKit/537.36 (KHTML, like Gecko) "
                        + "Chrome/136.0.0.0 Safari/537.36 "
                        + "Ssalddel-USDA-AMS-Directory-Collector/0.0");
                    client.Timeout = TimeSpan.FromSeconds(
                        Math.Max(30, options.TimeoutSeconds));
                })
            .RemoveAllLoggers();
        services.AddScoped<IUsdaAms공개사업체ArchiveService,
            UsdaAms공개사업체ArchiveService>();
        services.AddScoped<IUsdaAms공개사업체QueryService,
            UsdaAms공개사업체QueryService>();
        services.AddScoped<IKamis중심UsdaAms가격비교QueryService,
            Kamis중심UsdaAms가격비교QueryService>();
        services.AddScoped<IHs식품가격CardCatalogReader,
            Hs식품가격CardCatalogReader>();
        services.AddScoped<IHs식품국가가격CardQueryService,
            Hs식품국가가격CardQueryService>();
        services.AddScoped<IKamis중심같이수입가격QueryService,
            Kamis중심같이수입가격QueryService>();
        services
            .AddHttpClient<Bls평균소매가격ArchiveService>(client =>
            {
                client.BaseAddress = new Uri("https://api.bls.gov/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Ssalddel-BLS-Average-Retail-Price-Collector/0.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .RemoveAllLoggers();
        services.AddScoped<IBls평균소매가격ArchiveService>(serviceProvider =>
            serviceProvider.GetRequiredService<Bls평균소매가격ArchiveService>());
        services
            .AddHttpClient<StatCan평균소매가격공급자>(client =>
            {
                client.BaseAddress = new Uri("https://www150.statcan.gc.ca/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Ssalddel-StatCan-Average-Retail-Price-Collector/0.0");
                client.Timeout = TimeSpan.FromMinutes(3);
            })
            .RemoveAllLoggers();
        services.AddTransient<I국제농수산가격공급자>(serviceProvider =>
            serviceProvider.GetRequiredService<StatCan평균소매가격공급자>());
        services
            .AddHttpClient<Eurostat농산물절대가격공급자>(client =>
            {
                client.BaseAddress = new Uri("https://ec.europa.eu/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Ssalddel-Eurostat-Agricultural-Price-Collector/0.0");
                client.Timeout = TimeSpan.FromMinutes(2);
            })
            .RemoveAllLoggers();
        services.AddTransient<I국제농수산가격공급자>(serviceProvider =>
            serviceProvider.GetRequiredService<Eurostat농산물절대가격공급자>());
        services.AddScoped<I국제농수산가격ArchiveService,
            국제농수산가격ArchiveService>();
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
        services.AddScoped<IKamisDomesticPriceArchiveQueryService,
            KamisDomesticPriceArchiveQueryService>();
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
        services.AddScoped<I농수산물포장Fcl분석Service, 농수산물포장Fcl분석Service>();
        services.AddScoped<IFoodPriceComparisonService, FoodPriceComparisonService>();

        return services;
    }
}
