using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.Content;

public abstract class CommunityOfficialFoodRecipeCandidateSource
    : ICommunityInformationCandidateSource
{
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly TimeProvider _timeProvider;

    protected CommunityOfficialFoodRecipeCandidateSource(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider,
        CommunityInformationSourceDto source)
    {
        _db = db;
        _timeProvider = timeProvider;
        Source = source;
    }

    public CommunityInformationSourceDto Source { get; }

    public async Task<IReadOnlyList<CommunityInformationCandidateDto>> ReadAsync(
        CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (!string.IsNullOrWhiteSpace(query.ReviewState)
            && !string.Equals(
                query.ReviewState.Trim(),
                CommunityInformationReviewStates.PendingReview,
                StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var variants = _db.OfficialFoodRecipeVariants
            .AsNoTracking()
            .Include(variant => variant.Source)
            .Include(variant => variant.Dish)
            .Where(variant => variant.Source != null
                              && variant.Source.SourceKey == Source.SourceKey
                              && !variant.IsRemovedAtSource
                              && (!variant.ContentExpiresAtUtc.HasValue
                                  || variant.ContentExpiresAtUtc > nowUtc));

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var countryCode = query.CountryCode.Trim();
            variants = variants.Where(variant =>
                variant.Source != null && variant.Source.CountryCode == countryCode);
        }

        if (query.StartDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(
                query.StartDate.Value.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);
            variants = variants.Where(variant =>
                (variant.SourceModifiedAtUtc ?? variant.LastCollectedAtUtc) >= startUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusiveUtc = DateTime.SpecifyKind(
                query.EndDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);
            variants = variants.Where(variant =>
                (variant.SourceModifiedAtUtc ?? variant.LastCollectedAtUtc) < endExclusiveUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();
            variants = variants.Where(variant =>
                variant.Title.Contains(searchText)
                || variant.Summary.Contains(searchText)
                || variant.RegionName.Contains(searchText)
                || variant.Category.Contains(searchText));
        }

        var take = Math.Clamp(query.Take, 1, 100);
        var rows = await variants
            .OrderByDescending(variant => variant.SourceModifiedAtUtc ?? variant.LastCollectedAtUtc)
            .ThenBy(variant => variant.Title)
            .Take(take)
            .ToArrayAsync(cancellationToken);

        return rows.Select(ToCandidate).ToArray();
    }

    private static CommunityInformationCandidateDto ToCandidate(
        Domain.FoodCulture.OfficialFoodRecipeVariant variant)
    {
        var source = variant.Source
            ?? throw new InvalidOperationException("공식 음식 레시피 원천 관계가 없습니다.");
        var dish = variant.Dish
            ?? throw new InvalidOperationException("공식 음식 대표 후보 관계가 없습니다.");
        var referenceTime = variant.SourceModifiedAtUtc ?? variant.LastCollectedAtUtc;
        var tags = DeserializeTags(variant.TagsJson)
            .Concat(new[] { dish.CountryCode, variant.RegionName, variant.Category })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new CommunityInformationCandidateDto(
            $"official-food-recipe:{variant.RecordKey}",
            source.SourceKey,
            CommunityInformationSourceTypes.PublicData,
            source.Provider,
            variant.Title,
            string.IsNullOrWhiteSpace(variant.Summary)
                ? string.Join(
                    " · ",
                    new[] { variant.RegionName, variant.Category }
                        .Where(value => !string.IsNullOrWhiteSpace(value)))
                : variant.Summary,
            variant.OriginalUrl,
            null,
            null,
            DateOnly.FromDateTime(referenceTime),
            variant.LastCollectedAtUtc,
            source.CountryCode,
            source.LanguageCode,
            null,
            null,
            CommunityInformationReviewStates.PendingReview,
            tags,
            variant.AttributionText,
            $"대표 음식 여부는 아직 검토 전입니다. {variant.TextReusePolicyAtCollection} {variant.ImageReusePolicyAtCollection}");
    }

    private static IReadOnlyList<string> DeserializeTags(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

public sealed class CommunityMfdsCookRecipeCandidateSource
    : CommunityOfficialFoodRecipeCandidateSource
{
    public CommunityMfdsCookRecipeCandidateSource(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider)
        : base(
            db,
            timeProvider,
            CreateSource(
                CommunityInformationSourceKeys.MfdsCookRecipes,
                "식품의약품안전처 식품안전나라",
                "식약처 조리식품 레시피 후보",
                "실시간 원천, 일 1회 이하 수집",
                "https://www.data.go.kr/data/15060073/openapi.do"))
    {
    }

    private static CommunityInformationSourceDto CreateSource(
        string key,
        string provider,
        string name,
        string cycle,
        string url)
        => OfficialFoodRecipeSourceDescriptor.Create(key, provider, name, cycle, url);
}

public sealed class CommunityRdaLocalFoodRecipeCandidateSource
    : CommunityOfficialFoodRecipeCandidateSource
{
    public CommunityRdaLocalFoodRecipeCandidateSource(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider)
        : base(
            db,
            timeProvider,
            OfficialFoodRecipeSourceDescriptor.Create(
                CommunityInformationSourceKeys.RdaLocalFoodRecipes,
                "농촌진흥청 농사로",
                "한국 향토 음식 레시피 후보",
                "실시간 원천, 월 1회 변경 확인",
                "https://www.data.go.kr/data/15101449/openapi.do"))
    {
    }
}

public sealed class CommunityMaffRegionalCuisineRecipeCandidateSource
    : CommunityOfficialFoodRecipeCandidateSource
{
    public CommunityMaffRegionalCuisineRecipeCandidateSource(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider)
        : base(
            db,
            timeProvider,
            OfficialFoodRecipeSourceDescriptor.Create(
                CommunityInformationSourceKeys.MaffRegionalCuisineRecipes,
                "일본 농림수산성(MAFF)",
                "일본 지역 대표 음식 레시피 후보",
                "월 1회 변경 확인",
                "https://www.maff.go.jp/e/policies/market/k_ryouri/"))
    {
    }
}

public sealed class CommunityNhsHealthierFamiliesRecipeCandidateSource
    : CommunityOfficialFoodRecipeCandidateSource
{
    public CommunityNhsHealthierFamiliesRecipeCandidateSource(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider)
        : base(
            db,
            timeProvider,
            OfficialFoodRecipeSourceDescriptor.Create(
                CommunityInformationSourceKeys.NhsHealthierFamiliesRecipes,
                "NHS England",
                "NHS 가족 건강 레시피 후보",
                "최소 7일마다 갱신, 7일 경과 자료 자동 제외",
                "https://www.nhs.uk/healthier-families/recipes/"))
    {
    }
}

internal static class OfficialFoodRecipeSourceDescriptor
{
    public static CommunityInformationSourceDto Create(
        string key,
        string provider,
        string name,
        string cycle,
        string url)
        => new(
            key,
            CommunityInformationSourceTypes.PublicData,
            provider,
            name,
            CommunityInformationCollectionModes.ScheduledArchive,
            cycle,
            "DB에 보관된 공식 원문도 대표성·번역·권리·최신성을 운영자가 검토한 뒤에만 커뮤니티 글의 근거로 사용합니다.",
            url,
            true);
}
