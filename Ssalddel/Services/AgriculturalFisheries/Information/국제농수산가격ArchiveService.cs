using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I국제농수산가격ArchiveService
{
    IReadOnlyList<국제농수산가격Source응답> GetSources();

    Task<국제농수산가격수집응답> CollectAsync(
        국제농수산가격수집요청 request,
        CancellationToken cancellationToken = default);

    Task<국제농수산가격Archive응답> GetArchiveAsync(
        국제농수산가격ArchiveQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class 국제농수산가격ArchiveService
    : I국제농수산가격ArchiveService
{
    private readonly IReadOnlyDictionary<string, I국제농수산가격공급자> _providers;
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<국제농수산가격ArchiveService> _logger;

    public 국제농수산가격ArchiveService(
        IEnumerable<I국제농수산가격공급자> providers,
        AgriculturalFisheriesDbContext db,
        TimeProvider timeProvider,
        ILogger<국제농수산가격ArchiveService> logger)
    {
        _providers = providers.ToDictionary(
            provider => provider.SourceKey,
            StringComparer.OrdinalIgnoreCase);
        _db = db;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public IReadOnlyList<국제농수산가격Source응답> GetSources()
        => 국제농수산가격SourceCatalog.ToResponse();

    public async Task<국제농수산가격수집응답> CollectAsync(
        국제농수산가격수집요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var source = 국제농수산가격SourceCatalog.Find(request.SourceKey)
                     ?? throw new ArgumentException(
                         "등록되지 않은 국제 농수산 가격 source입니다.",
                         nameof(request));
        if (!_providers.TryGetValue(source.SourceKey, out var provider))
        {
            throw new InvalidOperationException(
                $"국제 농수산 가격 source '{source.SourceKey}'의 공급자가 등록되지 않았습니다.");
        }

        var (yearFrom, yearTo) = ResolveYears(
            request,
            source,
            _timeProvider.GetUtcNow().Year);
        var collectedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var run = new 국제농수산가격수집Run
        {
            SourceKey = source.SourceKey,
            YearFrom = yearFrom,
            YearTo = yearTo,
            QuerySummary =
                $"{source.Provider} / {source.DisplayName} / {yearFrom}-{yearTo}",
            SourceUrl = source.ApiBaseUrl,
            StartedAtUtc = collectedAtUtc
        };
        _db.InternationalPriceCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var supplied = await provider.CollectAsync(
                yearFrom,
                yearTo,
                collectedAtUtc,
                cancellationToken);
            if (!string.Equals(
                    supplied.SourceKey,
                    source.SourceKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "국제 농수산 가격 공급 결과의 source key가 요청과 일치하지 않습니다.");
            }

            var incoming = supplied.Observations
                .Where(item =>
                    item.ReferenceDate.Year >= yearFrom
                    && item.ReferenceDate.Year <= yearTo)
                .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                .Select(group => group.Last())
                .ToArray();
            var existing = await ReadExistingAsync(
                incoming.Select(item => item.RecordKey),
                cancellationToken);
            var insertedCount = 0;
            var updatedCount = 0;
            var existingCount = 0;
            foreach (var item in incoming)
            {
                if (!existing.TryGetValue(item.RecordKey, out var stored))
                {
                    item.FirstCollectionRunId = run.Id;
                    _db.InternationalPriceObservations.Add(item);
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

            var sourceMessages = supplied.SourceMessages
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            run.StatusCode = 국제농수산가격Archive상태Codes.완료;
            run.CompletedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            run.LatestReferenceDate = incoming
                .Select(item => (DateOnly?)item.ReferenceDate)
                .DefaultIfEmpty()
                .Max();
            run.FetchedCount = incoming.Length;
            run.InsertedCount = insertedCount;
            run.UpdatedCount = updatedCount;
            run.ExistingCount = existingCount;
            run.SourceUrl = Truncate(supplied.SourceUrl, 1000);
            run.SourceMessagesJson = JsonSerializer.Serialize(sourceMessages);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "국제 농수산 가격 수집 완료. Source={SourceKey}, RunId={RunId}, Years={YearFrom}-{YearTo}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}",
                source.SourceKey,
                run.Id,
                yearFrom,
                yearTo,
                incoming.Length,
                insertedCount,
                updatedCount,
                existingCount);

            return new 국제농수산가격수집응답(
                run.Id,
                국제농수산가격상태Codes.완료,
                source.SourceKey,
                yearFrom,
                yearTo,
                incoming.Length,
                insertedCount,
                updatedCount,
                existingCount,
                run.LatestReferenceDate,
                sourceMessages);
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException
            || !cancellationToken.IsCancellationRequested)
        {
            ResetObservationChanges();
            run.StatusCode = 국제농수산가격Archive상태Codes.실패;
            run.CompletedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            run.ErrorMessage = Truncate(exception.Message, 2000);
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<국제농수산가격Archive응답> GetArchiveAsync(
        국제농수산가격ArchiveQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.YearFrom.HasValue
            && query.YearTo.HasValue
            && query.YearFrom.Value > query.YearTo.Value)
        {
            throw new ArgumentException(
                "조회 시작 연도는 종료 연도보다 늦을 수 없습니다.");
        }

        var observations = _db.InternationalPriceObservations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.SourceKey))
        {
            var sourceKey = query.SourceKey.Trim();
            observations = observations.Where(item => item.SourceKey == sourceKey);
        }

        if (!string.IsNullOrWhiteSpace(query.DatasetCode))
        {
            var datasetCode = query.DatasetCode.Trim();
            observations = observations.Where(item => item.DatasetCode == datasetCode);
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var countryCode = query.CountryCode.Trim();
            observations = observations.Where(item => item.CountryCode == countryCode);
        }

        if (!string.IsNullOrWhiteSpace(query.GeographyCode))
        {
            var geographyCode = query.GeographyCode.Trim();
            observations = observations.Where(item =>
                item.GeographyCode == geographyCode);
        }

        if (!string.IsNullOrWhiteSpace(query.OfficialProductCode))
        {
            var officialProductCode = query.OfficialProductCode.Trim();
            observations = observations.Where(item =>
                item.OfficialProductCode == officialProductCode);
        }

        if (!string.IsNullOrWhiteSpace(query.ProductName))
        {
            var productName = query.ProductName.Trim();
            observations = observations.Where(item =>
                item.ProductNameOriginal.Contains(productName));
        }

        if (query.YearFrom.HasValue)
        {
            var from = new DateOnly(query.YearFrom.Value, 1, 1);
            observations = observations.Where(item => item.ReferenceDate >= from);
        }

        if (query.YearTo.HasValue)
        {
            var toExclusive = new DateOnly(query.YearTo.Value, 1, 1).AddYears(1);
            observations = observations.Where(item =>
                item.ReferenceDate < toExclusive);
        }

        var take = Math.Clamp(query.Take, 1, 500);
        var items = await observations
            .OrderByDescending(item => item.ReferenceDate)
            .ThenBy(item => item.SourceKey)
            .ThenBy(item => item.CountryCode)
            .ThenBy(item => item.OfficialProductCode)
            .Take(take)
            .Select(item => new 국제농수산가격관측응답(
                item.RecordKey,
                item.SourceKey,
                item.DatasetCode,
                item.CountryCode,
                item.CountryName,
                item.GeographyCode,
                item.GeographyName,
                item.MarketStageCode,
                item.OfficialSeriesCode,
                item.OfficialProductCode,
                item.ProductNameOriginal,
                item.CanonicalProductKey,
                item.ReferenceDate,
                item.FrequencyCode,
                item.ValueRaw,
                item.Price,
                item.CurrencyCode,
                item.OriginalUnit,
                item.IsIndex,
                item.BasePeriod,
                item.IsValueMissing,
                item.ObservationStatus,
                item.SourceUrl,
                item.FirstCollectedAtUtc,
                item.LastSeenAtUtc))
            .ToArrayAsync(cancellationToken);
        var sourceNotice = query.SourceKey is { Length: > 0 }
                           && 국제농수산가격SourceCatalog.Find(query.SourceKey) is { } source
            ? $"{source.Provider} 공식 자료를 서버가 수집·보관한 원통화·원단위 관측입니다."
            : "국가별 공식 가격원의 원통화·원단위를 보존한 관측입니다.";
        return new 국제농수산가격Archive응답(
            items.Length == 0
                ? 국제농수산가격상태Codes.자료없음
                : 국제농수산가격상태Codes.완료,
            _timeProvider.GetUtcNow().UtcDateTime,
            items.Length,
            items,
            sourceNotice,
            [
                "시장 단계·품목·품종·규격·지역·기간·통화·원단위가 일치하기 전에는 가격 차액이나 순위를 계산하지 않습니다.",
                "환율과 중량 표준화는 공식 원값과 별도의 검토된 파생값으로만 관리해야 합니다.",
                "이 자료는 정보 제공용이며 주문·견적·계약을 생성하지 않습니다."
            ]);
    }

    private async Task<Dictionary<string, 국제농수산가격관측>> ReadExistingAsync(
        IEnumerable<string> recordKeys,
        CancellationToken cancellationToken)
    {
        var existing = new Dictionary<string, 국제농수산가격관측>(
            StringComparer.Ordinal);
        foreach (var keyBatch in recordKeys
                     .Distinct(StringComparer.Ordinal)
                     .Chunk(1000))
        {
            var keys = keyBatch.ToList();
            var batch = await _db.InternationalPriceObservations
                .Where(item => keys.Contains(item.RecordKey))
                .ToArrayAsync(cancellationToken);
            foreach (var item in batch)
            {
                existing[item.RecordKey] = item;
            }
        }

        return existing;
    }

    private static (int YearFrom, int YearTo) ResolveYears(
        국제농수산가격수집요청 request,
        국제농수산가격SourceDefinition source,
        int currentYear)
    {
        var sourceDefaultYear = int.TryParse(source.LatestVerifiedPeriod.AsSpan(0, 4), out var year)
            ? year
            : currentYear;
        var yearFrom = request.YearFrom <= 0 ? sourceDefaultYear : request.YearFrom;
        var yearTo = request.YearTo <= 0 ? yearFrom : request.YearTo;
        if (yearFrom is < 2000 or > 2100
            || yearTo is < 2000 or > 2100
            || yearFrom > yearTo)
        {
            throw new ArgumentException(
                "수집 연도는 2000~2100 범위에서 시작 연도가 종료 연도보다 늦지 않아야 합니다.");
        }

        return (yearFrom, yearTo);
    }

    private static bool HasBusinessChanges(
        국제농수산가격관측 stored,
        국제농수산가격관측 incoming)
        => stored.ValueRaw != incoming.ValueRaw
           || stored.Price != incoming.Price
           || stored.CurrencyCode != incoming.CurrencyCode
           || stored.OriginalUnit != incoming.OriginalUnit
           || stored.IsValueMissing != incoming.IsValueMissing
           || stored.ObservationStatus != incoming.ObservationStatus
           || stored.ProductNameOriginal != incoming.ProductNameOriginal
           || stored.GeographyName != incoming.GeographyName;

    private static void ApplyBusinessChanges(
        국제농수산가격관측 stored,
        국제농수산가격관측 incoming)
    {
        stored.DatasetCode = incoming.DatasetCode;
        stored.CountryCode = incoming.CountryCode;
        stored.CountryName = incoming.CountryName;
        stored.GeographyCode = incoming.GeographyCode;
        stored.GeographyName = incoming.GeographyName;
        stored.MarketStageCode = incoming.MarketStageCode;
        stored.OfficialSeriesCode = incoming.OfficialSeriesCode;
        stored.OfficialProductCode = incoming.OfficialProductCode;
        stored.ProductNameOriginal = incoming.ProductNameOriginal;
        stored.CanonicalProductKey = incoming.CanonicalProductKey;
        stored.FrequencyCode = incoming.FrequencyCode;
        stored.ValueRaw = incoming.ValueRaw;
        stored.Price = incoming.Price;
        stored.CurrencyCode = incoming.CurrencyCode;
        stored.OriginalUnit = incoming.OriginalUnit;
        stored.IsIndex = incoming.IsIndex;
        stored.BasePeriod = incoming.BasePeriod;
        stored.IsValueMissing = incoming.IsValueMissing;
        stored.ObservationStatus = incoming.ObservationStatus;
        stored.SourceUrl = incoming.SourceUrl;
        stored.RawJson = incoming.RawJson;
    }

    private void ResetObservationChanges()
    {
        foreach (var entry in _db.ChangeTracker.Entries<국제농수산가격관측>())
        {
            entry.State = entry.State == EntityState.Added
                ? EntityState.Detached
                : EntityState.Unchanged;
        }
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
