using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationFarmRealityEvidenceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 21, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 감자승인묶음은_세원천관계와_원단위를보존한다()
    {
        await using var operational = CreateOperationalDb();
        SeedOperational(operational);
        await operational.SaveChangesAsync();
        var reader = new SimulationFarmRealityOperationalReader(
            operational, new FixedTimeProvider(Now));

        var bundle = await reader.ReadApprovedAsync(
            SimulationFarmRealityEvidenceCodes.FarmAreaSetStableId,
            SimulationFarmRealityEvidenceCodes.PotatoProductStableId,
            CancellationToken.None);

        Assert.Equal(4, bundle.Sources.Length);
        Assert.Equal(SimulationFarmRealityEvidenceCodes.Unlinked,
            bundle.Sources.Single(item => item.SourceId == "nongsaro"
                && item.DatasetId == "farm-working-plan-new").RelationStatusCode);
        Assert.Equal("30699", bundle.Sources.Single(item =>
            item.DatasetId == "farm-working-plan-new").ExternalCode);
        Assert.Equal(SimulationFarmRealityEvidenceCodes.Confirmed,
            bundle.Sources.Single(item => item.SourceId == "kamis").RelationStatusCode);
        Assert.Contains("20kg", bundle.Sources.Single(item =>
            item.SourceId == "kamis").UnitCodes);
        Assert.Equal(SimulationFarmRealityEvidenceCodes.Candidate,
            bundle.Sources.Single(item => item.SourceId == "usda-ams-market-news")
                .RelationStatusCode);
        Assert.Contains("USD", bundle.Sources.Single(item =>
            item.SourceId == "usda-ams-market-news").UnitCodes);
        Assert.False(bundle.ChangesSimulationRules);
        Assert.False(bundle.CreatesIncidentOrEffect);
    }

    [Fact]
    public async Task 동기화는_같은입력에멱등이고_파생원장만쓴다()
    {
        await using var operational = CreateOperationalDb();
        SeedOperational(operational);
        await operational.SaveChangesAsync();
        await using var derived = CreateDerivedDb();
        var service = new SimulationFarmRealityEvidenceService(
            new SimulationFarmRealityOperationalReader(
                operational, new FixedTimeProvider(Now)),
            new SimulationFarmRealityEvidenceStore(
                derived, new FixedTimeProvider(Now)));
        var request = new SimulationFarmRealityEvidenceSyncRequest();

        var first = await service.SyncAsync(request, CancellationToken.None);
        var second = await service.SyncAsync(request, CancellationToken.None);

        Assert.True(first.Inserted);
        Assert.False(second.Inserted);
        Assert.Equal(first.InputHashSha256, second.InputHashSha256);
        Assert.Single(derived.FarmRealityEvidence);
        Assert.False(operational.ChangeTracker.HasChanges());
    }

    [Fact]
    public void 승인묶음은_정보와제안만만들고_업무효과를만들지않는다()
    {
        var bundle = CreateBundle();
        bundle.InputHashSha256 = SimulationFarmRealityEvidenceService.ComputeHash(bundle);

        var snapshot = SimulationFarmRealityEvidenceService.ToRealityContext(
            bundle, "reality-context:test:potato", Now);

        Assert.Contains(snapshot.SemanticSignals, item =>
            item.AdvisoryCodes.Contains(SimulationRealityContextCodes.ReviewFieldWorkTiming));
        Assert.Contains(snapshot.SemanticSignals, item =>
            item.AdvisoryCodes.Contains(SimulationRealityContextCodes.InspectCropHealth));
        Assert.Contains(snapshot.SemanticSignals, item =>
            item.AdvisoryCodes.Contains(SimulationRealityContextCodes.ReviewShipmentTiming));
        Assert.False(snapshot.ChangesSimulationRules);
        Assert.False(snapshot.MovesSpatialDefinitions);
        Assert.False(snapshot.CreatesIncidentOrEffect);
    }

    [Fact]
    public void 미래관측과잘못된해시는_동결전에거부한다()
    {
        var bundle = CreateBundle();
        bundle.Sources[0].RetrievedAtUtc = Now.AddHours(1);
        bundle.InputHashSha256 = SimulationFarmRealityEvidenceService.ComputeHash(bundle);

        var error = Assert.Throws<InvalidOperationException>(() =>
            SimulationFarmRealityEvidenceService.ToRealityContext(
                bundle, "reality-context:test:invalid", Now));

        Assert.Equal("SimulationFarmRealityEvidenceInvalid", error.Message);
    }

    [Fact]
    public void 입력hash불일치는_동결전에거부한다()
    {
        var bundle = CreateBundle();
        bundle.InputHashSha256 = Hash('f');

        var error = Assert.Throws<InvalidOperationException>(() =>
            SimulationFarmRealityEvidenceService.ToRealityContext(
                bundle, "reality-context:test:hash-mismatch", Now));

        Assert.Equal("SimulationFarmRealityEvidenceHashMismatch", error.Message);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("duplicate")]
    [InlineData("invalid-unit")]
    public void 누락_중복_잘못된단위는_승인묶음으로받지않는다(string defect)
    {
        var bundle = CreateBundle();
        if (defect == "missing")
            bundle.Sources = bundle.Sources.Take(3).ToArray();
        else if (defect == "duplicate")
            bundle.Sources[1].SourceEvidenceStableId =
                bundle.Sources[0].SourceEvidenceStableId;
        else
            bundle.Sources[0].UnitCodes = new[] { "" };
        bundle.InputHashSha256 = SimulationFarmRealityEvidenceService.ComputeHash(bundle);

        var error = Assert.Throws<InvalidOperationException>(() =>
            SimulationFarmRealityEvidenceService.ToRealityContext(
                bundle, "reality-context:test:invalid-bundle", Now));

        Assert.Equal("SimulationFarmRealityEvidenceInvalid", error.Message);
    }

    [Fact]
    public void 오래된관측은_제안을만들지않고_Stale로남긴다()
    {
        var bundle = CreateBundle();
        foreach (var source in bundle.Sources)
        {
            source.ObservedAtUtc = Now.AddDays(-10);
            source.RetrievedAtUtc = Now.AddDays(-10);
        }
        bundle.InputHashSha256 = SimulationFarmRealityEvidenceService.ComputeHash(bundle);

        var snapshot = SimulationFarmRealityEvidenceService.ToRealityContext(
            bundle, "reality-context:test:stale-potato", Now);

        Assert.Equal(SimulationRealityContextCodes.Unavailable,
            snapshot.AvailabilityCode);
        Assert.Empty(snapshot.SemanticSignals);
        Assert.All(snapshot.SourceEvidence, item => Assert.Equal(
            SimulationRealityContextCodes.Stale, item.FreshnessCode));
    }

    private static AgriculturalFisheriesDbContext CreateOperationalDb() => new(
        new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase("farm-reality-operational-" + Guid.NewGuid()).Options);

    private static SimulationWorld파생DbContext CreateDerivedDb() => new(
        new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase("farm-reality-derived-" + Guid.NewGuid()).Options);

    private static void SeedOperational(AgriculturalFisheriesDbContext db)
    {
        var identity = new 공통식품품목Identity
        {
            CanonicalProductStableId = "product:potato",
            DisplayName = "감자",
            Revision = "common-food-product-identity.v1",
        };
        identity.CodeRelations.Add(Relation("kamis", "KAMIS_ITEM", "152", "Confirmed"));
        identity.CodeRelations.Add(Relation("usda-ams-market-news",
            "USDA_AMS_COMMODITY", "Potatoes", "Candidate"));
        identity.CodeRelations.Add(Relation("nongsaro:farm-working-plan-new",
            "NONGSARO_KIND_OF_COMMODITY", null, "Unlinked"));
        db.CommonFoodProductIdentities.Add(identity);
        db.NongsaroPotatoProfiles.Add(new Nongsaro감자ProfileArchive
        {
            StableId = "crop-requirement-profile:nongsaro.potato.1",
            Revision = 1,
            CanonicalProductStableId = "product:potato",
            WorkScheduleGroupCode = "210005",
            WorkScheduleContentNo = "30699",
            ProductRelationStatusCode = "Unlinked",
            ReviewStatusCode = "PendingHumanReview",
            ApprovedForSimulationContext = true,
            ProfileJson = "{}",
            SourceSetHashSha256 = Hash('a'),
            DisasterPreventionHashSha256 = Hash('b'),
            RetrievedAtUtc = Now.UtcDateTime.AddHours(-3),
            DisasterPreventionRetrievedAtUtc = Now.UtcDateTime.AddHours(-2),
            ArchivedAtUtc = Now.UtcDateTime.AddHours(-1),
            ApprovedAtUtc = Now.UtcDateTime,
        });
        db.KamisPriceObservations.Add(new KamisPriceObservation
        {
            RecordKey = "kamis-potato",
            CategoryCode = "100",
            ItemCode = "152",
            ItemName = "감자",
            Unit = "20kg",
            SurveyDate = new DateOnly(2026, 8, 20),
            RawJson = "{\"item_code\":\"152\"}",
            SourceUrl = "https://www.kamis.or.kr",
            LastSeenAtUtc = Now.UtcDateTime,
        });
        db.UsdaAmsMarketPriceObservations.Add(new UsdaAms시장가격관측
        {
            RecordKey = "ams-potato",
            SourceKey = "usda-ams-market-news",
            Commodity = "Potatoes",
            OriginalUnit = "50 lb sack",
            CurrencyCode = "USD",
            ReportEndDate = new DateOnly(2026, 8, 20),
            RawJson = "{\"commodity\":\"Potatoes\"}",
            LastSeenAtUtc = Now.UtcDateTime,
        });
    }

    private static 공통식품품목Code관계 Relation(
        string sourceKey, string scheme, string? code, string status) => new()
    {
        RelationStableId = "relation:test:" + sourceKey,
        SourceKey = sourceKey,
        CodeScheme = scheme,
        ExternalCode = code,
        Label = code ?? "미연결",
        RelationStatusCode = status,
        MatchQualityCode = status,
        EvidenceNote = "test",
    };

    private static SimulationFarmRealityEvidenceBundle CreateBundle()
    {
        var sources = new[]
        {
            Source("nongsaro-work", SimulationRealityContextCodes.ReviewFieldWorkTiming),
            Source("nongsaro-disaster", SimulationRealityContextCodes.InspectCropHealth),
            Source("kamis", SimulationRealityContextCodes.ReviewShipmentTiming),
            Source("ams", SimulationRealityContextCodes.ReviewShipmentTiming),
        };
        return new SimulationFarmRealityEvidenceBundle
        {
            AreaSetStableId = SimulationFarmRealityEvidenceCodes.FarmAreaSetStableId,
            CanonicalProductStableId = SimulationFarmRealityEvidenceCodes.PotatoProductStableId,
            ProductDisplayName = "감자",
            ProductIdentityRevision = "v1",
            CreatedAtUtc = Now,
            Sources = sources,
        };
    }

    private static SimulationFarmRealitySourceEvidence Source(string id, string advisory) => new()
    {
        SourceEvidenceStableId = id,
        SourceId = id,
        DatasetId = id,
        SourceName = id,
        RelationStatusCode = "Confirmed",
        AvailabilityCode = SimulationRealityContextCodes.Available,
        QualityCode = SimulationRealityContextCodes.Valid,
        ObservedAtUtc = Now.AddHours(-2),
        RetrievedAtUtc = Now.AddHours(-1),
        SpatialPrecisionCode = "Reference",
        UnitCodes = new[] { "content" },
        MaxAgeHours = 48,
        SourceHashSha256 = Hash('c'),
        SourceHref = "https://example.test",
        AdvisoryCodes = new[] { advisory },
    };

    private static string Hash(char value) => new(value, 64);

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
