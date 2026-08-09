using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData;

namespace Ssalddel.Infrastructure.Persistence.PublicData;

public sealed class PublicDataIngestionDbContext : DbContext
{
    public PublicDataIngestionDbContext(DbContextOptions<PublicDataIngestionDbContext> options)
        : base(options)
    {
    }

    public DbSet<외부데이터수집Run> IngestionRuns => Set<외부데이터수집Run>();
    public DbSet<외부데이터RawSnapshot> RawSnapshots => Set<외부데이터RawSnapshot>();
    public DbSet<외부데이터정규화Record> NormalizedRecords => Set<외부데이터정규화Record>();
    public DbSet<외부지역CodeMapping> RegionMappings => Set<외부지역CodeMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new 외부데이터수집RunConfiguration());
        modelBuilder.ApplyConfiguration(new 외부데이터RawSnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new 외부데이터정규화RecordConfiguration());
        modelBuilder.ApplyConfiguration(new 외부지역CodeMappingConfiguration());
    }
}
