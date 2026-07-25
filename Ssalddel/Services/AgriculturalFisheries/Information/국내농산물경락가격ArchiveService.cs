using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Persistence,
    "국내 공영도매시장 경락·정산가격의 멱등 수집과 비식별 시계열 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공식 원천의 거래 식별자와 가격 조건만 저장하며 원문에 포함된 출하자·생산자·중도매인 개인정보는 저장하지 않습니다.")]
public sealed class 국내농산물경락가격ArchiveService
    : I국내농산물경락가격ArchiveService
{
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly I국내농산물경락가격조회Service _lookupService;
    private readonly AgriculturalFisheriesBatchOptions _batchOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<국내농산물경락가격ArchiveService> _logger;

    public 국내농산물경락가격ArchiveService(
        AgriculturalFisheriesDbContext db,
        I국내농산물경락가격조회Service lookupService,
        IOptions<AgriculturalFisheriesBatchOptions> batchOptions,
        TimeProvider timeProvider,
        ILogger<국내농산물경락가격ArchiveService> logger)
    {
        _db = db;
        _lookupService = lookupService;
        _batchOptions = batchOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<국내농산물경락가격수집Result> CollectAsync(
        DateOnly settlementDate,
        CancellationToken cancellationToken = default)
    {
        var sourceKey = 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;
        var run = new 국내농산물경락가격수집Run
        {
            RunKey = Guid.NewGuid().ToString("N"),
            SourceKey = sourceKey,
            SettlementDate = settlementDate,
            StartedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
        };
        _db.DomesticAuctionPriceCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var pageSize = Math.Clamp(_batchOptions.DomesticAuctionPageSize, 1, 1000);
            var maxPages = Math.Clamp(_batchOptions.DomesticAuctionMaxPagesPerRun, 1, 500);
            var totalCount = int.MaxValue;
            var existing = await _db.DomesticAuctionPriceObservations
                .Where(observation =>
                    observation.SourceKey == sourceKey
                    && observation.SettlementDate == settlementDate)
                .ToDictionaryAsync(
                    observation => observation.RecordKey,
                    StringComparer.Ordinal,
                    cancellationToken);

            for (var page = 1; page <= maxPages && run.FetchedCount < totalCount; page++)
            {
                var response = await _lookupService.조회Async(
                    new 국내농산물경락가격조회요청
                    {
                        SourceKey = sourceKey,
                        SettlementDate = settlementDate.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture),
                        Page = page,
                        PageSize = pageSize
                    },
                    cancellationToken);

                if (!response.Success)
                {
                    throw new InvalidOperationException(
                        response.ErrorMessage
                        ?? $"경락가격 수집 실패. StatusCode={response.StatusCode}");
                }

                totalCount = Math.Max(0, response.TotalCount);
                var items = response.Items
                    .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                    .Select(group => group.Last())
                    .ToArray();
                run.FetchedCount += items.Length;
                run.CompletedPages = page;

                foreach (var item in items)
                {
                    if (!existing.TryGetValue(item.RecordKey, out var observation))
                    {
                        observation = Map(run.Id, item);
                        _db.DomesticAuctionPriceObservations.Add(observation);
                        existing.Add(item.RecordKey, observation);
                        run.InsertedCount++;
                        continue;
                    }

                    if (Apply(observation, item))
                    {
                        run.UpdatedCount++;
                    }
                    else
                    {
                        run.ExistingCount++;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                if (items.Length == 0 || run.FetchedCount >= totalCount)
                {
                    break;
                }
            }

            run.IsTruncated = run.FetchedCount < totalCount;
            run.StatusCode = 국내농산물경락가격수집상태Codes.완료;
            run.CompletedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "국내 공영도매시장 경락가격 수집 완료. SettlementDate={SettlementDate}, RunId={RunId}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}, Pages={Pages}, Truncated={Truncated}",
                settlementDate,
                run.Id,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.ExistingCount,
                run.CompletedPages,
                run.IsTruncated);

            return Result(run);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            run.StatusCode = 국내농산물경락가격수집상태Codes.실패;
            run.ErrorMessage = exception.Message.Length > 2000
                ? exception.Message[..2000]
                : exception.Message;
            run.CompletedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<국내농산물경락가격조회응답> SearchAsync(
        국내농산물경락가격조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!DateOnly.TryParseExact(
                request.SettlementDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var settlementDate))
        {
            return Fail(request, "SettlementDate는 yyyy-MM-dd 형식이어야 합니다.");
        }

        var source = _lookupService.GetSources().FirstOrDefault(candidate =>
            string.Equals(candidate.Key, request.SourceKey, StringComparison.OrdinalIgnoreCase));
        if (source is null)
        {
            return new 국내농산물경락가격조회응답
            {
                StatusCode = 국내농산물경락가격조회상태Codes.지원하지않는출처,
                ErrorMessage = $"지원하지 않는 경락가격 원천입니다. SourceKey={request.SourceKey}",
                Query = request,
                Notices = 국내농산물경락가격조회Service.DefaultNotices
            };
        }

        var query = _db.DomesticAuctionPriceObservations
            .AsNoTracking()
            .Where(observation =>
                observation.SourceKey == source.Key
                && observation.SettlementDate == settlementDate);
        if (!string.IsNullOrWhiteSpace(request.WholesaleMarketCode))
        {
            var marketCode = request.WholesaleMarketCode.Trim();
            query = query.Where(observation =>
                observation.WholesaleMarketCode == marketCode);
        }

        if (!string.IsNullOrWhiteSpace(request.CorporationCode))
        {
            var corporationCode = request.CorporationCode.Trim();
            query = query.Where(observation =>
                observation.CorporationCode == corporationCode);
        }

        if (!string.IsNullOrWhiteSpace(request.ItemName))
        {
            var itemName = request.ItemName.Trim();
            query = query.Where(observation =>
                observation.ItemName.Contains(itemName)
                || observation.VarietyName.Contains(itemName));
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 1000);
        var totalCount = await query.CountAsync(cancellationToken);
        var observations = await query
            .OrderBy(observation => observation.ItemName)
            .ThenBy(observation => observation.VarietyName)
            .ThenBy(observation => observation.WholesaleMarketCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new 국내농산물경락가격조회응답
        {
            Success = true,
            StatusCode = 국내농산물경락가격조회상태Codes.완료,
            Source = source,
            Query = request,
            Items = observations.Select(Map).ToArray(),
            TotalCount = totalCount,
            LatestCollectedAtUtc = observations.Length == 0
                ? null
                : new DateTimeOffset(
                    observations.Max(item => item.LastSeenAtUtc),
                    TimeSpan.Zero),
            Notices = 국내농산물경락가격조회Service.DefaultNotices
        };
    }

    private static 국내농산물경락가격관측 Map(
        long runId,
        국내농산물경락가격항목 item)
        => new()
        {
            FirstCollectionRunId = runId,
            RecordKey = item.RecordKey,
            SourceKey = item.SourceKey,
            SettlementDate = item.SettlementDate,
            WholesaleMarketCode = item.WholesaleMarketCode,
            CorporationCode = item.CorporationCode,
            SlipNumber = item.SlipNumber,
            AuctionSequence1 = item.AuctionSequence1,
            AuctionSequence2 = item.AuctionSequence2,
            TradingMethodCode = item.TradingMethodCode,
            LargeCategoryCode = item.LargeCategoryCode,
            MiddleCategoryCode = item.MiddleCategoryCode,
            SmallCategoryCode = item.SmallCategoryCode,
            CorporationItemCode = item.CorporationItemCode,
            ItemName = item.ItemName,
            VarietyName = item.VarietyName,
            UnitWeight = item.UnitWeight,
            UnitCode = item.UnitCode,
            PackageCode = item.PackageCode,
            SizeCode = item.SizeCode,
            GradeCode = item.GradeCode,
            Quantity = item.Quantity,
            AuctionPriceKrw = item.AuctionPriceKrw,
            OriginCode = item.OriginCode,
            OriginName = item.OriginName,
            TotalQuantity = item.TotalQuantity,
            TotalAmountKrw = item.TotalAmountKrw,
            AwardedTime = item.AwardedTime,
            FirstCollectedAtUtc = item.CollectedAtUtc.UtcDateTime,
            LastSeenAtUtc = item.CollectedAtUtc.UtcDateTime
        };

    private static bool Apply(
        국내농산물경락가격관측 target,
        국내농산물경락가격항목 source)
    {
        var changed = target.ItemName != source.ItemName
                      || target.VarietyName != source.VarietyName
                      || target.UnitWeight != source.UnitWeight
                      || target.UnitCode != source.UnitCode
                      || target.PackageCode != source.PackageCode
                      || target.SizeCode != source.SizeCode
                      || target.GradeCode != source.GradeCode
                      || target.Quantity != source.Quantity
                      || target.AuctionPriceKrw != source.AuctionPriceKrw
                      || target.OriginCode != source.OriginCode
                      || target.OriginName != source.OriginName
                      || target.TotalQuantity != source.TotalQuantity
                      || target.TotalAmountKrw != source.TotalAmountKrw
                      || target.AwardedTime != source.AwardedTime;

        target.ItemName = source.ItemName;
        target.VarietyName = source.VarietyName;
        target.UnitWeight = source.UnitWeight;
        target.UnitCode = source.UnitCode;
        target.PackageCode = source.PackageCode;
        target.SizeCode = source.SizeCode;
        target.GradeCode = source.GradeCode;
        target.Quantity = source.Quantity;
        target.AuctionPriceKrw = source.AuctionPriceKrw;
        target.OriginCode = source.OriginCode;
        target.OriginName = source.OriginName;
        target.TotalQuantity = source.TotalQuantity;
        target.TotalAmountKrw = source.TotalAmountKrw;
        target.AwardedTime = source.AwardedTime;
        target.LastSeenAtUtc = source.CollectedAtUtc.UtcDateTime;
        return changed;
    }

    private static 국내농산물경락가격항목 Map(
        국내농산물경락가격관측 observation)
        => new()
        {
            RecordKey = observation.RecordKey,
            SourceKey = observation.SourceKey,
            SettlementDate = observation.SettlementDate,
            WholesaleMarketCode = observation.WholesaleMarketCode,
            CorporationCode = observation.CorporationCode,
            SlipNumber = observation.SlipNumber,
            AuctionSequence1 = observation.AuctionSequence1,
            AuctionSequence2 = observation.AuctionSequence2,
            TradingMethodCode = observation.TradingMethodCode,
            LargeCategoryCode = observation.LargeCategoryCode,
            MiddleCategoryCode = observation.MiddleCategoryCode,
            SmallCategoryCode = observation.SmallCategoryCode,
            CorporationItemCode = observation.CorporationItemCode,
            ItemName = observation.ItemName,
            VarietyName = observation.VarietyName,
            UnitWeight = observation.UnitWeight,
            UnitCode = observation.UnitCode,
            PackageCode = observation.PackageCode,
            SizeCode = observation.SizeCode,
            GradeCode = observation.GradeCode,
            Quantity = observation.Quantity,
            AuctionPriceKrw = observation.AuctionPriceKrw,
            OriginCode = observation.OriginCode,
            OriginName = observation.OriginName,
            TotalQuantity = observation.TotalQuantity,
            TotalAmountKrw = observation.TotalAmountKrw,
            AwardedTime = observation.AwardedTime,
            CollectedAtUtc = new DateTimeOffset(
                observation.LastSeenAtUtc,
                TimeSpan.Zero)
        };

    private static 국내농산물경락가격수집Result Result(
        국내농산물경락가격수집Run run)
        => new(
            run.Id,
            run.FetchedCount,
            run.InsertedCount,
            run.UpdatedCount,
            run.ExistingCount,
            run.CompletedPages,
            run.IsTruncated);

    private static 국내농산물경락가격조회응답 Fail(
        국내농산물경락가격조회요청 request,
        string message)
        => new()
        {
            StatusCode = 국내농산물경락가격조회상태Codes.잘못된요청,
            ErrorMessage = message,
            Query = request,
            Notices = 국내농산물경락가격조회Service.DefaultNotices
        };
}
