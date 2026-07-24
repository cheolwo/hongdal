using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class UnitedStatesImportedFoodStateCommunityPostSourceTests
{
    [Fact]
    public async Task 현재미국주근거를_월간정보시세글로누적집계한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var run = new OfficialFoodIngredientCompanyResearchRun
        {
            RunKey = "run-us-202607",
            TriggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Scheduled,
            StatusCode = OfficialFoodIngredientCompanyResearchRunStatusCodes.Completed,
            StartedAtUtc = Utc(2026, 7, 24),
            CompletedAtUtc = Utc(2026, 7, 24)
        };
        var onion = Ingredient("ingredient:onion", "양파");
        var garlic = Ingredient("ingredient:garlic", "마늘");
        var clam = Ingredient("ingredient:clam", "조개");
        db.AddRange(run, onion, garlic, clam);
        await db.SaveChangesAsync();
        db.OfficialFoodIngredientCompanyEvidence.AddRange(
            Evidence(
                run.Id,
                onion.Id,
                "california-onion",
                "california-company",
                "US-CA",
                "캘리포니아주",
                Utc(2026, 7, 3),
                Utc(2026, 7, 3),
                1),
            Evidence(
                run.Id,
                garlic.Id,
                "california-garlic",
                "california-company",
                "US-CA",
                "캘리포니아주",
                Utc(2026, 6, 1),
                Utc(2026, 7, 20),
                2),
            Evidence(
                run.Id,
                onion.Id,
                "washington-onion",
                "washington-company",
                "US-WA",
                "워싱턴주",
                Utc(2026, 6, 1),
                Utc(2026, 6, 1),
                1),
            Evidence(
                run.Id,
                clam.Id,
                "dc-clam",
                "dc-company",
                "US-DC",
                "워싱턴 D.C.",
                Utc(2026, 6, 1),
                Utc(2026, 6, 1),
                1),
            Evidence(
                run.Id,
                clam.Id,
                "unclassified-clam",
                "foreign-address-company",
                UnitedStatesImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
                "미국 기타·미분류",
                Utc(2026, 7, 4),
                Utc(2026, 7, 4),
                1));
        await db.SaveChangesAsync();
        var source = new UnitedStatesImportedFoodStateCommunityPostSource(db);

        var draft = await source.BuildAsync(
            new DateOnly(2026, 7, 24),
            TimeZoneInfo.Utc);

        Assert.NotNull(draft);
        Assert.Equal("202607", draft!.PeriodKey);
        Assert.Equal(CommunityBoardCatalog.PeriodicDataMfds.DisplayName, draft.Category);
        Assert.Equal("미국 수입식품 공개근거", draft.WorkflowTag);
        Assert.Equal(
            UnitedStatesImportedFoodStateCommunityPostSource.OfficialSourceUrl,
            draft.SharedLinkUrl);
        Assert.Contains("제품 근거 5행", draft.Body);
        Assert.Contains("미국 50개 주 3행/2개 주", draft.Body);
        Assert.Contains("캘리포니아주: 제품 근거 2행", draft.Body);
        Assert.Contains("이번 달 신규 1행 · 재확인 1행", draft.Body);
        Assert.Contains("워싱턴 D.C.: 제품 근거 1행", draft.Body);
        Assert.Contains("미국 기타·미분류: 제품 근거 1행", draft.Body);
        Assert.Contains("수입량·수입액·거래 가능 업체 수가 아닙니다", draft.Body);
        Assert.Contains("원재료 생산·재배·어획 주", draft.Body);
        Assert.DoesNotContain("foreign-address-company", draft.Body);
    }

    [Fact]
    public async Task 현재미국주근거가없으면_게시초안을만들지않는다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var source = new UnitedStatesImportedFoodStateCommunityPostSource(db);

        var draft = await source.BuildAsync(
            new DateOnly(2026, 7, 24),
            TimeZoneInfo.Utc);

        Assert.Null(draft);
    }

    private static OfficialFoodIngredient Ingredient(string key, string name)
        => new()
        {
            IngredientKey = key,
            LanguageCode = "ko",
            CanonicalName = name,
            NormalizedName = name,
            CategoryCode = OfficialFoodIngredientCategoryCodes.Other,
            ClassificationMethod = "Test",
            ClassificationConfidence = 1m,
            ClassificationState = OfficialFoodIngredientClassificationStates.Confirmed,
            CreatedAtUtc = Utc(2026, 6, 1),
            UpdatedAtUtc = Utc(2026, 7, 24)
        };

    private static OfficialFoodIngredientCompanyEvidence Evidence(
        long runId,
        long ingredientId,
        string candidateKey,
        string organizationKey,
        string regionCode,
        string regionName,
        DateTime firstObservedAtUtc,
        DateTime lastObservedAtUtc,
        int observationCount)
        => new()
        {
            IngredientId = ingredientId,
            LastResearchRunId = runId,
            CandidateKey = candidateKey,
            OrganizationKey = organizationKey,
            OrganizationName = "공개 제조업소",
            NormalizedOrganizationName = "공개제조업소",
            CountryCode = "US",
            CountryName = "미국",
            RelationCode = OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer,
            EvidenceCode =
                OfficialFoodIngredientCompanyEvidenceCodes.ImportedProductIngredientLabel,
            SourceKey = "mfds",
            SourceName = "식품의약품안전처",
            SourceUrl =
                UnitedStatesImportedFoodStateCommunityPostSource.OfficialSourceUrl,
            ResearchQueryTerm = "재료",
            ManufacturerRegionCode = regionCode,
            ManufacturerRegionName = regionName,
            FirstObservedAtUtc = firstObservedAtUtc,
            LastObservedAtUtc = lastObservedAtUtc,
            ObservationCount = observationCount,
            IsCurrent = true,
            RequiresLiveRecheck = true
        };

    private static DateTime Utc(int year, int month, int day)
        => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
}
