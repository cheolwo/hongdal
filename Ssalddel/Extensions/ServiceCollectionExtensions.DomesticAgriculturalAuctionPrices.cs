using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomesticAgriculturalAuctionPriceArchive(
        this IServiceCollection services)
    {
        services.AddScoped<I국내농산물경락가격ArchiveService,
            국내농산물경락가격ArchiveService>();
        return services;
    }
}
