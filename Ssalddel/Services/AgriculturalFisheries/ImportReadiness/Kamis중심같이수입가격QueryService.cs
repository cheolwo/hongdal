using System.Globalization;
using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.ImportReadiness;

public interface IKamis중심같이수입가격QueryService
{
    Task<Kamis중심같이수입가격응답> GetAsync(
        Kamis중심같이수입가격Query query,
        CancellationToken cancellationToken = default);
}

public sealed class Kamis중심같이수입가격QueryService(
    IKamis중심UsdaAms가격비교QueryService marketPriceService,
    IFoodPriceCrosswalkCatalog crosswalkCatalog,
    IHs공공데이터수집Service publicDataService,
    TimeProvider timeProvider) : IKamis중심같이수입가격QueryService
{
    private const int MaxPageSize = 20;
    private const int MaxCandidatesPerItem = 5;
    private const int MaxExternalLookupsPerPage = 20;
    private const int MaxConcurrentLookups = 3;

    private sealed record CandidatePlan(
        string KamisItemCode,
        FoodPriceCrosswalk Crosswalk,
        string LookupHsCode,
        bool IsSelected,
        string OmissionReasonCode);

    private sealed record LookupResult(
        string KamisItemCode,
        string LookupHsCode,
        Kamis중심Hs수입통계단가응답 Price);

    public async Task<Kamis중심같이수입가격응답> GetAsync(
        Kamis중심같이수입가격Query query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = timeProvider.GetUtcNow();
        var countryCode = NormalizeCountryCode(query.CountryCode);
        if (countryCode.Length != 2)
        {
            throw new ArgumentException(
                "수입 국가 코드는 ISO 2자리 영문 코드로 입력해야 합니다.",
                nameof(query));
        }

        var referenceMonth = NormalizeReferenceMonth(query.ReferenceMonth, now);
        var lookbackMonths = Math.Clamp(
            query.ImportLookbackMonths <= 0 ? 3 : query.ImportLookbackMonths,
            1,
            12);
        if (query.FxRateKrwPerUsd is <= 0)
        {
            throw new ArgumentException(
                "환율을 입력할 때는 0보다 큰 원/USD 값을 사용해야 합니다.",
                nameof(query));
        }

        var requestedHsCode = NormalizeHsCode(query.HsCode);
        FoodPriceCrosswalk? requestedCrosswalk = null;
        if (requestedHsCode.Length > 0)
        {
            if (requestedHsCode.Length is < 4 or > 10)
            {
                throw new ArgumentException(
                    "HS 코드는 4~10자리 숫자로 입력해야 합니다.",
                    nameof(query));
            }

            requestedCrosswalk = crosswalkCatalog.Find(requestedHsCode)
                ?? throw new ArgumentException(
                    "입력한 HS 코드와 연결된 KAMIS 품목을 찾지 못했습니다.",
                    nameof(query));
            if (!string.IsNullOrWhiteSpace(query.ItemCode)
                && !string.Equals(
                    query.ItemCode.Trim(),
                    requestedCrosswalk.AtItemCode,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "입력한 HS 코드와 KAMIS 품목코드가 서로 연결되지 않습니다.",
                    nameof(query));
            }
        }

        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var marketPrices = await marketPriceService.GetAsync(
            new Kamis중심UsdaAms가격비교Query
            {
                Year = query.Year,
                CategoryCode = query.CategoryCode,
                ItemCode = requestedCrosswalk?.AtItemCode ?? query.ItemCode,
                Query = query.Query,
                FrequencyCode = query.FrequencyCode,
                OnlyMapped = query.OnlyAmsMapped,
                Page = query.Page,
                PageSize = pageSize,
                KamisPointsPerItem = query.KamisPointsPerItem,
                AmsPointsPerStage = query.AmsPointsPerStage
            },
            cancellationToken);

        var candidatesPerItem = Math.Clamp(
            query.HsPriceCandidatesPerItem <= 0
                ? 1
                : query.HsPriceCandidatesPerItem,
            1,
            MaxCandidatesPerItem);
        var plans = BuildCandidatePlans(
            marketPrices.Items,
            requestedHsCode,
            requestedCrosswalk,
            candidatesPerItem);
        var selectedPlans = plans
            .Where(plan => plan.IsSelected)
            .Take(MaxExternalLookupsPerPage)
            .ToArray();
        var selectedKeys = selectedPlans
            .Select(plan => BuildLookupKey(plan.KamisItemCode, plan.LookupHsCode))
            .ToHashSet(StringComparer.Ordinal);
        plans = plans
            .Select(plan =>
            {
                if (!plan.IsSelected
                    || selectedKeys.Contains(BuildLookupKey(
                        plan.KamisItemCode,
                        plan.LookupHsCode)))
                {
                    return plan;
                }

                return plan with
                {
                    IsSelected = false,
                    OmissionReasonCode =
                        Kamis중심Hs수입가격조회상태Codes.전체조회제한
                };
            })
            .ToArray();

        var lookupResults = await LookupImportPricesAsync(
            selectedPlans,
            countryCode,
            referenceMonth,
            lookbackMonths,
            query.FxRateKrwPerUsd,
            now,
            cancellationToken);
        var lookupByKey = lookupResults.ToDictionary(
            result => BuildLookupKey(result.KamisItemCode, result.LookupHsCode),
            result => result.Price,
            StringComparer.Ordinal);
        var plansByItemCode = plans
            .GroupBy(plan => plan.KamisItemCode, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<CandidatePlan>)group.ToArray(),
                StringComparer.Ordinal);

        var items = marketPrices.Items
            .Select(item => new Kamis중심같이수입품목가격응답(
                item,
                BuildCandidateResponses(item.KamisItemCode, plansByItemCode, lookupByKey)))
            .ToArray();
        var skippedLookupCount = plans.Count(plan => !plan.IsSelected);

        return new Kamis중심같이수입가격응답(
            marketPrices.StatusCode,
            now.UtcDateTime,
            marketPrices.Year,
            countryCode,
            referenceMonth,
            lookbackMonths,
            query.FxRateKrwPerUsd,
            marketPrices.ObservedKamisItemCount,
            marketPrices.FilteredKamisItemCount,
            marketPrices.MappedKamisItemCount,
            marketPrices.UnmappedKamisItemCount,
            marketPrices.Page,
            marketPrices.PageSize,
            selectedPlans.Length,
            skippedLookupCount,
            items,
            marketPrices.ComparisonBoundaries
                .Concat(
                [
                    "HS 코드는 KAMIS 품목에 연결한 분류 후보입니다. 실제 계약·신고 전에는 재질, 가공도, 용도와 한국 HSK 10단위를 관세사 등 전문가가 확인해야 합니다.",
                    "수입가격은 관세청 신고 수입금액 합계를 순중량 합계로 나눈 기간 가중평균 CIF 통계단가이며 공급 견적이나 판매가격이 아닙니다.",
                    "관세·부가세·검역·통관·보험·국내운송·보관·플랫폼 비용은 포함하지 않으므로 이 응답만으로 착지가격이나 수입 실행을 확정하지 않습니다.",
                    "KAMIS와 USDA AMS의 원 거래단위는 보존합니다. 단위·품종·등급·원산지·시점이 일치하기 전에는 CIF 원/kg 값과 직접 차액을 계산하지 않습니다.",
                    query.FxRateKrwPerUsd.HasValue
                        ? $"원화 참고값은 요청 환율 {query.FxRateKrwPerUsd.Value:N2}원/USD를 적용했습니다."
                        : "환율을 입력하지 않았으므로 CIF 원/kg 값은 제공하지 않고 USD/kg 통계단가만 제공합니다.",
                    $"외부 관세청 조회는 품목당 최대 {MaxCandidatesPerItem}개, 페이지당 최대 {MaxExternalLookupsPerPage}개로 제한하며 30분 캐시를 재사용합니다."
                ])
                .ToArray());
    }

    private IReadOnlyList<CandidatePlan> BuildCandidatePlans(
        IReadOnlyList<Kamis중심UsdaAms품목가격응답> items,
        string requestedHsCode,
        FoodPriceCrosswalk? requestedCrosswalk,
        int candidatesPerItem)
    {
        var crosswalkByItemCode = crosswalkCatalog.GetAll()
            .GroupBy(item => item.AtItemCode, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(item => item.HsPrefix.Length)
                    .ThenBy(item => item.HsPrefix, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
        var result = new List<CandidatePlan>();

        foreach (var item in items)
        {
            IReadOnlyList<(FoodPriceCrosswalk Crosswalk, string LookupHsCode)> candidates;
            if (requestedCrosswalk is not null
                && string.Equals(
                    requestedCrosswalk.AtItemCode,
                    item.KamisItemCode,
                    StringComparison.Ordinal))
            {
                candidates = [(requestedCrosswalk, requestedHsCode)];
            }
            else if (crosswalkByItemCode.TryGetValue(
                         item.KamisItemCode,
                         out var itemCrosswalks))
            {
                candidates = itemCrosswalks
                    .Select(crosswalk => (crosswalk, crosswalk.HsPrefix))
                    .ToArray();
            }
            else
            {
                candidates = [];
            }

            for (var index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                var selected = index < candidatesPerItem;
                result.Add(new CandidatePlan(
                    item.KamisItemCode,
                    candidate.Crosswalk,
                    candidate.LookupHsCode,
                    selected,
                    selected
                        ? string.Empty
                        : Kamis중심Hs수입가격조회상태Codes.후보제한));
            }
        }

        return result;
    }

    private async Task<IReadOnlyList<LookupResult>> LookupImportPricesAsync(
        IReadOnlyList<CandidatePlan> plans,
        string countryCode,
        string referenceMonth,
        int lookbackMonths,
        decimal? fxRateKrwPerUsd,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(MaxConcurrentLookups);
        var tasks = plans.Select(async plan =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var response = await publicDataService.수집Async(
                    new Hs공공데이터수집요청
                    {
                        HsCode = plan.LookupHsCode,
                        CountryCode = countryCode,
                        ReferenceMonth = referenceMonth,
                        LookbackMonths = lookbackMonths,
                        ReferenceDate = now.ToString(
                            "yyyyMMdd",
                            CultureInfo.InvariantCulture),
                        ExpectedFxRateKrwPerUsd = fxRateKrwPerUsd,
                        SourceKeys = [Hs공공데이터출처Keys.수입평균단가]
                    },
                    cancellationToken);
                return new LookupResult(
                    plan.KamisItemCode,
                    plan.LookupHsCode,
                    MapImportPrice(response, fxRateKrwPerUsd));
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }

    private static IReadOnlyList<Kamis중심Hs코드수입가격후보응답>
        BuildCandidateResponses(
            string kamisItemCode,
            IReadOnlyDictionary<string, IReadOnlyList<CandidatePlan>> plansByItemCode,
            IReadOnlyDictionary<string, Kamis중심Hs수입통계단가응답> lookupByKey)
    {
        if (!plansByItemCode.TryGetValue(kamisItemCode, out var plans))
        {
            return [];
        }

        return plans
            .Select(plan =>
            {
                lookupByKey.TryGetValue(
                    BuildLookupKey(kamisItemCode, plan.LookupHsCode),
                    out var price);
                return new Kamis중심Hs코드수입가격후보응답(
                    plan.LookupHsCode,
                    ResolveHsCodeScheme(plan.LookupHsCode),
                    plan.Crosswalk.ProductName,
                    plan.Crosswalk.MatchQualityCode,
                    plan.Crosswalk.MatchQualityLabel,
                    plan.Crosswalk.Note,
                    RequiresProfessionalReview: true,
                    plan.IsSelected,
                    plan.OmissionReasonCode,
                    price);
            })
            .ToArray();
    }

    private static Kamis중심Hs수입통계단가응답 MapImportPrice(
        Hs공공데이터묶음응답 response,
        decimal? fxRateKrwPerUsd)
    {
        var source = response.Sources.FirstOrDefault(item =>
            string.Equals(
                item.SourceKey,
                Hs공공데이터출처Keys.수입평균단가,
                StringComparison.OrdinalIgnoreCase))
            ?? new Hs공공데이터출처응답
            {
                SourceKey = Hs공공데이터출처Keys.수입평균단가,
                Provider = "관세청",
                DisplayName = "품목별 국가별 수입실적",
                StatusCode = Hs공공데이터수집상태Codes.오류,
                Summary = "관세청 수입 통계단가 응답을 찾지 못했습니다.",
                CollectedAtUtc = response.CollectedAtUtc
            };
        var fields = source.Items.FirstOrDefault()?.Fields
            ?? new Dictionary<string, string?>();

        return new Kamis중심Hs수입통계단가응답(
            source.StatusCode,
            response.CountryCode,
            GetField(fields, "startMonth"),
            GetField(fields, "endMonth"),
            ParseDecimal(fields, "totalImportWeightKg"),
            ParseDecimal(fields, "totalImportValueUsd"),
            ParseDecimal(fields, "averageImportUnitValueUsdPerKg"),
            fxRateKrwPerUsd,
            ParseDecimal(fields, "averageImportUnitValueKrwPerKg"),
            "kg",
            "관세청 신고 CIF 수입금액",
            "조회기간 수입금액 합계 / 조회기간 순중량 합계",
            source.Provider,
            source.DisplayName,
            source.DocumentationUrl,
            source.CollectedAtUtc,
            source.Summary);
    }

    private static string NormalizeCountryCode(string? value)
        => Regex.Replace(value ?? string.Empty, "[^A-Za-z]", string.Empty)
            .ToUpperInvariant();

    private static string NormalizeHsCode(string? value)
        => Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);

    private static string NormalizeReferenceMonth(
        string? value,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return now.AddMonths(-1).ToString(
                "yyyyMM",
                CultureInfo.InvariantCulture);
        }

        var normalized = Regex.Replace(value, "[^0-9]", string.Empty);
        if (!DateTime.TryParseExact(
                normalized,
                "yyyyMM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            throw new ArgumentException(
                "수입 통계 기준월은 yyyyMM 형식으로 입력해야 합니다.",
                nameof(value));
        }

        var currentMonth = new DateTime(now.Year, now.Month, 1);
        if (parsed > currentMonth)
        {
            throw new ArgumentException(
                "수입 통계 기준월은 현재 월보다 이후일 수 없습니다.",
                nameof(value));
        }

        return normalized;
    }

    private static string ResolveHsCodeScheme(string hsCode)
        => hsCode.Length switch
        {
            4 => "HS4",
            6 => "HS6",
            10 => "HSK10",
            _ => $"HS{hsCode.Length}"
        };

    private static string BuildLookupKey(string kamisItemCode, string hsCode)
        => $"{kamisItemCode}:{hsCode}";

    private static string GetField(
        IReadOnlyDictionary<string, string?> fields,
        string key)
        => fields.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static decimal? ParseDecimal(
        IReadOnlyDictionary<string, string?> fields,
        string key)
        => fields.TryGetValue(key, out var value)
           && decimal.TryParse(
               value,
               NumberStyles.Number,
               CultureInfo.InvariantCulture,
               out var parsed)
            ? parsed
            : null;
}
