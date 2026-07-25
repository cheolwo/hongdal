using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed class 농산물지역가격비교QueryService
    : I농산물지역가격비교QueryService
{
    private const int 최대비교기간일수 = 31;

    private static readonly IReadOnlyList<string> Notices =
    [
        "공영도매시장 경락·정산가격을 거래 중량으로 환산한 원/kg 비교값입니다.",
        "원산지는 생산지 기준이며 도매시장 소재지와 다를 수 있습니다.",
        "KAMIS 중도매·소매 조사 가격과 시장 단계가 다르므로 직접 같은 가격으로 해석하지 않습니다.",
        "단위 중량 또는 가격이 없는 거래는 비교 집계에서 제외합니다."
    ];

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly I국내농산물경락가격조회Service _sourceService;

    public 농산물지역가격비교QueryService(
        AgriculturalFisheriesDbContext db,
        I국내농산물경락가격조회Service sourceService)
    {
        _db = db;
        _sourceService = sourceService;
    }

    public async Task<농산물지역가격비교선택지응답> GetOptionsAsync(
        농산물지역가격비교선택지요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var source = FindSource(request.SourceKey);
        if (source is null)
        {
            return OptionsFail(
                국내농산물경락가격조회상태Codes.지원하지않는출처,
                $"지원하지 않는 경락가격 원천입니다. SourceKey={request.SourceKey}");
        }

        DateOnly? requestedDate = null;
        if (!string.IsNullOrWhiteSpace(request.SettlementDate)
            && !TryParseDate(request.SettlementDate, out requestedDate))
        {
            return OptionsFail(
                국내농산물경락가격조회상태Codes.잘못된요청,
                "SettlementDate는 yyyy-MM-dd 형식이어야 합니다.");
        }

        var sourceQuery = _db.DomesticAuctionPriceObservations
            .AsNoTracking()
            .Where(item => item.SourceKey == source.Key);
        var settlementDate = requestedDate
            ?? await sourceQuery
                .Select(item => (DateOnly?)item.SettlementDate)
                .MaxAsync(cancellationToken);
        if (settlementDate is null)
        {
            return new 농산물지역가격비교선택지응답
            {
                Success = true,
                StatusCode = 국내농산물경락가격조회상태Codes.완료,
                Source = source,
                Notices = Notices
            };
        }

        var dateQuery = sourceQuery.Where(item => item.SettlementDate == settlementDate);
        var itemNames = await dateQuery
            .Where(item => item.ItemName != string.Empty)
            .Select(item => item.ItemName)
            .Distinct()
            .OrderBy(item => item)
            .ToArrayAsync(cancellationToken);

        var selectedItemName = request.ItemName?.Trim();
        var detailQuery = string.IsNullOrWhiteSpace(selectedItemName)
            ? dateQuery.Where(_ => false)
            : dateQuery.Where(item => item.ItemName == selectedItemName);
        var varietyNames = await detailQuery
            .Where(item => item.VarietyName != string.Empty)
            .Select(item => item.VarietyName)
            .Distinct()
            .OrderBy(item => item)
            .ToArrayAsync(cancellationToken);
        var origins = await detailQuery
            .Where(item => item.OriginName != string.Empty)
            .Select(item => new { item.OriginCode, item.OriginName })
            .Distinct()
            .OrderBy(item => item.OriginName)
            .ToArrayAsync(cancellationToken);
        var markets = await detailQuery
            .Where(item => item.WholesaleMarketCode != string.Empty)
            .Select(item => item.WholesaleMarketCode)
            .Distinct()
            .OrderBy(item => item)
            .ToArrayAsync(cancellationToken);

        return new 농산물지역가격비교선택지응답
        {
            Success = true,
            StatusCode = 국내농산물경락가격조회상태Codes.완료,
            Source = source,
            SettlementDate = settlementDate,
            ItemNames = itemNames,
            VarietyNames = varietyNames,
            OriginRegions = origins.Select(item => new 농산물가격비교지역선택지
            {
                Code = item.OriginCode,
                Name = item.OriginName
            }).ToArray(),
            WholesaleMarkets = markets.Select(code => new 농산물가격비교지역선택지
            {
                Code = code,
                Name = code
            }).ToArray(),
            Notices = Notices
        };
    }

    public async Task<농산물지역가격비교응답> CompareAsync(
        농산물지역가격비교요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var source = FindSource(request.SourceKey);
        if (source is null)
        {
            return ComparisonFail(
                request,
                국내농산물경락가격조회상태Codes.지원하지않는출처,
                $"지원하지 않는 경락가격 원천입니다. SourceKey={request.SourceKey}");
        }

        var itemName = (request.ItemName ?? string.Empty).Trim();
        if (itemName.Length == 0)
        {
            return ComparisonFail(
                request,
                국내농산물경락가격조회상태Codes.잘못된요청,
                "ItemName은 필수입니다.");
        }

        var regionBasisCode = NormalizeRegionBasisCode(request.RegionBasisCode);
        if (regionBasisCode is null)
        {
            return ComparisonFail(
                request,
                국내농산물경락가격조회상태Codes.잘못된요청,
                "RegionBasisCode는 Origin 또는 WholesaleMarket이어야 합니다.");
        }

        var varietyName = string.IsNullOrWhiteSpace(request.VarietyName)
            ? null
            : request.VarietyName.Trim();
        var normalizedQuery = new 농산물지역가격비교요청
        {
            SourceKey = source.Key,
            ItemName = itemName,
            VarietyName = varietyName,
            StartDate = request.StartDate?.Trim(),
            EndDate = request.EndDate?.Trim(),
            RegionBasisCode = regionBasisCode
        };
        var dateResolution = await ResolveDateRangeAsync(
            source.Key,
            itemName,
            normalizedQuery.StartDate,
            normalizedQuery.EndDate,
            cancellationToken);
        if (dateResolution.ErrorMessage is not null)
        {
            return ComparisonFail(
                normalizedQuery,
                국내농산물경락가격조회상태Codes.잘못된요청,
                dateResolution.ErrorMessage);
        }

        if (dateResolution.StartDate is null || dateResolution.EndDate is null)
        {
            return new 농산물지역가격비교응답
            {
                Success = true,
                StatusCode = 국내농산물경락가격조회상태Codes.완료,
                Source = source,
                Query = normalizedQuery,
                Notices = Notices
            };
        }

        var startDate = dateResolution.StartDate.Value;
        var endDate = dateResolution.EndDate.Value;
        var query = _db.DomesticAuctionPriceObservations
            .AsNoTracking()
            .Where(item =>
                item.SourceKey == source.Key
                && item.ItemName == itemName
                && item.SettlementDate >= startDate
                && item.SettlementDate <= endDate
                && item.AuctionPriceKrw.HasValue
                && item.UnitWeight.HasValue
                && item.UnitWeight > 0m);
        if (varietyName is not null)
        {
            query = query.Where(item => item.VarietyName == varietyName);
        }

        var aggregates = regionBasisCode == 농산물지역가격비교기준Codes.원산지
            ? await AggregateByOriginAsync(query, cancellationToken)
            : await AggregateByMarketAsync(query, cancellationToken);
        var totalWeight = aggregates.Sum(item => item.TotalQuantityKg);
        var totalAmount = aggregates.Sum(item => item.TotalAmountKrw);
        var overallAverage = totalWeight > 0m ? totalAmount / totalWeight : 0m;

        return new 농산물지역가격비교응답
        {
            Success = true,
            StatusCode = 국내농산물경락가격조회상태Codes.완료,
            Source = source,
            Query = normalizedQuery,
            ResolvedStartDate = startDate,
            ResolvedEndDate = endDate,
            OverallAveragePriceKrwPerKg = overallAverage > 0m
                ? decimal.Round(overallAverage, 2)
                : null,
            Regions = aggregates
                .Where(item => item.TotalQuantityKg > 0m)
                .Select(item =>
                {
                    var average = item.TotalAmountKrw / item.TotalQuantityKg;
                    return new 농산물지역가격비교항목
                    {
                        RegionCode = item.RegionCode,
                        RegionName = item.RegionName,
                        ObservationCount = item.ObservationCount,
                        TradingDayCount = item.TradingDayCount,
                        TotalQuantityKg = decimal.Round(item.TotalQuantityKg, 3),
                        AveragePriceKrwPerKg = decimal.Round(average, 2),
                        MinimumPriceKrwPerKg = decimal.Round(item.MinimumPriceKrwPerKg, 2),
                        MaximumPriceKrwPerKg = decimal.Round(item.MaximumPriceKrwPerKg, 2),
                        ComparisonIndex = overallAverage > 0m
                            ? decimal.Round(average / overallAverage * 100m, 1)
                            : 0m
                    };
                })
                .OrderBy(item => item.AveragePriceKrwPerKg)
                .ThenBy(item => item.RegionName)
                .ToArray(),
            LatestCollectedAtUtc = aggregates.Length == 0
                ? null
                : new DateTimeOffset(
                    aggregates.Max(item => item.LatestCollectedAtUtc),
                    TimeSpan.Zero),
            Notices = Notices
        };
    }

    private async Task<DateResolution> ResolveDateRangeAsync(
        string sourceKey,
        string itemName,
        string? startDateText,
        string? endDateText,
        CancellationToken cancellationToken)
    {
        DateOnly? startDate = null;
        DateOnly? endDate = null;
        if (!string.IsNullOrWhiteSpace(startDateText)
            && !TryParseDate(startDateText, out startDate))
        {
            return new(null, null, "StartDate는 yyyy-MM-dd 형식이어야 합니다.");
        }

        if (!string.IsNullOrWhiteSpace(endDateText)
            && !TryParseDate(endDateText, out endDate))
        {
            return new(null, null, "EndDate는 yyyy-MM-dd 형식이어야 합니다.");
        }

        if (startDate is null && endDate is null)
        {
            var latestDate = await _db.DomesticAuctionPriceObservations
                .AsNoTracking()
                .Where(item => item.SourceKey == sourceKey && item.ItemName == itemName)
                .Select(item => (DateOnly?)item.SettlementDate)
                .MaxAsync(cancellationToken);
            return new(latestDate, latestDate, null);
        }

        startDate ??= endDate;
        endDate ??= startDate;
        var resolvedStartDate = startDate!.Value;
        var resolvedEndDate = endDate!.Value;
        if (resolvedStartDate > resolvedEndDate)
        {
            return new(null, null, "StartDate는 EndDate보다 늦을 수 없습니다.");
        }

        if (resolvedEndDate.DayNumber - resolvedStartDate.DayNumber + 1
            > 최대비교기간일수)
        {
            return new(null, null, $"비교 기간은 최대 {최대비교기간일수}일입니다.");
        }

        return new(resolvedStartDate, resolvedEndDate, null);
    }

    private static Task<지역가격Aggregate[]> AggregateByOriginAsync(
        IQueryable<Domain.AgriculturalFisheries.국내농산물경락가격관측> query,
        CancellationToken cancellationToken)
        => BuildOriginAggregateQuery(query)
            .ToArrayAsync(cancellationToken);

    internal static IQueryable<지역가격Aggregate> BuildOriginAggregateQuery(
        IQueryable<Domain.AgriculturalFisheries.국내농산물경락가격관측> query)
        => query
            .Where(item => item.OriginName != string.Empty)
            .GroupBy(item => new { item.OriginCode, item.OriginName })
            .Select(group => new 지역가격Aggregate(
                group.Key.OriginCode,
                group.Key.OriginName,
                group.Count(),
                group.Select(item => item.SettlementDate).Distinct().Count(),
                group.Sum(item =>
                    item.TotalQuantity.HasValue && item.TotalQuantity > 0m
                        ? item.TotalQuantity.Value
                        : (item.Quantity ?? 0m) * item.UnitWeight!.Value),
                group.Sum(item =>
                    item.TotalAmountKrw.HasValue && item.TotalAmountKrw > 0m
                        ? item.TotalAmountKrw.Value
                        : item.AuctionPriceKrw!.Value * (item.Quantity ?? 0m)),
                group.Min(item => item.AuctionPriceKrw!.Value / item.UnitWeight!.Value),
                group.Max(item => item.AuctionPriceKrw!.Value / item.UnitWeight!.Value),
                group.Max(item => item.LastSeenAtUtc)));

    private static Task<지역가격Aggregate[]> AggregateByMarketAsync(
        IQueryable<Domain.AgriculturalFisheries.국내농산물경락가격관측> query,
        CancellationToken cancellationToken)
        => BuildMarketAggregateQuery(query)
            .ToArrayAsync(cancellationToken);

    internal static IQueryable<지역가격Aggregate> BuildMarketAggregateQuery(
        IQueryable<Domain.AgriculturalFisheries.국내농산물경락가격관측> query)
        => query
            .Where(item => item.WholesaleMarketCode != string.Empty)
            .GroupBy(item => item.WholesaleMarketCode)
            .Select(group => new 지역가격Aggregate(
                group.Key,
                group.Key,
                group.Count(),
                group.Select(item => item.SettlementDate).Distinct().Count(),
                group.Sum(item =>
                    item.TotalQuantity.HasValue && item.TotalQuantity > 0m
                        ? item.TotalQuantity.Value
                        : (item.Quantity ?? 0m) * item.UnitWeight!.Value),
                group.Sum(item =>
                    item.TotalAmountKrw.HasValue && item.TotalAmountKrw > 0m
                        ? item.TotalAmountKrw.Value
                        : item.AuctionPriceKrw!.Value * (item.Quantity ?? 0m)),
                group.Min(item => item.AuctionPriceKrw!.Value / item.UnitWeight!.Value),
                group.Max(item => item.AuctionPriceKrw!.Value / item.UnitWeight!.Value),
                group.Max(item => item.LastSeenAtUtc)));

    private 국내농산물경락가격원천응답? FindSource(string? sourceKey)
        => _sourceService.GetSources().FirstOrDefault(source =>
            string.Equals(source.Key, sourceKey?.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? NormalizeRegionBasisCode(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(
                normalized,
                농산물지역가격비교기준Codes.원산지,
                StringComparison.OrdinalIgnoreCase))
        {
            return 농산물지역가격비교기준Codes.원산지;
        }

        if (string.Equals(
                normalized,
                농산물지역가격비교기준Codes.도매시장,
                StringComparison.OrdinalIgnoreCase))
        {
            return 농산물지역가격비교기준Codes.도매시장;
        }

        return null;
    }

    private static bool TryParseDate(string value, out DateOnly? date)
    {
        var parsed = DateOnly.TryParseExact(
            value.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result);
        date = parsed ? result : null;
        return parsed;
    }

    private static 농산물지역가격비교선택지응답 OptionsFail(
        string statusCode,
        string message)
        => new()
        {
            StatusCode = statusCode,
            ErrorMessage = message,
            Notices = Notices
        };

    private static 농산물지역가격비교응답 ComparisonFail(
        농산물지역가격비교요청 request,
        string statusCode,
        string message)
        => new()
        {
            StatusCode = statusCode,
            ErrorMessage = message,
            Query = request,
            Notices = Notices
        };

    private sealed record DateResolution(
        DateOnly? StartDate,
        DateOnly? EndDate,
        string? ErrorMessage);

    internal sealed record 지역가격Aggregate(
        string RegionCode,
        string RegionName,
        int ObservationCount,
        int TradingDayCount,
        decimal TotalQuantityKg,
        decimal TotalAmountKrw,
        decimal MinimumPriceKrwPerKg,
        decimal MaximumPriceKrwPerKg,
        DateTime LatestCollectedAtUtc);
}
