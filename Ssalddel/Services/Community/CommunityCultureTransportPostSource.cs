using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.Community;

public sealed class CommunityCultureTransportPostSource : ICommunityAutomatedPostSource
{
    private const int CandidateVariantLimit = 500;

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly TimeProvider _timeProvider;

    public CommunityCultureTransportPostSource(
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public string SourceKey => CommunityAutomatedPostSourceKeys.CultureTransport;

    public async Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var variants = await _db.OfficialFoodRecipeVariants
            .AsNoTracking()
            .Where(variant =>
                variant.Dish != null
                && variant.Source != null
                && variant.Dish.ReviewState == OfficialFoodRecipeReviewStates.Approved
                && variant.Dish.RepresentationState
                == OfficialFoodRecipeRepresentationStates.Representative
                && (variant.Source.AutomationState
                    == OfficialFoodRecipeAutomationStates.Enabled
                    || variant.Source.AutomationState
                    == OfficialFoodRecipeAutomationStates.EnabledWhenConfigured)
                && variant.Source.RightsVerifiedAtUtc > DateTime.UnixEpoch
                && variant.Source.RightsVerifiedAtUtc <= nowUtc
                && !variant.IsRemovedAtSource
                && (!variant.ContentExpiresAtUtc.HasValue
                    || variant.ContentExpiresAtUtc > nowUtc))
            .OrderBy(variant => variant.Dish!.CountryCode)
            .ThenBy(variant => variant.Dish!.RegionName)
            .ThenBy(variant => variant.Dish!.Name)
            .ThenBy(variant => variant.Dish!.DishKey)
            .ThenByDescending(variant => variant.LastCollectedAtUtc)
            .Take(CandidateVariantLimit)
            .Select(variant => new CultureTransportCandidate(
                variant.Dish!.DishKey,
                variant.Dish.CountryCode,
                variant.Dish.RegionName,
                variant.Dish.Name,
                variant.Dish.OriginalName,
                variant.Dish.EnglishName,
                variant.Dish.Category,
                variant.Source!.Provider,
                variant.Source.UpdateCycle,
                variant.OriginalUrl,
                variant.AttributionText,
                variant.LastCollectedAtUtc))
            .ToArrayAsync(cancellationToken);

        var candidates = variants
            .Where(candidate => IsPublicHttpUrl(candidate.OriginalUrl))
            .GroupBy(candidate => candidate.DishKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        if (candidates.Length == 0)
        {
            return null;
        }

        var candidate = candidates[publicationDate.DayNumber % candidates.Length];
        return new CommunityAutomatedPostDraft(
            SourceKey,
            publicationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            CommunityBoardCatalog.Food.DisplayName,
            CultureTransportContentCatalog.FoodCultureWorkflowTag,
            "문화교통 공식 근거 질문",
            $"[문화교통] {candidate.Name}, 어떻게 먹고 어디로 이어지나요?",
            BuildBody(candidate, publicationDate, timeZone),
            "살뜰 문화교통 길잡이",
            candidate.OriginalUrl);
    }

    private static string BuildBody(
        CultureTransportCandidate candidate,
        DateOnly publicationDate,
        TimeZoneInfo timeZone)
    {
        var names = new[] { candidate.OriginalName, candidate.EnglishName }
            .Where(name => !string.IsNullOrWhiteSpace(name)
                           && !string.Equals(name, candidate.Name, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var place = string.Join(
            " · ",
            new[]
            {
                CountryName(candidate.CountryCode),
                candidate.RegionName,
                candidate.Category
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var lines = new List<string>
        {
            "[자동 작성 안내] 운영자가 대표 음식으로 승인한 공식 자료의 메타데이터를 바탕으로 문화교통 질문을 만들었습니다.",
            $"오늘 함께 볼 음식: {candidate.Name}{(names.Length == 0 ? string.Empty : $" ({string.Join(" / ", names)})")}",
            $"지역·분류: {place}",
            $"공식 자료 제공: {candidate.Provider}",
            $"자료 확인 시각: {candidate.LastCollectedAtUtc:yyyy-MM-dd HH:mm} UTC · 갱신 주기: {UpdateCycle(candidate.UpdateCycle)}",
            $"게시 기준일: {publicationDate:yyyy-MM-dd} ({timeZone.Id})",
            string.Empty,
            "함께 나눠 볼 이야기",
            "- 이 음식은 현지에서 언제, 누구와 함께 먹나요?",
            "- 지역이나 가정에 따라 재료와 조리법, 곁들이는 방식이 어떻게 달라지나요?",
            "- 다른 나라에서 재료를 구하기 어려울 때 어떤 대체 방식이 자연스러웠나요?",
            "- 음식 이름을 번역할 때 빠지기 쉬운 기억, 예절이나 문화적 맥락이 있나요?",
            "- 이 재료가 다른 지역으로 이동한다면 산지·포장·보관·수령 조건 가운데 무엇을 먼저 확인해야 하나요?",
            string.Empty,
            "문화교통은 문화와 근거를 먼저 나누고, 원할 때만 비구속 수요와 공급·이동 준비로 이어지는 0.0~1.5의 흐름입니다.",
            "한 사람의 경험을 한 국가나 문화 전체의 답으로 일반화하지 않고, 서로 다른 경험과 근거를 함께 남겨 주세요.",
            "이 글은 구매 권유, 판매자 추천 또는 현재 수입 가능성을 보증하는 글이 아닙니다.",
            $"출처 표기: {Attribution(candidate)}"
        };
        return string.Join(Environment.NewLine, lines);
    }

    private static string Attribution(CultureTransportCandidate candidate)
        => string.IsNullOrWhiteSpace(candidate.AttributionText)
            ? candidate.Provider
            : candidate.AttributionText.Trim();

    private static string UpdateCycle(string value)
        => string.IsNullOrWhiteSpace(value) ? "원천별 확인" : value.Trim();

    private static string CountryName(string countryCode)
        => countryCode.Trim().ToUpperInvariant() switch
        {
            "KR" => "한국",
            "JP" => "일본",
            "GB" => "영국",
            "US" => "미국",
            "CA" => "캐나다",
            "FR" => "프랑스",
            _ => countryCode.Trim().ToUpperInvariant()
        };

    private static bool IsPublicHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private sealed record CultureTransportCandidate(
        string DishKey,
        string CountryCode,
        string RegionName,
        string Name,
        string OriginalName,
        string EnglishName,
        string Category,
        string Provider,
        string UpdateCycle,
        string OriginalUrl,
        string AttributionText,
        DateTime LastCollectedAtUtc);
}
