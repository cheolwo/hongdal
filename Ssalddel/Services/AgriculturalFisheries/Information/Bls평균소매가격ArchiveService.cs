using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IBls평균소매가격ArchiveService
{
    IReadOnlyList<Bls평균소매가격Series응답> GetSeriesCatalog();

    BlsKamis비교Catalog응답 GetKamisComparisonCatalog();

    Task<Bls평균소매가격수집응답> CollectAsync(
        Bls평균소매가격수집요청 request,
        CancellationToken cancellationToken = default);

    Task<Bls평균소매가격Archive응답> GetArchiveAsync(
        Bls평균소매가격ArchiveQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class Bls평균소매가격ArchiveService : IBls평균소매가격ArchiveService
{
    private const int Version1SeriesPerRequestLimit = 25;
    private const int FredSeriesPerRequestLimit = 10;

    private readonly HttpClient _httpClient;
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<Bls평균소매가격ArchiveService> _logger;

    public Bls평균소매가격ArchiveService(
        HttpClient httpClient,
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider,
        ILogger<Bls평균소매가격ArchiveService> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public IReadOnlyList<Bls평균소매가격Series응답> GetSeriesCatalog()
        => Bls평균소매가격SeriesCatalog.ToResponse();

    public BlsKamis비교Catalog응답 GetKamisComparisonCatalog()
        => Bls평균소매가격SeriesCatalog.ToKamisComparisonResponse();

    public async Task<Bls평균소매가격수집응답> CollectAsync(
        Bls평균소매가격수집요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var currentYear = _timeProvider.GetUtcNow().Year;
        var (yearFrom, yearTo) = ResolveYears(request, currentYear);
        var collectedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var series = Bls평균소매가격SeriesCatalog.All;
        var run = new Bls평균소매가격수집Run
        {
            YearFrom = yearFrom,
            YearTo = yearTo,
            RequestedSeriesCount = series.Count,
            QuerySummary =
                $"BLS CPI Average Price / U.S. city average / food / monthly / {yearFrom}-{yearTo}",
            SourceUrl = Bls평균소매가격SeriesCatalog.SourceUrl,
            StartedAtUtc = collectedAtUtc
        };
        _db.BlsAverageRetailPriceCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);
        var sourceMessages = new List<string>();

        try
        {
            var fetchedObservations = new List<Bls평균소매가격관측>();
            var shouldUseFredFallback = false;
            foreach (var seriesBatch in series.Chunk(Version1SeriesPerRequestLimit))
            {
                var payload = new
                {
                    seriesid = seriesBatch.Select(item => item.SeriesId).ToArray(),
                    startyear = yearFrom.ToString(CultureInfo.InvariantCulture),
                    endyear = yearTo.ToString(CultureInfo.InvariantCulture)
                };
                using var response = await _httpClient.PostAsJsonAsync(
                    "publicAPI/v1/timeseries/data/",
                    payload,
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream =
                    await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken);
                var batchMessages = ReadStringArray(document.RootElement, "message");
                sourceMessages.AddRange(batchMessages);
                var status = ReadString(document.RootElement, "status");
                if (!string.Equals(status, "REQUEST_SUCCEEDED", StringComparison.Ordinal))
                {
                    if (IsUnregisteredDailyLimitReached(batchMessages))
                    {
                        shouldUseFredFallback = true;
                        break;
                    }

                    throw new InvalidOperationException(
                        batchMessages.Count == 0
                            ? $"BLS API가 수집 요청을 거부했습니다. Status={status}"
                            : $"BLS API가 수집 요청을 거부했습니다: {string.Join(" | ", batchMessages)}");
                }

                fetchedObservations.AddRange(
                    ReadObservations(document.RootElement, collectedAtUtc));
            }

            if (shouldUseFredFallback)
            {
                fetchedObservations.Clear();
                fetchedObservations.AddRange(await ReadFredObservationsAsync(
                    series,
                    yearFrom,
                    yearTo,
                    collectedAtUtc,
                    cancellationToken));
                sourceMessages.Add(
                    "BLS API v1 무등록 일일 한도 도달로 FRED CSV를 사용했습니다. 각 계열의 원자료 출처는 U.S. Bureau of Labor Statistics입니다.");
                run.QuerySummary += " / FRED CSV transparent fallback";
                run.SourceUrl = Bls평균소매가격SeriesCatalog.FredGraphCsvUrl;
            }

            var normalizedSourceMessages = sourceMessages
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var incoming = fetchedObservations
                .Where(item =>
                    item.ReferenceMonth.Year >= yearFrom
                    && item.ReferenceMonth.Year <= yearTo)
                .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                .Select(group => group.Last())
                .ToArray();
            var recordKeys = incoming
                .Select(item => item.RecordKey)
                .ToHashSet(StringComparer.Ordinal);
            var existing = await _db.BlsAverageRetailPriceObservations
                .Where(item => recordKeys.Contains(item.RecordKey))
                .ToDictionaryAsync(
                    item => item.RecordKey,
                    StringComparer.Ordinal,
                    cancellationToken);

            var insertedCount = 0;
            var updatedCount = 0;
            var existingCount = 0;
            foreach (var item in incoming)
            {
                if (!existing.TryGetValue(item.RecordKey, out var stored))
                {
                    item.FirstCollectionRunId = run.Id;
                    _db.BlsAverageRetailPriceObservations.Add(item);
                    insertedCount++;
                    continue;
                }

                stored.LastSeenAtUtc = collectedAtUtc;
                if (HasBusinessChanges(stored, item))
                {
                    ApplyBusinessChanges(stored, item);
                    updatedCount++;
                }
                else
                {
                    existingCount++;
                }
            }

            run.StatusCode = Bls평균소매가격Archive상태Codes.완료;
            run.CompletedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            run.LatestReferenceMonth = incoming
                .Select(item => (DateOnly?)item.ReferenceMonth)
                .DefaultIfEmpty()
                .Max();
            run.FetchedCount = incoming.Length;
            run.InsertedCount = insertedCount;
            run.UpdatedCount = updatedCount;
            run.ExistingCount = existingCount;
            run.SourceMessagesJson = JsonSerializer.Serialize(normalizedSourceMessages);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "BLS 평균 소매가격 수집 완료. RunId={RunId}, Years={YearFrom}-{YearTo}, Series={SeriesCount}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}",
                run.Id,
                yearFrom,
                yearTo,
                series.Count,
                incoming.Length,
                insertedCount,
                updatedCount,
                existingCount);

            return new Bls평균소매가격수집응답(
                run.Id,
                Bls평균소매가격상태Codes.완료,
                yearFrom,
                yearTo,
                series.Count,
                incoming.Length,
                insertedCount,
                updatedCount,
                existingCount,
                run.LatestReferenceMonth,
                normalizedSourceMessages);
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException
            || !cancellationToken.IsCancellationRequested)
        {
            ResetObservationChanges();
            run.StatusCode = Bls평균소매가격Archive상태Codes.실패;
            run.CompletedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            run.SourceMessagesJson = JsonSerializer.Serialize(sourceMessages);
            run.ErrorMessage = Truncate(exception.Message, 2000);
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<Bls평균소매가격Archive응답> GetArchiveAsync(
        Bls평균소매가격ArchiveQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.YearFrom.HasValue
            && query.YearTo.HasValue
            && query.YearFrom.Value > query.YearTo.Value)
        {
            throw new ArgumentException("조회 시작 연도는 종료 연도보다 늦을 수 없습니다.");
        }

        var observations = _db.BlsAverageRetailPriceObservations.AsNoTracking();
        var seriesId = query.SeriesId?.Trim();
        if (!string.IsNullOrWhiteSpace(seriesId))
        {
            observations = observations.Where(item => item.SeriesId == seriesId);
        }

        var productKey = query.CanonicalProductKey?.Trim();
        if (!string.IsNullOrWhiteSpace(productKey))
        {
            observations = observations.Where(item => item.CanonicalProductKey == productKey);
        }

        if (query.YearFrom.HasValue)
        {
            var from = new DateOnly(query.YearFrom.Value, 1, 1);
            observations = observations.Where(item => item.ReferenceMonth >= from);
        }

        if (query.YearTo.HasValue)
        {
            var toExclusive = new DateOnly(query.YearTo.Value, 1, 1).AddYears(1);
            observations = observations.Where(item => item.ReferenceMonth < toExclusive);
        }

        var take = Math.Clamp(query.Take, 1, 500);
        var items = await observations
            .OrderByDescending(item => item.ReferenceMonth)
            .ThenBy(item => item.SeriesId)
            .Take(take)
            .Select(item => new Bls평균소매가격관측응답(
                item.RecordKey,
                item.SeriesId,
                item.ItemCode,
                item.CanonicalProductKey,
                item.ProductNameKo,
                item.ItemNameEn,
                item.AreaCode,
                item.AreaName,
                item.ReferenceMonth,
                item.PeriodCode,
                item.PeriodName,
                item.ValueRaw,
                item.PriceUsd,
                item.CurrencyCode,
                item.OriginalUnit,
                item.IsValueMissing,
                item.Footnote,
                item.SourceUrl,
                item.FirstCollectedAtUtc,
                item.LastSeenAtUtc))
            .ToArrayAsync(cancellationToken);

        return new Bls평균소매가격Archive응답(
            items.Length == 0
                ? Bls평균소매가격상태Codes.자료없음
                : Bls평균소매가격상태Codes.완료,
            _timeProvider.GetUtcNow().UtcDateTime,
            items.Length,
            items,
            "U.S. Bureau of Labor Statistics CPI Average Price Data API v1 또는 FRED가 배포하는 동일 BLS 계열 CSV에서 서버가 수집·보관한 월평균 소비자 소매가격입니다.",
            [
                "전국 도시 평균이며 특정 매장·도시의 판매가격이나 구매 견적이 아닙니다.",
                "원 거래단위와 미국 달러 가격을 보존하며 근거 없는 kg·원화 환산을 제공하지 않습니다.",
                "BLS API 무등록 일일 한도에 도달하면 FRED가 배포하는 동일 BLS Series ID의 CSV를 사용하고 관측별 SourceUrl과 실행 메시지에 이를 표시합니다.",
                "2026년 관측이 확인된 전국 식품 계열만 포함하고 중단된 계열은 최신값으로 취급하지 않습니다."
            ]);
    }

    private static (int YearFrom, int YearTo) ResolveYears(
        Bls평균소매가격수집요청 request,
        int currentYear)
    {
        var yearFrom = request.YearFrom == 0 ? currentYear : request.YearFrom;
        var yearTo = request.YearTo == 0 ? yearFrom : request.YearTo;
        if (yearFrom is < 1980 or > 2100
            || yearTo is < 1980 or > 2100
            || yearFrom > yearTo)
        {
            throw new ArgumentException("BLS 수집 연도 범위를 확인해 주세요.");
        }

        if (yearTo - yearFrom > 9)
        {
            throw new ArgumentException("BLS API v1은 한 요청에서 최대 10년까지만 수집합니다.");
        }

        if (yearTo > currentYear)
        {
            throw new ArgumentException("BLS 수집 종료 연도는 현재 연도보다 늦을 수 없습니다.");
        }

        return (yearFrom, yearTo);
    }

    private static IReadOnlyList<Bls평균소매가격관측> ReadObservations(
        JsonElement root,
        DateTime collectedAtUtc)
    {
        if (!TryGetProperty(root, "Results", out var results)
            || results.ValueKind != JsonValueKind.Object
            || !TryGetProperty(results, "series", out var seriesArray)
            || seriesArray.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("BLS 응답에 Results.series 배열이 없습니다.");
        }

        var observations = new List<Bls평균소매가격관측>();
        foreach (var seriesElement in seriesArray.EnumerateArray())
        {
            var seriesId = ReadString(seriesElement, "seriesID");
            var definition = Bls평균소매가격SeriesCatalog.Find(seriesId);
            if (definition is null
                || !TryGetProperty(seriesElement, "data", out var data)
                || data.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in data.EnumerateArray())
            {
                var yearText = ReadString(item, "year");
                var periodCode = ReadString(item, "period");
                if (!int.TryParse(
                        yearText,
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out var year)
                    || !TryParseMonth(periodCode, out var month))
                {
                    continue;
                }

                var valueRaw = ReadString(item, "value");
                var price = decimal.TryParse(
                    valueRaw,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var parsedPrice)
                    ? parsedPrice
                    : (decimal?)null;
                observations.Add(new Bls평균소매가격관측
                {
                    RecordKey = $"bls-ap:{definition.SeriesId}:{year}:{periodCode}",
                    SeriesId = definition.SeriesId,
                    ItemCode = definition.ItemCode,
                    CanonicalProductKey = definition.CanonicalProductKey,
                    ProductNameKo = definition.ProductNameKo,
                    ItemNameEn = definition.ItemNameEn,
                    ReferenceMonth = new DateOnly(year, month, 1),
                    PeriodCode = periodCode,
                    PeriodName = ReadString(item, "periodName"),
                    ValueRaw = valueRaw,
                    PriceUsd = price,
                    OriginalUnit = definition.OriginalUnit,
                    IsValueMissing = !price.HasValue,
                    Footnote = ReadFootnotes(item),
                    SourceUrl = Bls평균소매가격SeriesCatalog.SourceUrl,
                    RawJson = item.GetRawText(),
                    FirstCollectedAtUtc = collectedAtUtc,
                    LastSeenAtUtc = collectedAtUtc
                });
            }
        }

        return observations;
    }

    private async Task<IReadOnlyList<Bls평균소매가격관측>> ReadFredObservationsAsync(
        IReadOnlyList<Bls평균소매가격SeriesDefinition> series,
        int yearFrom,
        int yearTo,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var observations = new List<Bls평균소매가격관측>();
        foreach (var seriesBatch in series.Chunk(FredSeriesPerRequestLimit))
        {
            observations.AddRange(await ReadFredBatchObservationsAsync(
                seriesBatch,
                yearFrom,
                yearTo,
                collectedAtUtc,
                cancellationToken));
        }

        return observations;
    }

    private async Task<IReadOnlyList<Bls평균소매가격관측>>
        ReadFredBatchObservationsAsync(
            IReadOnlyList<Bls평균소매가격SeriesDefinition> series,
            int yearFrom,
            int yearTo,
            DateTime collectedAtUtc,
            CancellationToken cancellationToken)
    {
        var requestUrl =
            $"{Bls평균소매가격SeriesCatalog.FredGraphCsvUrl}?id={string.Join(',', series.Select(item => item.SeriesId))}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl)
        {
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };
        request.Headers.UserAgent.ParseAdd(
            "curl/8.0 Ssalddel-FRED-CSV-Collector/0.0");
        request.Headers.Accept.ParseAdd("text/csv");
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidOperationException("FRED CSV 응답에 header가 없습니다.");
        }

        var headers = headerLine.Split(',');
        if (headers.Length < 2
            || !string.Equals(
                headers[0].Trim(),
                "observation_date",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "FRED CSV 응답의 observation_date header를 확인할 수 없습니다.");
        }

        var requestedSeriesIds = series
            .Select(item => item.SeriesId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var definitionsByColumn = headers
            .Select((header, index) => new
            {
                Index = index,
                Definition = Bls평균소매가격SeriesCatalog.Find(header.Trim())
            })
            .Where(item =>
                item.Index > 0
                && item.Definition is not null
                && requestedSeriesIds.Contains(item.Definition.SeriesId))
            .Select(item => (item.Index, Definition: item.Definition!))
            .ToArray();
        if (definitionsByColumn.Length == 0)
        {
            throw new InvalidOperationException(
                "FRED CSV 응답에 요청한 BLS Series ID가 없습니다.");
        }

        var returnedSeriesIds = definitionsByColumn
            .Select(item => item.Definition.SeriesId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingSeriesIds = series
            .Select(item => item.SeriesId)
            .Where(seriesId => !returnedSeriesIds.Contains(seriesId))
            .ToArray();
        if (missingSeriesIds.Length > 0)
        {
            throw new InvalidOperationException(
                $"FRED CSV 응답에서 요청 계열 {missingSeriesIds.Length}개가 누락되었습니다: {string.Join(", ", missingSeriesIds)}");
        }

        var observations = new List<Bls평균소매가격관측>();
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split(',');
            if (columns.Length == 0
                || !DateOnly.TryParseExact(
                    columns[0].Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var referenceMonth)
                || referenceMonth.Year < yearFrom
                || referenceMonth.Year > yearTo)
            {
                continue;
            }

            foreach (var (index, definition) in definitionsByColumn)
            {
                var valueRaw = index < columns.Length
                    ? columns[index].Trim()
                    : string.Empty;
                var price = decimal.TryParse(
                    valueRaw,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var parsedPrice)
                    ? parsedPrice
                    : (decimal?)null;
                var periodCode = $"M{referenceMonth.Month:00}";
                observations.Add(new Bls평균소매가격관측
                {
                    RecordKey =
                        $"bls-ap:{definition.SeriesId}:{referenceMonth.Year}:{periodCode}",
                    SeriesId = definition.SeriesId,
                    ItemCode = definition.ItemCode,
                    CanonicalProductKey = definition.CanonicalProductKey,
                    ProductNameKo = definition.ProductNameKo,
                    ItemNameEn = definition.ItemNameEn,
                    ReferenceMonth = new DateOnly(
                        referenceMonth.Year,
                        referenceMonth.Month,
                        1),
                    PeriodCode = periodCode,
                    PeriodName = CultureInfo.InvariantCulture.DateTimeFormat
                        .GetMonthName(referenceMonth.Month),
                    ValueRaw = valueRaw,
                    PriceUsd = price,
                    OriginalUnit = definition.OriginalUnit,
                    IsValueMissing = !price.HasValue,
                    SourceUrl =
                        $"https://fred.stlouisfed.org/series/{definition.SeriesId}",
                    RawJson = JsonSerializer.Serialize(new
                    {
                        observation_date = referenceMonth.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture),
                        series_id = definition.SeriesId,
                        value = valueRaw,
                        retrieval_source =
                            Bls평균소매가격SeriesCatalog.FredGraphCsvUrl,
                        original_source = "U.S. Bureau of Labor Statistics"
                    }),
                    FirstCollectedAtUtc = collectedAtUtc,
                    LastSeenAtUtc = collectedAtUtc
                });
            }
        }

        return observations;
    }

    private static bool IsUnregisteredDailyLimitReached(
        IReadOnlyList<string> sourceMessages)
        => sourceMessages.Any(message =>
            message.Contains("daily threshold", StringComparison.OrdinalIgnoreCase)
            && message.Contains("registration key", StringComparison.OrdinalIgnoreCase));

    private static bool HasBusinessChanges(
        Bls평균소매가격관측 stored,
        Bls평균소매가격관측 incoming)
        => stored.ValueRaw != incoming.ValueRaw
           || stored.PriceUsd != incoming.PriceUsd
           || stored.IsValueMissing != incoming.IsValueMissing
           || stored.Footnote != incoming.Footnote
           || stored.ItemNameEn != incoming.ItemNameEn
           || stored.ProductNameKo != incoming.ProductNameKo
           || stored.OriginalUnit != incoming.OriginalUnit;

    private static void ApplyBusinessChanges(
        Bls평균소매가격관측 stored,
        Bls평균소매가격관측 incoming)
    {
        stored.ItemCode = incoming.ItemCode;
        stored.CanonicalProductKey = incoming.CanonicalProductKey;
        stored.ProductNameKo = incoming.ProductNameKo;
        stored.ItemNameEn = incoming.ItemNameEn;
        stored.PeriodName = incoming.PeriodName;
        stored.ValueRaw = incoming.ValueRaw;
        stored.PriceUsd = incoming.PriceUsd;
        stored.OriginalUnit = incoming.OriginalUnit;
        stored.IsValueMissing = incoming.IsValueMissing;
        stored.Footnote = incoming.Footnote;
        stored.SourceUrl = incoming.SourceUrl;
        stored.RawJson = incoming.RawJson;
    }

    private void ResetObservationChanges()
    {
        foreach (var entry in _db.ChangeTracker.Entries<Bls평균소매가격관측>())
        {
            entry.State = entry.State == EntityState.Added
                ? EntityState.Detached
                : EntityState.Unchanged;
        }
    }

    private static bool TryParseMonth(string periodCode, out int month)
    {
        month = 0;
        return periodCode.Length == 3
               && periodCode[0] == 'M'
               && int.TryParse(
                   periodCode.AsSpan(1),
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out month)
               && month is >= 1 and <= 12;
    }

    private static string ReadFootnotes(JsonElement item)
    {
        if (!TryGetProperty(item, "footnotes", out var footnotes)
            || footnotes.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        return string.Join(
            " | ",
            footnotes
                .EnumerateArray()
                .Select(footnote => string.Join(
                    ": ",
                    new[]
                    {
                        ReadString(footnote, "code"),
                        ReadString(footnote, "text")
                    }.Where(value => !string.IsNullOrWhiteSpace(value))))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal));
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var values)
            || values.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return values
            .EnumerateArray()
            .Select(value => value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim() ?? string.Empty
                : value.GetRawText())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static string ReadString(JsonElement element, string propertyName)
        => TryGetProperty(element, propertyName, out var value)
            ? value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim() ?? string.Empty
                : value.ToString()
            : string.Empty;

    private static bool TryGetProperty(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
