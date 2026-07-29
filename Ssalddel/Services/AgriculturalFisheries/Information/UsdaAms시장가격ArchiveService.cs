using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IUsdaAms시장가격ArchiveService
{
    Task<UsdaAms시장가격수집응답> CollectAsync(
        UsdaAms시장가격수집요청 request,
        CancellationToken cancellationToken = default);

    Task<UsdaAms시장가격Archive응답> GetArchiveAsync(
        UsdaAms시장가격ArchiveQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class UsdaAms시장가격ArchiveService(
    IUsdaAmsMarketNewsClient client,
    AgriculturalFisheriesDbContext db,
    TimeProvider timeProvider,
    ILogger<UsdaAms시장가격ArchiveService> logger)
    : IUsdaAms시장가격ArchiveService
{
    private static readonly IReadOnlyDictionary<string, MarketMapping> MarketMappings =
        new Dictionary<string, MarketMapping>(StringComparer.OrdinalIgnoreCase)
        {
            ["Shipping Point"] = new(
                농수산시세정보원Keys.UsdaAms산지출하가격,
                농수산시세시장단계Codes.산지출하),
            ["Terminal"] = new(
                농수산시세정보원Keys.UsdaAms도매터미널가격,
                농수산시세시장단계Codes.도매터미널),
            ["Retail - Specialty Crops"] = new(
                농수산시세정보원Keys.UsdaAms소매광고가격,
                농수산시세시장단계Codes.소매광고)
        };

    public async Task<UsdaAms시장가격수집응답> CollectAsync(
        UsdaAms시장가격수집요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (dateFrom, dateTo) = ResolveRange(request, timeProvider.GetUtcNow());
        var requestedMarketTypes = ResolveMarketTypes(request.MarketTypes);
        var startedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var run = new UsdaAms시장가격수집Run
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            RequestedMarketTypesJson = JsonSerializer.Serialize(requestedMarketTypes),
            SourceUrl = "https://marsapi.ams.usda.gov/services/v1.2/reports",
            StartedAtUtc = startedAtUtc
        };
        db.UsdaAmsMarketPriceCollectionRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        var messages = new List<string>();

        try
        {
            var reports = (await client.GetReportsAsync(cancellationToken))
                .Select(report => new
                {
                    Report = report,
                    MarketType = report.MarketTypes.FirstOrDefault(type =>
                        requestedMarketTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                })
                .Where(item =>
                    item.MarketType is not null
                    && item.Report.LatestReportDate >= dateFrom
                    && !item.Report.ReportTitle.Contains(
                        "Ornamental",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.MarketType, StringComparer.Ordinal)
                .ThenBy(item => item.Report.SlugId, StringComparer.Ordinal)
                .ToArray();
            run.DiscoveredReportCount = reports.Length;
            await db.SaveChangesAsync(cancellationToken);

            foreach (var item in reports)
            {
                var reportEnd = item.Report.LatestReportDate < dateTo
                    ? item.Report.LatestReportDate
                    : dateTo;
                foreach (var (sliceFrom, sliceTo) in EnumerateMonthlySlices(
                             dateFrom,
                             reportEnd))
                {
                    await CollectSliceRecursiveAsync(
                        run,
                        item.Report,
                        item.MarketType!,
                        sliceFrom,
                        sliceTo,
                        startedAtUtc,
                        cancellationToken);
                }
            }

            run.StatusCode = UsdaAms시장가격Archive상태Codes.완료;
            run.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            messages.Add(
                $"USDA AMS 활성 식품 보고서 {run.DiscoveredReportCount:N0}개를 보고서·월 단위로 수집했습니다.");
            messages.Add(
                "산지 출하·도매 터미널·소매 광고 시장 단계를 분리하고 원 포장·등급·가격범위를 보존했습니다.");
            run.SourceMessagesJson = JsonSerializer.Serialize(messages);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "USDA AMS 시장가격 수집 완료. RunId={RunId}, Range={DateFrom}-{DateTo}, Reports={Reports}, Slices={Slices}, Fetched={Fetched}, Inserted={Inserted}, Existing={Existing}",
                run.Id,
                dateFrom,
                dateTo,
                run.DiscoveredReportCount,
                run.CompletedSliceCount,
                run.FetchedCount,
                run.InsertedCount,
                run.ExistingCount);
            return ToResponse(run, messages);
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException
            || !cancellationToken.IsCancellationRequested)
        {
            run.StatusCode = UsdaAms시장가격Archive상태Codes.실패;
            run.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            run.ErrorMessage = Clip(exception.Message, 2000);
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<UsdaAms시장가격Archive응답> GetArchiveAsync(
        UsdaAms시장가격ArchiveQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var observations = db.UsdaAmsMarketPriceObservations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.SourceKey))
        {
            var value = query.SourceKey.Trim();
            observations = observations.Where(item => item.SourceKey == value);
        }

        if (!string.IsNullOrWhiteSpace(query.MarketType))
        {
            var value = query.MarketType.Trim();
            observations = observations.Where(item => item.MarketType == value);
        }

        if (!string.IsNullOrWhiteSpace(query.Commodity))
        {
            var value = query.Commodity.Trim();
            observations = observations.Where(item => item.Commodity.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(query.Variety))
        {
            var value = query.Variety.Trim();
            observations = observations.Where(item => item.Variety.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(query.MarketLocationState))
        {
            var value = query.MarketLocationState.Trim();
            observations = observations.Where(item => item.MarketLocationState == value);
        }

        if (!string.IsNullOrWhiteSpace(query.Origin))
        {
            var value = query.Origin.Trim();
            observations = observations.Where(item => item.Origin.Contains(value));
        }

        if (query.Year.HasValue)
        {
            var from = new DateOnly(query.Year.Value, 1, 1);
            var to = from.AddYears(1);
            observations = observations.Where(item =>
                item.ReportBeginDate >= from && item.ReportBeginDate < to);
        }

        var take = Math.Clamp(query.Take, 1, 500);
        var items = await observations
            .OrderByDescending(item => item.ReportBeginDate)
            .ThenBy(item => item.Commodity)
            .ThenBy(item => item.MarketLocationName)
            .Take(take)
            .Select(item => new UsdaAms시장가격관측응답(
                item.RecordKey,
                item.SourceKey,
                item.MarketStageCode,
                item.SlugId,
                item.SlugName,
                item.ReportTitle,
                item.ReportBeginDate,
                item.ReportEndDate,
                item.PublishedDateRaw,
                item.OfficeName,
                item.OfficeState,
                item.MarketType,
                item.MarketLocationName,
                item.MarketLocationState,
                item.Commodity,
                item.Variety,
                item.Package,
                item.UnitSales,
                item.ItemSize,
                item.Grade,
                item.Quality,
                item.Organic,
                item.Origin,
                item.District,
                item.LowPrice,
                item.HighPrice,
                item.MostlyLowPrice,
                item.MostlyHighPrice,
                item.WeightedAveragePrice,
                item.StoreCount,
                item.CurrencyCode,
                item.OriginalUnit,
                item.FirstCollectedAtUtc,
                item.LastSeenAtUtc))
            .ToArrayAsync(cancellationToken);
        return new UsdaAms시장가격Archive응답(
            items.Length == 0
                ? UsdaAms시장가격상태Codes.자료없음
                : UsdaAms시장가격상태Codes.완료,
            timeProvider.GetUtcNow().UtcDateTime,
            items.Length,
            items,
            [
                "산지 출하·도매 터미널·소매 광고 가격은 서로 다른 시장 단계이므로 직접 평균하지 않습니다.",
                "포장·품종·등급·크기·원산지·지역이 일치하기 전에는 KAMIS와 가격 차액을 계산하지 않습니다.",
                "소매 자료는 실제 결제 평균이 아니라 광고·프로모션 가격입니다."
            ]);
    }

    private async Task CollectSliceRecursiveAsync(
        UsdaAms시장가격수집Run run,
        UsdaAms보고서Descriptor report,
        string marketType,
        DateOnly dateFrom,
        DateOnly dateTo,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var slice = await client.GetReportDetailsAsync(
            report.SlugId,
            dateFrom,
            dateTo,
            cancellationToken);
        if (slice.TotalRows > slice.ReturnedRows)
        {
            if (dateFrom >= dateTo)
            {
                throw new InvalidOperationException(
                    $"USDA AMS 보고서 {report.SlugId}의 {dateFrom:yyyy-MM-dd} 하루 자료가 API 허용 행 수를 초과했습니다.");
            }

            var days = dateTo.DayNumber - dateFrom.DayNumber;
            var leftTo = dateFrom.AddDays(days / 2);
            await CollectSliceRecursiveAsync(
                run,
                report,
                marketType,
                dateFrom,
                leftTo,
                collectedAtUtc,
                cancellationToken);
            await CollectSliceRecursiveAsync(
                run,
                report,
                marketType,
                leftTo.AddDays(1),
                dateTo,
                collectedAtUtc,
                cancellationToken);
            return;
        }

        var mapping = MarketMappings[marketType];
        var incoming = slice.Rows
            .Select(row => Map(row, report, mapping, collectedAtUtc))
            .Where(item =>
                item.ReportBeginDate >= dateFrom
                && item.ReportBeginDate <= dateTo)
            .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToArray();
        var keys = incoming.Select(item => item.RecordKey).ToArray();
        var existing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var keyBatch in keys.Chunk(1000))
        {
            var batchKeys = keyBatch.ToList();
            var found = await db.UsdaAmsMarketPriceObservations
                .AsNoTracking()
                .Where(item => batchKeys.Contains(item.RecordKey))
                .Select(item => item.RecordKey)
                .ToArrayAsync(cancellationToken);
            existing.UnionWith(found);
        }

        var inserted = incoming
            .Where(item => !existing.Contains(item.RecordKey))
            .ToArray();
        foreach (var item in inserted)
        {
            item.FirstCollectionRunId = run.Id;
        }

        db.UsdaAmsMarketPriceObservations.AddRange(inserted);
        run.CompletedSliceCount++;
        run.FetchedCount += incoming.LongLength;
        run.InsertedCount += inserted.LongLength;
        run.ExistingCount += incoming.LongLength - inserted.LongLength;
        var latest = incoming
            .Select(item => (DateOnly?)item.ReportBeginDate)
            .DefaultIfEmpty()
            .Max();
        if (latest.HasValue
            && (!run.LatestReferenceDate.HasValue
                || latest.Value > run.LatestReferenceDate.Value))
        {
            run.LatestReferenceDate = latest;
        }

        await UpdateYearCommodityCatalogAsync(
            incoming,
            collectedAtUtc,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        foreach (var entry in db.ChangeTracker.Entries<UsdaAms시장가격관측>())
        {
            entry.State = EntityState.Detached;
        }
    }

    private async Task UpdateYearCommodityCatalogAsync(
        IReadOnlyList<UsdaAms시장가격관측> incoming,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var observed = incoming
            .Where(item =>
                item.ReportBeginDate != default
                && !string.IsNullOrWhiteSpace(item.Commodity)
                && !string.Equals(
                    item.Commodity,
                    "N/A",
                    StringComparison.OrdinalIgnoreCase))
            .GroupBy(item => item.ReportBeginDate.Year)
            .SelectMany(yearGroup => yearGroup
                .GroupBy(
                    item => item.Commodity,
                    StringComparer.OrdinalIgnoreCase)
                .Select(commodityGroup => new
                {
                    Year = yearGroup.Key,
                    Commodity = commodityGroup.Key,
                    FirstObservedDate = commodityGroup.Min(item =>
                        item.ReportBeginDate),
                    LastObservedDate = commodityGroup.Max(item =>
                        item.ReportBeginDate)
                }))
            .ToArray();
        if (observed.Length == 0)
        {
            return;
        }

        var years = observed
            .Select(item => item.Year)
            .Distinct()
            .ToList();
        var commodities = observed
            .Select(item => item.Commodity)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var existing = await db.UsdaAmsYearCommodityCatalog
            .Where(item =>
                years.Contains(item.Year)
                && commodities.Contains(item.Commodity))
            .ToArrayAsync(cancellationToken);
        var existingByKey = existing.ToDictionary(
            item => CatalogKey(item.Year, item.Commodity),
            StringComparer.OrdinalIgnoreCase);
        foreach (var item in observed)
        {
            var key = CatalogKey(item.Year, item.Commodity);
            if (!existingByKey.TryGetValue(key, out var catalog))
            {
                catalog = new UsdaAms연도상품Catalog
                {
                    Year = item.Year,
                    Commodity = item.Commodity,
                    FirstObservedDate = item.FirstObservedDate,
                    LastObservedDate = item.LastObservedDate,
                    UpdatedAtUtc = collectedAtUtc
                };
                db.UsdaAmsYearCommodityCatalog.Add(catalog);
                existingByKey.Add(key, catalog);
                continue;
            }

            var changed = false;
            if (item.FirstObservedDate < catalog.FirstObservedDate)
            {
                catalog.FirstObservedDate = item.FirstObservedDate;
                changed = true;
            }

            if (item.LastObservedDate > catalog.LastObservedDate)
            {
                catalog.LastObservedDate = item.LastObservedDate;
                changed = true;
            }

            if (changed)
            {
                catalog.UpdatedAtUtc = collectedAtUtc;
            }
        }
    }

    private static string CatalogKey(int year, string commodity)
        => $"{year}\u001f{commodity}";

    private static UsdaAms시장가격관측 Map(
        UsdaAms시장가격Row row,
        UsdaAms보고서Descriptor report,
        MarketMapping mapping,
        DateTime collectedAtUtc)
    {
        var reportBeginDate = UsdaAmsMarketNewsClient.ParseDate(row.ReportBeginDate);
        var reportEndDate = UsdaAmsMarketNewsClient.ParseDate(row.ReportEndDate);
        var marketLocationName = string.IsNullOrWhiteSpace(row.MarketLocationName)
            ? row.Region
            : row.MarketLocationName;
        var originalUnit = string.Join(
            " | ",
            new[]
            {
                Label("package", row.Package),
                Label("unit sales", row.UnitSales),
                Label("item size", row.ItemSize)
            }.Where(value => value.Length > 0));
        if (originalUnit.Length == 0)
        {
            originalUnit = "published item unit";
        }

        return new UsdaAms시장가격관측
        {
            RecordKey = Sha256($"{report.SlugId}|Report Details|{row.RawJson}"),
            SourceKey = mapping.SourceKey,
            MarketStageCode = mapping.MarketStageCode,
            SlugId = Clip(report.SlugId, 20),
            SlugName = Clip(
                string.IsNullOrWhiteSpace(row.SlugName) ? report.SlugName : row.SlugName,
                80),
            ReportTitle = Clip(
                string.IsNullOrWhiteSpace(row.ReportTitle)
                    ? report.ReportTitle
                    : row.ReportTitle,
                500),
            ReportBeginDate = reportBeginDate,
            ReportEndDate = reportEndDate == default ? reportBeginDate : reportEndDate,
            PublishedDateRaw = Clip(row.PublishedDate, 50),
            OfficeName = Clip(row.OfficeName, 120),
            OfficeState = Clip(row.OfficeState, 20),
            OfficeCity = Clip(row.OfficeCity, 120),
            MarketType = Clip(row.MarketType, 80),
            MarketLocationName = Clip(marketLocationName, 200),
            MarketLocationState = Clip(row.MarketLocationState, 20),
            MarketLocationCity = Clip(row.MarketLocationCity, 120),
            Community = Clip(row.Community, 80),
            Group = Clip(row.Group, 160),
            Category = Clip(row.Category, 160),
            Commodity = Clip(row.Commodity, 200),
            Variety = Clip(row.Variety, 300),
            Repack = Clip(row.Repack, 80),
            Package = Clip(row.Package, 200),
            Storage = Clip(row.Storage, 120),
            TransportationMode = Clip(row.TransportationMode, 120),
            Grade = Clip(row.Grade, 160),
            UnitSales = Clip(row.UnitSales, 160),
            ItemSize = Clip(row.ItemSize, 300),
            Appearance = Clip(row.Appearance, 160),
            Quality = Clip(row.Quality, 160),
            Condition = Clip(row.Condition, 160),
            Organic = Clip(row.Organic, 50),
            Crop = Clip(row.Crop, 120),
            Origin = Clip(row.Origin, 200),
            District = Clip(row.District, 200),
            Environment = Clip(row.Environment, 120),
            LowPrice = ParseDecimal(row.LowPrice),
            HighPrice = ParseDecimal(row.HighPrice),
            MostlyLowPrice = ParseDecimal(row.MostlyLowPrice),
            MostlyHighPrice = ParseDecimal(row.MostlyHighPrice),
            WeightedAveragePrice = ParseDecimal(row.WeightedAveragePrice),
            StoreCount = int.TryParse(
                row.StoreCount,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var storeCount)
                ? storeCount
                : null,
            OriginalUnit = Clip(originalUnit, 500),
            RawJson = row.RawJson,
            FirstCollectedAtUtc = collectedAtUtc,
            LastSeenAtUtc = collectedAtUtc
        };
    }

    private static IReadOnlyList<string> ResolveMarketTypes(
        IReadOnlyList<string> requested)
    {
        var values = requested
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (values.Length == 0)
        {
            return MarketMappings.Keys.ToArray();
        }

        var unsupported = values
            .Where(value => !MarketMappings.ContainsKey(value))
            .ToArray();
        if (unsupported.Length > 0)
        {
            throw new ArgumentException(
                $"지원하지 않는 USDA AMS market type입니다: {string.Join(", ", unsupported)}");
        }

        return values;
    }

    private static (DateOnly DateFrom, DateOnly DateTo) ResolveRange(
        UsdaAms시장가격수집요청 request,
        DateTimeOffset now)
    {
        var year = request.Year <= 0 ? now.Year : request.Year;
        if (year is < 2000 or > 2100)
        {
            throw new ArgumentException("수집 연도는 2000~2100 범위여야 합니다.");
        }

        var dateFrom = new DateOnly(year, 1, 1);
        var yearEnd = new DateOnly(year, 12, 31);
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var defaultTo = year == today.Year && today < yearEnd ? today : yearEnd;
        var dateTo = string.IsNullOrWhiteSpace(request.DateTo)
            ? defaultTo
            : UsdaAmsMarketNewsClient.ParseDate(request.DateTo);
        if (dateTo == default
            || dateTo.Year != year
            || dateTo < dateFrom
            || dateTo > today)
        {
            throw new ArgumentException(
                "수집 종료일은 해당 연도 안의 오늘 이전 날짜여야 합니다.");
        }

        return (dateFrom, dateTo);
    }

    private static IEnumerable<(DateOnly From, DateOnly To)> EnumerateMonthlySlices(
        DateOnly dateFrom,
        DateOnly dateTo)
    {
        if (dateTo < dateFrom)
        {
            yield break;
        }

        var current = dateFrom;
        while (current <= dateTo)
        {
            var monthEnd = new DateOnly(
                current.Year,
                current.Month,
                DateTime.DaysInMonth(current.Year, current.Month));
            var sliceTo = monthEnd < dateTo ? monthEnd : dateTo;
            yield return (current, sliceTo);
            current = sliceTo.AddDays(1);
        }
    }

    private static decimal? ParseDecimal(string value)
    {
        var normalized = value
            .Replace("$", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Trim();
        return decimal.TryParse(
            normalized,
            NumberStyles.Number | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : null;
    }

    private static string Label(string name, string value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : $"{name}: {value.Trim()}";

    private static string Sha256(string value)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string Clip(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];

    private static UsdaAms시장가격수집응답 ToResponse(
        UsdaAms시장가격수집Run run,
        IReadOnlyList<string> messages)
        => new(
            run.Id,
            UsdaAms시장가격상태Codes.완료,
            run.DateFrom,
            run.DateTo,
            run.DiscoveredReportCount,
            run.CompletedSliceCount,
            run.FetchedCount,
            run.InsertedCount,
            run.ExistingCount,
            run.LatestReferenceDate,
            messages);

    private sealed record MarketMapping(
        string SourceKey,
        string MarketStageCode);
}
