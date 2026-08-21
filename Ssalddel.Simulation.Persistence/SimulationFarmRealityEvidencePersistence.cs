using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationFarmRealityEvidenceEntity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public string CanonicalProductStableId { get; set; } = string.Empty;
    public string EvidenceRevision { get; set; } = string.Empty;
    public string InputHashSha256 { get; set; } = string.Empty;
    public string BundleJson { get; set; } = "{}";
    public DateTime SyncedAtUtc { get; set; }
}

public sealed class SimulationFarmRealityOperationalReader(
    AgriculturalFisheriesDbContext db,
    TimeProvider timeProvider) : ISimulationFarmRealityOperationalReader
{
    public async Task<SimulationFarmRealityEvidenceBundle> ReadApprovedAsync(
        string areaSetStableId, string canonicalProductStableId,
        CancellationToken cancellationToken)
    {
        var identity = await db.CommonFoodProductIdentities.AsNoTracking()
            .Include(item => item.CodeRelations.Where(relation => relation.IsActive))
            .SingleAsync(item => item.CanonicalProductStableId == canonicalProductStableId
                && item.IsActive, cancellationToken);
        var nongsaro = await db.NongsaroPotatoProfiles.AsNoTracking()
            .Where(item => item.CanonicalProductStableId == canonicalProductStableId
                && item.ApprovedForSimulationContext)
            .OrderByDescending(item => item.RetrievedAtUtc)
            .ThenByDescending(item => item.Revision)
            .FirstOrDefaultAsync(cancellationToken);
        var kamis = await db.KamisPriceObservations.AsNoTracking()
            .Where(item => item.CategoryCode == "100" && item.ItemCode == "152")
            .OrderByDescending(item => item.SurveyDate)
            .ThenByDescending(item => item.LastSeenAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var ams = await db.UsdaAmsMarketPriceObservations.AsNoTracking()
            .Where(item => item.Commodity == "Potatoes")
            .OrderByDescending(item => item.ReportEndDate)
            .ThenByDescending(item => item.LastSeenAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return new SimulationFarmRealityEvidenceBundle
        {
            AreaSetStableId = areaSetStableId,
            CanonicalProductStableId = canonicalProductStableId,
            ProductDisplayName = identity.DisplayName,
            ProductIdentityRevision = identity.Revision,
            CreatedAtUtc = timeProvider.GetUtcNow(),
            Sources = new[]
            {
                NongsaroWork(nongsaro, Relation(identity,
                    "nongsaro:farm-working-plan-new")),
                NongsaroDisaster(nongsaro, Relation(identity,
                    "nongsaro:farm-working-plan-new")),
                Kamis(kamis, Relation(identity, "kamis")),
                Ams(ams, Relation(identity, "usda-ams-market-news")),
            },
            ChangesSimulationRules = false,
            MovesSpatialDefinitions = false,
            CreatesIncidentOrEffect = false,
        };
    }

    private static Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계 Relation(
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Identity identity,
        string sourceKey) => identity.CodeRelations.Single(item => item.SourceKey == sourceKey);

    private static SimulationFarmRealitySourceEvidence NongsaroWork(
        Ssalddel.Domain.AgriculturalFisheries.Nongsaro감자ProfileArchive? item,
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계 relation)
        => item is null
            ? Missing("farm-reality:nongsaro-work-schedule", "nongsaro",
                "farm-working-plan-new", "농사로 농작업일정정보",
                relation.RelationStatusCode,
                new[] { SimulationRealityContextCodes.ReviewFieldWorkTiming })
            : Available("farm-reality:nongsaro-work-schedule", "nongsaro",
                "farm-working-plan-new", "농사로 농작업일정정보",
                "NONGSARO_CONTENT", item.WorkScheduleContentNo,
                relation.RelationStatusCode,
                item.RetrievedAtUtc, item.RetrievedAtUtc,
                "NationalReferenceContent", new[] { "content" },
                8760,
                item.SourceSetHashSha256, "https://www.nongsaro.go.kr",
                new[] { "NotAutomaticWorkOrIncidentRule", "NongsaroProductCodeUnlinked" },
                new[] { SimulationRealityContextCodes.ReviewFieldWorkTiming });

    private static SimulationFarmRealitySourceEvidence NongsaroDisaster(
        Ssalddel.Domain.AgriculturalFisheries.Nongsaro감자ProfileArchive? item,
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계 relation)
        => item is null
            ? Missing("farm-reality:nongsaro-disaster-prevention", "nongsaro",
                "crop-disaster-prevention", "농사로 농작물재해예방정보",
                relation.RelationStatusCode,
                new[] { SimulationRealityContextCodes.InspectCropHealth })
            : Available("farm-reality:nongsaro-disaster-prevention", "nongsaro",
                "crop-disaster-prevention", "농사로 농작물재해예방정보",
                "NONGSARO_REFERENCE", string.Empty,
                relation.RelationStatusCode,
                item.DisasterPreventionRetrievedAtUtc,
                item.DisasterPreventionRetrievedAtUtc,
                "NationalReferenceContent", new[] { "content" },
                8760,
                item.DisasterPreventionHashSha256, "https://www.nongsaro.go.kr",
                new[] { "NotAutomaticWorkOrIncidentRule", "ReferenceIsNotFarmObservation" },
                new[] { SimulationRealityContextCodes.InspectCropHealth });

    private static SimulationFarmRealitySourceEvidence Kamis(
        Ssalddel.Domain.AgriculturalFisheries.KamisPriceObservation? item,
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계 relation)
        => item is null
            ? Missing("farm-reality:kamis-potato", "kamis", "price-observations",
                "KAMIS 감자 가격 관측", relation.RelationStatusCode,
                new[] { SimulationRealityContextCodes.ReviewShipmentTiming })
            : Available("farm-reality:kamis-potato", "kamis", "price-observations",
                "KAMIS 감자 가격 관측", relation.CodeScheme,
                relation.ExternalCode ?? string.Empty, relation.RelationStatusCode,
                item.SurveyDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                DateTime.SpecifyKind(item.LastSeenAtUtc, DateTimeKind.Utc),
                "MarketSurvey", Units(item.Unit, "KRW"), 168, Sha256(item.RawJson),
                item.SourceUrl, new[] { "MarketContextOnly",
                    "UnitAndMarketStageAlignmentRequired", "NotProductionProfitOrSalePriceRule" },
                new[] { SimulationRealityContextCodes.ReviewShipmentTiming });

    private static SimulationFarmRealitySourceEvidence Ams(
        Ssalddel.Domain.AgriculturalFisheries.UsdaAms시장가격관측? item,
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계 relation)
        => item is null
            ? Missing("farm-reality:usda-ams-potato", "usda-ams-market-news",
                "market-price-observations", "USDA AMS 감자 시장가격 관측",
                relation.RelationStatusCode,
                new[] { SimulationRealityContextCodes.ReviewShipmentTiming })
            : Available("farm-reality:usda-ams-potato", "usda-ams-market-news",
                "market-price-observations", "USDA AMS 감자 시장가격 관측",
                relation.CodeScheme, relation.ExternalCode ?? string.Empty,
                relation.RelationStatusCode,
                item.ReportEndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                DateTime.SpecifyKind(item.LastSeenAtUtc, DateTimeKind.Utc),
                "ReportedMarket", Units(item.OriginalUnit, item.CurrencyCode), 168,
                Sha256(item.RawJson), "https://www.ams.usda.gov/market-news",
                new[] { "CandidateProductRelation", "MarketContextOnly",
                    "UnitAndMarketStageAlignmentRequired", "NoCrossSourcePriceRanking" },
                new[] { SimulationRealityContextCodes.ReviewShipmentTiming });

    private static SimulationFarmRealitySourceEvidence Missing(
        string stableId, string sourceId, string datasetId, string name,
        string relationStatus, string[] advisories) => new()
    {
        SourceEvidenceStableId = stableId,
        SourceId = sourceId,
        DatasetId = datasetId,
        SourceName = name,
        RelationStatusCode = relationStatus,
        AvailabilityCode = SimulationRealityContextCodes.Unavailable,
        QualityCode = SimulationRealityContextCodes.Unavailable,
        LimitationCodes = new[] { "ApprovedObservationNotCollected", "NoScenarioFallback" },
        AdvisoryCodes = advisories,
    };

    private static SimulationFarmRealitySourceEvidence Available(
        string stableId, string sourceId, string datasetId, string name,
        string codeScheme, string externalCode, string relationStatus,
        DateTime observedAtUtc, DateTime retrievedAtUtc,
        string spatialPrecision, string[] units, int maxAgeHours, string hash, string href,
        string[] limitations, string[] advisories) => new()
    {
        SourceEvidenceStableId = stableId,
        SourceId = sourceId,
        DatasetId = datasetId,
        SourceName = name,
        CodeScheme = codeScheme,
        ExternalCode = externalCode,
        RelationStatusCode = relationStatus,
        AvailabilityCode = SimulationRealityContextCodes.Available,
        QualityCode = SimulationRealityContextCodes.Valid,
        ObservedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(observedAtUtc, DateTimeKind.Utc)),
        RetrievedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(retrievedAtUtc, DateTimeKind.Utc)),
        SpatialPrecisionCode = spatialPrecision,
        UnitCodes = units,
        MaxAgeHours = maxAgeHours,
        SourceHashSha256 = hash,
        SourceHref = href,
        LimitationCodes = limitations,
        AdvisoryCodes = advisories,
    };

    private static string[] Units(params string[] values) => values
        .Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();

    private static string Sha256(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.SimulationFarmRealityEvidence,
    SsalddelCodeLayer.Infrastructure,
    "승인 현실근거 묶음을 입력 hash 기준으로 Simulation World 파생 DB에 멱등 저장한다.",
    StepKey = "infrastructure.farm-reality-store",
    DependsOnStepKeys = new[] { "application.farm-reality-sync" },
    ExecutionStage = SsalddelCodeExecutionStage.Persistence,
    ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
    WritesTo = SsalddelCodeDataScope.DerivedWorld,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    FlowOrder = 40,
    Boundary = "Simulation World 파생 DB만 변경하며 같은 revision의 다른 hash를 거부한다.")]
