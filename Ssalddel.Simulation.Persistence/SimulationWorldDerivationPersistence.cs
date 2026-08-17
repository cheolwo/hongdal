using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld파생DbContext(
    DbContextOptions<SimulationWorld파생DbContext> options) : DbContext(options)
{
    public DbSet<SimulationWorld파생RunEntity> Runs => Set<SimulationWorld파생RunEntity>();
    public DbSet<SimulationWorld원본계보Entity> Sources => Set<SimulationWorld원본계보Entity>();
    public DbSet<SimulationWorld파생NodeEntity> Nodes => Set<SimulationWorld파생NodeEntity>();
    public DbSet<SimulationWorld파생RelationEntity> Relations => Set<SimulationWorld파생RelationEntity>();
    public DbSet<SimulationWorld건물배치Entity> BuildingPlacements => Set<SimulationWorld건물배치Entity>();
    public DbSet<SimulationWorld그래픽표현Entity> GraphicsPlans => Set<SimulationWorld그래픽표현Entity>();
    public DbSet<SimulationWorldUnity공간변환Entity> UnityTransformProfiles => Set<SimulationWorldUnity공간변환Entity>();
    public DbSet<SimulationWorldUnity타일ManifestEntity> UnityTileManifests => Set<SimulationWorldUnity타일ManifestEntity>();
    public DbSet<SimulationWorldUnity산출물Entity> UnityArtifacts => Set<SimulationWorldUnity산출물Entity>();
    public DbSet<SimulationWorld시각배치Entity> VisualPlacements => Set<SimulationWorld시각배치Entity>();
    public DbSet<SimulationWorldSynty경관RunEntity> SyntyLandscapeRuns => Set<SimulationWorldSynty경관RunEntity>();
    public DbSet<SimulationWorldSynty그래픽표현Entity> SyntyGraphicsPlans => Set<SimulationWorldSynty그래픽표현Entity>();
    public DbSet<SimulationWorldSynty시각배치Entity> SyntyVisualPlacements => Set<SimulationWorldSynty시각배치Entity>();
    public DbSet<SimulationWorldSynty배치거부Entity> SyntyRejections => Set<SimulationWorldSynty배치거부Entity>();
    public DbSet<SimulationWorld객체표현규칙CatalogEntity> ObjectRepresentationRuleCatalogs => Set<SimulationWorld객체표현규칙CatalogEntity>();
    public DbSet<SimulationWorld업무규칙CatalogEntity> BusinessRuleCatalogs => Set<SimulationWorld업무규칙CatalogEntity>();
    public DbSet<SimulationWorld시설의미Entity> FacilitySemantics => Set<SimulationWorld시설의미Entity>();
    public DbSet<SimulationWorld시설기능Entity> FacilityCapabilities => Set<SimulationWorld시설기능Entity>();
    public DbSet<SimulationWorld업무Simulation규칙Entity> BusinessSimulationRules => Set<SimulationWorld업무Simulation규칙Entity>();
    public DbSet<SimulationWorld업무Simulation규칙ParameterEntity> BusinessSimulationRuleParameters => Set<SimulationWorld업무Simulation규칙ParameterEntity>();
    public DbSet<SimulationWorld객체업무규칙연결Entity> ObjectBusinessRuleBindings => Set<SimulationWorld객체업무규칙연결Entity>();
    public DbSet<SimulationWorldScenario규칙묶음Entity> ScenarioRuleSets => Set<SimulationWorldScenario규칙묶음Entity>();
    public DbSet<SimulationWorldScenario규칙항목Entity> ScenarioRuleItems => Set<SimulationWorldScenario규칙항목Entity>();
    public DbSet<SimulationWorldUI기획CatalogEntity> UiPlanningCatalogs => Set<SimulationWorldUI기획CatalogEntity>();
    public DbSet<SimulationWorldUI설계근거Entity> UiDesignEvidence => Set<SimulationWorldUI설계근거Entity>();
    public DbSet<SimulationWorldUI화면영역Entity> UiSurfaces => Set<SimulationWorldUI화면영역Entity>();
    public DbSet<SimulationWorldUI정보항목Entity> UiInformationItems => Set<SimulationWorldUI정보항목Entity>();
    public DbSet<SimulationWorldUI상태표현Entity> UiStatePresentations => Set<SimulationWorldUI상태표현Entity>();
    public DbSet<SimulationWorldUI행동후보Entity> UiActionCandidates => Set<SimulationWorldUI행동후보Entity>();
    public DbSet<SimulationWorldUI업무규칙연결Entity> UiBusinessRuleBindings => Set<SimulationWorldUI업무규칙연결Entity>();
    public DbSet<SimulationWorld공간규칙MetadataEntity> SpatialRuleMetadata => Set<SimulationWorld공간규칙MetadataEntity>();
    public DbSet<SimulationWorldSimulation규칙MetadataEntity> SimulationRuleMetadata => Set<SimulationWorldSimulation규칙MetadataEntity>();
    public DbSet<SimulationWorld객체표현결합규칙Entity> ObjectRepresentationBindingRules => Set<SimulationWorld객체표현결합규칙Entity>();
    public DbSet<SimulationWorld객체표현해석RunEntity> ObjectRepresentationInterpretationRuns => Set<SimulationWorld객체표현해석RunEntity>();
    public DbSet<SimulationWorld객체표현해석ResultEntity> ObjectRepresentationInterpretationResults => Set<SimulationWorld객체표현해석ResultEntity>();
    public DbSet<SimulationWorld지역표현요약ProfileEntity> RegionSummaryProfiles => Set<SimulationWorld지역표현요약ProfileEntity>();
    public DbSet<SimulationWorld지역표현요약RunEntity> RegionSummaryRuns => Set<SimulationWorld지역표현요약RunEntity>();
    public DbSet<SimulationWorld지역표현요약ItemEntity> RegionSummaryItems => Set<SimulationWorld지역표현요약ItemEntity>();
    public DbSet<SimulationWorld지역표현요약CategoryReportEntity> RegionSummaryCategoryReports => Set<SimulationWorld지역표현요약CategoryReportEntity>();
    public DbSet<SimulationWorld경관조립실행Entity> LandscapeAssemblyRuns => Set<SimulationWorld경관조립실행Entity>();
    public DbSet<SimulationWorld경관공간NodeEntity> LandscapeNodes => Set<SimulationWorld경관공간NodeEntity>();
    public DbSet<SimulationWorld경관공간EdgeEntity> LandscapeEdges => Set<SimulationWorld경관공간EdgeEntity>();
    public DbSet<SimulationWorld경관모판배치Entity> LandscapePlacements => Set<SimulationWorld경관모판배치Entity>();
    public DbSet<SimulationWorld경관조립미해결Entity> LandscapeUnresolved => Set<SimulationWorld경관조립미해결Entity>();
    public DbSet<SimulationWorldAreaSet정의Entity> AreaSetDefinitions => Set<SimulationWorldAreaSet정의Entity>();
    public DbSet<SimulationWorldAreaSet공간참조Entity> AreaSetSpatialRefs => Set<SimulationWorldAreaSet공간참조Entity>();
    public DbSet<SimulationWorldAreaSetGraph참조Entity> AreaSetGraphRefs => Set<SimulationWorldAreaSetGraph참조Entity>();
    public DbSet<SimulationWorld경관Graph정의Entity> LandscapeGraphDefinitions => Set<SimulationWorld경관Graph정의Entity>();
    public DbSet<SimulationWorld경관Graph공간참조Entity> LandscapeGraphSpatialRefs => Set<SimulationWorld경관Graph공간참조Entity>();
    public DbSet<SimulationWorld경관GraphTile참조Entity> LandscapeGraphTileRefs => Set<SimulationWorld경관GraphTile참조Entity>();
    public DbSet<SimulationWorld경관Graph관계Entity> LandscapeGraphRelations => Set<SimulationWorld경관Graph관계Entity>();
    public DbSet<SimulationWorld상호작용Graph준비도Entity> WorldInteractionGraphReadiness =>
        Set<SimulationWorld상호작용Graph준비도Entity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SimulationWorld파생RunConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld원본계보Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld파생NodeConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld파생RelationConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld건물배치Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld그래픽표현Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUnity공간변환Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUnity타일ManifestConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUnity산출물Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld시각배치Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldSynty경관RunConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorldSynty그래픽표현Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldSynty시각배치Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldSynty배치거부Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld객체표현규칙CatalogConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld업무규칙CatalogConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld시설의미Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld시설기능Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld업무Simulation규칙Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld업무Simulation규칙ParameterConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld객체업무규칙연결Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldScenario규칙묶음Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldScenario규칙항목Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI기획CatalogConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI설계근거Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI화면영역Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI정보항목Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI상태표현Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI행동후보Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldUI업무규칙연결Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld공간규칙MetadataConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorldSimulation규칙MetadataConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld객체표현결합규칙Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld객체표현해석RunConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld객체표현해석ResultConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld지역표현요약ProfileConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld지역표현요약RunConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld지역표현요약ItemConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld지역표현요약CategoryReportConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관조립실행Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관공간NodeConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관공간EdgeConfiguration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관모판배치Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관조립미해결Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldAreaSet정의Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldAreaSet공간참조Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorldAreaSetGraph참조Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관Graph정의Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관Graph공간참조Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관GraphTile참조Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld경관Graph관계Configuration());
        modelBuilder.ApplyConfiguration(new SimulationWorld상호작용Graph준비도Configuration());
    }
}

