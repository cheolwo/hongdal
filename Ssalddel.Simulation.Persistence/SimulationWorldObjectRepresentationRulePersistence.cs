using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld객체표현규칙CatalogEntity
{
    public long Id { get; set; }
    public int SchemaVersion { get; set; }
    public string CatalogRevision { get; set; } = string.Empty;
    public string CatalogHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld공간규칙MetadataEntity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string SpatialFactKindCode { get; set; } = string.Empty;
    public string OperatorCode { get; set; } = string.Empty;
    public string ExpectedValueCode { get; set; } = string.Empty;
    public string RequiredEvidenceKindCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SimulationWorld객체표현규칙CatalogEntity Catalog { get; set; } = null!;
}

public sealed class SimulationWorldSimulation규칙MetadataEntity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StateTypeCode { get; set; } = string.Empty;
    public string ExpectedStateCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SimulationWorld객체표현규칙CatalogEntity Catalog { get; set; } = null!;
}

public sealed class SimulationWorld객체표현결합규칙Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string ObjectSemanticCode { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public string SpatialRuleStableId { get; set; } = string.Empty;
    public string SpatialRuleRevision { get; set; } = string.Empty;
    public string? SimulationRuleStableId { get; set; }
    public string? SimulationRuleRevision { get; set; }
    public bool SimulationRuleRequired { get; set; }
    public string MinimumEvidenceKindCode { get; set; } = string.Empty;
    public string DefaultCompositionKey { get; set; } = string.Empty;
    public string? DynamicIntentBundleKey { get; set; }
    public string UnmetRuleHandlingCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool PresentationOnly { get; set; }
    public SimulationWorld객체표현규칙CatalogEntity Catalog { get; set; } = null!;
}

