using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Persistence;

public static class SimulationSharedPublicDataPersistence
{
    public static IServiceCollection AddSimulationSharedPublicDataPersistence(
        this IServiceCollection services,
        string connectionString,
        int maxItems)
    {
        services.AddAgriculturalFisheriesReadOnlyPersistence(connectionString);
        services.AddSingleton(Options.Create(
            new SimulationSharedPublicDataQueryOptions { MaxItems = maxItems }));
        services.AddScoped<ISimulation공유공공데이터조회Port,
            Simulation공유공공데이터Reader>();
        return services;
    }
}

public sealed class SimulationSharedPublicDataQueryOptions
{
    public int MaxItems { get; set; } = 50;
}

public sealed class Simulation공유공공데이터Reader(
    AgriculturalFisheriesDbContext dbContext,
    IOptions<SimulationSharedPublicDataQueryOptions> optionsAccessor)
    : ISimulation공유공공데이터조회Port
{
    public async Task<Simulation공유공공데이터조회결과> Kamis가격관측조회Async(
        string? itemName,
        int limit,
        CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;
        var safeLimit = Math.Clamp(limit, 1, options.MaxItems);
        var query = dbContext.KamisPriceObservations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            var normalizedItemName = itemName.Trim();
            query = query.Where(item => item.ItemName == normalizedItemName);
        }

        var rows = await query
            .OrderByDescending(item => item.SurveyDate)
            .ThenBy(item => item.ItemName)
            .ThenBy(item => item.KindName)
            .ThenBy(item => item.RankName)
            .Take(safeLimit)
            .Select(item => new
            {
                item.RecordKey,
                item.SurveyDate,
                item.ItemName,
                item.ItemCode,
                item.KindName,
                item.KindCode,
                item.RankName,
                item.Unit,
                item.PriceKrw,
                item.IsPriceMissing,
                item.SourcePackageLabel,
                item.SourceUrl,
                item.LastSeenAtUtc,
            })
            .ToListAsync(cancellationToken);
        var items = rows
            .Select(item => new SimulationKamis가격관측
            {
                StableId = "public-data:kamis:" + item.RecordKey,
                SurveyDate = item.SurveyDate.ToString("yyyy-MM-dd"),
                ItemName = item.ItemName,
                ItemCode = item.ItemCode,
                KindName = item.KindName,
                KindCode = item.KindCode,
                RankName = item.RankName,
                Unit = item.Unit,
                PriceKrw = item.PriceKrw,
                IsPriceMissing = item.IsPriceMissing,
                SourcePackageLabel = item.SourcePackageLabel,
                SourceUrl = item.SourceUrl,
                LastSeenAtUtc = new DateTimeOffset(
                    DateTime.SpecifyKind(item.LastSeenAtUtc, DateTimeKind.Utc)),
            })
            .ToArray();

        return new Simulation공유공공데이터조회결과
        {
            ReferenceTimeUtc = items.Length == 0
                ? null
                : items.Max(item => item.LastSeenAtUtc),
            Items = items,
        };
    }
}