public sealed class SimulationWorld파생RunEntity
{
    public long Id { get; set; }
    public int SchemaVersion { get; set; }
    public string BuildStableId { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string RecipeRevision { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string? VisualCatalogRevision { get; set; }
    public int Seed { get; set; }
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld원본계보Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string SourceStableId { get; set; } = string.Empty;
    public string SourceDatabaseCode { get; set; } = string.Empty;
    public string DatasetCode { get; set; } = string.Empty;
    public string SourceRevision { get; set; } = string.Empty;
    public string SourceHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset? ReferenceTimeUtc { get; set; }
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorld파생NodeEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string NodeKindCode { get; set; } = string.Empty;
    public string? SourceStableId { get; set; }
    public string? SourceRecordStableId { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? TileKey { get; set; }
    public string? AreaStableId { get; set; }
    public string? DisplayName { get; set; }
    public string? RepresentativeGroupCode { get; set; }
    public int? RepresentedRecordCount { get; set; }
    public int? RepresentativeRank { get; set; }
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorld건물배치Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string AreaNodeStableId { get; set; } = string.Empty;
    public string BuildingNodeStableId { get; set; } = string.Empty;
    public string PlacementBasisCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string BuildingCategoryCode { get; set; } = string.Empty;
    public string VisualFamilyCode { get; set; } = string.Empty;
    public int FloorCount { get; set; }
    public decimal? FootprintAreaSquareMeters { get; set; }
    public decimal? HeightMeters { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal PositionZ { get; set; }
    public decimal RotationY { get; set; }
    public bool PresentationOnly { get; set; }
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorld파생RelationEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string FromNodeStableId { get; set; } = string.Empty;
    public string RelationCode { get; set; } = string.Empty;
    public string ToNodeStableId { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string? SourceStableId { get; set; }
    public decimal Confidence { get; set; }
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorld시각배치Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
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
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorld그래픽표현Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
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
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorldUnity공간변환Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string SourceCrsCode { get; set; } = string.Empty;
    public string AxisMappingCode { get; set; } = string.Empty;
    public decimal? OriginEastingMeters { get; set; }
    public decimal? OriginNorthingMeters { get; set; }
    public decimal? ReferenceElevationMeters { get; set; }
    public decimal HorizontalScale { get; set; }
    public decimal VerticalExaggeration { get; set; }
    public decimal MetersPerUnityUnit { get; set; }
    public string RuleRevision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string ProfileHashSha256 { get; set; } = string.Empty;
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorldUnity타일ManifestEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string TransformProfileStableId { get; set; } = string.Empty;
    public string TileKey { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal SizeMeters { get; set; }
    public decimal HaloMeters { get; set; }
    public decimal MinEastingMeters { get; set; }
    public decimal MinNorthingMeters { get; set; }
    public decimal MaxEastingMeters { get; set; }
    public decimal MaxNorthingMeters { get; set; }
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string ManifestHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

public sealed class SimulationWorldUnity산출물Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string TileManifestStableId { get; set; } = string.Empty;
    public string ArtifactKindCode { get; set; } = string.Empty;
    public string LodCode { get; set; } = string.Empty;
    public string? StorageObjectKey { get; set; }
    public string? ArtifactHashSha256 { get; set; }
    public string? SourceRevision { get; set; }
    public string? SourceHashSha256 { get; set; }
    public string? SourceReferenceDate { get; set; }
    public string? HorizontalCrsCode { get; set; }
    public string? VerticalDatumCode { get; set; }
    public decimal? ResolutionMeters { get; set; }
    public string? NoDataValue { get; set; }
    public string? ArtifactFormatCode { get; set; }
    public long? ArtifactByteLength { get; set; }
    public int? SampleWidth { get; set; }
    public int? SampleHeight { get; set; }
    public long? VertexCount { get; set; }
    public long? TriangleCount { get; set; }
    public int? MaterialSlotCount { get; set; }
    public int? EstimatedDrawCallCount { get; set; }
    public string? BoundaryVertexHashSha256 { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public SimulationWorld파생RunEntity Run { get; set; } = null!;
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.SimulationWorldDerivation,
    SsalddelCodeLayer.Infrastructure,
    "파생 World 원장과 입력·출력 hash를 별도 DB에 멱등 저장한다.",
    StepKey = "infrastructure.derived-world-store",
    DependsOnStepKeys = new string[] { "application.pyeongchang-derivation" },
    ExecutionStage = SsalddelCodeExecutionStage.Persistence,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
    WritesTo = SsalddelCodeDataScope.DerivedWorld,
    FlowOrder = 30,
    Boundary = "SimulationWorldDerived DB만 변경하며 입력 fingerprint가 다른 같은 식별자는 충돌로 거부한다.")]
public sealed class SimulationWorld파생원장Store(
    SimulationWorld파생DbContext dbContext) : ISimulationWorld파생원장Store
{
    public const string ConflictCode = "SimulationWorldDerivationConflict";

    public async Task<SimulationWorld파생원장저장결과> 저장Async(
        SimulationWorld파생원장 ledger,
        CancellationToken cancellationToken)
    {
        SimulationWorld파생원장Validator.Validate(ledger);
        var outputHash = SimulationWorld파생원장Hash.Compute(ledger);
        var existing = await dbContext.Runs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.BuildStableId == ledger.BuildStableId,
                cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.InputFingerprintSha256, ledger.InputFingerprintSha256,
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.OutputHashSha256, outputHash,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(ConflictCode);
            return Result(false, ledger, outputHash);
        }

        var run = new SimulationWorld파생RunEntity
        {
            SchemaVersion = ledger.SchemaVersion,
            BuildStableId = ledger.BuildStableId,
            AreaSetStableId = ledger.AreaSetStableId,
            RecipeRevision = ledger.RecipeRevision,
            RuleRevision = ledger.RuleRevision,
            VisualCatalogRevision = ledger.VisualCatalogRevision,
            Seed = ledger.Seed,
            InputFingerprintSha256 = ledger.InputFingerprintSha256.ToLowerInvariant(),
            OutputHashSha256 = outputHash,
            GeneratedAtUtc = ledger.GeneratedAtUtc,
            StoredAtUtc = DateTimeOffset.UtcNow,
        };
        dbContext.Runs.Add(run);

        dbContext.Sources.AddRange(ledger.Sources.Select(item => new SimulationWorld원본계보Entity
        {
            Run = run,
            SourceStableId = item.SourceStableId,
            SourceDatabaseCode = item.SourceDatabaseCode,
            DatasetCode = item.DatasetCode,
            SourceRevision = item.SourceRevision,
            SourceHashSha256 = item.SourceHashSha256.ToLowerInvariant(),
            ReferenceTimeUtc = item.ReferenceTimeUtc,
        }));
        dbContext.Nodes.AddRange(ledger.Nodes.Select(item => new SimulationWorld파생NodeEntity
        {
            Run = run,
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
        }));
        dbContext.Relations.AddRange(ledger.Relations.Select(item => new SimulationWorld파생RelationEntity
        {
            Run = run,
            StableId = item.StableId,
            FromNodeStableId = item.FromNodeStableId,
            RelationCode = item.RelationCode,
            ToNodeStableId = item.ToNodeStableId,
            EvidenceKindCode = item.EvidenceKindCode,
            SourceStableId = item.SourceStableId,
            Confidence = item.Confidence,
        }));
        dbContext.BuildingPlacements.AddRange(ledger.BuildingPlacements.Select(item => new SimulationWorld건물배치Entity
        {
            Run = run,
            StableId = item.StableId,
            AreaNodeStableId = item.AreaNodeStableId,
            BuildingNodeStableId = item.BuildingNodeStableId,
            PlacementBasisCode = item.PlacementBasisCode,
            EvidenceKindCode = item.EvidenceKindCode,
            BuildingCategoryCode = item.BuildingCategoryCode,
            VisualFamilyCode = item.VisualFamilyCode,
            FloorCount = item.FloorCount,
            FootprintAreaSquareMeters = item.FootprintAreaSquareMeters,
            HeightMeters = item.HeightMeters,
            PositionX = item.PositionX,
            PositionY = item.PositionY,
            PositionZ = item.PositionZ,
            RotationY = item.RotationY,
            PresentationOnly = item.PresentationOnly,
        }));
        dbContext.GraphicsPlans.AddRange(ledger.GraphicsPlans.Select(item => new SimulationWorld그래픽표현Entity
        {
            Run = run,
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
        dbContext.UnityTransformProfiles.AddRange(ledger.UnityTransformProfiles.Select(item => new SimulationWorldUnity공간변환Entity
        {
            Run = run,
            StableId = item.StableId,
            AreaSetStableId = item.AreaSetStableId,
            SourceCrsCode = item.SourceCrsCode,
            AxisMappingCode = item.AxisMappingCode,
            OriginEastingMeters = item.OriginEastingMeters,
            OriginNorthingMeters = item.OriginNorthingMeters,
            ReferenceElevationMeters = item.ReferenceElevationMeters,
            HorizontalScale = item.HorizontalScale,
            VerticalExaggeration = item.VerticalExaggeration,
            MetersPerUnityUnit = item.MetersPerUnityUnit,
            RuleRevision = item.RuleRevision,
            StatusCode = item.StatusCode,
            ProfileHashSha256 = item.ProfileHashSha256,
        }));
        dbContext.UnityTileManifests.AddRange(ledger.UnityTileManifests.Select(item => new SimulationWorldUnity타일ManifestEntity
        {
            Run = run,
            StableId = item.StableId,
            TransformProfileStableId = item.TransformProfileStableId,
            TileKey = item.TileKey,
            Level = item.Level,
            SizeMeters = item.SizeMeters,
            HaloMeters = item.HaloMeters,
            MinEastingMeters = item.MinEastingMeters,
            MinNorthingMeters = item.MinNorthingMeters,
            MaxEastingMeters = item.MaxEastingMeters,
            MaxNorthingMeters = item.MaxNorthingMeters,
            InputFingerprintSha256 = item.InputFingerprintSha256,
            ManifestHashSha256 = item.ManifestHashSha256,
            StatusCode = item.StatusCode,
        }));
        dbContext.UnityArtifacts.AddRange(ledger.UnityArtifacts.Select(item => new SimulationWorldUnity산출물Entity
        {
            Run = run,
            StableId = item.StableId,
            TileManifestStableId = item.TileManifestStableId,
            ArtifactKindCode = item.ArtifactKindCode,
            LodCode = item.LodCode,
            StorageObjectKey = item.StorageObjectKey,
            ArtifactHashSha256 = item.ArtifactHashSha256,
            SourceRevision = item.SourceRevision,
            SourceHashSha256 = item.SourceHashSha256,
            SourceReferenceDate = item.SourceReferenceDate,
            HorizontalCrsCode = item.HorizontalCrsCode,
            VerticalDatumCode = item.VerticalDatumCode,
            ResolutionMeters = item.ResolutionMeters,
            NoDataValue = item.NoDataValue,
            ArtifactFormatCode = item.ArtifactFormatCode,
            ArtifactByteLength = item.ArtifactByteLength,
            SampleWidth = item.SampleWidth,
            SampleHeight = item.SampleHeight,
            VertexCount = item.VertexCount,
            TriangleCount = item.TriangleCount,
            MaterialSlotCount = item.MaterialSlotCount,
            EstimatedDrawCallCount = item.EstimatedDrawCallCount,
            BoundaryVertexHashSha256 = item.BoundaryVertexHashSha256,
            StatusCode = item.StatusCode,
        }));
        dbContext.VisualPlacements.AddRange(ledger.VisualPlacements.Select(item => new SimulationWorld시각배치Entity
        {
            Run = run,
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
        dbContext.Add지역표현요약(run, ledger);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result(true, ledger, outputHash);
    }

    private static SimulationWorld파생원장저장결과 Result(
        bool inserted,
        SimulationWorld파생원장 ledger,
        string outputHash) => new()
        {
            Inserted = inserted,
            BuildStableId = ledger.BuildStableId,
            OutputHashSha256 = outputHash,
            SourceCount = ledger.Sources.Count,
            NodeCount = ledger.Nodes.Count,
            RelationCount = ledger.Relations.Count,
            BuildingPlacementCount = ledger.BuildingPlacements.Count,
            GraphicsPlanCount = ledger.GraphicsPlans.Count,
            UnityTransformProfileCount = ledger.UnityTransformProfiles.Count,
            UnityTileManifestCount = ledger.UnityTileManifests.Count,
            UnityArtifactCount = ledger.UnityArtifacts.Count,
            VisualPlacementCount = ledger.VisualPlacements.Count,
        };
}

public static class SimulationWorldDerivationPersistenceRegistration
{
    public static IServiceCollection AddSimulationWorldDerivationPersistence(
        this IServiceCollection services,
        string connectionString,
        string? landscapeGrammarManifestPath = null,
        string? areaSetDefinitionPath = null)
    {
        services.AddDbContext<SimulationWorld파생DbContext>(options => options.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 4, 0)),
            mysql =>
            {
                mysql.MigrationsAssembly("Ssalddel.Simulation.Persistence");
                mysql.MigrationsHistoryTable("__EF마이그레이션이력_시뮬레이션월드파생");
            }));
        services.AddScoped<ISimulationWorld파생원장Store, SimulationWorld파생원장Store>();
        services.AddScoped<ISimulationWorld지역ProjectionReader, SimulationWorld지역ProjectionReader>();
        services.AddScoped<ISimulationWorld지역표현요약Reader, SimulationWorld지역표현요약Reader>();
        services.AddScoped<ISimulationWorldTileArtifactReader, SimulationWorldTileArtifactReader>();
        services.AddScoped<ISimulationWorld객체표현규칙Store, SimulationWorld객체표현규칙Store>();
        services.AddScoped<SimulationWorld객체표현해석JobShell>();
        services.AddScoped<SimulationWorld건물종류DemoPipeline>();
        services.AddScoped<ISimulationWorld공간실행Reader, SimulationWorld공간실행Reader>();
        services.AddScoped<ISimulationWorldSynty경관Store, SimulationWorldSynty경관Store>();
        services.AddScoped<ISimulationWorldSynty경관Planner, SimulationWorld기본Synty경관Planner>();
        services.AddScoped<SimulationWorldSynty경관JobShell>();
        services.AddScoped<ISimulationWorld업무규칙집결Store, SimulationWorld업무규칙집결Store>();
        services.AddScoped<SimulationWorld업무규칙집결JobShell>();
        services.AddScoped<ISimulationWorld업무규칙집결Reader, SimulationWorld업무규칙집결Reader>();
        services.AddSingleton<ISimulationWorldUI기획Assembler, PyeongchangSimulationWorldUI기획Assembler>();
        services.AddScoped<ISimulationWorldUI기획Store, SimulationWorldUI기획Store>();
        services.AddScoped<SimulationWorldUI기획JobShell>();
        services.AddScoped<ISimulationWorldLandscapeCompositionStore,
            SimulationWorldLandscapeCompositionStore>();
        services.AddScoped<ISimulationWorldLandscapeCompositionReader,
            SimulationWorldLandscapeCompositionStore>();
        services.AddScoped<ISimulationWorldAreaSetGraphStore,
            SimulationWorldAreaSetGraphStore>();
        services.AddScoped<ISimulationWorld상호작용GraphReadinessStore,
            SimulationWorld상호작용GraphReadinessStore>();
        services.AddSingleton<ISimulationWorldLandscapeGrammarCatalogReader>(
            new SimulationWorldLandscapeGrammarManifestReader(
                landscapeGrammarManifestPath ?? string.Empty));
        services.AddSingleton<ISimulationWorldAreaSetDefinitionReader>(
            new FileSimulationWorldAreaSetDefinitionReader(
                areaSetDefinitionPath ?? string.Empty));
        services.AddScoped<ISimulationDatabaseReadinessProbe,
            SimulationWorldDerivedReadinessProbe>();
        return services;
    }
}

internal sealed class SimulationWorld파생RunConfiguration
    : IEntityTypeConfiguration<SimulationWorld파생RunEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld파생RunEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_파생실행");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.BuildStableId).IsUnique();
        Column(builder.Property(item => item.Id), "식별번호");
        Column(builder.Property(item => item.SchemaVersion), "스키마버전");
        Text(builder.Property(item => item.BuildStableId), "파생실행고유식별자", 200);
        Text(builder.Property(item => item.AreaSetStableId), "영역묶음고유식별자", 200);
        Text(builder.Property(item => item.RecipeRevision), "생성조리법개정번호", 120);
        Text(builder.Property(item => item.RuleRevision), "관계규칙개정번호", 120);
        builder.Property(item => item.VisualCatalogRevision)
            .HasColumnName("시각자산대장개정번호")
            .HasMaxLength(120);
        Column(builder.Property(item => item.Seed), "배치시드");
        Text(builder.Property(item => item.InputFingerprintSha256), "입력지문SHA256", 64);
        Text(builder.Property(item => item.OutputHashSha256), "출력해시SHA256", 64);
        Column(builder.Property(item => item.GeneratedAtUtc), "생성시각UTC");
        Column(builder.Property(item => item.StoredAtUtc), "저장시각UTC");
    }

    internal static void Text(
        PropertyBuilder<string> property,
        string columnName,
        int length) =>
        property.HasColumnName(columnName).HasMaxLength(length).IsRequired();

    internal static void Column<T>(PropertyBuilder<T> property, string columnName) =>
        property.HasColumnName(columnName);
}

internal sealed class SimulationWorld원본계보Configuration
    : IEntityTypeConfiguration<SimulationWorld원본계보Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld원본계보Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_원본계보");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.SourceStableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SourceStableId), "원본계보고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SourceDatabaseCode), "원본DB코드", 80);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.DatasetCode), "자료코드", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SourceRevision), "원본개정번호", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SourceHashSha256), "원본SHA256", 64);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.ReferenceTimeUtc), "자료기준시각UTC");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld파생NodeConfiguration
    : IEntityTypeConfiguration<SimulationWorld파생NodeEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld파생NodeEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_파생노드");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.NodeKindCode), "노드종류코드", 80);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        builder.Property(item => item.SourceStableId).HasColumnName("원본계보고유식별자").HasMaxLength(200);
        builder.Property(item => item.SourceRecordStableId).HasColumnName("원본레코드고유식별자").HasMaxLength(200);
        builder.Property(item => item.RegionCode).HasColumnName("행정구역코드").HasMaxLength(40);
        builder.Property(item => item.TileKey).HasColumnName("타일키").HasMaxLength(120);
        builder.Property(item => item.AreaStableId).HasColumnName("영역고유식별자").HasMaxLength(200);
        builder.Property(item => item.DisplayName).HasColumnName("표시이름").HasMaxLength(240);
        builder.Property(item => item.RepresentativeGroupCode).HasColumnName("대표군코드").HasMaxLength(300);
        builder.Property(item => item.RepresentedRecordCount).HasColumnName("대표원본건수");
        builder.Property(item => item.RepresentativeRank).HasColumnName("대표순위");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld건물배치Configuration
    : IEntityTypeConfiguration<SimulationWorld건물배치Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld건물배치Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_건물배치계획");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "건물배치고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.AreaNodeStableId), "영역노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.BuildingNodeStableId), "건물노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.PlacementBasisCode), "배치근거코드", 50);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.BuildingCategoryCode), "건물분류코드", 100);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.VisualFamilyCode), "시각Family코드", 120);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.FloorCount), "표현층수");
        builder.Property(item => item.FootprintAreaSquareMeters).HasColumnName("건물바닥면적제곱미터").HasPrecision(18, 4);
        builder.Property(item => item.HeightMeters).HasColumnName("높이미터").HasPrecision(12, 4);
        builder.Property(item => item.PositionX).HasColumnName("위치X").HasPrecision(18, 4);
        builder.Property(item => item.PositionY).HasColumnName("위치Y").HasPrecision(18, 4);
        builder.Property(item => item.PositionZ).HasColumnName("위치Z").HasPrecision(18, 4);
        builder.Property(item => item.RotationY).HasColumnName("Y축회전").HasPrecision(9, 4);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld파생RelationConfiguration
    : IEntityTypeConfiguration<SimulationWorld파생RelationEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld파생RelationEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_파생관계");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "관계고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.FromNodeStableId), "시작노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.RelationCode), "관계코드", 100);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ToNodeStableId), "도착노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        builder.Property(item => item.SourceStableId).HasColumnName("원본계보고유식별자").HasMaxLength(200);
        builder.Property(item => item.Confidence).HasColumnName("신뢰도").HasPrecision(6, 5);
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld시각배치Configuration
    : IEntityTypeConfiguration<SimulationWorld시각배치Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld시각배치Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_시각배치계획");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "시각배치고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TargetNodeStableId), "대상노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.VisualKey), "시각키", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.LodCode), "세부표현단계코드", 40);
        builder.Property(item => item.PositionX).HasColumnName("위치X").HasPrecision(18, 4);
        builder.Property(item => item.PositionY).HasColumnName("위치Y").HasPrecision(18, 4);
        builder.Property(item => item.PositionZ).HasColumnName("위치Z").HasPrecision(18, 4);
        builder.Property(item => item.RotationY).HasColumnName("Y축회전").HasPrecision(9, 4);
        builder.Property(item => item.UniformScale).HasColumnName("균일축척").HasPrecision(9, 4);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld그래픽표현Configuration
    : IEntityTypeConfiguration<SimulationWorld그래픽표현Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld그래픽표현Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_그래픽표현계획");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "그래픽표현고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TargetNodeStableId), "대상노드고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.PresentationScopeCode), "표현범위코드", 80);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TextureSetKey), "질감세트키", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.MaterialVariantKey), "재질변형키", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ColorPaletteKey), "색조팔레트키", 120);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.BackgroundProfileKey), "배경Profile키", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.LightingProfileKey), "조명Profile키", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TimeOfDayProfileKey), "시간대Profile키", 160);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ShadowPolicyCode), "그림자정책코드", 40);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.CastShadows), "그림자투사여부");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.ReceiveShadows), "그림자수신여부");
        builder.Property(item => item.ContactShadowStrength).HasColumnName("접지그림자강도").HasPrecision(5, 4);
        builder.Property(item => item.ShadowDistanceMeters).HasColumnName("그림자거리미터").HasPrecision(12, 4);
        builder.Property(item => item.AmbientOcclusionStrength).HasColumnName("주변광차폐강도").HasPrecision(5, 4);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.LodCode), "세부표현단계코드", 40);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.QualityTierCode), "품질단계코드", 40);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorldUnity공간변환Configuration
    : IEntityTypeConfiguration<SimulationWorldUnity공간변환Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldUnity공간변환Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_Unity공간변환Profile");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "공간변환고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.AreaSetStableId), "영역묶음고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.SourceCrsCode), "원본좌표계코드", 40);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.AxisMappingCode), "좌표축변환코드", 80);
        builder.Property(item => item.OriginEastingMeters).HasColumnName("Unity원점동쪽좌표미터").HasPrecision(18, 4);
        builder.Property(item => item.OriginNorthingMeters).HasColumnName("Unity원점북쪽좌표미터").HasPrecision(18, 4);
        builder.Property(item => item.ReferenceElevationMeters).HasColumnName("기준표고미터").HasPrecision(12, 4);
        builder.Property(item => item.HorizontalScale).HasColumnName("수평축척률").HasPrecision(12, 6);
        builder.Property(item => item.VerticalExaggeration).HasColumnName("높이과장률").HasPrecision(12, 6);
        builder.Property(item => item.MetersPerUnityUnit).HasColumnName("Unity단위당미터").HasPrecision(12, 6);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.RuleRevision), "변환규칙개정번호", 120);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StatusCode), "변환상태코드", 50);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ProfileHashSha256), "변환ProfileSHA256", 64);
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorldUnity타일ManifestConfiguration
    : IEntityTypeConfiguration<SimulationWorldUnity타일ManifestEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldUnity타일ManifestEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_Unity타일Manifest");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        builder.HasIndex(item => new { item.RunId, item.TileKey });
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "타일Manifest고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TransformProfileStableId), "공간변환고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TileKey), "타일키", 120);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Level), "타일단계");
        builder.Property(item => item.SizeMeters).HasColumnName("타일크기미터").HasPrecision(12, 4);
        builder.Property(item => item.HaloMeters).HasColumnName("여유영역미터").HasPrecision(12, 4);
        builder.Property(item => item.MinEastingMeters).HasColumnName("최소동쪽좌표미터").HasPrecision(18, 4);
        builder.Property(item => item.MinNorthingMeters).HasColumnName("최소북쪽좌표미터").HasPrecision(18, 4);
        builder.Property(item => item.MaxEastingMeters).HasColumnName("최대동쪽좌표미터").HasPrecision(18, 4);
        builder.Property(item => item.MaxNorthingMeters).HasColumnName("최대북쪽좌표미터").HasPrecision(18, 4);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.InputFingerprintSha256), "입력지문SHA256", 64);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ManifestHashSha256), "ManifestSHA256", 64);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StatusCode), "생성상태코드", 50);
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorldUnity산출물Configuration
    : IEntityTypeConfiguration<SimulationWorldUnity산출물Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldUnity산출물Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_Unity산출물");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.StableId }).IsUnique();
        builder.HasIndex(item => new { item.RunId, item.TileManifestStableId });
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.Id), "식별번호");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.RunId), "파생실행식별번호");
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StableId), "산출물고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.TileManifestStableId), "타일Manifest고유식별자", 200);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.ArtifactKindCode), "산출물종류코드", 80);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.LodCode), "세부표현단계코드", 40);
        builder.Property(item => item.StorageObjectKey).HasColumnName("산출물보관객체키").HasMaxLength(500);
        builder.Property(item => item.ArtifactHashSha256).HasColumnName("산출물SHA256").HasMaxLength(64);
        builder.Property(item => item.SourceRevision).HasColumnName("원본개정번호").HasMaxLength(160);
        builder.Property(item => item.SourceHashSha256).HasColumnName("원본SHA256").HasMaxLength(64);
        builder.Property(item => item.SourceReferenceDate).HasColumnName("원본기준일").HasMaxLength(40);
        builder.Property(item => item.HorizontalCrsCode).HasColumnName("수평좌표계코드").HasMaxLength(40);
        builder.Property(item => item.VerticalDatumCode).HasColumnName("높이기준코드").HasMaxLength(80);
        builder.Property(item => item.ResolutionMeters).HasColumnName("원본해상도미터").HasPrecision(12, 4);
        builder.Property(item => item.NoDataValue).HasColumnName("NoData값").HasMaxLength(80);
        builder.Property(item => item.ArtifactFormatCode).HasColumnName("산출물형식코드").HasMaxLength(80);
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.ArtifactByteLength), "산출물바이트길이");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SampleWidth), "표본너비");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.SampleHeight), "표본높이");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.VertexCount), "정점수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.TriangleCount), "삼각형수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.MaterialSlotCount), "재질슬롯수");
        SimulationWorld파생RunConfiguration.Column(builder.Property(item => item.EstimatedDrawCallCount), "예상DrawCall수");
        builder.Property(item => item.BoundaryVertexHashSha256).HasColumnName("경계정점SHA256").HasMaxLength(64);
        SimulationWorld파생RunConfiguration.Text(builder.Property(item => item.StatusCode), "생성상태코드", 50);
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
