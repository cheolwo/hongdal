using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityCultureTransportPostSourceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 23, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 승인된대표음식만_구매유도가아닌문화교류질문으로작성한다()
    {
        await using var db = CreateContext();
        AddVariant(
            db,
            "pending-dish",
            "미승인 음식",
            OfficialFoodRecipeReviewStates.PendingReview,
            OfficialFoodRecipeRepresentationStates.Candidate);
        AddVariant(
            db,
            "approved-dish",
            "비빔밥",
            OfficialFoodRecipeReviewStates.Approved,
            OfficialFoodRecipeRepresentationStates.Representative,
            originalName: "비빔밥",
            englishName: "Bibimbap",
            regionName: "전주");
        await db.SaveChangesAsync();
        var source = new CommunityCultureTransportPostSource(
            db,
            new FixedTimeProvider(Now));

        var draft = await source.BuildAsync(
            new DateOnly(2026, 7, 23),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));

        Assert.NotNull(draft);
        Assert.Equal(CommunityAutomatedPostSourceKeys.CultureTransport, draft.SourceKey);
        Assert.Equal(CommunityBoardCatalog.Food.DisplayName, draft.Category);
        Assert.Equal(CultureTransportContentCatalog.FoodCultureWorkflowTag, draft.WorkflowTag);
        Assert.StartsWith("[문화교통]", draft.Title, StringComparison.Ordinal);
        Assert.Contains("비빔밥", draft.Title);
        Assert.Contains("현지에서 언제, 누구와 함께", draft.Body);
        Assert.Contains("지역이나 가정에 따라", draft.Body);
        Assert.Contains("번역할 때 빠지기 쉬운", draft.Body);
        Assert.Contains("산지·포장·보관·수령 조건", draft.Body);
        Assert.Contains("0.0~1.5", draft.Body);
        Assert.Contains("자료 확인 시각", draft.Body);
        Assert.Contains("갱신 주기: 일 1회 이하", draft.Body);
        Assert.Contains("구매 권유", draft.Body);
        Assert.Contains("국가나 문화 전체의 답으로 일반화하지 않고", draft.Body);
        Assert.DoesNotContain("미승인 음식", draft.Body);
        Assert.DoesNotContain("같이 구매", draft.Body);
        Assert.Equal("https://example.test/approved-dish", draft.SharedLinkUrl);
    }

    [Fact]
    public async Task 승인이나권리확인이없거나_자료가만료되면_빈글을만들지않는다()
    {
        await using var db = CreateContext();
        AddVariant(
            db,
            "expired",
            "만료 음식",
            OfficialFoodRecipeReviewStates.Approved,
            OfficialFoodRecipeRepresentationStates.Representative,
            expiresAtUtc: Now.UtcDateTime.AddMinutes(-1));
        AddVariant(
            db,
            "metadata-only",
            "권리 미확인 음식",
            OfficialFoodRecipeReviewStates.Approved,
            OfficialFoodRecipeRepresentationStates.Representative,
            automationState: OfficialFoodRecipeAutomationStates.MetadataOnly);
        await db.SaveChangesAsync();
        var source = new CommunityCultureTransportPostSource(
            db,
            new FixedTimeProvider(Now));

        var draft = await source.BuildAsync(
            new DateOnly(2026, 7, 23),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));

        Assert.Null(draft);
    }

    private static void AddVariant(
        AgriculturalFisheriesDbContext db,
        string dishKey,
        string name,
        string reviewState,
        string representationState,
        string originalName = "",
        string englishName = "",
        string regionName = "서울",
        string automationState = OfficialFoodRecipeAutomationStates.Enabled,
        DateTime? expiresAtUtc = null)
    {
        var source = new OfficialFoodRecipeSource
        {
            SourceKey = $"source-{dishKey}",
            Provider = "공식 음식 기관",
            DisplayName = "공식 음식 자료",
            CountryCode = "KR",
            LanguageCode = "ko",
            AutomationState = automationState,
            UpdateCycle = "일 1회 이하",
            RightsVerifiedAtUtc = Now.UtcDateTime.AddDays(-1)
        };
        var dish = new OfficialFoodDish
        {
            DishKey = dishKey,
            CountryCode = "KR",
            RegionName = regionName,
            Name = name,
            OriginalName = originalName,
            EnglishName = englishName,
            Category = "한 그릇 음식",
            ReviewState = reviewState,
            RepresentationState = representationState
        };
        db.OfficialFoodRecipeVariants.Add(new OfficialFoodRecipeVariant
        {
            Source = source,
            Dish = dish,
            RecordKey = $"record-{dishKey}",
            ExternalId = dishKey,
            Title = name,
            OriginalUrl = $"https://example.test/{dishKey}",
            AttributionText = "공식 음식 기관 · 원문 링크",
            LastCollectedAtUtc = Now.UtcDateTime.AddHours(-1),
            ContentExpiresAtUtc = expiresAtUtc
        });
    }

    private static AgriculturalFisheriesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase($"culture-transport-editorial-{Guid.NewGuid():N}")
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
