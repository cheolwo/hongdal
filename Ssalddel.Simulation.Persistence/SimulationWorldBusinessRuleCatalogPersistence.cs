using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld업무규칙CatalogEntity
{
    public long Id { get; set; }
    public int SchemaVersion { get; set; }
    public string CatalogRevision { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string CatalogHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld시설의미Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string SpatialNodeStableId { get; set; } = string.Empty;
    public string FacilityTypeCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string EvidenceSourceStableId { get; set; } = string.Empty;
    public string ConfidenceCode { get; set; } = string.Empty;
    public bool ScenarioAssigned { get; set; }
}

public sealed class SimulationWorld시설기능Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string FacilityStableId { get; set; } = string.Empty;
    public string CapabilityCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
}

public sealed class SimulationWorld업무Simulation규칙Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public string RuleTypeCode { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string EngineKey { get; set; } = string.Empty;
    public string InputContractKey { get; set; } = string.Empty;
    public string OutputContractKey { get; set; } = string.Empty;
    public bool Deterministic { get; set; }
    public bool SimulationOnly { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class SimulationWorld업무Simulation규칙ParameterEntity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string ParameterCode { get; set; } = string.Empty;
    public string ValueTypeCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? UnitCode { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
}

public sealed class SimulationWorld객체업무규칙연결Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string FacilityStableId { get; set; } = string.Empty;
    public string CapabilityCode { get; set; } = string.Empty;
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public sealed class SimulationWorldScenario규칙묶음Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
}

public sealed class SimulationWorldScenario규칙항목Entity
{
    public long Id { get; set; }
    public long CatalogId { get; set; }
    public string RuleSetStableId { get; set; } = string.Empty;
    public string RuleSetRevision { get; set; } = string.Empty;
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public int ApplyOrder { get; set; }
    public bool Required { get; set; }
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Server 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "운영 상태와 Simulation 상태의 권위 경계를 유지한다.")]
public sealed class SimulationWorld업무규칙집결Store(SimulationWorld파생DbContext dbContext)
    : ISimulationWorld업무규칙집결Store
{
    public async Task<SimulationWorld업무규칙집결저장결과> 저장Async(
        SimulationWorld업무규칙집결원장 catalog,
        CancellationToken cancellationToken)
    {
        SimulationWorld업무규칙집결Validator.Validate(catalog);
        var hash = SimulationWorld업무규칙집결Validator.ComputeHash(catalog);
        var existing = await dbContext.BusinessRuleCatalogs.AsNoTracking()
            .SingleOrDefaultAsync(x => x.CatalogRevision == catalog.CatalogRevision, cancellationToken);
        if (existing != null)
        {
            if (!string.Equals(existing.CatalogHashSha256, hash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("SimulationWorldBusinessRuleCatalogRevisionConflict");
            return Result(false, catalog, hash);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var entity = new SimulationWorld업무규칙CatalogEntity
        {
            SchemaVersion = catalog.SchemaVersion, CatalogRevision = catalog.CatalogRevision,
            SpatialBuildStableId = catalog.SpatialBuildStableId,
            SpatialOutputHashSha256 = catalog.SpatialOutputHashSha256,
            CatalogHashSha256 = hash, CreatedAtUtc = catalog.CreatedAtUtc,
            StoredAtUtc = DateTimeOffset.UtcNow,
        };
        dbContext.BusinessRuleCatalogs.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.FacilitySemantics.AddRange(catalog.Facilities.Select(x => new SimulationWorld시설의미Entity
        {
            CatalogId = entity.Id, StableId = x.StableId, SpatialNodeStableId = x.SpatialNodeStableId,
            FacilityTypeCode = x.FacilityTypeCode, EvidenceKindCode = x.EvidenceKindCode,
            EvidenceSourceStableId = x.EvidenceSourceStableId, ConfidenceCode = x.ConfidenceCode,
            ScenarioAssigned = x.ScenarioAssigned,
        }));
        dbContext.FacilityCapabilities.AddRange(catalog.Capabilities.Select(x => new SimulationWorld시설기능Entity
        {
            CatalogId = entity.Id, StableId = x.StableId, FacilityStableId = x.FacilityStableId,
            CapabilityCode = x.CapabilityCode, EvidenceKindCode = x.EvidenceKindCode,
        }));
        dbContext.BusinessSimulationRules.AddRange(catalog.Rules.Select(x => new SimulationWorld업무Simulation규칙Entity
        {
            CatalogId = entity.Id, StableId = x.StableId, Revision = x.Revision,
            DomainCode = x.DomainCode, RuleTypeCode = x.RuleTypeCode, StatusCode = x.StatusCode,
            EngineKey = x.EngineKey, InputContractKey = x.InputContractKey, OutputContractKey = x.OutputContractKey,
            Deterministic = x.Deterministic, SimulationOnly = x.SimulationOnly, Description = x.Description,
        }));
        dbContext.BusinessSimulationRuleParameters.AddRange(catalog.Parameters.Select(x => new SimulationWorld업무Simulation규칙ParameterEntity
        {
            CatalogId = entity.Id, RuleStableId = x.RuleStableId, RuleRevision = x.RuleRevision,
            ParameterCode = x.ParameterCode, ValueTypeCode = x.ValueTypeCode, Value = x.Value,
            UnitCode = x.UnitCode, EvidenceKindCode = x.EvidenceKindCode,
        }));
        dbContext.ObjectBusinessRuleBindings.AddRange(catalog.Bindings.Select(x => new SimulationWorld객체업무규칙연결Entity
        {
            CatalogId = entity.Id, StableId = x.StableId, FacilityStableId = x.FacilityStableId,
            CapabilityCode = x.CapabilityCode, RuleStableId = x.RuleStableId, RuleRevision = x.RuleRevision,
            ScopeCode = x.ScopeCode, Priority = x.Priority, EvidenceKindCode = x.EvidenceKindCode, Active = x.Active,
        }));
        foreach (var set in catalog.ScenarioRuleSets)
        {
            dbContext.ScenarioRuleSets.Add(new SimulationWorldScenario규칙묶음Entity
            {
                CatalogId = entity.Id, StableId = set.StableId, Revision = set.Revision,
                AreaSetStableId = set.AreaSetStableId,
            });
            dbContext.ScenarioRuleItems.AddRange(set.Items.Select(x => new SimulationWorldScenario규칙항목Entity
            {
                CatalogId = entity.Id, RuleSetStableId = set.StableId, RuleSetRevision = set.Revision,
                RuleStableId = x.RuleStableId, RuleRevision = x.RuleRevision,
                ApplyOrder = x.ApplyOrder, Required = x.Required,
            }));
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result(true, catalog, hash);
    }

    private static SimulationWorld업무규칙집결저장결과 Result(bool inserted, SimulationWorld업무규칙집결원장 catalog, string hash) => new()
    {
        Inserted = inserted, CatalogRevision = catalog.CatalogRevision, CatalogHashSha256 = hash,
        FacilityCount = catalog.Facilities.Count, CapabilityCount = catalog.Capabilities.Count,
        RuleCount = catalog.Rules.Count, BindingCount = catalog.Bindings.Count,
        ScenarioRuleSetCount = catalog.ScenarioRuleSets.Count,
    };
}

internal static class SimulationWorld업무규칙Column
{
    public static void Text(PropertyBuilder<string> p, string name, int length) => p.HasColumnName(name).HasMaxLength(length).IsRequired();
    public static void Optional(PropertyBuilder<string?> p, string name, int length) => p.HasColumnName(name).HasMaxLength(length);
    public static void Value<T>(PropertyBuilder<T> p, string name) => p.HasColumnName(name);
    public static void Base<TEntity>(EntityTypeBuilder<TEntity> b, string table) where TEntity : class
    {
        b.ToTable(table);
        b.HasKey("Id");

        if (typeof(TEntity) != typeof(SimulationWorld업무규칙CatalogEntity))
        {
            b.HasOne<SimulationWorld업무규칙CatalogEntity>()
                .WithMany()
                .HasForeignKey("CatalogId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

internal sealed class SimulationWorld업무규칙CatalogConfiguration : IEntityTypeConfiguration<SimulationWorld업무규칙CatalogEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld업무규칙CatalogEntity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_업무Simulation규칙대장"); b.HasIndex(x => x.CatalogRevision).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.SchemaVersion), "스키마버전"); SimulationWorld업무규칙Column.Text(b.Property(x => x.CatalogRevision), "규칙대장개정번호", 120); SimulationWorld업무규칙Column.Text(b.Property(x => x.SpatialBuildStableId), "공간실행고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.SpatialOutputHashSha256), "공간출력SHA256", 64); SimulationWorld업무규칙Column.Text(b.Property(x => x.CatalogHashSha256), "규칙대장SHA256", 64); SimulationWorld업무규칙Column.Value(b.Property(x => x.CreatedAtUtc), "생성시각UTC"); SimulationWorld업무규칙Column.Value(b.Property(x => x.StoredAtUtc), "저장시각UTC"); }
}

internal sealed class SimulationWorld시설의미Configuration : IEntityTypeConfiguration<SimulationWorld시설의미Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld시설의미Entity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_시설의미대장"); b.HasIndex(x => new { x.CatalogId, x.StableId }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.StableId), "시설고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.SpatialNodeStableId), "공간노드고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.FacilityTypeCode), "시설종류코드", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.EvidenceKindCode), "근거종류코드", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.EvidenceSourceStableId), "근거원본고유식별자", 240); SimulationWorld업무규칙Column.Text(b.Property(x => x.ConfidenceCode), "분류신뢰수준코드", 80); SimulationWorld업무규칙Column.Value(b.Property(x => x.ScenarioAssigned), "Scenario지정여부"); }
}

internal sealed class SimulationWorld시설기능Configuration : IEntityTypeConfiguration<SimulationWorld시설기능Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld시설기능Entity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_시설기능대장"); b.HasIndex(x => new { x.CatalogId, x.StableId }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.StableId), "시설기능고유식별자", 300); SimulationWorld업무규칙Column.Text(b.Property(x => x.FacilityStableId), "시설고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.CapabilityCode), "기능코드", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.EvidenceKindCode), "근거종류코드", 80); }
}

