using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.Content;
using Ssalddel.Domain.HsCodes;
using 살뜰.Data;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IHs식품국가가격CardQueryService
{
    Task<Hs식품국가가격Card응답> GetAsync(
        string hsCode,
        Hs식품국가가격CardQuery query,
        CancellationToken cancellationToken = default);
}

public interface IHs식품가격CardCatalogReader
{
    Task<Hs식품가격CardCatalog항목?> FindAsync(
        string hsCode,
        CancellationToken cancellationToken = default);
}

public sealed record Hs식품가격CardCatalog항목(
    string HsCode,
    string ProductName,
    string? RepresentativeImageUrl,
    string ImageReviewStatusCode);

public sealed class Hs식품가격CardCatalogReader(SsalddelContext db)
    : IHs식품가격CardCatalogReader
{
    public async Task<Hs식품가격CardCatalog항목?> FindAsync(
        string hsCode,
        CancellationToken cancellationToken = default)
    {
        var item = await db.HsCodeEntries
            .AsNoTracking()
            .Where(entry =>
                entry.IsActive
                && entry.Level == HsCodeLevel.Subheading
                && entry.BusinessCategory == HsCodeBusinessCategory.Food
                && entry.NormalizedCode == hsCode)
            .Select(entry => new
            {
                entry.NormalizedCode,
                entry.KoreanName
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (item is null)
        {
            return null;
        }

        var titlePrefix = $"HS {hsCode} ";
        var image = await db.앱문맥이미지자산들
            .AsNoTracking()
            .Where(asset =>
                asset.활성화여부
                && asset.앱PackId.StartsWith("hs-food-representatives")
                && asset.제목.StartsWith(titlePrefix)
                && asset.품질상태 != 앱문맥이미지품질상태.제외)
            .OrderByDescending(asset =>
                asset.품질상태 == 앱문맥이미지품질상태.사용가능)
            .ThenByDescending(asset => asset.PromptVersion)
            .Select(asset => new
            {
                asset.이미지Url,
                asset.품질상태
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new Hs식품가격CardCatalog항목(
            item.NormalizedCode,
            item.KoreanName,
            image?.이미지Url,
            image?.품질상태 switch
            {
                앱문맥이미지품질상태.사용가능 => "Available",
                앱문맥이미지품질상태.보정필요 => "CorrectionRequired",
                앱문맥이미지품질상태.미검토 => "Unreviewed",
                _ => "Unavailable"
            });
    }
}

public sealed class Hs식품국가가격CardQueryService(
    IHs식품가격CardCatalogReader catalogReader,
    IAgriculturalFisheriesInformationService domesticPriceService,
    IHsCountryTradeUnitPriceLookupService tradePriceService,
    TimeProvider timeProvider) : IHs식품국가가격CardQueryService
{
    private sealed record CountryDefinition(
        int DisplayOrder,
        string Code,
        string Name);

    private static readonly CountryDefinition[] TradeCountries =
    [
        new(2, "US", "미국"),
        new(3, "JP", "일본"),
        new(4, "CN", "중국")
    ];

    public async Task<Hs식품국가가격Card응답> GetAsync(
        string hsCode,
        Hs식품국가가격CardQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var normalizedHsCode = NormalizeHs6(hsCode);
        var referenceMonth = ResolveReferenceMonth(query.Month);
        var lookbackMonths = Math.Clamp(
            query.LookbackMonths <= 0 ? 3 : query.LookbackMonths,
            1,
            12);
        var catalogItem = await catalogReader.FindAsync(
            normalizedHsCode,
            cancellationToken);
        if (catalogItem is null)
        {
            return Empty(
                Hs식품국가가격Card상태Codes.품목없음,
                normalizedHsCode,
                referenceMonth,
                lookbackMonths);
        }

        var domestic = await ReadDomesticAsync(
            normalizedHsCode,
            cancellationToken);
        var tradeTasks = TradeCountries.Select(country => ReadTradeAsync(
            catalogItem,
            country,
            referenceMonth,
            lookbackMonths,
            cancellationToken));
        var trade = await Task.WhenAll(tradeTasks);
        var countries = new[] { domestic }
            .Concat(trade)
            .OrderBy(item => item.DisplayOrder)
            .ToArray();
        var status = countries.All(item =>
                item.DataStatusCode == Hs식품국가가격관측상태Codes.관측됨)
            ? Hs식품국가가격Card상태Codes.완료
            : Hs식품국가가격Card상태Codes.일부자료;

        return new Hs식품국가가격Card응답(
            status,
            timeProvider.GetUtcNow(),
            catalogItem.HsCode,
            "HS6",
            catalogItem.ProductName,
            catalogItem.RepresentativeImageUrl,
            catalogItem.ImageReviewStatusCode,
            referenceMonth,
            lookbackMonths,
            countries,
            ComparisonBoundaries,
            true);
    }

    private async Task<Hs식품국가가격응답> ReadDomesticAsync(
        string hsCode,
        CancellationToken cancellationToken)
    {
        AgriculturalFisheriesDomesticPriceResponse result;
        try
        {
            result = await domesticPriceService.GetDomesticPriceAsync(
                new AgriculturalFisheriesDomesticPriceRequest
                {
                    HsCode = hsCode,
                    LookbackDays = 14
                },
                cancellationToken);
        }
        catch (Exception exception) when (
            !cancellationToken.IsCancellationRequested
            && exception is InvalidOperationException
                or HttpRequestException
                or TaskCanceledException)
        {
            return CountryUnavailable(
                1,
                "KR",
                "한국",
                Hs식품국가가격관측상태Codes.조회불가,
                "KAMIS 국내 가격 조회가 현재 연결되지 않았습니다.");
        }

        var observations = new List<Hs식품국가가격관측응답>();
        AddDomesticObservation(observations, result.Price?.Wholesale, "Wholesale", "도매");
        AddDomesticObservation(observations, result.Price?.Retail, "Retail", "소매");
        var status = observations.Count > 0
            ? Hs식품국가가격관측상태Codes.관측됨
            : result.StatusCode == "MappingRequired"
                ? Hs식품국가가격관측상태Codes.자료없음
                : Hs식품국가가격관측상태Codes.조회불가;
        return new Hs식품국가가격응답(
            1,
            "KR",
            "한국",
            status,
            observations,
            observations.Count > 0
                ? "KAMIS 국내 도매·소매 조사 가격입니다."
                : result.Summary);
    }

    private static void AddDomesticObservation(
        ICollection<Hs식품국가가격관측응답> target,
        AtDomesticFoodPriceAggregate? value,
        string stageCode,
        string stageLabel)
    {
        if (value is null || value.SampleCount <= 0)
        {
            return;
        }

        target.Add(new Hs식품국가가격관측응답(
            Hs식품국가가격맥락Codes.국내시장조사가격,
            "국내 시장 조사 가격",
            stageCode,
            stageLabel,
            Hs식품국가가격관측상태Codes.관측됨,
            value.LatestSurveyDate,
            "KRW",
            "KRW/kg",
            value.AverageKrwPerKg,
            value.MinimumKrwPerKg,
            value.MaximumKrwPerKg,
            value.SampleCount,
            $"KamisDomesticSurvey-{stageCode}-KrwPerKg",
            true,
            "at-kamis-daily-food-price",
            "한국농수산식품유통공사 KAMIS",
            "https://www.kamis.or.kr/",
            "같은 품목·조사단계의 표본을 kg 기준으로 정규화한 평균·최저·최고 가격",
            "등급·산지·포장·신선도와 조사 표본 차이를 함께 확인해야 합니다."));
    }

    private async Task<Hs식품국가가격응답> ReadTradeAsync(
        Hs식품가격CardCatalog항목 catalogItem,
        CountryDefinition country,
        string referenceMonth,
        int lookbackMonths,
        CancellationToken cancellationToken)
    {
        HsCountryImportUnitPriceSimulationResult result;
        try
        {
            result = await tradePriceService.SimulateImportUnitPriceAsync(
                new HsCountryMonthlyTradeUnitPriceRequest
                {
                    InternalProductCode = $"hs-food-{catalogItem.HsCode}",
                    ProductName = catalogItem.ProductName,
                    HsCode = catalogItem.HsCode,
                    HsCodeScheme = "HS6",
                    CountryCode = country.Code,
                    Month = referenceMonth,
                    LookbackMonths = lookbackMonths
                },
                cancellationToken);
        }
        catch (Exception exception) when (
            !cancellationToken.IsCancellationRequested
            && exception is InvalidOperationException
                or HttpRequestException
                or TaskCanceledException)
        {
            return CountryUnavailable(
                country.DisplayOrder,
                country.Code,
                country.Name,
                Hs식품국가가격관측상태Codes.조회불가,
                "관세청 국가별 수입통계 조회가 현재 연결되지 않았습니다.");
        }

        if (!result.Success || !result.AverageImportUnitValueUsdPerKg.HasValue)
        {
            var noData = result.ErrorMessage?.Contains(
                "No import statistics",
                StringComparison.OrdinalIgnoreCase) == true;
            return CountryUnavailable(
                country.DisplayOrder,
                country.Code,
                country.Name,
                noData
                    ? Hs식품국가가격관측상태Codes.자료없음
                    : Hs식품국가가격관측상태Codes.조회불가,
                noData
                    ? "선택한 기간에 해당 국가 원산 수입 신고 실적이 없습니다."
                    : "관세청 국가별 수입통계 단가를 계산하지 못했습니다.");
        }

        var observedMonths = result.MonthlyItems
            .Where(item => item.ImportWeightKg > 0 && item.ImportValueUsd > 0)
            .ToArray();
        var observation = new Hs식품국가가격관측응답(
            Hs식품국가가격맥락Codes.수입통계단가,
            "한국 수입통계 평균단가",
            "CustomsImport",
            "수입 신고(CIF)",
            Hs식품국가가격관측상태Codes.관측됨,
            $"{result.StartMonth}-{result.EndMonth}",
            "USD",
            "USD/kg",
            result.AverageImportUnitValueUsdPerKg,
            observedMonths.Length == 0
                ? null
                : observedMonths.Min(item => item.AverageImportUnitValueUsdPerKg),
            observedMonths.Length == 0
                ? null
                : observedMonths.Max(item => item.AverageImportUnitValueUsdPerKg),
            observedMonths.Length,
            $"KcsHs6ImportCifUsdPerKg-{result.StartMonth}-{result.EndMonth}",
            true,
            "kcs-hs-country-monthly-trade",
            "관세청 품목·국가별 수출입실적",
            result.DataSourceUrl,
            result.CalculationMethod,
            "시장 견적이나 도착원가가 아니라 신고금액을 순중량으로 나눈 통계 단가입니다.");
        return new Hs식품국가가격응답(
            country.DisplayOrder,
            country.Code,
            country.Name,
            Hs식품국가가격관측상태Codes.관측됨,
            [observation],
            $"{country.Name} 원산 수입 신고의 기간 가중평균 단가입니다.");
    }

    private static Hs식품국가가격응답 CountryUnavailable(
        int displayOrder,
        string countryCode,
        string countryName,
        string statusCode,
        string summary)
        => new(
            displayOrder,
            countryCode,
            countryName,
            statusCode,
            [],
            summary);

    private Hs식품국가가격Card응답 Empty(
        string statusCode,
        string hsCode,
        string referenceMonth,
        int lookbackMonths)
        => new(
            statusCode,
            timeProvider.GetUtcNow(),
            hsCode,
            "HS6",
            string.Empty,
            null,
            "Unavailable",
            referenceMonth,
            lookbackMonths,
            [],
            ComparisonBoundaries,
            true);

    private string ResolveReferenceMonth(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return timeProvider.GetUtcNow()
                .AddMonths(-1)
                .ToString("yyyyMM", CultureInfo.InvariantCulture);
        }

        var digits = Regex.Replace(value, "[^0-9]", string.Empty);
        if (digits.Length != 6
            || !DateTime.TryParseExact(
                digits + "01",
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            throw new ArgumentException("기준월은 yyyyMM 형식이어야 합니다.", nameof(value));
        }

        return digits;
    }

    private static string NormalizeHs6(string value)
    {
        var digits = Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);
        if (digits.Length < 6)
        {
            throw new ArgumentException("HS 코드는 최소 6자리여야 합니다.", nameof(value));
        }

        return digits[..6];
    }

    private static readonly IReadOnlyList<string> ComparisonBoundaries =
    [
        "한국 KAMIS 가격은 국내 시장 조사값이고 미국·일본·중국 값은 한국 수입 신고의 CIF 통계 단가이므로 서로 직접 차액이나 순위를 계산하지 않습니다.",
        "미국·일본·중국 수입통계 단가는 같은 HS6·기간·통화·중량 산식일 때만 같은 비교 그룹으로 봅니다.",
        "통계 단가는 실제 견적, 운임·보험·관세·통관비를 포함한 도착원가 또는 판매가격이 아닙니다.",
        "HS 분류, 원산지, 가공상태, 등급, 포장과 거래조건은 실제 계약·신고 전에 별도로 확인해야 합니다."
    ];
}
