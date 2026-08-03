using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IKamisDomesticPriceArchiveQueryService
{
    Task<AtDomesticFoodPriceLookupResult> LookupAsync(
        AtDomesticFoodPriceRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class KamisDomesticPriceArchiveQueryService(
    AgriculturalFisheriesDbContext db) : IKamisDomesticPriceArchiveQueryService
{
    public async Task<AtDomesticFoodPriceLookupResult> LookupAsync(
        AtDomesticFoodPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryParseDate(request.StartDate, out var startDate)
            || !TryParseDate(request.EndDate, out var endDate)
            || endDate < startDate)
        {
            return Fail(request, "저장 원장 조회기간을 yyyyMMdd 형식으로 확인해 주세요.");
        }

        var categoryCode = request.CategoryCode.Trim();
        var itemCode = request.ItemCode.Trim();
        var rows = await db.KamisPriceObservations
            .AsNoTracking()
            .Where(observation => observation.CategoryCode == categoryCode
                                  && observation.ItemCode == itemCode
                                  && observation.FrequencyCode == "Daily"
                                  && observation.SurveyDate >= startDate
                                  && observation.SurveyDate <= endDate
                                  && !observation.IsPriceMissing
                                  && observation.PriceKrw.HasValue
                                  && observation.PriceKrw > 0
                                  && observation.ComparisonUnit == "1kg")
            .ToListAsync(cancellationToken);

        var observations = rows
            .Select(observation => new Kamis국내가격Observation(
                observation.SurveyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                observation.ProductClassCode,
                observation.ItemName,
                observation.KindCode,
                string.Join(
                    ' ',
                    observation.ItemName,
                    observation.KindName,
                    observation.RankName),
                observation.PriceKrw!.Value))
            .ToArray();

        var wholesale = Kamis국내가격Aggregation.Aggregate(
            observations,
            request,
            Kamis국내가격Aggregation.WholesaleCode,
            "국내 중도매가격");
        var retail = Kamis국내가격Aggregation.Aggregate(
            observations,
            request,
            Kamis국내가격Aggregation.RetailCode,
            "국내 소매가격");
        if (wholesale is null && retail is null)
        {
            return Fail(request, "조회기간에 저장된 KAMIS 공식 가격 관측값이 없습니다.");
        }

        return new AtDomesticFoodPriceLookupResult
        {
            Success = true,
            CategoryCode = categoryCode,
            ItemCode = itemCode,
            ItemName = observations
                .Select(observation => observation.ItemName)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty,
            StartDate = startDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            EndDate = endDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            Wholesale = wholesale,
            Retail = retail,
            DataSource = "한국농수산식품유통공사 KAMIS Open API 저장 원장"
        };
    }

    private static bool TryParseDate(string? value, out DateOnly date)
        => DateOnly.TryParseExact(
            value,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);

    private static AtDomesticFoodPriceLookupResult Fail(
        AtDomesticFoodPriceRequest request,
        string errorMessage)
        => new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            CategoryCode = request.CategoryCode,
            ItemCode = request.ItemCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DataSource = "한국농수산식품유통공사 KAMIS Open API 저장 원장"
        };
}
