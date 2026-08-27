using Microsoft.Extensions.Options;
using Ssalddel.Services.External.Apify;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApifyInteriorProductObservation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApifyPlatform(configuration);
        services.Configure<ApifyInteriorProductsOptions>(
            configuration.GetSection(ApifyInteriorProductsOptions.SectionName));
        services.PostConfigure<ApifyOptions>(options =>
        {
            var productOptions = configuration
                .GetSection(ApifyInteriorProductsOptions.SectionName)
                .Get<ApifyInteriorProductsOptions>() ?? new ApifyInteriorProductsOptions();
            options.AllowedActorIds = (options.AllowedActorIds ?? [])
                .Concat(productOptions.Sources.Select(value => value.ActorId))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        });
        services.AddScoped<IApify상품관측Collector, Apify상품관측Collector>();
        services.AddSingleton<IInteriorProductRawObservationStore, FileInteriorProductRawObservationStore>();
        services.AddSingleton<IApifyInteriorProductNormalizer, ApifyAmazonInteriorProductNormalizer>();
        services.AddSingleton<IApifyInteriorProductNormalizer, ApifyAlibabaInteriorProductNormalizer>();
        services.AddSingleton<InteriorReferenceApprovalService>();
        return services;
    }
}