internal sealed class SimulationWorld업무Simulation규칙Configuration : IEntityTypeConfiguration<SimulationWorld업무Simulation규칙Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld업무Simulation규칙Entity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_업무Simulation규칙"); b.HasIndex(x => new { x.CatalogId, x.StableId, x.Revision }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.StableId), "규칙고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.Revision), "규칙개정번호", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.DomainCode), "규칙영역코드", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleTypeCode), "규칙종류코드", 100); SimulationWorld업무규칙Column.Text(b.Property(x => x.StatusCode), "규칙상태코드", 40); SimulationWorld업무규칙Column.Text(b.Property(x => x.EngineKey), "규칙Engine키", 180); SimulationWorld업무규칙Column.Text(b.Property(x => x.InputContractKey), "입력계약키", 180); SimulationWorld업무규칙Column.Text(b.Property(x => x.OutputContractKey), "출력계약키", 180); SimulationWorld업무규칙Column.Value(b.Property(x => x.Deterministic), "결정적실행여부"); SimulationWorld업무규칙Column.Value(b.Property(x => x.SimulationOnly), "Simulation전용여부"); SimulationWorld업무규칙Column.Text(b.Property(x => x.Description), "규칙설명", 1000); }
}

internal sealed class SimulationWorld업무Simulation규칙ParameterConfiguration : IEntityTypeConfiguration<SimulationWorld업무Simulation규칙ParameterEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld업무Simulation규칙ParameterEntity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_업무Simulation규칙Parameter"); b.HasIndex(x => new { x.CatalogId, x.RuleStableId, x.RuleRevision, x.ParameterCode }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleStableId), "규칙고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleRevision), "규칙개정번호", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.ParameterCode), "Parameter코드", 120); SimulationWorld업무규칙Column.Text(b.Property(x => x.ValueTypeCode), "값종류코드", 40); SimulationWorld업무규칙Column.Text(b.Property(x => x.Value), "값", 500); SimulationWorld업무규칙Column.Optional(b.Property(x => x.UnitCode), "단위코드", 40); SimulationWorld업무규칙Column.Text(b.Property(x => x.EvidenceKindCode), "근거종류코드", 80); }
}

