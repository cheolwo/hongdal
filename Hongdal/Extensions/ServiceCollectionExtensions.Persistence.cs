using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using StackExchange.Redis;
using Hongdal.Services.Security;
using 홍달.Services.Options;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection configuration is required.");
        }

        services.AddDbContext<HongdalContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.MigrationsAssembly("Hongdal");
                    mysqlOptions.EnableRetryOnFailure();
                }));

        services.AddTraditionalMarketModule(connectionString);

        var redisConnectionString = configuration.GetSection(RedisOptions.SectionName).GetValue<string>(nameof(RedisOptions.ConnectionString))
                                    ?? Environment.GetEnvironmentVariable("Redis__ConnectionString");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Redis:ConnectionString configuration is required.");
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IIsmsPTransportKeyStatusStore, RedisIsmsPTransportKeyStatusStore>();

        var mongoOptions = configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>() ?? new MongoDbOptions();
        var mongoConnectionString = string.IsNullOrWhiteSpace(mongoOptions.ConnectionString)
            ? Environment.GetEnvironmentVariable("MongoDb__ConnectionString")
            : mongoOptions.ConnectionString;
        if (string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            throw new InvalidOperationException("MongoDb:ConnectionString configuration is required.");
        }

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

        return services;
    }
}
