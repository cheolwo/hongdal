using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodIngredientCompanyArchiveServiceTests
{
    [Fact]
    public async Task 전체재료를_조사하고_업체별제품근거와원천이력을_전산화한다()
    {
        await using var fixture = await ArchiveFixture.CreateAsync(query =>
            ResearchResponse(
                query,
                query.IngredientName == "참깨"
                    ? [
                        Candidate("domestic-1", "국내식품", "참깨 소스"),
                        Candidate("domestic-2", "국내식품", "참깨 페이스트")
                    ]
                    : [Candidate("domestic-rice", "쌀식품", "쌀가루")],
                OfficialFoodIngredientCompanyResearchStatusCodes.Available));
        await fixture.AddIngredientAsync("ingredient:sesame", "참깨");
        await fixture.AddIngredientAsync("ingredient:rice", "쌀");

        var result = await fixture.Service.CollectCatalogAsync(
            new OfficialFoodIngredientCompanyCollectionRequest(
                MaxIngredients: 20,
                CandidatesPerIngredient: 100,
                Force: true,
                RefreshAfterDays: 30,
                RequestDelayMilliseconds: 0));
        var archive = await fixture.Service.GetArchiveAsync("ingredient:sesame", null);
        var coverage = await fixture.Service.GetCoverageAsync();

        Assert.Equal(2, result.ProcessedIngredientCount);
        Assert.Equal(3, result.ObservedEvidenceCount);
        Assert.NotNull(archive);
        var organization = Assert.Single(archive!.Organizations);
        Assert.Equal("국내식품", organization.OrganizationName);
        Assert.Equal(2, organization.EvidenceCount);
        Assert.All(organization.Evidence, evidence =>
        {
            Assert.False(evidence.CanAutoSelect);
            Assert.False(evidence.CanAutoContact);
            Assert.True(evidence.RequiresLiveRecheck);
            Assert.Equal("참깨, 소금", evidence.RawIngredientText);
        });
        Assert.Equal(2, coverage.ResearchedIngredientCount);
        Assert.Equal(3, coverage.CurrentEvidenceCount);
        Assert.Equal(2, coverage.CurrentOrganizationCount);
        Assert.Equal(2, await fixture.Db.OfficialFoodIngredientCompanyProfiles.CountAsync());
        Assert.Equal(6, await fixture.Db.OfficialFoodIngredientCompanySourceObservations.CountAsync());
    }

    [Fact]
    public async Task 원천조회실패는_기존현재근거를삭제하지않는다()
    {
        var callCount = 0;
        await using var fixture = await ArchiveFixture.CreateAsync(query =>
        {
            callCount++;
            return callCount == 1
                ? ResearchResponse(
                    query,
                    [Candidate("domestic-1", "국내식품", "양파 소스")],
                    OfficialFoodIngredientCompanyResearchStatusCodes.Available)
                : new OfficialFoodIngredientCompanyResearchResponse(
                    OfficialFoodIngredientCompanyResearchStatusCodes.Partial,
                    query.IngredientKey,
                    query.IngredientName,
                    new DateTimeOffset(2026, 7, 23, 3, 0, 0, TimeSpan.Zero),
                    [
                        Source(
                            MfdsIngredientProductCompanySource.SourceKey,
                            OfficialFoodIngredientCompanySourceStatusCodes.Failed),
                        Source(
                            MfdsImportedFoodIngredientCompanySource.LabelSourceKey,
                            OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured),
                        Source(
                            MfdsImportedFoodIngredientCompanySource.ForeignFacilitySourceKey,
                            OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured,
                            providesDirectEvidence: false)
                    ],
                    [],
                    []);
        });
        await fixture.AddIngredientAsync("ingredient:onion", "양파");

        await fixture.Service.ResearchAndArchiveAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = "ingredient:onion",
            IngredientName = "양파"
        });
        fixture.TimeProvider.Advance(TimeSpan.FromDays(1));
        await fixture.Service.ResearchAndArchiveAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = "ingredient:onion",
            IngredientName = "양파"
        });
        var archive = await fixture.Service.GetArchiveAsync("ingredient:onion", null);

        Assert.NotNull(archive);
        Assert.Equal(OfficialFoodIngredientCompanyResearchStatusCodes.Partial, archive!.StatusCode);
        Assert.Single(archive.Organizations);
        Assert.Single(archive.Organizations[0].Evidence);
        Assert.True(archive.Organizations[0].Evidence[0].IsCurrent);
    }

    [Fact]
    public async Task 성공한원천의_재조사에서사라진근거는_비활성화한다()
    {
        var callCount = 0;
        await using var fixture = await ArchiveFixture.CreateAsync(query =>
        {
            callCount++;
            return ResearchResponse(
                query,
                callCount == 1
                    ? [Candidate("domestic-1", "국내식품", "마늘 소스")]
                    : [],
                callCount == 1
                    ? OfficialFoodIngredientCompanyResearchStatusCodes.Available
                    : OfficialFoodIngredientCompanyResearchStatusCodes.NoResults);
        });
        await fixture.AddIngredientAsync("ingredient:garlic", "마늘");

        await fixture.Service.ResearchAndArchiveAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = "ingredient:garlic",
            IngredientName = "마늘"
        });
        fixture.TimeProvider.Advance(TimeSpan.FromDays(1));
        await fixture.Service.ResearchAndArchiveAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = "ingredient:garlic",
            IngredientName = "마늘"
        });

        var current = await fixture.Service.GetArchiveAsync("ingredient:garlic", null);
        var history = await fixture.Service.GetArchiveAsync(
            "ingredient:garlic",
            null,
            includeInactive: true);
        Assert.NotNull(current);
        Assert.Empty(current!.Organizations);
        Assert.NotNull(history);
        Assert.Single(history!.Organizations);
        Assert.False(history.Organizations[0].Evidence[0].IsCurrent);
    }

    private static OfficialFoodIngredientCompanyResearchResponse ResearchResponse(
        OfficialFoodIngredientCompanyQuery query,
        IReadOnlyList<OfficialFoodIngredientCompanyCandidateDto> candidates,
        string statusCode)
        => new(
            statusCode,
            query.IngredientKey,
            query.IngredientName,
            new DateTimeOffset(2026, 7, 22, 3, 0, 0, TimeSpan.Zero),
            [
                Source(
                    MfdsIngredientProductCompanySource.SourceKey,
                    OfficialFoodIngredientCompanySourceStatusCodes.Available),
                Source(
                    MfdsImportedFoodIngredientCompanySource.LabelSourceKey,
                    OfficialFoodIngredientCompanySourceStatusCodes.Available),
                Source(
                    MfdsImportedFoodIngredientCompanySource.ForeignFacilitySourceKey,
                    OfficialFoodIngredientCompanySourceStatusCodes.SupportingSource,
                    providesDirectEvidence: false)
            ],
            candidates,
            []);

    private static OfficialFoodIngredientCompanySourceDto Source(
        string sourceKey,
        string statusCode,
        bool providesDirectEvidence = true)
        => new(
            sourceKey,
            "공식기관",
            sourceKey,
            "공식 범위",
            "https://example.test/source",
            statusCode,
            "테스트 원천 상태",
            providesDirectEvidence,
            false,
            true);

    private static OfficialFoodIngredientCompanyCandidateDto Candidate(
        string candidateKey,
        string organizationName,
        string productName)
        => new(
            candidateKey,
            organizationName,
            "KR",
            "대한민국",
            OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer,
            OfficialFoodIngredientCompanyEvidenceCodes.DomesticProductIngredientReport,
            "공식 품목제조보고 원재료 근거",
            productName,
            "소스",
            "license-1",
            OfficialFoodIngredientCompanyVerificationStatusCodes.OfficialProductReport,
            false,
            string.Empty,
            MfdsIngredientProductCompanySource.SourceKey,
            "식품 품목제조보고",
            "https://example.test/source",
            new DateTimeOffset(2026, 7, 22, 3, 0, 0, TimeSpan.Zero),
            true,
            false,
            false)
        {
            RawIngredientText = "참깨, 소금",
            EvidenceDate = "20260701",
            EvidenceLastChangedDate = "20260720",
            EvidenceSequence = "1",
            EvidenceRecordIdentifier = $"report-{candidateKey}"
        };

    private sealed class ArchiveFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private ArchiveFixture(
            SqliteConnection connection,
            AgriculturalFisheriesDbContext db,
            MutableTimeProvider timeProvider,
            OfficialFoodIngredientCompanyArchiveService service)
        {
            _connection = connection;
            Db = db;
            TimeProvider = timeProvider;
            Service = service;
        }

        public AgriculturalFisheriesDbContext Db { get; }

        public MutableTimeProvider TimeProvider { get; }

        public OfficialFoodIngredientCompanyArchiveService Service { get; }

        public static async Task<ArchiveFixture> CreateAsync(
            Func<OfficialFoodIngredientCompanyQuery,
                OfficialFoodIngredientCompanyResearchResponse> responseFactory)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new AgriculturalFisheriesDbContext(options);
            await db.Database.EnsureCreatedAsync();
            var timeProvider = new MutableTimeProvider(
                new DateTimeOffset(2026, 7, 22, 3, 0, 0, TimeSpan.Zero));
            var service = new OfficialFoodIngredientCompanyArchiveService(
                db,
                new StubResearchService(responseFactory),
                timeProvider,
                NullLogger<OfficialFoodIngredientCompanyArchiveService>.Instance);
            return new ArchiveFixture(connection, db, timeProvider, service);
        }

        public async Task AddIngredientAsync(string ingredientKey, string canonicalName)
        {
            Db.OfficialFoodIngredients.Add(new OfficialFoodIngredient
            {
                IngredientKey = ingredientKey,
                LanguageCode = "ko",
                CanonicalName = canonicalName,
                NormalizedName = OfficialFoodRecipeIngredientParser.NormalizeName(canonicalName),
                CategoryCode = OfficialFoodIngredientCategoryCodes.Other,
                ClassificationMethod = "Test",
                ClassificationConfidence = 1m,
                ClassificationState = OfficialFoodIngredientClassificationStates.Confirmed,
                CreatedAtUtc = TimeProvider.GetUtcNow().UtcDateTime,
                UpdatedAtUtc = TimeProvider.GetUtcNow().UtcDateTime
            });
            await Db.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class StubResearchService(
        Func<OfficialFoodIngredientCompanyQuery,
            OfficialFoodIngredientCompanyResearchResponse> responseFactory)
        : IOfficialFoodIngredientCompanyResearchService
    {
        public Task<OfficialFoodIngredientCompanyResearchResponse> ResearchAsync(
            OfficialFoodIngredientCompanyQuery query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(responseFactory(query));
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
