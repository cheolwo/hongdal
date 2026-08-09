using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection configuration is required.");
        }

        services.AddDbContext<SsalddelContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.MigrationsAssembly("Ssalddel");
                    mysqlOptions.EnableRetryOnFailure();
                }));

        services.AddTraditionalMarketModule(connectionString);
        services.AddAgriculturalFisheriesPersistence(connectionString);
        services.AddPublicDataIngestionPersistence(connectionString);
        services.AddSsalddelTransientState(configuration);

        var mongoOptions = configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>() ?? new MongoDbOptions();
        var mongoConnectionString = string.IsNullOrWhiteSpace(mongoOptions.ConnectionString)
            ? Environment.GetEnvironmentVariable("MongoDb__ConnectionString")
            : mongoOptions.ConnectionString;
        if (string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            throw new InvalidOperationException("MongoDb:ConnectionString configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(mongoOptions.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        var mongoSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);
        mongoSettings.ApplicationName ??= "Ssalddel";
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings));

        return services;
    }
}
