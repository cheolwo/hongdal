using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Microsoft.EntityFrameworkCore;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddAgriculturalFisheriesPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AgriculturalFisheriesDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.MigrationsAssembly("Ssalddel.Infrastructure");
                    mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_AgriculturalFisheries");
                    mysqlOptions.EnableRetryOnFailure();
                }));
        return services;
    }
}
