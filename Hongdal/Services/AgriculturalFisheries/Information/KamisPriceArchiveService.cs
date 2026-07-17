using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Hongdal.Domain.AgriculturalFisheries;
using Hongdal.Infrastructure.Persistence.AgriculturalFisheries;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.AgriculturalFisheries.Information;

public sealed record KamisPriceArchiveResult(
    long CollectionRunId,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    DateOnly? LatestSurveyDate);

public interface IKamisPriceArchiveService
{
    Task<KamisPriceArchiveResult> CollectDailyPricesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default);

    Task<KamisPriceArchiveResult> CollectPeriodPricesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<KamisPriceArchiveResult> CollectMonthlyPricesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);
}

public sealed partial class KamisPriceArchiveService : IKamisPriceArchiveService
{
    private const string SourceUrl = "https://www.kamis.or.kr/service/price/xml.do";
    private const string NationwideCode = "ALL";
    private const string NationwideName = "전국";
    private const string ConvertedKilogramUnit = "1kg";
    private const int PeriodQueryBatchSize = 12;
    private const int PeriodQueryConcurrency = 2;
    private const int MonthlyQueryBatchSize = 20;
    private const int MonthlyQueryConcurrency = 4;

    private sealed record PeriodProductQuery(
        string Action,
        string ProductClassCode,
        string ProductClassName,
        string CategoryCode,
        string CategoryName,
        string ItemCode,
        string ItemName,
        string KindCode,
        string KindName,
        string RankCode,
        string RankName);

    private sealed record MonthlyProductQuery(
        string CategoryCode,
        string CategoryName,
        string ItemCode,
        string ItemName,
        string KindCode,
        string KindName,
        string RankCode,
        string RankName,
        string GradeRank);

