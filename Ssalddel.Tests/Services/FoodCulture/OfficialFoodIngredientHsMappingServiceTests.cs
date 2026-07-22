using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Domain.HsCodes;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.FoodCulture;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodIngredientHsMappingServiceTests
{
    [Fact]
    public async Task 재료를_국가별공식카탈로그후보와연결하되_신고값으로확정하지않는다()
    {
        await using var archiveDb = CreateArchiveContext();
        await using var customsDb = CreateCustomsContext();
        var rice = Ingredient("쌀", OfficialFoodIngredientCategoryCodes.GrainAndStarch);
        var unknown = Ingredient("홍달실험재료", OfficialFoodIngredientCategoryCodes.Other);
        archiveDb.OfficialFoodIngredients.AddRange(rice, unknown);
        AddCatalog(customsDb, "KR", "HSK", "2026", 10, "1006300000", "쌀 - 정미");
        AddCatalog(customsDb, "US", "HTSUS", "2026-r11", 10, "1006301020", "Rice, milled");
        AddCatalog(
            customsDb,
            "KR",
            "HSK",
            "2026",
            10,
            "8703211000",
            "쌀 운반용 승용차",
            HsCodeBusinessCategory.GeneralCargo);
        await archiveDb.SaveChangesAsync();
        await customsDb.SaveChangesAsync();
        var service = new OfficialFoodIngredientHsMappingService(
            archiveDb,
            customsDb,
            new TestTimeProvider(new DateTimeOffset(2026, 7, 22, 4, 0, 0, TimeSpan.Zero)));

        var result = await service.RebuildAsync(
            new OfficialFoodIngredientHsIndexRequest(100, Force: true));
        var response = await service.GetOrCreateAsync(new OfficialFoodIngredientHsQuery
        {
            IngredientKey = rice.IngredientKey,
            IngredientName = rice.CanonicalName
        });

        Assert.Equal(2, result.ProcessedIngredientCount);
        Assert.Equal(1, result.MappedIngredientCount);
        Assert.Equal(2, result.CandidateCount);
        Assert.Equal(2, response.Candidates.Count);
        Assert.Contains(response.Candidates, candidate =>
            candidate.CountryCode == "KR"
            && candidate.JurisdictionUseCode
            == OfficialFoodIngredientHsJurisdictionUseCodes.KoreaExportDeclaration);
        Assert.Contains(response.Candidates, candidate =>
            candidate.CountryCode == "US"
            && candidate.JurisdictionUseCode
            == OfficialFoodIngredientHsJurisdictionUseCodes.UnitedStatesImportEntry);
        Assert.All(response.Candidates, candidate =>
        {
            Assert.Equal(OfficialFoodIngredientHsMappingStates.Candidate, candidate.MappingState);
            Assert.True(candidate.RequiresProfessionalReview);
            Assert.False(candidate.IsDeclarationReady);
            Assert.NotEmpty(candidate.RequiredProductDetails);
            Assert.DoesNotContain("8703211000", candidate.NormalizedHsCode, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task 거절된후보는_강제재생성에도자동으로되살리지않는다()
    {
        await using var archiveDb = CreateArchiveContext();
        await using var customsDb = CreateCustomsContext();
        var garlic = Ingredient("마늘", OfficialFoodIngredientCategoryCodes.Vegetable);
        archiveDb.OfficialFoodIngredients.Add(garlic);
        AddCatalog(customsDb, "KR", "HSK", "2026", 10, "0703201000", "마늘 - 신선한 것");
        await archiveDb.SaveChangesAsync();
        await customsDb.SaveChangesAsync();
        var service = new OfficialFoodIngredientHsMappingService(
            archiveDb,
            customsDb,
            TimeProvider.System);
        await service.RebuildAsync(new OfficialFoodIngredientHsIndexRequest(10, Force: true));
        var mapping = await archiveDb.OfficialFoodIngredientHsMappings.SingleAsync();
        mapping.MappingState = OfficialFoodIngredientHsMappingStates.Rejected;
        mapping.IsActive = false;
        await archiveDb.SaveChangesAsync();

        await service.RebuildAsync(new OfficialFoodIngredientHsIndexRequest(10, Force: true));

        mapping = await archiveDb.OfficialFoodIngredientHsMappings.SingleAsync();
        Assert.Equal(OfficialFoodIngredientHsMappingStates.Rejected, mapping.MappingState);
        Assert.False(mapping.IsActive);
    }

    private static AgriculturalFisheriesDbContext CreateArchiveContext()
        => new(new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);

    private static SsalddelContext CreateCustomsContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options,
            new DummyPersonalDataEncryptionService());

    private static OfficialFoodIngredient Ingredient(string name, string categoryCode)
        => new()
        {
            IngredientKey = $"ingredient:{name}",
            LanguageCode = "ko",
            CanonicalName = name,
            NormalizedName = OfficialFoodRecipeIngredientParser.NormalizeName(name),
            CategoryCode = categoryCode,
            ClassificationMethod = "test",
            ClassificationConfidence = 0.95m,
            ClassificationState = OfficialFoodIngredientClassificationStates.AutoClassified
        };

    private static void AddCatalog(
        SsalddelContext db,
        string countryCode,
        string standardCode,
        string revision,
        int codeDigits,
        string code,
        string name,
        HsCodeBusinessCategory businessCategory = HsCodeBusinessCategory.Food)
    {
        var version = db.HsCodeCatalogVersions.Local.FirstOrDefault(item =>
            item.CountryCode == countryCode
            && item.StandardCode == standardCode
            && item.Revision == revision)
            ?? new HsCodeCatalogVersion
            {
                CountryCode = countryCode,
                StandardCode = standardCode,
                Revision = revision,
                CodeDigits = codeDigits,
                SourceName = countryCode == "US" ? "USITC HTS" : "관세청 관세율표",
                SourceUrl = countryCode == "US"
                    ? "https://hts.usitc.gov/"
                    : "https://unipass.customs.go.kr/clip/index.do",
                EffectiveFrom = new DateTime(2026, 1, 1),
                ImportedAtUtc = new DateTime(2026, 7, 1),
                IsActive = true
            };
        if (version.Id == 0 && db.Entry(version).State == EntityState.Detached)
        {
            db.HsCodeCatalogVersions.Add(version);
        }

        db.HsCodeEntries.Add(new HsCodeEntry
        {
            CatalogVersion = version,
            Code = code,
            NormalizedCode = code,
            Level = HsCodeLevel.National,
            KoreanName = name,
            EnglishName = name,
            Description = name,
            SearchKeywords = name,
            BusinessCategory = businessCategory,
            IsActive = true
        });
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
