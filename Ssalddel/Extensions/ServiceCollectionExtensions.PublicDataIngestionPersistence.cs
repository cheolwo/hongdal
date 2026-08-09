using Microsoft.EntityFrameworkCore;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddPublicDataIngestionPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PublicDataIngestionDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.MigrationsAssembly("Ssalddel.Infrastructure");
                    mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_PublicDataIngestion");
                    mysqlOptions.EnableRetryOnFailure();
                }));
        return services;
    }
}