public sealed class SimulationWorld객체표현해석RunEntity
{
    public long Id { get; set; }
    public int SchemaVersion { get; set; }
    public string InterpretationStableId { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string? SimulationSessionStableId { get; set; }
    public long? SimulationSessionRevision { get; set; }
    public long? WorldTick { get; set; }
    public string RuleCatalogRevision { get; set; } = string.Empty;
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset InterpretedAtUtc { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld객체표현해석ResultEntity
{
    public long Id { get; set; }
    public long InterpretationRunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string ObjectSemanticCode { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public string ResolutionCode { get; set; } = string.Empty;
    public string? AppliedBindingRuleStableId { get; set; }
    public string? AppliedBindingRuleRevision { get; set; }
    public string? AppliedSpatialRuleStableId { get; set; }
    public string? AppliedSimulationRuleStableId { get; set; }
    public string? DefaultCompositionKey { get; set; }
    public string? DynamicIntentBundleKey { get; set; }
    public string UnmetRuleHandlingCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public bool PresentationOnly { get; set; }
    public SimulationWorld객체표현해석RunEntity InterpretationRun { get; set; } = null!;
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Server 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "운영 상태와 Simulation 상태의 권위 경계를 유지한다.")]
public sealed class SimulationWorld객체표현규칙Store(
    SimulationWorld파생DbContext dbContext) : ISimulationWorld객체표현규칙Store
{
    public const string CatalogConflictCode = "SimulationWorldObjectRepresentationCatalogConflict";
    public const string InterpretationConflictCode = "SimulationWorldObjectRepresentationInterpretationConflict";
    public const string SpatialBuildNotFoundCode = "SimulationWorldObjectRepresentationSpatialBuildNotFound";
    public const string SpatialOutputMismatchCode = "SimulationWorldObjectRepresentationSpatialOutputMismatch";
    public const string CatalogNotFoundCode = "SimulationWorldObjectRepresentationCatalogNotFound";
    public const string TargetNodeNotFoundCode = "SimulationWorldObjectRepresentationTargetNodeNotFound";
    public const string InterpretationRuleMismatchCode = "SimulationWorldObjectRepresentationRuleMismatch";

    public async Task<SimulationWorld객체표현규칙대장저장결과> 규칙대장저장Async(
        SimulationWorld객체표현규칙대장 catalog,
        CancellationToken cancellationToken)
    {
        SimulationWorld객체표현규칙Validator.Validate(catalog);
        var hash = SimulationWorld객체표현해석기.ComputeCatalogHash(catalog);
        var existing = await dbContext.ObjectRepresentationRuleCatalogs.AsNoTracking()
            .SingleOrDefaultAsync(item => item.CatalogRevision == catalog.CatalogRevision, cancellationToken);
        if (existing != null)
        {
            if (!string.Equals(existing.CatalogHashSha256, hash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(CatalogConflictCode);
            return CatalogResult(false, catalog, hash);
        }

        var entity = new SimulationWorld객체표현규칙CatalogEntity
        {
            SchemaVersion = catalog.SchemaVersion,
            CatalogRevision = catalog.CatalogRevision,
            CatalogHashSha256 = hash,
            CreatedAtUtc = catalog.CreatedAtUtc,
            StoredAtUtc = DateTimeOffset.UtcNow,
        };
        dbContext.ObjectRepresentationRuleCatalogs.Add(entity);
        dbContext.SpatialRuleMetadata.AddRange(catalog.SpatialRules.Select(item => new SimulationWorld공간규칙MetadataEntity
        {
            Catalog = entity, StableId = item.StableId, Revision = item.Revision, StatusCode = item.StatusCode,
            SpatialFactKindCode = item.SpatialFactKindCode, OperatorCode = item.OperatorCode,
            ExpectedValueCode = item.ExpectedValueCode, RequiredEvidenceKindCode = item.RequiredEvidenceKindCode,
            Description = item.Description,
        }));
        dbContext.SimulationRuleMetadata.AddRange(catalog.SimulationRules.Select(item => new SimulationWorldSimulation규칙MetadataEntity
        {
            Catalog = entity, StableId = item.StableId, Revision = item.Revision, StatusCode = item.StatusCode,
            StateTypeCode = item.StateTypeCode, ExpectedStateCode = item.ExpectedStateCode, Description = item.Description,
        }));
        dbContext.ObjectRepresentationBindingRules.AddRange(catalog.BindingRules.Select(item => new SimulationWorld객체표현결합규칙Entity
        {
            Catalog = entity, StableId = item.StableId, Revision = item.Revision, StatusCode = item.StatusCode,
            ObjectSemanticCode = item.ObjectSemanticCode, ScopeCode = item.ScopeCode,
            SpatialRuleStableId = item.SpatialRuleStableId, SpatialRuleRevision = item.SpatialRuleRevision,
            SimulationRuleStableId = item.SimulationRuleStableId, SimulationRuleRevision = item.SimulationRuleRevision,
            SimulationRuleRequired = item.SimulationRuleRequired, MinimumEvidenceKindCode = item.MinimumEvidenceKindCode,
            DefaultCompositionKey = item.DefaultCompositionKey, DynamicIntentBundleKey = item.DynamicIntentBundleKey,
            UnmetRuleHandlingCode = item.UnmetRuleHandlingCode, Priority = item.Priority,
            PresentationOnly = item.PresentationOnly,
        }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return CatalogResult(true, catalog, hash);
    }

    public async Task<SimulationWorld객체표현해석저장결과> 해석결과저장Async(
        SimulationWorld객체표현해석원장 ledger,
        CancellationToken cancellationToken)
    {
        SimulationWorld객체표현규칙Validator.Validate(ledger);
        var existing = await dbContext.ObjectRepresentationInterpretationRuns.AsNoTracking()
            .SingleOrDefaultAsync(item => item.InterpretationStableId == ledger.InterpretationStableId, cancellationToken);
        if (existing != null)
        {
            if (!string.Equals(existing.InputFingerprintSha256, ledger.InputFingerprintSha256, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.OutputHashSha256, ledger.OutputHashSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(InterpretationConflictCode);
            return InterpretationResult(false, ledger);
        }
        var spatial = await dbContext.Runs.AsNoTracking()
            .SingleOrDefaultAsync(item => item.BuildStableId == ledger.SpatialBuildStableId, cancellationToken);
        if (spatial == null) throw new InvalidOperationException(SpatialBuildNotFoundCode);
        if (!string.Equals(spatial.OutputHashSha256, ledger.SpatialOutputHashSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(SpatialOutputMismatchCode);
        var catalog = await dbContext.ObjectRepresentationRuleCatalogs.AsNoTracking()
            .SingleOrDefaultAsync(item => item.CatalogRevision == ledger.RuleCatalogRevision, cancellationToken);
        if (catalog == null) throw new InvalidOperationException(CatalogNotFoundCode);
        var nodeIds = await dbContext.Nodes.AsNoTracking().Where(item => item.RunId == spatial.Id)
            .Select(item => item.StableId).ToArrayAsync(cancellationToken);
        if (ledger.Results.Any(item => !nodeIds.Contains(item.TargetNodeStableId, StringComparer.Ordinal)))
            throw new InvalidOperationException(TargetNodeNotFoundCode);
        var bindings = await dbContext.ObjectRepresentationBindingRules.AsNoTracking()
            .Where(item => item.CatalogId == catalog.Id).ToArrayAsync(cancellationToken);
        foreach (var result in ledger.Results.Where(item => item.AppliedBindingRuleStableId != null))
        {
            var binding = bindings.SingleOrDefault(item => item.StableId == result.AppliedBindingRuleStableId
                && item.Revision == result.AppliedBindingRuleRevision);
            if (binding == null
                || binding.SpatialRuleStableId != result.AppliedSpatialRuleStableId
                || binding.SimulationRuleStableId != result.AppliedSimulationRuleStableId
                || binding.DefaultCompositionKey != result.DefaultCompositionKey
                || binding.DynamicIntentBundleKey != result.DynamicIntentBundleKey)
                throw new InvalidOperationException(InterpretationRuleMismatchCode);
        }

        var run = new SimulationWorld객체표현해석RunEntity
        {
            SchemaVersion = ledger.SchemaVersion, InterpretationStableId = ledger.InterpretationStableId,
            SpatialBuildStableId = ledger.SpatialBuildStableId, SpatialOutputHashSha256 = ledger.SpatialOutputHashSha256,
            SimulationSessionStableId = ledger.SimulationSessionStableId,
            SimulationSessionRevision = ledger.SimulationSessionRevision, WorldTick = ledger.WorldTick,
            RuleCatalogRevision = ledger.RuleCatalogRevision, InputFingerprintSha256 = ledger.InputFingerprintSha256,
            OutputHashSha256 = ledger.OutputHashSha256, InterpretedAtUtc = ledger.InterpretedAtUtc,
            StoredAtUtc = DateTimeOffset.UtcNow,
        };
        dbContext.ObjectRepresentationInterpretationRuns.Add(run);
        dbContext.ObjectRepresentationInterpretationResults.AddRange(ledger.Results.Select(item =>
            new SimulationWorld객체표현해석ResultEntity
            {
                InterpretationRun = run, StableId = item.StableId, TargetNodeStableId = item.TargetNodeStableId,
                ObjectSemanticCode = item.ObjectSemanticCode, ScopeCode = item.ScopeCode,
                ResolutionCode = item.ResolutionCode, AppliedBindingRuleStableId = item.AppliedBindingRuleStableId,
                AppliedBindingRuleRevision = item.AppliedBindingRuleRevision,
                AppliedSpatialRuleStableId = item.AppliedSpatialRuleStableId,
                AppliedSimulationRuleStableId = item.AppliedSimulationRuleStableId,
                DefaultCompositionKey = item.DefaultCompositionKey, DynamicIntentBundleKey = item.DynamicIntentBundleKey,
                UnmetRuleHandlingCode = item.UnmetRuleHandlingCode, EvidenceKindCode = item.EvidenceKindCode,
                PresentationOnly = item.PresentationOnly,
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return InterpretationResult(true, ledger);
    }

    private static SimulationWorld객체표현규칙대장저장결과 CatalogResult(
        bool inserted, SimulationWorld객체표현규칙대장 catalog, string hash) => new()
        {
            Inserted = inserted, CatalogRevision = catalog.CatalogRevision, CatalogHashSha256 = hash,
            SpatialRuleCount = catalog.SpatialRules.Count, SimulationRuleCount = catalog.SimulationRules.Count,
            BindingRuleCount = catalog.BindingRules.Count,
        };

    private static SimulationWorld객체표현해석저장결과 InterpretationResult(
        bool inserted, SimulationWorld객체표현해석원장 ledger) => new()
        {
            Inserted = inserted, InterpretationStableId = ledger.InterpretationStableId,
            OutputHashSha256 = ledger.OutputHashSha256, ResultCount = ledger.Results.Count,
        };
}

internal static class SimulationWorld객체표현RuleColumn
{
    public static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length).IsRequired();
    public static void NullableText(PropertyBuilder<string?> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length);
    public static void Column<T>(PropertyBuilder<T> property, string name) => property.HasColumnName(name);
}

internal sealed class SimulationWorld객체표현규칙CatalogConfiguration : IEntityTypeConfiguration<SimulationWorld객체표현규칙CatalogEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld객체표현규칙CatalogEntity> b)
    {
        b.ToTable("시뮬레이션월드_객체표현규칙대장"); b.HasKey(x => x.Id); b.HasIndex(x => x.CatalogRevision).IsUnique();
        SimulationWorld객체표현RuleColumn.Column(b.Property(x => x.Id), "식별번호");
        SimulationWorld객체표현RuleColumn.Column(b.Property(x => x.SchemaVersion), "스키마버전");
        SimulationWorld객체표현RuleColumn.Text(b.Property(x => x.CatalogRevision), "규칙대장개정번호", 100);
        SimulationWorld객체표현RuleColumn.Text(b.Property(x => x.CatalogHashSha256), "규칙대장SHA256", 64);
        SimulationWorld객체표현RuleColumn.Column(b.Property(x => x.CreatedAtUtc), "생성시각UTC");
        SimulationWorld객체표현RuleColumn.Column(b.Property(x => x.StoredAtUtc), "저장시각UTC");
    }
}

internal sealed class SimulationWorld공간규칙MetadataConfiguration : IEntityTypeConfiguration<SimulationWorld공간규칙MetadataEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld공간규칙MetadataEntity> b)
    {
        b.ToTable("시뮬레이션월드_공간규칙Metadata"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.CatalogId, x.StableId, x.Revision }).IsUnique();
        Col(b.Property(x => x.Id), "식별번호"); Col(b.Property(x => x.CatalogId), "규칙대장식별번호");
        Txt(b.Property(x => x.StableId), "공간규칙고유식별자", 160); Txt(b.Property(x => x.Revision), "공간규칙개정번호", 100);
        Txt(b.Property(x => x.StatusCode), "규칙상태코드", 40); Txt(b.Property(x => x.SpatialFactKindCode), "공간사실종류코드", 80);
        Txt(b.Property(x => x.OperatorCode), "연산자코드", 40); Txt(b.Property(x => x.ExpectedValueCode), "기대값코드", 160);
        Txt(b.Property(x => x.RequiredEvidenceKindCode), "필수근거종류코드", 80); Txt(b.Property(x => x.Description), "규칙설명", 1000);
        b.HasOne(x => x.Catalog).WithMany().HasForeignKey(x => x.CatalogId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void Txt(PropertyBuilder<string> p, string n, int l) => SimulationWorld객체표현RuleColumn.Text(p, n, l);
    private static void Col<T>(PropertyBuilder<T> p, string n) => SimulationWorld객체표현RuleColumn.Column(p, n);
}

internal sealed class SimulationWorldSimulation규칙MetadataConfiguration : IEntityTypeConfiguration<SimulationWorldSimulation규칙MetadataEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldSimulation규칙MetadataEntity> b)
    {
        b.ToTable("시뮬레이션월드_Simulation규칙Metadata"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.CatalogId, x.StableId, x.Revision }).IsUnique();
        Col(b.Property(x => x.Id), "식별번호"); Col(b.Property(x => x.CatalogId), "규칙대장식별번호");
        Txt(b.Property(x => x.StableId), "Simulation규칙고유식별자", 160); Txt(b.Property(x => x.Revision), "Simulation규칙개정번호", 100);
        Txt(b.Property(x => x.StatusCode), "규칙상태코드", 40); Txt(b.Property(x => x.StateTypeCode), "상태종류코드", 100);
        Txt(b.Property(x => x.ExpectedStateCode), "기대상태코드", 160); Txt(b.Property(x => x.Description), "규칙설명", 1000);
        b.HasOne(x => x.Catalog).WithMany().HasForeignKey(x => x.CatalogId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void Txt(PropertyBuilder<string> p, string n, int l) => SimulationWorld객체표현RuleColumn.Text(p, n, l);
    private static void Col<T>(PropertyBuilder<T> p, string n) => SimulationWorld객체표현RuleColumn.Column(p, n);
}

internal sealed class SimulationWorld객체표현결합규칙Configuration : IEntityTypeConfiguration<SimulationWorld객체표현결합규칙Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld객체표현결합규칙Entity> b)
    {
        b.ToTable("시뮬레이션월드_객체표현결합규칙"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.CatalogId, x.StableId, x.Revision }).IsUnique();
        Col(b.Property(x => x.Id), "식별번호"); Col(b.Property(x => x.CatalogId), "규칙대장식별번호");
        Txt(b.Property(x => x.StableId), "결합규칙고유식별자", 180); Txt(b.Property(x => x.Revision), "결합규칙개정번호", 100);
        Txt(b.Property(x => x.StatusCode), "규칙상태코드", 40); Txt(b.Property(x => x.ObjectSemanticCode), "객체의미코드", 120);
        Txt(b.Property(x => x.ScopeCode), "적용범위코드", 40); Txt(b.Property(x => x.SpatialRuleStableId), "공간규칙고유식별자", 160);
        Txt(b.Property(x => x.SpatialRuleRevision), "공간규칙개정번호", 100);
        SimulationWorld객체표현RuleColumn.NullableText(b.Property(x => x.SimulationRuleStableId), "Simulation규칙고유식별자", 160);
        SimulationWorld객체표현RuleColumn.NullableText(b.Property(x => x.SimulationRuleRevision), "Simulation규칙개정번호", 100);
        Col(b.Property(x => x.SimulationRuleRequired), "Simulation규칙필수여부");
        Txt(b.Property(x => x.MinimumEvidenceKindCode), "최소근거종류코드", 80); Txt(b.Property(x => x.DefaultCompositionKey), "기본구성키", 180);
        SimulationWorld객체표현RuleColumn.NullableText(b.Property(x => x.DynamicIntentBundleKey), "동적표현의도묶음키", 180);
        Txt(b.Property(x => x.UnmetRuleHandlingCode), "규칙미충족처리코드", 40); Col(b.Property(x => x.Priority), "우선순위");
        Col(b.Property(x => x.PresentationOnly), "표현전용여부");
        b.HasOne(x => x.Catalog).WithMany().HasForeignKey(x => x.CatalogId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void Txt(PropertyBuilder<string> p, string n, int l) => SimulationWorld객체표현RuleColumn.Text(p, n, l);
    private static void Col<T>(PropertyBuilder<T> p, string n) => SimulationWorld객체표현RuleColumn.Column(p, n);
}

internal sealed class SimulationWorld객체표현해석RunConfiguration : IEntityTypeConfiguration<SimulationWorld객체표현해석RunEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld객체표현해석RunEntity> b)
    {
        b.ToTable("시뮬레이션월드_객체표현해석실행"); b.HasKey(x => x.Id); b.HasIndex(x => x.InterpretationStableId).IsUnique();
        Col(b.Property(x => x.Id), "식별번호"); Col(b.Property(x => x.SchemaVersion), "스키마버전");
        Txt(b.Property(x => x.InterpretationStableId), "해석실행고유식별자", 200); Txt(b.Property(x => x.SpatialBuildStableId), "공간실행고유식별자", 200);
        Txt(b.Property(x => x.SpatialOutputHashSha256), "공간출력SHA256", 64);
        SimulationWorld객체표현RuleColumn.NullableText(b.Property(x => x.SimulationSessionStableId), "Simulation세션고유식별자", 200);
        Col(b.Property(x => x.SimulationSessionRevision), "Simulation세션개정번호"); Col(b.Property(x => x.WorldTick), "WorldTick");
        Txt(b.Property(x => x.RuleCatalogRevision), "규칙대장개정번호", 100); Txt(b.Property(x => x.InputFingerprintSha256), "입력FingerprintSHA256", 64);
        Txt(b.Property(x => x.OutputHashSha256), "출력SHA256", 64); Col(b.Property(x => x.InterpretedAtUtc), "해석시각UTC"); Col(b.Property(x => x.StoredAtUtc), "저장시각UTC");
    }
    private static void Txt(PropertyBuilder<string> p, string n, int l) => SimulationWorld객체표현RuleColumn.Text(p, n, l);
    private static void Col<T>(PropertyBuilder<T> p, string n) => SimulationWorld객체표현RuleColumn.Column(p, n);
}

internal sealed class SimulationWorld객체표현해석ResultConfiguration : IEntityTypeConfiguration<SimulationWorld객체표현해석ResultEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld객체표현해석ResultEntity> b)
    {
        b.ToTable("시뮬레이션월드_객체표현해석결과"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.InterpretationRunId, x.StableId }).IsUnique();
        Col(b.Property(x => x.Id), "식별번호"); Col(b.Property(x => x.InterpretationRunId), "해석실행식별번호");
        Txt(b.Property(x => x.StableId), "해석결과고유식별자", 200); Txt(b.Property(x => x.TargetNodeStableId), "대상노드고유식별자", 200);
        Txt(b.Property(x => x.ObjectSemanticCode), "객체의미코드", 120); Txt(b.Property(x => x.ScopeCode), "적용범위코드", 40); Txt(b.Property(x => x.ResolutionCode), "해석결과코드", 80);
        Nul(b.Property(x => x.AppliedBindingRuleStableId), "적용결합규칙고유식별자", 180); Nul(b.Property(x => x.AppliedBindingRuleRevision), "적용결합규칙개정번호", 100);
        Nul(b.Property(x => x.AppliedSpatialRuleStableId), "적용공간규칙고유식별자", 160); Nul(b.Property(x => x.AppliedSimulationRuleStableId), "적용Simulation규칙고유식별자", 160);
        Nul(b.Property(x => x.DefaultCompositionKey), "기본구성키", 180); Nul(b.Property(x => x.DynamicIntentBundleKey), "동적표현의도묶음키", 180);
        Txt(b.Property(x => x.UnmetRuleHandlingCode), "규칙미충족처리코드", 40); Txt(b.Property(x => x.EvidenceKindCode), "근거종류코드", 80); Col(b.Property(x => x.PresentationOnly), "표현전용여부");
        b.HasOne(x => x.InterpretationRun).WithMany().HasForeignKey(x => x.InterpretationRunId).OnDelete(DeleteBehavior.Cascade);
    }
    private static void Txt(PropertyBuilder<string> p, string n, int l) => SimulationWorld객체표현RuleColumn.Text(p, n, l);
    private static void Nul(PropertyBuilder<string?> p, string n, int l) => SimulationWorld객체표현RuleColumn.NullableText(p, n, l);
    private static void Col<T>(PropertyBuilder<T> p, string n) => SimulationWorld객체표현RuleColumn.Column(p, n);
}
