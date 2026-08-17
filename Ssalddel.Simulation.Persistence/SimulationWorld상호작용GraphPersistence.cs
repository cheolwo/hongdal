using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld상호작용Graph준비도Entity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public int AreaSetRevision { get; set; }
    public string AreaSetDefinitionHashSha256 { get; set; } = string.Empty;
    public string BindingCatalogRevision { get; set; } = string.Empty;
    public string BindingCatalogHashSha256 { get; set; } = string.Empty;
    public string OverallStatusCode { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld상호작용GraphReadinessStore(
    SimulationWorld파생DbContext dbContext) : ISimulationWorld상호작용GraphReadinessStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task ReplaceAsync(
        SimulationWorld상호작용Graph준비도Response readiness,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(readiness);
        var payload = JsonSerializer.Serialize(readiness, JsonOptions);
        var entity = await dbContext.WorldInteractionGraphReadiness.SingleOrDefaultAsync(
            item => item.AreaSetStableId == readiness.AreaSetStableId,
            cancellationToken);
        if (entity != null
            && string.Equals(entity.AreaSetDefinitionHashSha256,
                readiness.AreaSetDefinitionHashSha256, StringComparison.Ordinal)
            && string.Equals(entity.BindingCatalogHashSha256,
                readiness.BindingCatalogHashSha256, StringComparison.Ordinal)
            && string.Equals(entity.PayloadJson, payload, StringComparison.Ordinal))
            return;
        if (entity == null)
        {
            entity = new SimulationWorld상호작용Graph준비도Entity
            {
                AreaSetStableId = readiness.AreaSetStableId,
            };
            dbContext.WorldInteractionGraphReadiness.Add(entity);
        }
        entity.AreaSetRevision = readiness.AreaSetRevision;
        entity.AreaSetDefinitionHashSha256 = readiness.AreaSetDefinitionHashSha256;
        entity.BindingCatalogRevision = readiness.BindingCatalogRevision;
        entity.BindingCatalogHashSha256 = readiness.BindingCatalogHashSha256;
        entity.OverallStatusCode = readiness.OverallStatusCode;
        entity.PayloadJson = payload;
        entity.StoredAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SimulationWorld상호작용Graph준비도Response?> ReadLatestAsync(
        string areaSetStableId,
        CancellationToken cancellationToken = default)
    {
        var payload = await dbContext.WorldInteractionGraphReadiness.AsNoTracking()
            .Where(item => item.AreaSetStableId == areaSetStableId)
            .Select(item => item.PayloadJson)
            .SingleOrDefaultAsync(cancellationToken);
        return payload == null
            ? null
            : JsonSerializer.Deserialize<SimulationWorld상호작용Graph준비도Response>(
                payload, JsonOptions);
    }
}

internal sealed class SimulationWorld상호작용Graph준비도Configuration :
    IEntityTypeConfiguration<SimulationWorld상호작용Graph준비도Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld상호작용Graph준비도Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_WI공간Graph준비도");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.AreaSetStableId).IsUnique();
        builder.Property(item => item.Id).HasColumnName("식별번호");
        builder.Property(item => item.AreaSetStableId).HasColumnName("AreaSet고유식별자")
            .HasMaxLength(200).IsRequired();
        builder.Property(item => item.AreaSetRevision).HasColumnName("AreaSet개정");
        builder.Property(item => item.AreaSetDefinitionHashSha256)
            .HasColumnName("AreaSet정의SHA256").HasMaxLength(64).IsRequired();
        builder.Property(item => item.BindingCatalogRevision)
            .HasColumnName("공간연결대장개정").HasMaxLength(100).IsRequired();
        builder.Property(item => item.BindingCatalogHashSha256)
            .HasColumnName("공간연결대장SHA256").HasMaxLength(64).IsRequired();
        builder.Property(item => item.OverallStatusCode)
            .HasColumnName("종합상태코드").HasMaxLength(80).IsRequired();
        builder.Property(item => item.PayloadJson).HasColumnName("준비도JSON")
            .HasColumnType("longtext").IsRequired();
        builder.Property(item => item.StoredAtUtc).HasColumnName("저장시각UTC");
    }
}
