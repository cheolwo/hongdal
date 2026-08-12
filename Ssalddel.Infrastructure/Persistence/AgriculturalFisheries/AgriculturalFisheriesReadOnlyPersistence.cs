using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

public static class AgriculturalFisheriesReadOnlyPersistence
{
    public static IServiceCollection AddAgriculturalFisheriesReadOnlyPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<AgriculturalFisheriesReadOnlySaveChangesInterceptor>();
        services.AddDbContext<AgriculturalFisheriesDbContext>((serviceProvider, options) =>
            options
                .UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 4, 0)),
                    mysqlOptions =>
                    {
                        mysqlOptions.MigrationsAssembly("Ssalddel.Infrastructure");
                        mysqlOptions.MigrationsHistoryTable(
                            "__EFMigrationsHistory_AgriculturalFisheries");
                        mysqlOptions.EnableRetryOnFailure();
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .AddInterceptors(serviceProvider.GetRequiredService<
                    AgriculturalFisheriesReadOnlySaveChangesInterceptor>()));

        return services;
    }
}

public sealed class AgriculturalFisheriesReadOnlySaveChangesInterceptor
    : SaveChangesInterceptor
{
    public const string ErrorCode = "AgriculturalFisheriesReadOnlyWriteForbidden";

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
        => throw new InvalidOperationException(ErrorCode);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(ErrorCode);
}
