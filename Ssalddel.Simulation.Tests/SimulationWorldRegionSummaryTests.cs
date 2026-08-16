using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldRegionSummaryTests
{
    [Fact]
    public void 같은원본과Profile은_입력순서가달라도같은L1요약을만든다()
    {
        var candidates = Candidates("region:test:rural");
        var profile = SimulationWorld지역표현요약Profile.CreateDefault();
        var generatedAt = DateTimeOffset.Parse("2026-08-15T00:00:00Z");

        var first = SimulationWorld지역표현요약Engine.Generate(
            profile, "region:test:rural", null, "L1", candidates, Hash('a'), generatedAt);
        var second = SimulationWorld지역표현요약Engine.Generate(
            profile, "region:test:rural", null, "L1", candidates.Reverse(), Hash('a'), generatedAt);

        Assert.Equal(first.SummaryHashSha256, second.SummaryHashSha256);
        Assert.Equal(32, first.AllocatedVisualSlotCount);
        Assert.Equal(19, first.Items.Where(item => item.SelectionReasonCode == "DistributionQuota")
            .Sum(item => item.VisualSlotCount));
        Assert.Equal(8, first.Items.Where(item => item.SelectionReasonCode == "RegionalSignature")
            .Sum(item => item.VisualSlotCount));
        Assert.Equal(5, first.Items.Where(item => item.SelectionReasonCode == "GameplayContext")
            .Sum(item => item.VisualSlotCount));
        Assert.All(first.CategoryReports, report =>
            Assert.InRange(report.AllocatedVisualSlotCount, 0, 12));
    }

    [Fact]
    public void 같은Engine은_농촌과도시지역식별자를입력으로받고_지역별해시를분리한다()
    {
        var profile = SimulationWorld지역표현요약Profile.CreateDefault();
        var generatedAt = DateTimeOffset.Parse("2026-08-15T00:00:00Z");
        var rural = SimulationWorld지역표현요약Engine.Generate(
            profile, "region:test:rural", null, "L0", Candidates("region:test:rural"), Hash('b'), generatedAt);
        var urban = SimulationWorld지역표현요약Engine.Generate(
            profile, "region:test:urban", null, "L0", Candidates("region:test:urban"), Hash('b'), generatedAt);

        Assert.NotEqual(rural.SummaryHashSha256, urban.SummaryHashSha256);
        Assert.Equal(rural.AllocatedVisualSlotCount, urban.AllocatedVisualSlotCount);
        Assert.DoesNotContain("51760", urban.SummaryHashSha256, StringComparison.Ordinal);
    }

    [Fact]
    public void 대표건축물Selector는_평창코드없이도_다른지역의종류별대표를선정한다()
    {
        var buildings = Enumerable.Range(0, 6).Select(index => new 건축물대장표제부Record
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{index + 1:000000000000}"),
            BuildingName = index % 2 == 0 ? "자료가 충실한 건물 " + index : null,
            BuildingAreaSquareMeters = index % 2 == 0 ? 100m + index : null,
        }).ToArray();
        var categories = buildings.ToDictionary(
            item => item.Id,
            item => "urban-category-" + Array.IndexOf(buildings, item) % 2);

        var selected = 지역대표건축물Selector.SelectOnePerCategory(
            "region:kr:sigungu:11110", buildings, categories);

        Assert.Equal(2, selected.Count);
        Assert.Equal(6, selected.Sum(item => item.RepresentedRecordCount));
        Assert.All(selected, item => Assert.Equal(1, item.Rank));
    }

    [Fact]
    public async Task 파생원장저장은_한국어열의요약원장을함께저장하고_상호명은상세조회에서만반환한다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        var store = new SimulationWorld파생원장Store(db);

        await store.저장Async(Ledger(), CancellationToken.None);
        db.ChangeTracker.Clear();

        Assert.Equal(1, await db.RegionSummaryProfiles.CountAsync());
        Assert.Equal(6, await db.RegionSummaryRuns.CountAsync());
        Assert.NotEmpty(await db.RegionSummaryItems.ToArrayAsync());
        Assert.NotEmpty(await db.RegionSummaryCategoryReports.ToArrayAsync());
        var reader = new SimulationWorld지역표현요약Reader(db);
        var service = new SimulationWorld지역표현요약Service(reader);
        var summary = await service.지역요약조회Async(
            "region:kr:bjd:5176036000", "L1", CancellationToken.None);
        var summaryJson = JsonSerializer.Serialize(summary);

        Assert.DoesNotContain("진부 공개상호", summaryJson, StringComparison.Ordinal);
        Assert.Contains(summary.Items, item => item.HasPublicDetail);
        Assert.Equal(
            summary.TotalRepresentedRecordCount,
            summary.CategoryReports.Sum(item => item.TotalRepresentedRecordCount));
        Assert.Equal(
            summary.TotalRepresentedRecordCount,
            summary.SelectedRepresentedRecordCount + summary.OmittedRepresentedRecordCount);

        var detail = await service.공개객체상세조회Async(
            "business:public:001", CancellationToken.None);
        Assert.Equal("진부 공개상호", detail.PublicDisplayName);
        Assert.DoesNotContain("전화", JsonSerializer.Serialize(detail), StringComparison.Ordinal);
        Assert.Contains("대표자", detail.DisclosureNotice, StringComparison.Ordinal);
        var unlinked = await Assert.ThrowsAsync<SimulationNotFoundException>(() =>
            service.공개객체상세조회Async("business:public:unlinked", CancellationToken.None));
        Assert.Equal("SimulationWorldPublicObjectDetailNotFound", unlinked.ErrorCode);

        var profileEntity = db.Model.FindEntityType(typeof(SimulationWorld지역표현요약ProfileEntity));
        var runEntity = db.Model.FindEntityType(typeof(SimulationWorld지역표현요약RunEntity));
        Assert.Equal("시뮬레이션월드_지역표현요약프로필", profileEntity!.GetTableName());
        Assert.Equal("시뮬레이션월드_지역표현요약실행", runEntity!.GetTableName());
        Assert.Contains(profileEntity!.GetProperties(), property => property.GetColumnName() == "요약프로필개정번호");
        Assert.Contains(runEntity!.GetProperties(), property => property.GetColumnName() == "화면생략대표원본수");
    }

    private static IEnumerable<SimulationWorld지역표현요약Candidate> Candidates(string regionStableId)
    {
        for (var index = 0; index < 40; index++)
        {
            yield return Candidate(
                regionStableId,
                "distribution:" + index,
                "building-" + index % 5,
                1,
                null,
                null,
                0);
        }
        for (var index = 0; index < 12; index++)
        {
            yield return Candidate(
                regionStableId,
                "signature:" + index,
                "signature-" + index % 4,
                2,
                0.20m,
                0.05m,
                0);
        }
        for (var index = 0; index < 10; index++)
        {
            yield return Candidate(
                regionStableId,
                "gameplay:" + index,
                "gameplay-" + index % 5,
                1,
                null,
                null,
                100 - index);
        }
    }

    private static SimulationWorld지역표현요약Candidate Candidate(
        string regionStableId,
        string stableId,
        string categoryCode,
        int representedCount,
        decimal? regionalShare,
        decimal? baselineShare,
        int gameplayPriority) => new()
    {
        StableId = stableId,
        RegionStableId = regionStableId,
        CategoryCode = categoryCode,
        ObjectTypeCode = "Building",
        EvidenceKindCode = gameplayPriority > 0
            ? SimulationWorld근거종류Codes.시나리오
            : SimulationWorld근거종류Codes.관측,
        VisualKey = "summary.building." + categoryCode,
        RepresentedRecordCount = representedCount,
        RegionalShare = regionalShare,
        BaselineShare = baselineShare,
        GameplayPriority = gameplayPriority,
        QualityScore = representedCount,
        SpatialBucketCode = stableId,
    };

    private static SimulationWorld파생원장 Ledger() => new()
    {
        SchemaVersion = 2,
        BuildStableId = "world-build:test:region-summary",
        AreaSetStableId = "area-set:test:pyeongchang",
        RecipeRevision = "world-recipe.test.r1",
        RuleRevision = "world-rule.test.r1",
        Seed = 51760,
        InputFingerprintSha256 = Hash('c'),
        GeneratedAtUtc = DateTimeOffset.Parse("2026-08-15T00:00:00Z"),
        Sources = new[]
        {
            new SimulationWorld원본계보
            {
                SourceStableId = "source:test:public-business",
                SourceDatabaseCode = "SharedPublicData",
                DatasetCode = "public-licensed-business",
                SourceRevision = "public-business.test.r1",
                SourceHashSha256 = Hash('d'),
                ReferenceTimeUtc = DateTimeOffset.Parse("2026-08-14T00:00:00Z"),
            },
        },
        Nodes = new[]
        {
            new SimulationWorld파생Node
            {
                StableId = "area:test:jinbu",
                NodeKindCode = "Area",
                EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
                RegionCode = "5176036000",
            },
            new SimulationWorld파생Node
            {
                StableId = "building:public:001",
                NodeKindCode = "Building",
                SourceStableId = "source:test:public-business",
                SourceRecordStableId = "building-record:001",
                EvidenceKindCode = SimulationWorld근거종류Codes.관측,
                RegionCode = "5176036000",
                AreaStableId = "area:test:jinbu",
                RepresentativeGroupCode = "building-category:warehouse",
                RepresentedRecordCount = 12,
                RepresentativeRank = 1,
            },
            new SimulationWorld파생Node
            {
                StableId = "business:public:001",
                NodeKindCode = "PublicLicensedBusiness",
                SourceStableId = "source:test:public-business",
                SourceRecordStableId = "business-record:001",
                EvidenceKindCode = SimulationWorld근거종류Codes.관측,
                DisplayName = "진부 공개상호",
            },
            new SimulationWorld파생Node
            {
                StableId = "business:public:unlinked",
                NodeKindCode = "PublicLicensedBusiness",
                SourceStableId = "source:test:public-business",
                SourceRecordStableId = "business-record:unlinked",
                EvidenceKindCode = SimulationWorld근거종류Codes.관측,
                DisplayName = "연결되지 않은 공개상호",
            },
        },
        Relations = new[]
        {
            new SimulationWorld파생Relation
            {
                StableId = "relation:area-building",
                FromNodeStableId = "area:test:jinbu",
                RelationCode = "Contains",
                ToNodeStableId = "building:public:001",
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                SourceStableId = "source:test:public-business",
                Confidence = 1m,
            },
            new SimulationWorld파생Relation
            {
                StableId = "relation:building-business",
                FromNodeStableId = "building:public:001",
                RelationCode = "HostsPublicLicensedBusiness",
                ToNodeStableId = "business:public:001",
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                SourceStableId = "source:test:public-business",
                Confidence = 1m,
            },
        },
    };

    private static SimulationWorld파생DbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase("region-summary-" + Guid.NewGuid())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new SimulationWorld파생DbContext(options);
    }

    private static string Hash(char value) => new(value, 64);
}