    private static readonly IReadOnlyDictionary<string, string> ProductClasses =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["01"] = "소매",
            ["02"] = "도매"
        };

    private static readonly IReadOnlyDictionary<string, string> Categories =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["100"] = "식량작물",
            ["200"] = "채소류",
            ["300"] = "특용작물",
            ["400"] = "과일류",
            ["500"] = "축산물",
            ["600"] = "수산물"
        };

    private readonly HttpClient _httpClient;
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly PublicDataOptions _options;
    private readonly ILogger<KamisPriceArchiveService> _logger;

    public KamisPriceArchiveService(
        HttpClient httpClient,
        AgriculturalFisheriesDbContext db,
        IOptions<PublicDataOptions> options,
        ILogger<KamisPriceArchiveService> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<KamisPriceArchiveResult> CollectDailyPricesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        if (requestedDate.Year is < 1990 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedDate));
        }

        EnsureKamisConfigured();

        var run = new KamisPriceCollectionRun
        {
            RequestedDate = requestedDate,
            QuerySummary =
                $"KAMIS 전국 일별 부류별 가격 / 도매·소매 / 6개 부류 / 요청일 {requestedDate:yyyy-MM-dd} / kg 환산",
            SourceUrl = SourceUrl,
            StartedAtUtc = DateTime.UtcNow
        };
        _db.KamisCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var collectedAtUtc = DateTime.UtcNow;
            var incoming = new List<KamisPriceObservation>();
            foreach (var productClass in ProductClasses)
            {
                foreach (var category in Categories)
                {
                    incoming.AddRange(await FetchCategoryAsync(
                        requestedDate,
                        productClass.Key,
                        productClass.Value,
                        category.Key,
                        category.Value,
                        collectedAtUtc,
                        cancellationToken));
                }
            }

            var deduplicated = incoming
                .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                .Select(group => group.Last())
                .ToArray();
            var recordKeys = deduplicated
                .Select(item => item.RecordKey)
                .ToHashSet(StringComparer.Ordinal);
            var existing = await _db.KamisPriceObservations
                .Where(item => recordKeys.Contains(item.RecordKey))
                .ToDictionaryAsync(item => item.RecordKey, StringComparer.Ordinal, cancellationToken);

            var updatedCount = 0;
            foreach (var item in deduplicated)
            {
                if (existing.TryGetValue(item.RecordKey, out var stored))
                {
                    if (HasMaterialChanges(stored, item))
                    {
                        CopyMutableValues(stored, item);
                        stored.UpdatedAtUtc = collectedAtUtc;
                        updatedCount++;
                    }

                    stored.LastSeenAtUtc = collectedAtUtc;
                    continue;
                }

                item.FirstCollectionRunId = run.Id;
                _db.KamisPriceObservations.Add(item);
            }

            var latestSurveyDate = deduplicated
                .Select(item => item.SurveyDate)
                .Cast<DateOnly?>()
                .DefaultIfEmpty()
                .Max();
            run.StatusCode = KamisArchiveStatusCodes.Completed;
            run.CompletedAtUtc = DateTime.UtcNow;
            run.LatestSurveyDate = latestSurveyDate;
            run.FetchedCount = deduplicated.Length;
            run.InsertedCount = deduplicated.Length - existing.Count;
            run.UpdatedCount = updatedCount;
            run.ExistingCount = existing.Count;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "KAMIS 국내 농수산물 가격 수집 완료. RunId={RunId}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}",
                run.Id,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.ExistingCount);

            return new KamisPriceArchiveResult(
                run.Id,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.ExistingCount,
                latestSurveyDate);
        }
        catch (Exception ex)
        {
            _db.ChangeTracker.Clear();
            var failedRun = await _db.KamisCollectionRuns
                .SingleAsync(item => item.Id == run.Id, CancellationToken.None);
            failedRun.StatusCode = KamisArchiveStatusCodes.Failed;
            failedRun.CompletedAtUtc = DateTime.UtcNow;
            failedRun.ErrorMessage = ex.Message.Length <= 2000 ? ex.Message : ex.Message[..2000];
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<KamisPriceArchiveResult> CollectPeriodPricesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(startDate, endDate);
        EnsureKamisConfigured();

        var run = new KamisPriceCollectionRun
        {
            RequestedDate = endDate,
            QuerySummary =
                $"KAMIS 전국 기간 가격 / 도매·소매 / 품목·품종·등급별 / {startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd} / kg 환산",
            SourceUrl = SourceUrl,
            StartedAtUtc = DateTime.UtcNow
        };
        _db.KamisCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        var fetchedCount = 0;
        var insertedCount = 0;
        var updatedCount = 0;
        var existingCount = 0;
        DateOnly? latestSurveyDate = null;

        try
        {
            var queries = await FetchPeriodProductQueriesAsync(cancellationToken);
            using var concurrency = new SemaphoreSlim(PeriodQueryConcurrency);
            var completedQueryCount = 0;

            foreach (var batch in queries.Chunk(PeriodQueryBatchSize))
            {
                var tasks = batch.Select(async query =>
                {
                    await concurrency.WaitAsync(cancellationToken);
                    try
                    {
                        return await FetchPeriodPricesAsync(
                            query,
                            startDate,
                            endDate,
                            DateTime.UtcNow,
                            cancellationToken);
                    }
                    finally
                    {
                        concurrency.Release();
                    }
                });

                var batchResults = await Task.WhenAll(tasks);
                var deduplicated = batchResults
                    .SelectMany(result => result)
                    .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                    .Select(group => group.Last())
                    .ToArray();
                var batchCounts = await UpsertArchiveBatchAsync(
                    run.Id,
                    deduplicated,
                    cancellationToken);

                fetchedCount += deduplicated.Length;
                insertedCount += batchCounts.Inserted;
                updatedCount += batchCounts.Updated;
                existingCount += batchCounts.Existing;
                var batchLatestDate = deduplicated
                    .Select(item => item.SurveyDate)
                    .Cast<DateOnly?>()
                    .DefaultIfEmpty()
                    .Max();
                if (batchLatestDate is not null
                    && (latestSurveyDate is null || batchLatestDate > latestSurveyDate))
                {
                    latestSurveyDate = batchLatestDate;
                }

                completedQueryCount += batch.Length;
                _logger.LogInformation(
                    "KAMIS 기간 가격 수집 진행. RunId={RunId}, Queries={Completed}/{Total}, Fetched={Fetched}, Inserted={Inserted}",
                    run.Id,
                    completedQueryCount,
                    queries.Count,
                    fetchedCount,
                    insertedCount);
            }

            _db.ChangeTracker.Clear();
            var completedRun = await _db.KamisCollectionRuns
                .SingleAsync(item => item.Id == run.Id, cancellationToken);
            completedRun.StatusCode = KamisArchiveStatusCodes.Completed;
            completedRun.CompletedAtUtc = DateTime.UtcNow;
            completedRun.LatestSurveyDate = latestSurveyDate;
            completedRun.FetchedCount = fetchedCount;
            completedRun.InsertedCount = insertedCount;
            completedRun.UpdatedCount = updatedCount;
            completedRun.ExistingCount = existingCount;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "KAMIS 국내 농수산물 1년 가격 수집 완료. RunId={RunId}, Queries={Queries}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}",
                run.Id,
                queries.Count,
                fetchedCount,
                insertedCount,
                updatedCount,
                existingCount);

            return new KamisPriceArchiveResult(
                run.Id,
                fetchedCount,
                insertedCount,
                updatedCount,
                existingCount,
                latestSurveyDate);
        }
        catch (Exception ex)
        {
            _db.ChangeTracker.Clear();
            var failedRun = await _db.KamisCollectionRuns
                .SingleAsync(item => item.Id == run.Id, CancellationToken.None);
            failedRun.StatusCode = KamisArchiveStatusCodes.Failed;
            failedRun.CompletedAtUtc = DateTime.UtcNow;
            failedRun.LatestSurveyDate = latestSurveyDate;
            failedRun.FetchedCount = fetchedCount;
            failedRun.InsertedCount = insertedCount;
            failedRun.UpdatedCount = updatedCount;
            failedRun.ExistingCount = existingCount;
            failedRun.ErrorMessage = ex.Message.Length <= 2000 ? ex.Message : ex.Message[..2000];
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<KamisPriceArchiveResult> CollectMonthlyPricesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(startDate, endDate);
        EnsureKamisConfigured();

        var run = new KamisPriceCollectionRun
        {
            RequestedDate = endDate,
            QuerySummary =
                $"KAMIS 전국 월평균 가격 / 도매·소매 / 품목·품종·상품·중품 / {startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd} / kg 환산",
            SourceUrl = SourceUrl,
            StartedAtUtc = DateTime.UtcNow
        };
        _db.KamisCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        var fetchedCount = 0;
        var insertedCount = 0;
        var updatedCount = 0;
        var existingCount = 0;
        DateOnly? latestSurveyDate = null;

        try
        {
            var queries = await FetchMonthlyProductQueriesAsync(cancellationToken);
            using var concurrency = new SemaphoreSlim(MonthlyQueryConcurrency);
            var completedQueryCount = 0;

            foreach (var batch in queries.Chunk(MonthlyQueryBatchSize))
            {
                var tasks = batch.Select(async query =>
                {
                    await concurrency.WaitAsync(cancellationToken);
                    try
                    {
                        return await FetchMonthlyPricesAsync(
                            query,
                            startDate,
                            endDate,
                            DateTime.UtcNow,
                            cancellationToken);
                    }
                    finally
                    {
                        concurrency.Release();
                    }
                });

                var batchResults = await Task.WhenAll(tasks);
                var deduplicated = batchResults
                    .SelectMany(result => result)
                    .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                    .Select(group => group.Last())
                    .ToArray();
                var batchCounts = await UpsertArchiveBatchAsync(
                    run.Id,
                    deduplicated,
                    cancellationToken);

                fetchedCount += deduplicated.Length;
                insertedCount += batchCounts.Inserted;
                updatedCount += batchCounts.Updated;
                existingCount += batchCounts.Existing;
                var batchLatestDate = deduplicated
                    .Select(item => item.SurveyDate)
                    .Cast<DateOnly?>()
                    .DefaultIfEmpty()
                    .Max();
                if (batchLatestDate is not null
                    && (latestSurveyDate is null || batchLatestDate > latestSurveyDate))
                {
                    latestSurveyDate = batchLatestDate;
                }

                completedQueryCount += batch.Length;
                _logger.LogInformation(
                    "KAMIS 월평균 가격 수집 진행. RunId={RunId}, Queries={Completed}/{Total}, Fetched={Fetched}, Inserted={Inserted}",
                    run.Id,
                    completedQueryCount,
                    queries.Count,
                    fetchedCount,
                    insertedCount);
            }

            _db.ChangeTracker.Clear();
            var completedRun = await _db.KamisCollectionRuns
                .SingleAsync(item => item.Id == run.Id, cancellationToken);
            completedRun.StatusCode = KamisArchiveStatusCodes.Completed;
            completedRun.CompletedAtUtc = DateTime.UtcNow;
            completedRun.LatestSurveyDate = latestSurveyDate;
            completedRun.FetchedCount = fetchedCount;
            completedRun.InsertedCount = insertedCount;
            completedRun.UpdatedCount = updatedCount;
            completedRun.ExistingCount = existingCount;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "KAMIS 국내 농수산물 월평균 가격 수집 완료. RunId={RunId}, Queries={Queries}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}",
                run.Id,
                queries.Count,
                fetchedCount,
                insertedCount,
                updatedCount,
                existingCount);

            return new KamisPriceArchiveResult(
                run.Id,
                fetchedCount,
                insertedCount,
                updatedCount,
                existingCount,
                latestSurveyDate);
        }
        catch (Exception ex)
        {
            _db.ChangeTracker.Clear();
            var failedRun = await _db.KamisCollectionRuns
                .SingleAsync(item => item.Id == run.Id, CancellationToken.None);
            failedRun.StatusCode = KamisArchiveStatusCodes.Failed;
            failedRun.CompletedAtUtc = DateTime.UtcNow;
            failedRun.LatestSurveyDate = latestSurveyDate;
            failedRun.FetchedCount = fetchedCount;
            failedRun.InsertedCount = insertedCount;
            failedRun.UpdatedCount = updatedCount;
            failedRun.ExistingCount = existingCount;
            failedRun.ErrorMessage = ex.Message.Length <= 2000 ? ex.Message : ex.Message[..2000];
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<IReadOnlyList<MonthlyProductQuery>> FetchMonthlyProductQueriesAsync(
        CancellationToken cancellationToken)
    {
        var kamis = _options.Kamis;
        var requestPath = QueryHelpers.AddQueryString(
            kamis.DailyCategoryPricePath.TrimStart('/'),
            new Dictionary<string, string?>
            {
                ["action"] = "productInfo",
                ["p_returntype"] = "json"
            });

        using var document = await GetKamisJsonDocumentAsync(requestPath, cancellationToken);
        var root = document.RootElement;
        var resultCode = ReadString(root, "error_code");
        if (!string.Equals(resultCode, "000", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"KAMIS 품목 목록 요청이 거부되었습니다. 코드={resultCode}");
        }

        if (!TryGetProperty(root, "info", out var info)
            || info.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("KAMIS 품목 목록 응답의 info 항목이 올바르지 않습니다.");
        }

        var queries = new List<MonthlyProductQuery>();
        foreach (var product in info.EnumerateArray())
        {
            var rankCodes = new[]
                {
                    ReadString(product, "whole_productrankcode"),
                    ReadString(product, "retail_productrankcode")
                }
                .SelectMany(value => value.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Where(rankCode => rankCode is "04" or "05")
                .Distinct(StringComparer.Ordinal);

            foreach (var rankCode in rankCodes)
            {
                queries.Add(new MonthlyProductQuery(
                    ReadString(product, "itemcategorycode"),
                    ReadString(product, "itemcategoryname"),
                    ReadString(product, "itemcode"),
                    ReadString(product, "itemname"),
                    ReadString(product, "kindcode"),
                    ReadString(product, "kindname"),
                    rankCode,
                    GetRankName(rankCode),
                    rankCode == "04" ? "1" : "2"));
            }
        }

        return queries
            .Where(query => query.CategoryCode.Length > 0
                            && query.ItemCode.Length > 0
                            && query.KindCode.Length > 0)
            .DistinctBy(query => string.Join(
                '\u001f',
                query.CategoryCode,
                query.ItemCode,
                query.KindCode,
                query.RankCode))
            .ToArray();
    }

    private async Task<IReadOnlyList<KamisPriceObservation>> FetchMonthlyPricesAsync(
        MonthlyProductQuery query,
        DateOnly startDate,
        DateOnly endDate,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var kamis = _options.Kamis;
        var periodYears = Math.Max(1, endDate.Year - startDate.Year + 1);
        var requestPath = QueryHelpers.AddQueryString(
            kamis.DailyCategoryPricePath.TrimStart('/'),
            new Dictionary<string, string?>
            {
                ["action"] = "monthlySalesList",
                ["p_yyyy"] = endDate.Year.ToString(CultureInfo.InvariantCulture),
                ["p_period"] = periodYears.ToString(CultureInfo.InvariantCulture),
                ["p_itemcategorycode"] = query.CategoryCode,
                ["p_itemcode"] = query.ItemCode,
                ["p_kindcode"] = query.KindCode,
                ["p_graderank"] = query.GradeRank,
                ["p_countycode"] = string.Empty,
                ["p_convert_kg_yn"] = "Y",
                ["p_cert_key"] = kamis.CertificationKey,
                ["p_cert_id"] = kamis.RequesterId,
                ["p_returntype"] = "json"
            });

        using var document = await GetKamisJsonDocumentAsync(requestPath, cancellationToken);
        var root = document.RootElement;
        var resultCode = ReadString(root, "error_code");
        if (!string.Equals(resultCode, "000", StringComparison.Ordinal))
        {
            if (string.Equals(resultCode, "001", StringComparison.Ordinal))
            {
                return [];
            }

            throw new InvalidOperationException(
                $"KAMIS 월평균 가격 요청이 거부되었습니다. 부류={query.CategoryCode}, 품목={query.ItemCode}, 코드={resultCode}");
        }

        if (!TryGetProperty(root, "price", out var prices))
        {
            return [];
        }

        var priceGroups = prices.ValueKind switch
        {
            JsonValueKind.Array => prices.EnumerateArray().ToArray(),
            JsonValueKind.Object => [prices],
            _ => []
        };
        var observations = new List<KamisPriceObservation>();
        var startMonth = new DateOnly(startDate.Year, startDate.Month, 1);
        var endMonth = new DateOnly(endDate.Year, endDate.Month, 1);

        foreach (var priceGroup in priceGroups)
        {
            var productClassCode = ReadString(priceGroup, "productclscode");
            if (!ProductClasses.TryGetValue(productClassCode, out var productClassName)
                || !TryGetProperty(priceGroup, "item", out var yearlyItems))
            {
                continue;
            }

            var years = yearlyItems.ValueKind switch
            {
                JsonValueKind.Array => yearlyItems.EnumerateArray().ToArray(),
                JsonValueKind.Object => [yearlyItems],
                _ => []
            };
            foreach (var yearItem in years)
            {
                if (!int.TryParse(ReadString(yearItem, "yyyy"), out var year)
                    || year is < 1990 or > 2100)
                {
                    continue;
                }

                for (var month = 1; month <= 12; month++)
                {
                    var monthStart = new DateOnly(year, month, 1);
                    if (monthStart < startMonth || monthStart > endMonth)
                    {
                        continue;
                    }

                    observations.Add(MapMonthlyObservation(
                        yearItem,
                        query,
                        productClassCode,
                        productClassName,
                        year,
                        month,
                        endDate,
                        collectedAtUtc));
                }
            }
        }

        return observations;
    }

    private async Task<IReadOnlyList<PeriodProductQuery>> FetchPeriodProductQueriesAsync(
        CancellationToken cancellationToken)
    {
        var kamis = _options.Kamis;
        var requestPath = QueryHelpers.AddQueryString(
            kamis.DailyCategoryPricePath.TrimStart('/'),
            new Dictionary<string, string?>
            {
                ["action"] = "productInfo",
                ["p_returntype"] = "json"
            });

        using var document = await GetKamisJsonDocumentAsync(requestPath, cancellationToken);
        var root = document.RootElement;
        var resultCode = ReadString(root, "error_code");
        if (!string.Equals(resultCode, "000", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"KAMIS 품목 목록 요청이 거부되었습니다. 코드={resultCode}");
        }

        if (!TryGetProperty(root, "info", out var info)
            || info.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("KAMIS 품목 목록 응답의 info 항목이 올바르지 않습니다.");
        }

        var queries = new List<PeriodProductQuery>();
        foreach (var product in info.EnumerateArray())
        {
            AddPeriodProductQueries(
                queries,
                product,
                "periodWholesaleProductList",
                "02",
                ProductClasses["02"],
                "whole_productrankcode");
            AddPeriodProductQueries(
                queries,
                product,
                "periodRetailProductList",
                "01",
                ProductClasses["01"],
                "retail_productrankcode");
        }

        return queries
            .DistinctBy(query => string.Join(
                '\u001f',
                query.Action,
                query.CategoryCode,
                query.ItemCode,
                query.KindCode,
                query.RankCode))
            .ToArray();
    }

    private static void AddPeriodProductQueries(
        ICollection<PeriodProductQuery> target,
        JsonElement product,
        string action,
        string productClassCode,
        string productClassName,
        string rankPropertyName)
    {
        var categoryCode = ReadString(product, "itemcategorycode");
        var itemCode = ReadString(product, "itemcode");
        var kindCode = ReadString(product, "kindcode");
        var rankCodes = ReadString(product, rankPropertyName)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (categoryCode.Length == 0
            || itemCode.Length == 0
            || kindCode.Length == 0
            || rankCodes.Length == 0)
        {
            return;
        }

        var categoryName = ReadString(product, "itemcategoryname");
        var itemName = ReadString(product, "itemname");
        var kindName = ReadString(product, "kindname");
        foreach (var rankCode in rankCodes)
        {
            target.Add(new PeriodProductQuery(
                action,
                productClassCode,
                productClassName,
                categoryCode,
                categoryName,
                itemCode,
                itemName,
                kindCode,
                kindName,
                rankCode,
                GetRankName(rankCode)));
        }
    }

    private async Task<IReadOnlyList<KamisPriceObservation>> FetchPeriodPricesAsync(
        PeriodProductQuery query,
        DateOnly startDate,
        DateOnly endDate,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var kamis = _options.Kamis;
        var requestPath = QueryHelpers.AddQueryString(
            kamis.DailyCategoryPricePath.TrimStart('/'),
            new Dictionary<string, string?>
            {
                ["action"] = query.Action,
                ["p_startday"] = startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["p_endday"] = endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["p_product_cls_code"] = query.ProductClassCode,
                ["p_item_category_code"] = query.CategoryCode,
                ["p_item_code"] = query.ItemCode,
                ["p_kind_code"] = query.KindCode,
                ["p_product_rank_code"] = query.RankCode,
                ["p_county_code"] = string.Empty,
                ["p_convert_kg_yn"] = "Y",
                ["p_cert_key"] = kamis.CertificationKey,
                ["p_cert_id"] = kamis.RequesterId,
                ["p_returntype"] = "json"
            });

        using var document = await GetKamisJsonDocumentAsync(requestPath, cancellationToken);
        var data = ReadDataObject(document.RootElement, query.ProductClassCode, query.CategoryCode);
        var resultCode = ReadString(data, "error_code");
        if (!string.Equals(resultCode, "000", StringComparison.Ordinal))
        {
            if (string.Equals(resultCode, "001", StringComparison.Ordinal))
            {
                return [];
            }

            throw new InvalidOperationException(
                $"KAMIS 기간 가격 요청이 거부되었습니다. 가격구분={query.ProductClassCode}, 부류={query.CategoryCode}, 품목={query.ItemCode}, 코드={resultCode}");
        }

        if (!TryGetProperty(data, "item", out var items))
        {
            return [];
        }

        var sourceItems = items.ValueKind switch
        {
            JsonValueKind.Array => items.EnumerateArray().ToArray(),
            JsonValueKind.Object => [items],
            _ => []
        };
        var resolvedItemName = sourceItems
            .Select(item => ReadString(item, "itemname"))
            .FirstOrDefault(value => value.Length > 0) ?? query.ItemName;
        var resolvedKindName = sourceItems
            .Select(item => ReadString(item, "kindname"))
            .FirstOrDefault(value => value.Length > 0) ?? query.KindName;

        return sourceItems
            .Where(item => string.Equals(
                ReadString(item, "countyname"),
                "평균",
                StringComparison.Ordinal))
            .Select(item => MapPeriodObservation(
                item,
                query,
                resolvedItemName,
                resolvedKindName,
                endDate,
                collectedAtUtc))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();
    }

    private async Task<(int Inserted, int Updated, int Existing)> UpsertArchiveBatchAsync(
        long collectionRunId,
        IReadOnlyCollection<KamisPriceObservation> incoming,
        CancellationToken cancellationToken)
    {
        if (incoming.Count == 0)
        {
            return (0, 0, 0);
        }

        var recordKeys = incoming
            .Select(item => item.RecordKey)
            .ToHashSet(StringComparer.Ordinal);
        var existing = await _db.KamisPriceObservations
            .Where(item => recordKeys.Contains(item.RecordKey))
            .ToDictionaryAsync(item => item.RecordKey, StringComparer.Ordinal, cancellationToken);
        var updatedCount = 0;
        var seenAtUtc = DateTime.UtcNow;

        foreach (var item in incoming)
        {
            if (existing.TryGetValue(item.RecordKey, out var stored))
            {
                if (HasPeriodMaterialChanges(stored, item))
                {
                    CopyPeriodMutableValues(stored, item);
                    stored.UpdatedAtUtc = seenAtUtc;
                    updatedCount++;
                }

                stored.LastSeenAtUtc = seenAtUtc;
                continue;
            }

            item.FirstCollectionRunId = collectionRunId;
            _db.KamisPriceObservations.Add(item);
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return (incoming.Count - existing.Count, updatedCount, existing.Count);
    }

    private async Task<JsonDocument> GetKamisJsonDocumentAsync(
        string requestPath,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(
                    requestPath,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                }

                var isTransient = (int)response.StatusCode == 408
                                  || (int)response.StatusCode == 429
                                  || (int)response.StatusCode >= 500;
                if (!isTransient || attempt == maxAttempts)
                {
                    throw new InvalidOperationException(
                        $"KAMIS HTTP 요청이 실패했습니다. 상태 코드={(int)response.StatusCode}");
                }

                _logger.LogWarning(
                    "KAMIS HTTP 요청을 재시도합니다. Attempt={Attempt}/{MaxAttempts}, StatusCode={StatusCode}",
                    attempt,
                    maxAttempts,
                    (int)response.StatusCode);
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    "KAMIS 네트워크 요청을 재시도합니다. Attempt={Attempt}/{MaxAttempts}",
                    attempt,
                    maxAttempts);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested
                                                && attempt < maxAttempts)
            {
                _logger.LogWarning(
                    "KAMIS 시간 초과 요청을 재시도합니다. Attempt={Attempt}/{MaxAttempts}",
                    attempt,
                    maxAttempts);
            }
            catch (HttpRequestException)
            {
                throw new InvalidOperationException("KAMIS 네트워크 요청에 실패했습니다.");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException("KAMIS 요청 제한 시간을 초과했습니다.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), cancellationToken);
        }

        throw new InvalidOperationException("KAMIS 요청 재시도 횟수를 초과했습니다.");
    }

    private async Task<IReadOnlyList<KamisPriceObservation>> FetchCategoryAsync(
        DateOnly requestedDate,
        string productClassCode,
        string productClassName,
        string categoryCode,
        string categoryName,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var kamis = _options.Kamis;
        var requestPath = QueryHelpers.AddQueryString(
            kamis.DailyCategoryPricePath.TrimStart('/'),
            new Dictionary<string, string?>
            {
                ["action"] = "dailyPriceByCategoryList",
                ["p_product_cls_code"] = productClassCode,
                ["p_country_code"] = string.Empty,
                ["p_regday"] = requestedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["p_convert_kg_yn"] = "Y",
                ["p_item_category_code"] = categoryCode,
                ["p_cert_key"] = kamis.CertificationKey,
                ["p_cert_id"] = kamis.RequesterId,
                ["p_returntype"] = "json"
            });

        using var document = await GetKamisJsonDocumentAsync(requestPath, cancellationToken);
        var data = ReadDataObject(document.RootElement, productClassCode, categoryCode);
        var resultCode = ReadString(data, "error_code");
        if (!string.Equals(resultCode, "000", StringComparison.Ordinal))
        {
            if (string.Equals(resultCode, "001", StringComparison.Ordinal))
            {
                return [];
            }

            throw new InvalidOperationException(
                $"KAMIS 요청이 거부되었습니다. 가격구분={productClassCode}, 부류={categoryCode}, 코드={resultCode}");
        }

        if (!TryGetProperty(data, "item", out var items))
        {
            return [];
        }

        return items.ValueKind switch
        {
            JsonValueKind.Array => items.EnumerateArray()
                .Select(item => MapObservation(
                    item,
                    requestedDate,
                    productClassCode,
                    productClassName,
                    categoryCode,
                    categoryName,
                    collectedAtUtc))
                .ToArray(),
            JsonValueKind.Object =>
            [
                MapObservation(
                    items,
                    requestedDate,
                    productClassCode,
                    productClassName,
                    categoryCode,
                    categoryName,
                    collectedAtUtc)
            ],
            _ => []
        };
    }

    private static JsonElement ReadDataObject(
        JsonElement root,
        string productClassCode,
        string categoryCode)
    {
        if (!TryGetProperty(root, "data", out var data))
        {
            throw new InvalidOperationException("KAMIS 응답에 data 항목이 없습니다.");
        }

        if (data.ValueKind == JsonValueKind.Object)
        {
            return data;
        }

        if (data.ValueKind == JsonValueKind.Array)
        {
            var first = data.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.Object)
            {
                return first;
            }

            var code = first.ValueKind == JsonValueKind.String
                ? first.GetString()
                : first.GetRawText();
            throw new InvalidOperationException(
                $"KAMIS 응답 형식이 올바르지 않습니다. 가격구분={productClassCode}, 부류={categoryCode}, 코드={code}");
        }

        throw new InvalidOperationException("KAMIS 응답의 data 항목 형식이 올바르지 않습니다.");
    }

    private static KamisPriceObservation MapObservation(
        JsonElement source,
        DateOnly requestedDate,
        string productClassCode,
        string productClassName,
        string categoryCode,
        string categoryName,
        DateTime collectedAtUtc)
    {
        var itemName = ReadString(source, "item_name", "itemname");
        var itemCode = ReadString(source, "item_code", "itemcode");
        var kindName = ReadString(source, "kind_name", "kindname");
        var kindCode = ReadString(source, "kind_code", "kindcode");
        var rankName = ReadString(source, "rank");
        var rankCode = ReadString(source, "rank_code", "rankcode");
        var unit = ConvertedKilogramUnit;
        var priceRaw = ReadString(source, "dpr1");
        var surveyDate = ParseSurveyDate(ReadString(source, "day1"), requestedDate);
        var identity = string.Join(
            '\u001f',
            productClassCode,
            categoryCode,
            NationwideCode,
            surveyDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            itemCode,
            kindCode,
            rankCode,
            unit);

        return new KamisPriceObservation
        {
            RecordKey = UsdaNassPriceArchiveService.Sha256(identity),
            ProductClassCode = productClassCode,
            ProductClassName = productClassName,
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            CountryCode = NationwideCode,
            CountryName = NationwideName,
            RequestedDate = requestedDate,
            SurveyDate = surveyDate,
            FrequencyCode = "Daily",
            ItemName = itemName,
            ItemCode = itemCode,
            KindName = kindName,
            KindCode = kindCode,
            RankName = rankName,
            RankCode = rankCode,
            Unit = unit,
            PriceRaw = priceRaw,
            PriceKrw = ParsePrice(priceRaw),
            PreviousDayLabel = ReadString(source, "day2"),
            PreviousDayPriceKrw = ParsePrice(ReadString(source, "dpr2")),
            OneWeekAgoLabel = ReadString(source, "day3"),
            OneWeekAgoPriceKrw = ParsePrice(ReadString(source, "dpr3")),
            TwoWeeksAgoLabel = ReadString(source, "day4"),
            TwoWeeksAgoPriceKrw = ParsePrice(ReadString(source, "dpr4")),
            OneMonthAgoLabel = ReadString(source, "day5"),
            OneMonthAgoPriceKrw = ParsePrice(ReadString(source, "dpr5")),
            OneYearAgoLabel = ReadString(source, "day6"),
            OneYearAgoPriceKrw = ParsePrice(ReadString(source, "dpr6")),
            NormalYearLabel = ReadString(source, "day7"),
            NormalYearPriceKrw = ParsePrice(ReadString(source, "dpr7")),
            IsPriceMissing = ParsePrice(priceRaw) is null,
            SourceUrl = SourceUrl,
            RawJson = source.GetRawText(),
            FirstCollectedAtUtc = collectedAtUtc,
            LastSeenAtUtc = collectedAtUtc,
            UpdatedAtUtc = collectedAtUtc
        };
    }

    private static KamisPriceObservation MapMonthlyObservation(
        JsonElement source,
        MonthlyProductQuery query,
        string productClassCode,
        string productClassName,
        int year,
        int month,
        DateOnly requestedDate,
        DateTime collectedAtUtc)
    {
        var surveyDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        var priceRaw = ReadString(source, $"m{month}");
        var identity = string.Join(
            '\u001f',
            "Monthly",
            productClassCode,
            query.CategoryCode,
            NationwideCode,
            surveyDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            query.ItemCode,
            query.KindCode,
            query.RankCode,
            ConvertedKilogramUnit);

        return new KamisPriceObservation
        {
            RecordKey = UsdaNassPriceArchiveService.Sha256(identity),
            ProductClassCode = productClassCode,
            ProductClassName = productClassName,
            CategoryCode = query.CategoryCode,
            CategoryName = query.CategoryName,
            CountryCode = NationwideCode,
            CountryName = NationwideName,
            RequestedDate = requestedDate,
            SurveyDate = surveyDate,
            FrequencyCode = "Monthly",
            ItemName = query.ItemName,
            ItemCode = query.ItemCode,
            KindName = query.KindName,
            KindCode = query.KindCode,
            RankName = query.RankName,
            RankCode = query.RankCode,
            Unit = ConvertedKilogramUnit,
            PriceRaw = priceRaw,
            PriceKrw = ParsePrice(priceRaw),
            IsPriceMissing = ParsePrice(priceRaw) is null,
            SourceUrl = SourceUrl,
            RawJson = source.GetRawText(),
            FirstCollectedAtUtc = collectedAtUtc,
            LastSeenAtUtc = collectedAtUtc,
            UpdatedAtUtc = collectedAtUtc
        };
    }

    private static KamisPriceObservation? MapPeriodObservation(
        JsonElement source,
        PeriodProductQuery query,
        string itemName,
        string kindName,
        DateOnly requestedDate,
        DateTime collectedAtUtc)
    {
        var surveyDate = ParsePeriodSurveyDate(
            ReadString(source, "yyyy"),
            ReadString(source, "regday"));
        if (surveyDate is null)
        {
            return null;
        }

        var priceRaw = ReadString(source, "price");
        var identity = string.Join(
            '\u001f',
            query.ProductClassCode,
            query.CategoryCode,
            NationwideCode,
            surveyDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            query.ItemCode,
            query.KindCode,
            query.RankCode,
            ConvertedKilogramUnit);

        return new KamisPriceObservation
        {
            RecordKey = UsdaNassPriceArchiveService.Sha256(identity),
            ProductClassCode = query.ProductClassCode,
            ProductClassName = query.ProductClassName,
            CategoryCode = query.CategoryCode,
            CategoryName = query.CategoryName,
            CountryCode = NationwideCode,
            CountryName = NationwideName,
            RequestedDate = requestedDate,
            SurveyDate = surveyDate.Value,
            FrequencyCode = "Daily",
            ItemName = itemName,
            ItemCode = query.ItemCode,
            KindName = kindName,
            KindCode = query.KindCode,
            RankName = query.RankName,
            RankCode = query.RankCode,
            Unit = ConvertedKilogramUnit,
            PriceRaw = priceRaw,
            PriceKrw = ParsePrice(priceRaw),
            IsPriceMissing = ParsePrice(priceRaw) is null,
            SourceUrl = SourceUrl,
            RawJson = source.GetRawText(),
            FirstCollectedAtUtc = collectedAtUtc,
            LastSeenAtUtc = collectedAtUtc,
            UpdatedAtUtc = collectedAtUtc
        };
    }

    internal static DateOnly? ParsePeriodSurveyDate(string year, string monthDay)
    {
        var parts = monthDay.Trim().Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !int.TryParse(year.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var parsedYear)
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var parsedMonth)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var parsedDay))
        {
            return null;
        }

        try
        {
            return new DateOnly(parsedYear, parsedMonth, parsedDay);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    internal static void ValidatePeriod(DateOnly startDate, DateOnly endDate)
    {
        if (startDate.Year is < 1990 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(startDate));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("종료일은 시작일보다 빠를 수 없습니다.", nameof(endDate));
        }

        if (endDate >= startDate.AddYears(1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "KAMIS 기간 조회는 시작일을 포함해 최대 1년 미만 범위로 요청해야 합니다.");
        }
    }

    private void EnsureKamisConfigured()
    {
        var kamis = _options.Kamis;
        if (string.IsNullOrWhiteSpace(kamis.CertificationKey)
            || string.IsNullOrWhiteSpace(kamis.RequesterId))
        {
            throw new InvalidOperationException(
                "KAMIS 인증값이 설정되지 않았습니다. PublicData:Kamis 설정을 확인해 주세요.");
        }
    }

    private static string GetRankName(string rankCode)
        => rankCode switch
        {
            "04" => "상품",
            "05" => "중품",
            _ => $"등급 {rankCode}"
        };

    internal static DateOnly ParseSurveyDate(string value, DateOnly requestedDate)
    {
        var fullDate = FullDateRegex().Match(value);
        if (fullDate.Success
            && DateOnly.TryParseExact(
                fullDate.Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedFullDate))
        {
            return parsedFullDate;
        }

        var monthDay = MonthDayRegex().Match(value);
        if (!monthDay.Success
            || !int.TryParse(monthDay.Groups["month"].Value, out var month)
            || !int.TryParse(monthDay.Groups["day"].Value, out var day))
        {
            return requestedDate;
        }

        var candidate = new DateOnly(requestedDate.Year, month, day);
        return candidate > requestedDate.AddDays(1)
            ? candidate.AddYears(-1)
            : candidate;
    }

    internal static decimal? ParsePrice(string value)
    {
        var normalized = value.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;
    }

    private static string ReadString(JsonElement source, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!TryGetProperty(source, propertyName, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    private static bool TryGetProperty(
        JsonElement source,
        string propertyName,
        out JsonElement value)
    {
        if (source.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in source.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool HasPeriodMaterialChanges(
        KamisPriceObservation stored,
        KamisPriceObservation incoming)
        => stored.FrequencyCode != incoming.FrequencyCode
           || stored.ItemName != incoming.ItemName
           || stored.KindName != incoming.KindName
           || stored.RankName != incoming.RankName
           || stored.Unit != incoming.Unit
           || stored.PriceRaw != incoming.PriceRaw
           || stored.PriceKrw != incoming.PriceKrw
           || stored.IsPriceMissing != incoming.IsPriceMissing;

    private static void CopyPeriodMutableValues(
        KamisPriceObservation stored,
        KamisPriceObservation incoming)
    {
        stored.RequestedDate = incoming.RequestedDate;
        stored.FrequencyCode = incoming.FrequencyCode;
        stored.ItemName = incoming.ItemName;
        stored.KindName = incoming.KindName;
        stored.RankName = incoming.RankName;
        stored.Unit = incoming.Unit;
        stored.PriceRaw = incoming.PriceRaw;
        stored.PriceKrw = incoming.PriceKrw;
        stored.IsPriceMissing = incoming.IsPriceMissing;
        stored.RawJson = incoming.RawJson;
    }

    private static bool HasMaterialChanges(
        KamisPriceObservation stored,
        KamisPriceObservation incoming)
        => stored.FrequencyCode != incoming.FrequencyCode
           || stored.ItemName != incoming.ItemName
           || stored.KindName != incoming.KindName
           || stored.RankName != incoming.RankName
           || stored.Unit != incoming.Unit
           || stored.PriceRaw != incoming.PriceRaw
           || stored.PriceKrw != incoming.PriceKrw
           || stored.PreviousDayLabel != incoming.PreviousDayLabel
           || stored.PreviousDayPriceKrw != incoming.PreviousDayPriceKrw
           || stored.OneWeekAgoLabel != incoming.OneWeekAgoLabel
           || stored.OneWeekAgoPriceKrw != incoming.OneWeekAgoPriceKrw
           || stored.TwoWeeksAgoLabel != incoming.TwoWeeksAgoLabel
           || stored.TwoWeeksAgoPriceKrw != incoming.TwoWeeksAgoPriceKrw
           || stored.OneMonthAgoLabel != incoming.OneMonthAgoLabel
           || stored.OneMonthAgoPriceKrw != incoming.OneMonthAgoPriceKrw
           || stored.OneYearAgoLabel != incoming.OneYearAgoLabel
           || stored.OneYearAgoPriceKrw != incoming.OneYearAgoPriceKrw
           || stored.NormalYearLabel != incoming.NormalYearLabel
           || stored.NormalYearPriceKrw != incoming.NormalYearPriceKrw;

    private static void CopyMutableValues(
        KamisPriceObservation stored,
        KamisPriceObservation incoming)
    {
        stored.RequestedDate = incoming.RequestedDate;
        stored.FrequencyCode = incoming.FrequencyCode;
        stored.ItemName = incoming.ItemName;
        stored.KindName = incoming.KindName;
        stored.RankName = incoming.RankName;
        stored.Unit = incoming.Unit;
        stored.PriceRaw = incoming.PriceRaw;
        stored.PriceKrw = incoming.PriceKrw;
        stored.PreviousDayLabel = incoming.PreviousDayLabel;
        stored.PreviousDayPriceKrw = incoming.PreviousDayPriceKrw;
        stored.OneWeekAgoLabel = incoming.OneWeekAgoLabel;
        stored.OneWeekAgoPriceKrw = incoming.OneWeekAgoPriceKrw;
        stored.TwoWeeksAgoLabel = incoming.TwoWeeksAgoLabel;
        stored.TwoWeeksAgoPriceKrw = incoming.TwoWeeksAgoPriceKrw;
        stored.OneMonthAgoLabel = incoming.OneMonthAgoLabel;
        stored.OneMonthAgoPriceKrw = incoming.OneMonthAgoPriceKrw;
        stored.OneYearAgoLabel = incoming.OneYearAgoLabel;
        stored.OneYearAgoPriceKrw = incoming.OneYearAgoPriceKrw;
        stored.NormalYearLabel = incoming.NormalYearLabel;
        stored.NormalYearPriceKrw = incoming.NormalYearPriceKrw;
        stored.IsPriceMissing = incoming.IsPriceMissing;
        stored.RawJson = incoming.RawJson;
    }

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}", RegexOptions.CultureInvariant)]
    private static partial Regex FullDateRegex();

    [GeneratedRegex(@"(?<month>\d{1,2})/(?<day>\d{1,2})", RegexOptions.CultureInvariant)]
    private static partial Regex MonthDayRegex();
}
