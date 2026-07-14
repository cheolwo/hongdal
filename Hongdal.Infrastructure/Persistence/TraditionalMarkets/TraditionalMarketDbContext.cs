using Hongdal.Domain.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Infrastructure.Persistence.TraditionalMarkets;

public sealed class TraditionalMarketDbContext : DbContext
{
    public TraditionalMarketDbContext(DbContextOptions<TraditionalMarketDbContext> options)
        : base(options)
    {
    }

    public DbSet<TraditionalMarket> Markets => Set<TraditionalMarket>();
    public DbSet<TraditionalMarketSyncRun> SyncRuns => Set<TraditionalMarketSyncRun>();
    public DbSet<TraditionalMarketLogisticsHub> LogisticsHubs => Set<TraditionalMarketLogisticsHub>();
    public DbSet<전통시장생활권협의체> NeighborhoodCouncils => Set<전통시장생활권협의체>();
    public DbSet<전통시장교역안건> TradeAgendas => Set<전통시장교역안건>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TraditionalMarketConfiguration());
        modelBuilder.ApplyConfiguration(new TraditionalMarketSyncRunConfiguration());
        modelBuilder.ApplyConfiguration(new TraditionalMarketLogisticsHubConfiguration());
        modelBuilder.ApplyConfiguration(new 전통시장생활권협의체Configuration());
        modelBuilder.ApplyConfiguration(new 전통시장교역안건Configuration());
    }
}
