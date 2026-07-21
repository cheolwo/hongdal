using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.Content;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodRecipeArchiveServiceTests
{
    [Fact]
    public async Task 같은_원천레시피를_다시수집하면_변형을_중복생성하지않는다()
    {
        await using var fixture = await ArchiveFixture.CreateAsync(
            OfficialFoodRecipeSourceKeys.MfdsCookRecipe);
        var service = fixture.CreateService();
        var request = new OfficialFoodRecipeCollectionRequest(
            OfficialFoodRecipeSourceKeys.MfdsCookRecipe,
            1,
            10);

        var first = await service.CollectAsync(request);
        var second = await service.CollectAsync(request);
        var dishes = await service.SearchDishesAsync(new OfficialFoodRecipeQuery
        {
            CountryCode = "KR"
        });
        var variants = await service.GetVariantsAsync(dishes.Single().DishKey);

        Assert.Equal(1, first.InsertedCount);
        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(1, second.ExistingCount);
        Assert.Single(dishes);
        Assert.Single(variants);
        Assert.False(string.IsNullOrWhiteSpace(variants[0].AttributionText));
        Assert.Contains("이미지 파일은 복제하지", variants[0].ImageReusePolicy);
        Assert.Equal(2, variants[0].StructuredIngredients?.Count);
        Assert.Equal(2, await fixture.Db.OfficialFoodIngredients.CountAsync());
        Assert.Equal(2, await fixture.Db.OfficialFoodRecipeIngredients.CountAsync());
    }

    [Fact]
    public async Task 한_응답의_동명이음식_변형은_대표후보하나를_공유한다()
    {
        await using var fixture = await ArchiveFixture.CreateAsync(
            OfficialFoodRecipeSourceKeys.MfdsCookRecipe,
            includeSiblingVariant: true);
        var service = fixture.CreateService();

        var result = await service.CollectAsync(new OfficialFoodRecipeCollectionRequest(
            OfficialFoodRecipeSourceKeys.MfdsCookRecipe));
        var dishes = await service.SearchDishesAsync(new OfficialFoodRecipeQuery
        {
            CountryCode = "KR"
        });
        var variants = await service.GetVariantsAsync(dishes.Single().DishKey);

        Assert.Equal(2, result.InsertedCount);
        Assert.Single(dishes);
        Assert.Equal(2, variants.Count);
        Assert.Equal(2, dishes[0].VariantCount);
    }

    [Fact]
    public async Task 권리확인전_국가는_메타데이터만_보이고_자동수집은_거부한다()
    {
        await using var fixture = await ArchiveFixture.CreateAsync(null);
        var service = fixture.CreateService();

        var sources = await service.GetSourcesAsync();
        var usSource = Assert.Single(sources, source =>
            source.SourceKey == OfficialFoodRecipeSourceKeys.UsdaMyPlate);

        Assert.Equal(7, sources.Count);
        Assert.Equal(OfficialFoodRecipeAutomationStates.MetadataOnly, usSource.AutomationState);
        Assert.False(usSource.FullTextStorageAllowed);
        Assert.False(usSource.ImageBinaryStorageAllowed);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CollectAsync(new OfficialFoodRecipeCollectionRequest(
                OfficialFoodRecipeSourceKeys.UsdaMyPlate)));
    }

    [Fact]
    public async Task NHS_사본은_7일이지나면_커뮤니티_검토후보에서_제외한다()
    {
        var expiresAt = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc);
        await using var fixture = await ArchiveFixture.CreateAsync(
            OfficialFoodRecipeSourceKeys.NhsHealthierFamilies,
            expiresAt);
        var service = fixture.CreateService();
        await service.CollectAsync(new OfficialFoodRecipeCollectionRequest(
            OfficialFoodRecipeSourceKeys.NhsHealthierFamilies));
        var candidateSource = new CommunityNhsHealthierFamiliesRecipeCandidateSource(
            fixture.Db,
            fixture.TimeProvider);

        var fresh = await candidateSource.ReadAsync(new CommunityInformationCollectionQuery());
        fixture.TimeProvider.SetUtcNow(expiresAt.AddSeconds(1));
        var expired = await candidateSource.ReadAsync(new CommunityInformationCollectionQuery());

        Assert.Single(fresh);
        Assert.Empty(expired);
        Assert.Null(fresh[0].ThumbnailUrl);
        Assert.Equal(CommunityInformationReviewStates.PendingReview, fresh[0].ReviewState);
    }

    private sealed class ArchiveFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly IOfficialFoodRecipeRemoteSource[] _sources;

        private ArchiveFixture(
            SqliteConnection connection,
            AgriculturalFisheriesDbContext db,
            MutableTimeProvider timeProvider,
            IOfficialFoodRecipeRemoteSource[] sources)
        {
            _connection = connection;
            Db = db;
            TimeProvider = timeProvider;
            _sources = sources;
        }

        public AgriculturalFisheriesDbContext Db { get; }

        public MutableTimeProvider TimeProvider { get; }

        public static async Task<ArchiveFixture> CreateAsync(
            string? sourceKey,
            DateTime? contentExpiresAtUtc = null,
            bool includeSiblingVariant = false)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new AgriculturalFisheriesDbContext(options);
            await db.Database.EnsureCreatedAsync();
            var timeProvider = new MutableTimeProvider(
                new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero));
            var sources = sourceKey is null
                ? []
                : new IOfficialFoodRecipeRemoteSource[]
                {
                    new StubRemoteSource(sourceKey, contentExpiresAtUtc, includeSiblingVariant)
                };
            return new ArchiveFixture(connection, db, timeProvider, sources);
        }

        public OfficialFoodRecipeArchiveService CreateService()
        {
            var parser = new OfficialFoodRecipeIngredientParser();
            var ingredientIndexService = new OfficialFoodRecipeIngredientIndexService(
                Db,
                parser,
                TimeProvider);
            return new OfficialFoodRecipeArchiveService(
                Db,
                _sources,
                ingredientIndexService,
                TimeProvider);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class StubRemoteSource : IOfficialFoodRecipeRemoteSource
    {
        private readonly DateTime? _contentExpiresAtUtc;
        private readonly bool _includeSiblingVariant;

        public StubRemoteSource(
            string sourceKey,
            DateTime? contentExpiresAtUtc,
            bool includeSiblingVariant)
        {
            SourceKey = sourceKey;
            _contentExpiresAtUtc = contentExpiresAtUtc;
            _includeSiblingVariant = includeSiblingVariant;
        }

        public string SourceKey { get; }

        public Task<IReadOnlyList<OfficialFoodRecipeCollectedRecord>> FetchAsync(
            int maxPages,
            int maxItems,
            CancellationToken cancellationToken = default)
        {
            var records = new List<OfficialFoodRecipeCollectedRecord>
            {
                CreateRecord("recipe-1", "지역 채소를 넣어 함께 먹는 밥")
            };
            if (_includeSiblingVariant)
            {
                records.Add(CreateRecord("recipe-2", "같은 음식의 다른 공식 조리법"));
            }

            return Task.FromResult<IReadOnlyList<OfficialFoodRecipeCollectedRecord>>(records);
        }

        private OfficialFoodRecipeCollectedRecord CreateRecord(
            string externalId,
            string summary)
            => new(
                    externalId,
                    "이웃 채소밥",
                    "이웃 채소밥",
                    "Neighbour vegetable rice",
                    summary,
                    "서울",
                    "밥",
                    "4인분",
                    ["쌀 2컵", "채소 200g"],
                    ["재료를 손질한다.", "밥을 짓는다."],
                    new Dictionary<string, string> { ["energy_kcal"] = "320" },
                    ["채소", "공동 식사"],
                    "알레르기 재료를 확인한다.",
                    "https://example.test/recipe-1",
                    "https://example.test/recipe-1.jpg",
                    "{}",
                    ContentExpiresAtUtc: _contentExpiresAtUtc);
    }

    public sealed class MutableTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public MutableTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void SetUtcNow(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        }
    }
}