internal sealed class SimulationWorld객체업무규칙연결Configuration : IEntityTypeConfiguration<SimulationWorld객체업무규칙연결Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld객체업무규칙연결Entity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_객체업무규칙연결"); b.HasIndex(x => new { x.CatalogId, x.StableId }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.StableId), "연결고유식별자", 360); SimulationWorld업무규칙Column.Text(b.Property(x => x.FacilityStableId), "시설고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.CapabilityCode), "기능코드", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleStableId), "규칙고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleRevision), "규칙개정번호", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.ScopeCode), "적용범위코드", 40); SimulationWorld업무규칙Column.Value(b.Property(x => x.Priority), "우선순위"); SimulationWorld업무규칙Column.Text(b.Property(x => x.EvidenceKindCode), "근거종류코드", 80); SimulationWorld업무규칙Column.Value(b.Property(x => x.Active), "활성여부"); }
}

internal sealed class SimulationWorldScenario규칙묶음Configuration : IEntityTypeConfiguration<SimulationWorldScenario규칙묶음Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldScenario규칙묶음Entity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_Scenario규칙묶음"); b.HasIndex(x => new { x.CatalogId, x.StableId, x.Revision }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.StableId), "Scenario규칙묶음고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.Revision), "규칙묶음개정번호", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.AreaSetStableId), "AreaSet고유식별자", 200); }
}

internal sealed class SimulationWorldScenario규칙항목Configuration : IEntityTypeConfiguration<SimulationWorldScenario규칙항목Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldScenario규칙항목Entity> b) { SimulationWorld업무규칙Column.Base(b, "시뮬레이션월드_Scenario규칙항목"); b.HasIndex(x => new { x.CatalogId, x.RuleSetStableId, x.RuleSetRevision, x.RuleStableId, x.RuleRevision }).IsUnique(); SimulationWorld업무규칙Column.Value(b.Property(x => x.Id), "식별번호"); SimulationWorld업무규칙Column.Value(b.Property(x => x.CatalogId), "규칙대장식별번호"); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleSetStableId), "Scenario규칙묶음고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleSetRevision), "규칙묶음개정번호", 80); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleStableId), "규칙고유식별자", 200); SimulationWorld업무규칙Column.Text(b.Property(x => x.RuleRevision), "규칙개정번호", 80); SimulationWorld업무규칙Column.Value(b.Property(x => x.ApplyOrder), "적용순서"); SimulationWorld업무규칙Column.Value(b.Property(x => x.Required), "필수여부"); }
}
