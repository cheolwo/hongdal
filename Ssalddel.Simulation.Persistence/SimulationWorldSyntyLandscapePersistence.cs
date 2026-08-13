using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorldSynty경관RunEntity
{
    public long Id { get; set; }
    public int SchemaVersion { get; set; }
    public string VisualBuildStableId { get; set; } = string.Empty;
    public string JobStableId { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string ScopeKindCode { get; set; } = string.Empty;
    public string ScopeStableId { get; set; } = string.Empty;
    public string LandscapeRuleRevision { get; set; } = string.Empty;
    public string VisualCatalogRevision { get; set; } = string.Empty;
    public string UrpProfileCatalogRevision { get; set; } = string.Empty;
    public int Seed { get; set; }
    public string TargetPlatformCode { get; set; } = string.Empty;
    public string QualityTierCode { get; set; } = string.Empty;
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
    public string StatusCode { get; set; } = string.Empty;
}

public sealed class SimulationWorldSynty그래픽표현Entity
{
    public long Id { get; set; }
    public long VisualRunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string PresentationScopeCode { get; set; } = string.Empty;
    public string TextureSetKey { get; set; } = string.Empty;
    public string MaterialVariantKey { get; set; } = string.Empty;
    public string ColorPaletteKey { get; set; } = string.Empty;
    public string BackgroundProfileKey { get; set; } = string.Empty;
    public string LightingProfileKey { get; set; } = string.Empty;
    public string TimeOfDayProfileKey { get; set; } = string.Empty;
    public string ShadowPolicyCode { get; set; } = string.Empty;
    public bool CastShadows { get; set; }
    public bool ReceiveShadows { get; set; }
    public decimal ContactShadowStrength { get; set; }
    public decimal? ShadowDistanceMeters { get; set; }
    public decimal AmbientOcclusionStrength { get; set; }
    public string LodCode { get; set; } = string.Empty;
    public string QualityTierCode { get; set; } = string.Empty;
    public bool PresentationOnly { get; set; }
    public SimulationWorldSynty경관RunEntity VisualRun { get; set; } = null!;
}

public sealed class SimulationWorldSynty시각배치Entity
{
    public long Id { get; set; }
    public long VisualRunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string VisualKey { get; set; } = string.Empty;
    public string LodCode { get; set; } = string.Empty;
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal PositionZ { get; set; }
    public decimal RotationY { get; set; }
    public decimal UniformScale { get; set; }
    public bool PresentationOnly { get; set; }
    public SimulationWorldSynty경관RunEntity VisualRun { get; set; } = null!;
}

public sealed class SimulationWorldSynty배치거부Entity
{
    public long Id { get; set; }
    public long VisualRunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string? TargetNodeStableId { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public SimulationWorldSynty경관RunEntity VisualRun { get; set; } = null!;
}

public sealed class SimulationWorld공간실행Reader(
    SimulationWorld파생DbContext dbContext) : ISimulationWorld공간실행Reader
{
    public async Task<SimulationWorld공간실행Snapshot?> 조회Async(
        string buildStableId,
        CancellationToken cancellationToken)
    {
        var run = await dbContext.Runs.AsNoTracking()
            .SingleOrDefaultAsync(item => item.BuildStableId == buildStableId, cancellationToken);
        if (run == null)
            return null;
        var nodes = await dbContext.Nodes.AsNoTracking()
            .Where(item => item.RunId == run.Id)
            .OrderBy(item => item.StableId)
            .Select(item => new SimulationWorld파생Node
            {
                StableId = item.StableId,
                NodeKindCode = item.NodeKindCode,
                SourceStableId = item.SourceStableId,
                SourceRecordStableId = item.SourceRecordStableId,
                EvidenceKindCode = item.EvidenceKindCode,
                RegionCode = item.RegionCode,
                TileKey = item.TileKey,
                AreaStableId = item.AreaStableId,
                DisplayName = item.DisplayName,
                RepresentativeGroupCode = item.RepresentativeGroupCode,
                RepresentedRecordCount = item.RepresentedRecordCount,
                RepresentativeRank = item.RepresentativeRank,
            })
            .ToListAsync(cancellationToken);
        return new SimulationWorld공간실행Snapshot
        {
            BuildStableId = run.BuildStableId,
            AreaSetStableId = run.AreaSetStableId,
            OutputHashSha256 = run.OutputHashSha256,
            Nodes = nodes,
            BuildingPlacementCount = await dbContext.BuildingPlacements.AsNoTracking()
                .CountAsync(item => item.RunId == run.Id, cancellationToken),
            UnityArtifactCount = await dbContext.UnityArtifacts.AsNoTracking()
                .CountAsync(item => item.RunId == run.Id
                    && item.StatusCode == SimulationWorldUnity산출물상태Codes.완료, cancellationToken),
        };
    }
}

public sealed class SimulationWorldSynty경관Store(
    SimulationWorld파생DbContext dbContext) : ISimulationWorldSynty경관Store
{
    public const string ConflictCode = "SimulationWorldSyntyLandscapeConflict";

    public async Task<SimulationWorldSynty경관저장결과> 저장Async(
        SimulationWorldSynty경관실행원장 ledger,
        CancellationToken cancellationToken)
    {
        SimulationWorldSynty경관Validator.Validate(ledger);
        var outputHash = SimulationWorldSynty경관Hash.Compute(ledger);
        var existing = await dbContext.SyntyLandscapeRuns.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.VisualBuildStableId == ledger.VisualBuildStableId,
                cancellationToken);
        if (existing != null)
        {
            if (!string.Equals(existing.InputFingerprintSha256, ledger.InputFingerprintSha256,
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.OutputHashSha256, outputHash,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(ConflictCode);
            return Result(false, ledger, outputHash);
        }

        var run = new SimulationWorldSynty경관RunEntity
        {
            SchemaVersion = ledger.SchemaVersion,
            VisualBuildStableId = ledger.VisualBuildStableId,
            JobStableId = ledger.JobStableId,
            SpatialBuildStableId = ledger.SpatialBuildStableId,
            SpatialOutputHashSha256 = ledger.SpatialOutputHashSha256,
            AreaSetStableId = ledger.AreaSetStableId,
            ScopeKindCode = ledger.ScopeKindCode,
            ScopeStableId = ledger.ScopeStableId,
            LandscapeRuleRevision = ledger.LandscapeRuleRevision,
            VisualCatalogRevision = ledger.VisualCatalogRevision,
            UrpProfileCatalogRevision = ledger.UrpProfileCatalogRevision,
            Seed = ledger.Seed,
            TargetPlatformCode = ledger.TargetPlatformCode,
            QualityTierCode = ledger.QualityTierCode,
            InputFingerprintSha256 = ledger.InputFingerprintSha256,
            OutputHashSha256 = outputHash,
            GeneratedAtUtc = ledger.GeneratedAtUtc,
            StoredAtUtc = DateTimeOffset.UtcNow,
            StatusCode = ledger.StatusCode,
        };
        dbContext.SyntyLandscapeRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.SyntyGraphicsPlans.AddRange(ledger.GraphicsPlans.Select(item =>
            new SimulationWorldSynty그래픽표현Entity
            {
                VisualRunId = run.Id,
                StableId = item.StableId,
                TargetNodeStableId = item.TargetNodeStableId,
                PresentationScopeCode = item.PresentationScopeCode,
                TextureSetKey = item.TextureSetKey,
                MaterialVariantKey = item.MaterialVariantKey,
                ColorPaletteKey = item.ColorPaletteKey,
                BackgroundProfileKey = item.BackgroundProfileKey,
                LightingProfileKey = item.LightingProfileKey,
                TimeOfDayProfileKey = item.TimeOfDayProfileKey,
                ShadowPolicyCode = item.ShadowPolicyCode,
                CastShadows = item.CastShadows,
                ReceiveShadows = item.ReceiveShadows,
                ContactShadowStrength = item.ContactShadowStrength,
                ShadowDistanceMeters = item.ShadowDistanceMeters,
                AmbientOcclusionStrength = item.AmbientOcclusionStrength,
                LodCode = item.LodCode,
                QualityTierCode = item.QualityTierCode,
                PresentationOnly = item.PresentationOnly,
            }));
        dbContext.SyntyVisualPlacements.AddRange(ledger.VisualPlacements.Select(item =>
            new SimulationWorldSynty시각배치Entity
            {
                VisualRunId = run.Id,
                StableId = item.StableId,
                TargetNodeStableId = item.TargetNodeStableId,
                VisualKey = item.VisualKey,
                LodCode = item.LodCode,
                PositionX = item.PositionX,
                PositionY = item.PositionY,
                PositionZ = item.PositionZ,
                RotationY = item.RotationY,
                UniformScale = item.UniformScale,
                PresentationOnly = item.PresentationOnly,
            }));
        dbContext.SyntyRejections.AddRange(ledger.Rejections.Select(item =>
            new SimulationWorldSynty배치거부Entity
            {
                VisualRunId = run.Id,
                StableId = item.StableId,
                TargetNodeStableId = item.TargetNodeStableId,
                ReasonCode = item.ReasonCode,
                Detail = item.Detail,
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result(true, ledger, outputHash);
    }

    private static SimulationWorldSynty경관저장결과 Result(
        bool inserted,
        SimulationWorldSynty경관실행원장 ledger,
        string outputHash) => new()
        {
            Inserted = inserted,
            VisualBuildStableId = ledger.VisualBuildStableId,
            OutputHashSha256 = outputHash,
            StatusCode = ledger.StatusCode,
            GraphicsPlanCount = ledger.GraphicsPlans.Count,
            VisualPlacementCount = ledger.VisualPlacements.Count,
            RejectionCount = ledger.Rejections.Count,
        };
}

internal sealed class SimulationWorldSynty경관RunConfiguration
    : IEntityTypeConfiguration<SimulationWorldSynty경관RunEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldSynty경관RunEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_Synty경관실행");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.VisualBuildStableId).IsUnique();
        builder.HasIndex(item => item.JobStableId);
        builder.HasIndex(item => item.SpatialBuildStableId);
        Column(builder.Property(item => item.Id), "식별번호");
        Column(builder.Property(item => item.SchemaVersion), "스키마버전");
        Text(builder.Property(item => item.VisualBuildStableId), "시각실행고유식별자", 200);
        Text(builder.Property(item => item.JobStableId), "작업고유식별자", 200);
        Text(builder.Property(item => item.SpatialBuildStableId), "공간실행고유식별자", 200);
        Text(builder.Property(item => item.SpatialOutputHashSha256), "공간출력SHA256", 64);
        Text(builder.Property(item => item.AreaSetStableId), "영역묶음고유식별자", 200);
        Text(builder.Property(item => item.ScopeKindCode), "작업범위종류코드", 40);
        Text(builder.Property(item => item.ScopeStableId), "작업범위고유식별자", 200);
        Text(builder.Property(item => item.LandscapeRuleRevision), "경관규칙개정번호", 120);
        Text(builder.Property(item => item.VisualCatalogRevision), "Synty구성대장개정번호", 120);
        Text(builder.Property(item => item.UrpProfileCatalogRevision), "URP표현대장개정번호", 120);
        Column(builder.Property(item => item.Seed), "배치시드");
        Text(builder.Property(item => item.TargetPlatformCode), "대상플랫폼코드", 40);
        Text(builder.Property(item => item.QualityTierCode), "품질단계코드", 40);
        Text(builder.Property(item => item.InputFingerprintSha256), "입력지문SHA256", 64);
        Text(builder.Property(item => item.OutputHashSha256), "출력해시SHA256", 64);
        Column(builder.Property(item => item.GeneratedAtUtc), "생성시각UTC");
        Column(builder.Property(item => item.StoredAtUtc), "저장시각UTC");
        Text(builder.Property(item => item.StatusCode), "작업상태코드", 50);
    }

    private static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length).IsRequired();

    private static void Column<T>(PropertyBuilder<T> property, string name) =>
        property.HasColumnName(name);
}

internal sealed class SimulationWorldSynty그래픽표현Configuration
    : IEntityTypeConfiguration<SimulationWorldSynty그래픽표현Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldSynty그래픽표현Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_Synty그래픽표현계획");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.VisualRunId, item.StableId }).IsUnique();
        Column(builder.Property(item => item.Id), "식별번호");
        Column(builder.Property(item => item.VisualRunId), "Synty경관실행식별번호");
        Text(builder.Property(item => item.StableId), "그래픽표현고유식별자", 200);
        Text(builder.Property(item => item.TargetNodeStableId), "대상노드고유식별자", 200);
        Text(builder.Property(item => item.PresentationScopeCode), "표현범위코드", 80);
        Text(builder.Property(item => item.TextureSetKey), "질감세트키", 160);
        Text(builder.Property(item => item.MaterialVariantKey), "재질변형키", 160);
        Text(builder.Property(item => item.ColorPaletteKey), "색조팔레트키", 120);
        Text(builder.Property(item => item.BackgroundProfileKey), "배경Profile키", 160);
        Text(builder.Property(item => item.LightingProfileKey), "조명Profile키", 160);
        Text(builder.Property(item => item.TimeOfDayProfileKey), "시간대Profile키", 160);
        Text(builder.Property(item => item.ShadowPolicyCode), "그림자정책코드", 40);
        Column(builder.Property(item => item.CastShadows), "그림자투사여부");
        Column(builder.Property(item => item.ReceiveShadows), "그림자수신여부");
        builder.Property(item => item.ContactShadowStrength).HasColumnName("접지그림자강도").HasPrecision(5, 4);
        builder.Property(item => item.ShadowDistanceMeters).HasColumnName("그림자거리미터").HasPrecision(12, 4);
        builder.Property(item => item.AmbientOcclusionStrength).HasColumnName("주변광차폐강도").HasPrecision(5, 4);
        Text(builder.Property(item => item.LodCode), "세부표현단계코드", 40);
        Text(builder.Property(item => item.QualityTierCode), "품질단계코드", 40);
        Column(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.VisualRun).WithMany().HasForeignKey(item => item.VisualRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length).IsRequired();
    private static void Column<T>(PropertyBuilder<T> property, string name) =>
        property.HasColumnName(name);
}

internal sealed class SimulationWorldSynty시각배치Configuration
    : IEntityTypeConfiguration<SimulationWorldSynty시각배치Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldSynty시각배치Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_Synty시각배치계획");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.VisualRunId, item.StableId }).IsUnique();
        Column(builder.Property(item => item.Id), "식별번호");
        Column(builder.Property(item => item.VisualRunId), "Synty경관실행식별번호");
        Text(builder.Property(item => item.StableId), "시각배치고유식별자", 200);
        Text(builder.Property(item => item.TargetNodeStableId), "대상노드고유식별자", 200);
        Text(builder.Property(item => item.VisualKey), "시각키", 160);
        Text(builder.Property(item => item.LodCode), "세부표현단계코드", 40);
        builder.Property(item => item.PositionX).HasColumnName("위치X").HasPrecision(18, 4);
        builder.Property(item => item.PositionY).HasColumnName("위치Y").HasPrecision(18, 4);
        builder.Property(item => item.PositionZ).HasColumnName("위치Z").HasPrecision(18, 4);
        builder.Property(item => item.RotationY).HasColumnName("Y축회전").HasPrecision(9, 4);
        builder.Property(item => item.UniformScale).HasColumnName("균일축척").HasPrecision(9, 4);
        Column(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.VisualRun).WithMany().HasForeignKey(item => item.VisualRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length).IsRequired();
    private static void Column<T>(PropertyBuilder<T> property, string name) =>
        property.HasColumnName(name);
}

internal sealed class SimulationWorldSynty배치거부Configuration
    : IEntityTypeConfiguration<SimulationWorldSynty배치거부Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldSynty배치거부Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_Synty배치거부");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.VisualRunId, item.StableId }).IsUnique();
        Column(builder.Property(item => item.Id), "식별번호");
        Column(builder.Property(item => item.VisualRunId), "Synty경관실행식별번호");
        Text(builder.Property(item => item.StableId), "배치거부고유식별자", 240);
        builder.Property(item => item.TargetNodeStableId)
            .HasColumnName("대상노드고유식별자").HasMaxLength(200);
        Text(builder.Property(item => item.ReasonCode), "거부사유코드", 100);
        Text(builder.Property(item => item.Detail), "거부상세", 1000);
        builder.HasOne(item => item.VisualRun).WithMany().HasForeignKey(item => item.VisualRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length).IsRequired();
    private static void Column<T>(PropertyBuilder<T> property, string name) =>
        property.HasColumnName(name);
}