public sealed class SimulationFarmRealityEvidenceStore(
    SimulationWorld파생DbContext db,
    TimeProvider timeProvider) : ISimulationFarmRealityEvidenceStore
{
    public async Task<SimulationFarmRealityEvidenceSyncResponse> UpsertAsync(
        SimulationFarmRealityEvidenceBundle bundle,
        CancellationToken cancellationToken)
    {
        var existing = await db.FarmRealityEvidence.SingleOrDefaultAsync(item =>
            item.AreaSetStableId == bundle.AreaSetStableId
            && item.CanonicalProductStableId == bundle.CanonicalProductStableId
            && item.EvidenceRevision == bundle.EvidenceRevision, cancellationToken);
        if (existing is not null)
        {
            if (existing.InputHashSha256 != bundle.InputHashSha256)
                throw new InvalidOperationException("SimulationFarmRealityEvidenceRevisionConflict");
            return Result(false, bundle);
        }
        db.FarmRealityEvidence.Add(new SimulationFarmRealityEvidenceEntity
        {
            AreaSetStableId = bundle.AreaSetStableId,
            CanonicalProductStableId = bundle.CanonicalProductStableId,
            EvidenceRevision = bundle.EvidenceRevision,
            InputHashSha256 = bundle.InputHashSha256,
            BundleJson = JsonSerializer.Serialize(bundle),
            SyncedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result(true, bundle);
    }

    public async Task<SimulationFarmRealityEvidenceBundle> ReadLatestAsync(
        string areaSetStableId, string canonicalProductStableId,
        CancellationToken cancellationToken)
    {
        var json = await db.FarmRealityEvidence.AsNoTracking()
            .Where(item => item.AreaSetStableId == areaSetStableId
                && item.CanonicalProductStableId == canonicalProductStableId)
            .OrderByDescending(item => item.SyncedAtUtc).ThenByDescending(item => item.Id)
            .Select(item => item.BundleJson).FirstOrDefaultAsync(cancellationToken);
        if (json is null)
            throw new InvalidOperationException("SimulationFarmRealityEvidenceNotFound");
        var bundle = JsonSerializer.Deserialize<SimulationFarmRealityEvidenceBundle>(json)
            ?? throw new InvalidOperationException("SimulationFarmRealityEvidenceInvalid");
        if (!SimulationFarmRealityEvidenceService.HasValidInputHash(bundle))
            throw new InvalidOperationException("SimulationFarmRealityEvidenceHashMismatch");
        return bundle;
    }

    private static SimulationFarmRealityEvidenceSyncResponse Result(
        bool inserted, SimulationFarmRealityEvidenceBundle bundle) => new()
    {
        Inserted = inserted,
        EvidenceRevision = bundle.EvidenceRevision,
        InputHashSha256 = bundle.InputHashSha256,
        SourceCount = bundle.Sources.Length,
    };
}

public sealed class SimulationFarmRealityContextCatalogReader(
    IServiceScopeFactory scopeFactory,
    ISimulationRealityContextCatalogReader fallback)
    : ISimulationRealityContextCatalogReader
{
    public bool TryFreeze(string profileStableId, string areaSetStableId,
        string contextSnapshotStableId, DateTimeOffset frozenAtUtc,
        out SimulationRealityContextSnapshot snapshot, out string errorCode)
    {
        if (profileStableId == SimulationFarmRealityEvidenceCodes.RealityContextProfileStableId
            && areaSetStableId == SimulationFarmRealityEvidenceCodes.FarmAreaSetStableId)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetService<ISimulationFarmRealityEvidenceStore>();
                if (store is null)
                    return fallback.TryFreeze(profileStableId, areaSetStableId,
                        contextSnapshotStableId, frozenAtUtc, out snapshot, out errorCode);
                var bundle = store.ReadLatestAsync(areaSetStableId,
                    SimulationFarmRealityEvidenceCodes.PotatoProductStableId,
                    CancellationToken.None).GetAwaiter().GetResult();
                snapshot = SimulationFarmRealityEvidenceService.ToRealityContext(
                    bundle, contextSnapshotStableId, frozenAtUtc);
                errorCode = string.Empty;
                return true;
            }
            catch (InvalidOperationException error)
                when (error.Message == "SimulationFarmRealityEvidenceNotFound")
            {
                // 아직 동기화하지 않은 개발 환경은 기존 Unavailable 승인 대장을 사용한다.
            }
        }
        return fallback.TryFreeze(profileStableId, areaSetStableId,
            contextSnapshotStableId, frozenAtUtc, out snapshot, out errorCode);
    }
}

internal sealed class SimulationFarmRealityEvidenceConfiguration
    : IEntityTypeConfiguration<SimulationFarmRealityEvidenceEntity>
{
    public void Configure(EntityTypeBuilder<SimulationFarmRealityEvidenceEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_농장현실근거");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new
        {
            item.AreaSetStableId, item.CanonicalProductStableId, item.EvidenceRevision
        }).IsUnique();
        builder.Property(item => item.AreaSetStableId).HasMaxLength(200).IsRequired();
        builder.Property(item => item.CanonicalProductStableId).HasMaxLength(120).IsRequired();
        builder.Property(item => item.EvidenceRevision).HasMaxLength(120).IsRequired();
        builder.Property(item => item.InputHashSha256).HasMaxLength(64).IsRequired();
        builder.Property(item => item.BundleJson).HasColumnType("longtext").IsRequired();
    }
}
