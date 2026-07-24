using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Services.Community;

public interface ICommunityPostIngredientPriceHintService
{
    Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
        CommunityPostIngredientPriceHintRequest request,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Application,
    "게시글 본문의 농수산 식재료를 KAMIS 교차표와 연결해 보관된 최신 가격 힌트를 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.IndependentExecution,
    Boundary = "입력 중 외부 API를 호출하거나 본문을 변경하지 않으며 보관된 조사값만 읽기 전용으로 표시합니다.")]
public sealed partial class CommunityPostIngredientPriceHintService
    : ICommunityPostIngredientPriceHintService
{
    private const int MaxBodyLength = 4_000;
    private const int MaxDetectedTerms = 20;
    private const string Notice =
        "서버에 보관된 KAMIS 최신 조사값입니다. 작성 보조 정보이며 실시간 판매가·견적·구매 제안이 아닙니다.";

    private static readonly string[] KoreanParticles =
    [
        "으로부터",
        "에게서",
        "에서는",
        "으로",
        "에서",
        "에게",
        "까지",
        "부터",
        "처럼",
        "보다",
        "하고",
        "이나",
        "랑",
        "과",
        "와",
        "을",
        "를",
        "은",
        "는",
        "이",
        "가",
        "도",
        "만"
    ];

    private static readonly HashSet<string> AmbiguousTerms =
        new(StringComparer.Ordinal)
        {
            "배"
        };

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly IReadOnlyDictionary<string, FoodPriceCrosswalk> _catalogByName;

    public CommunityPostIngredientPriceHintService(
        AgriculturalFisheriesDbContext db,
        IFoodPriceCrosswalkCatalog crosswalkCatalog,
        TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
        _catalogByName = crosswalkCatalog.GetAll()
            .GroupBy(entry => Normalize(entry.ProductName), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(entry => entry.MatchQualityCode == "ExactCommodity" ? 0 : 1)
                    .ThenByDescending(entry => entry.HsPrefix.Length)
                    .First(),
                StringComparer.Ordinal);
    }

    public async Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
        CommunityPostIngredientPriceHintRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var body = request.Body ?? string.Empty;
        if (body.Length > MaxBodyLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"게시글 본문은 {MaxBodyLength:N0}자 이하여야 합니다.");
        }

        var detectedTerms = ExtractTerms(body)
            .Where(term => _catalogByName.ContainsKey(term.NormalizedText))
            .Take(MaxDetectedTerms)
            .ToArray();
        if (detectedTerms.Length == 0)
        {
            return EmptyResponse();
        }

        var matchedEntries = detectedTerms
            .Select(term => new MatchedEntry(term, _catalogByName[term.NormalizedText]))
            .DistinctBy(item => item.Entry.AtItemCode, StringComparer.Ordinal)
            .ToArray();
        var itemCodes = matchedEntries
            .Select(item => item.Entry.AtItemCode)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var observations = await LoadRecentObservationsAsync(itemCodes, cancellationToken);
        var hints = matchedEntries
            .Select(item => BuildHint(item, observations))
            .ToArray();

        return new CommunityPostIngredientPriceHintResponse(
            hints,
            Notice,
            _timeProvider.GetUtcNow().UtcDateTime);
    }

    private async Task<IReadOnlyList<KamisPriceObservation>> LoadRecentObservationsAsync(
        IReadOnlyCollection<string> itemCodes,
        CancellationToken cancellationToken)
    {
        var baseQuery = _db.KamisPriceObservations
            .AsNoTracking()
            .Where(observation =>
                itemCodes.Contains(observation.ItemCode)
                && !observation.IsPriceMissing
                && observation.PriceKrw.HasValue
                && observation.PriceKrw > 0);
        var latestDailyDate = await baseQuery
            .Where(observation => observation.FrequencyCode == "Daily")
            .MaxAsync(observation => (DateOnly?)observation.SurveyDate, cancellationToken);
        var latestMonthlyDate = await baseQuery
            .Where(observation => observation.FrequencyCode == "Monthly")
            .MaxAsync(observation => (DateOnly?)observation.SurveyDate, cancellationToken);
        if (!latestDailyDate.HasValue && !latestMonthlyDate.HasValue)
        {
            return [];
        }

        var dailyCutoff = latestDailyDate?.AddDays(-31);
        var monthlyCutoff = latestMonthlyDate?.AddMonths(-13);
        return await baseQuery
            .Where(observation =>
                (dailyCutoff.HasValue
                 && observation.FrequencyCode == "Daily"
                 && observation.SurveyDate >= dailyCutoff.Value)
                || (monthlyCutoff.HasValue
                    && observation.FrequencyCode == "Monthly"
                    && observation.SurveyDate >= monthlyCutoff.Value))
            .ToArrayAsync(cancellationToken);
    }

    private static CommunityPostIngredientPriceHint BuildHint(
        MatchedEntry item,
        IReadOnlyList<KamisPriceObservation> observations)
    {
        var entry = item.Entry;
        var candidates = observations
            .Where(observation => observation.ItemCode == entry.AtItemCode)
            .Where(observation => string.IsNullOrWhiteSpace(entry.AtCategoryCode)
                                  || observation.CategoryCode == entry.AtCategoryCode)
            .Where(observation => entry.AtVarietyCodes.Count == 0
                                  || entry.AtVarietyCodes.Contains(
                                      observation.KindCode,
                                      StringComparer.Ordinal))
            .Where(observation => !entry.ExcludedNameTokens.Any(token =>
                observation.KindName.Contains(token, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var productClassCode = candidates.Any(observation =>
            observation.ProductClassCode == "01")
            ? "01"
            : candidates.Any(observation => observation.ProductClassCode == "02")
                ? "02"
                : string.Empty;
        var classCandidates = candidates
            .Where(observation => observation.ProductClassCode == productClassCode)
            .ToArray();
        var frequencyCode = classCandidates.Any(observation =>
            observation.FrequencyCode == "Daily")
            ? "Daily"
            : classCandidates.Any(observation => observation.FrequencyCode == "Monthly")
                ? "Monthly"
                : string.Empty;
        var frequencyCandidates = classCandidates
            .Where(observation => observation.FrequencyCode == frequencyCode)
            .ToArray();
        if (frequencyCandidates.Length == 0)
        {
            return MissingPriceHint(item);
        }

        var latestDate = frequencyCandidates.Max(observation => observation.SurveyDate);
        var samples = frequencyCandidates
            .Where(observation => observation.SurveyDate == latestDate)
            .Where(observation => observation.PriceKrw is > 0)
            .ToArray();
        if (samples.Length == 0)
        {
            return MissingPriceHint(item);
        }

        var prices = samples
            .Select(observation => observation.PriceKrw!.Value)
            .ToArray();
        var first = samples[0];
        var requiresConfirmation = AmbiguousTerms.Contains(item.Term.NormalizedText);
        return new CommunityPostIngredientPriceHint(
            item.Term.MatchedText,
            entry.ProductName,
            entry.AtItemCode,
            requiresConfirmation,
            InterpretationNote(item.Term, entry, requiresConfirmation),
            true,
            decimal.Round(prices.Average(), 0, MidpointRounding.AwayFromZero),
            prices.Min(),
            prices.Max(),
            "KRW",
            first.Unit,
            latestDate,
            productClassCode == "01" ? "Retail" : "Wholesale",
            productClassCode == "01" ? "전국 소매 조사가격" : "전국 도매 조사가격",
            BuildVarietySummary(samples),
            samples.Length,
            "한국농수산식품유통공사 KAMIS",
            first.SourceUrl);
    }

    private static CommunityPostIngredientPriceHint MissingPriceHint(MatchedEntry item)
    {
        var requiresConfirmation = AmbiguousTerms.Contains(item.Term.NormalizedText);
        return new CommunityPostIngredientPriceHint(
            item.Term.MatchedText,
            item.Entry.ProductName,
            item.Entry.AtItemCode,
            requiresConfirmation,
            InterpretationNote(item.Term, item.Entry, requiresConfirmation),
            false,
            null,
            null,
            null,
            "KRW",
            string.Empty,
            null,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            "한국농수산식품유통공사 KAMIS",
            "https://www.kamis.or.kr/customer/reference/openapi_list.do");
    }

    private static string InterpretationNote(
        DetectedTerm term,
        FoodPriceCrosswalk entry,
        bool requiresConfirmation)
        => requiresConfirmation
            ? $"‘{term.MatchedText}’를 과일 ‘{entry.ProductName}’로 해석했습니다. 다른 뜻이라면 이 가격을 무시하세요."
            : $"본문의 ‘{term.MatchedText}’를 KAMIS ‘{entry.ProductName}’ 품목과 연결했습니다.";

    private static string BuildVarietySummary(
        IReadOnlyCollection<KamisPriceObservation> observations)
    {
        var names = observations
            .Select(observation => observation.KindName?.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        return names.Length == 0
            ? "품종 구분 없음"
            : string.Join(" · ", names);
    }

    private CommunityPostIngredientPriceHintResponse EmptyResponse()
        => new(
            [],
            Notice,
            _timeProvider.GetUtcNow().UtcDateTime);

    private static IReadOnlyList<DetectedTerm> ExtractTerms(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        var detected = new Dictionary<string, DetectedTerm>(StringComparer.Ordinal);
        foreach (Match match in WordPattern().Matches(body.Normalize(NormalizationForm.FormKC)))
        {
            var token = Normalize(match.Value);
            AddCandidate(token, match.Value, match.Index);

            foreach (var particle in KoreanParticles)
            {
                if (!token.EndsWith(particle, StringComparison.Ordinal)
                    || token.Length <= particle.Length)
                {
                    continue;
                }

                var stem = token[..^particle.Length];
                if (stem.Length >= 2)
                {
                    AddCandidate(stem, match.Value, match.Index);
                }

                break;
            }
        }

        return detected.Values
            .OrderBy(term => term.Index)
            .ThenByDescending(term => term.NormalizedText.Length)
            .ToArray();

        void AddCandidate(string normalizedText, string matchedText, int index)
        {
            if (string.IsNullOrWhiteSpace(normalizedText)
                || (normalizedText.Length < 2
                    && !AmbiguousTerms.Contains(normalizedText))
                || detected.ContainsKey(normalizedText))
            {
                return;
            }

            detected[normalizedText] = new DetectedTerm(
                normalizedText,
                matchedText,
                index);
        }
    }

    private static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormKC).Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    [GeneratedRegex(
        @"[가-힣A-Za-z0-9]+",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 100)]
    private static partial Regex WordPattern();

    private sealed record DetectedTerm(
        string NormalizedText,
        string MatchedText,
        int Index);

    private sealed record MatchedEntry(
        DetectedTerm Term,
        FoodPriceCrosswalk Entry);
}
