using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Hongdal.Contracts.Common.TraditionalMarkets;
using Hongdal.Domain.TraditionalMarkets;
using Hongdal.Infrastructure.Persistence.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.Services.External.PublicData;
using 홍달.Services.Options;

namespace Hongdal.Services.TraditionalMarkets;

public interface ITraditionalMarketCatalogService
{
    Task<TraditionalMarketListResponse> SearchAsync(
        TraditionalMarketSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<TraditionalMarketResponse?> GetAsync(
        string marketCode,
        CancellationToken cancellationToken = default);

    Task<TraditionalMarketSyncResponse> SyncAsync(CancellationToken cancellationToken = default);
}

public sealed class TraditionalMarketCatalogService : ITraditionalMarketCatalogService
{
    private static readonly SemaphoreSlim SyncGate = new(1, 1);

    private readonly TraditionalMarketDbContext _db;
    private readonly ITraditionalMarketPublicDataClient _publicDataClient;
    private readonly PublicDataOptions _options;

    public TraditionalMarketCatalogService(
        TraditionalMarketDbContext db,
        ITraditionalMarketPublicDataClient publicDataClient,
        IOptions<PublicDataOptions> options)
    {
        _db = db;
        _publicDataClient = publicDataClient;
        _options = options.Value;
    }

    public async Task<TraditionalMarketListResponse> SearchAsync(
        TraditionalMarketSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = _db.Markets.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword)
                || x.RoadAddress.Contains(keyword)
                || x.LotNumberAddress.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(request.Province))
        {
            var province = request.Province.Trim();
            query = query.Where(x => x.Province == province);
        }

        if (!string.IsNullOrWhiteSpace(request.CityCounty))
        {
            var cityCounty = request.CityCounty.Trim();
            query = query.Where(x => x.CityCounty == cityCounty);
        }

        if (!string.IsNullOrWhiteSpace(request.MarketType))
        {
            var marketType = request.MarketType.Trim();
            query = query.Where(x => x.MarketType == marketType);
        }

        if (request.HasSharedLogisticsWarehouse.HasValue)
        {
            var value = request.HasSharedLogisticsWarehouse.Value;
            query = query.Where(x => x.Facilities.HasSharedLogisticsWarehouse == value);
        }

        if (request.HasDedicatedParking.HasValue)
        {
            var value = request.HasDedicatedParking.Value;
            query = query.Where(x => x.Facilities.HasDedicatedParking == value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var markets = await query
            .OrderBy(x => x.Province)
            .ThenBy(x => x.CityCounty)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var source = await _db.Markets
            .AsNoTracking()
            .Where(x => x.SourceDatasetKey == DatasetKey)
            .OrderByDescending(x => x.LastSyncedAtUtc)
            .Select(x => new { x.LastSyncedAtUtc, x.SourceReferenceDate })
            .FirstOrDefaultAsync(cancellationToken);

        return new TraditionalMarketListResponse
        {
            Items = markets.Select(ToResponse).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            LastSyncedAtUtc = source?.LastSyncedAtUtc,
            SourceDatasetKey = DatasetKey,
            SourceReferenceDate = source?.SourceReferenceDate
        };
    }

    public async Task<TraditionalMarketResponse?> GetAsync(
        string marketCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(marketCode))
        {
            return null;
        }

        var normalizedCode = marketCode.Trim().ToLowerInvariant();
        var market = await _db.Markets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MarketCode == normalizedCode && x.IsActive, cancellationToken);
        return market is null ? null : ToResponse(market);
    }

    public async Task<TraditionalMarketSyncResponse> SyncAsync(CancellationToken cancellationToken = default)
    {
        await SyncGate.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var run = new TraditionalMarketSyncRun
            {
                Id = Guid.NewGuid(),
                Status = TraditionalMarketSyncStatuses.Running,
                SourceDatasetKey = DatasetKey,
                SourceReferenceDate = SourceReferenceDate,
                StartedAtUtc = now
            };
            _db.SyncRuns.Add(run);
            await _db.SaveChangesAsync(cancellationToken);

            try
            {
                var incoming = await FetchAllAsync(cancellationToken);
                run.FetchedCount = incoming.Count;

                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var existingMarkets = await _db.Markets
                    .Where(x => x.SourceDatasetKey == DatasetKey)
                    .ToListAsync(cancellationToken);
                var existing = existingMarkets.ToDictionary(
                    x => x.MarketCode,
                    StringComparer.OrdinalIgnoreCase);

                foreach (var item in incoming.Values)
                {
                    var hash = ComputeHash(item);
                    if (!existing.TryGetValue(item.MarketCode, out var market))
                    {
                        market = CreateEntity(item, hash, now);
                        _db.Markets.Add(market);
                        run.InsertedCount++;
                        continue;
                    }

                    if (!string.Equals(market.SourceHash, hash, StringComparison.Ordinal))
                    {
                        Apply(market, item, hash, now);
                        run.UpdatedCount++;
                    }
                    else
                    {
                        market.IsActive = true;
                        market.LastSyncedAtUtc = now;
                        market.SourceReferenceDate = SourceReferenceDate;
                        run.UnchangedCount++;
                    }
                }

                foreach (var market in existing.Values.Where(x => x.IsActive && !incoming.ContainsKey(x.MarketCode)))
                {
                    market.IsActive = false;
                    market.LastSyncedAtUtc = now;
                    market.UpdatedAtUtc = now;
                    run.DeactivatedCount++;
                }

                run.Status = TraditionalMarketSyncStatuses.Completed;
                run.CompletedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ToResponse(run);
            }
            catch (Exception ex)
            {
                _db.ChangeTracker.Clear();
                var failedRun = await _db.SyncRuns.FirstAsync(x => x.Id == run.Id, cancellationToken);
                failedRun.Status = TraditionalMarketSyncStatuses.Failed;
                failedRun.ErrorMessage = Truncate(ex.Message, 2000);
                failedRun.CompletedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                return ToResponse(failedRun);
            }
        }
        finally
        {
            SyncGate.Release();
        }
    }

    private async Task<Dictionary<string, TraditionalMarketPublicDataItem>> FetchAllAsync(
        CancellationToken cancellationToken)
    {
        var items = new Dictionary<string, TraditionalMarketPublicDataItem>(StringComparer.OrdinalIgnoreCase);
        var page = 1;
        var pageSize = Math.Clamp(_options.TraditionalMarket.PageSize, 10, 1000);

        while (page <= 1000)
        {
            var result = await _publicDataClient.FetchPageAsync(page, pageSize, cancellationToken);
            foreach (var item in result.Items)
            {
                var normalizedCode = item.MarketCode.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(normalizedCode))
                {
                    items[normalizedCode] = Normalize(item, normalizedCode);
                }
            }

            if (result.Items.Count == 0 || (result.TotalCount > 0 && items.Count >= result.TotalCount))
            {
                return items;
            }

            page++;
        }

        throw new InvalidOperationException("전통시장 공공데이터 페이지 수가 안전 한도를 초과했습니다.");
    }

    private TraditionalMarket CreateEntity(
        TraditionalMarketPublicDataItem item,
        string hash,
        DateTime now)
    {
        var market = new TraditionalMarket
        {
            MarketCode = item.MarketCode,
            CreatedAtUtc = now
        };
        Apply(market, item, hash, now);
        return market;
    }

    private void Apply(
        TraditionalMarket market,
        TraditionalMarketPublicDataItem item,
        string hash,
        DateTime now)
    {
        market.Name = item.Name;
        market.MarketType = item.MarketType;
        market.LotNumberAddress = item.LotNumberAddress;
        market.RoadAddress = item.RoadAddress;
        market.Province = item.Province;
        market.CityCounty = item.CityCounty;
        market.Facilities = ToEntity(item.Facilities);
        market.SourceDatasetKey = DatasetKey;
        market.SourceReferenceDate = SourceReferenceDate;
        market.SourceHash = hash;
        market.IsActive = true;
        market.LastSyncedAtUtc = now;
        market.UpdatedAtUtc = now;
    }

    private static TraditionalMarketPublicDataItem Normalize(
        TraditionalMarketPublicDataItem item,
        string marketCode)
        => new()
        {
            MarketCode = marketCode,
            Name = item.Name.Trim(),
            MarketType = item.MarketType.Trim(),
            LotNumberAddress = item.LotNumberAddress.Trim(),
            RoadAddress = item.RoadAddress.Trim(),
            Province = item.Province.Trim(),
            CityCounty = item.CityCounty.Trim(),
            Facilities = item.Facilities
        };

    private string DatasetKey
        => string.IsNullOrWhiteSpace(_options.TraditionalMarket.DatasetKey)
            ? "semas-traditional-market-status"
            : _options.TraditionalMarket.DatasetKey.Trim();

    private DateOnly SourceReferenceDate
        => DateOnly.TryParseExact(
            _options.TraditionalMarket.SourceReferenceDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var value)
                ? value
                : throw new InvalidOperationException(
                    "PublicData:TraditionalMarket:SourceReferenceDate는 yyyy-MM-dd 형식이어야 합니다.");

    private static string ComputeHash(TraditionalMarketPublicDataItem item)
    {
        var facilities = item.Facilities;
        var values = new object?[]
        {
            item.MarketCode, item.Name, item.MarketType, item.LotNumberAddress, item.RoadAddress,
            item.Province, item.CityCounty,
            facilities.HasArcade, facilities.HasElevatorOrEscalator, facilities.HasCustomerSupportCenter,
            facilities.HasSprinkler, facilities.HasFireDetector, facilities.HasChildrenPlayroom,
            facilities.HasCallCenter, facilities.HasCustomerLounge, facilities.HasNursingCenter,
            facilities.HasLocker, facilities.HasBicycleStorage, facilities.HasSportsFacility,
            facilities.HasLibrary, facilities.HasShoppingCart, facilities.HasForeignVisitorCenter,
            facilities.HasCustomerPath, facilities.HasBroadcastCenter, facilities.HasCultureClassroom,
            facilities.HasSharedLogisticsWarehouse, facilities.HasDedicatedParking,
            facilities.HasTrainingRoom, facilities.HasMeetingRoom, facilities.HasAed
        };
        var canonical = string.Join('\u001f', values.Select(x => x?.ToString() ?? string.Empty));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static TraditionalMarketResponse ToResponse(TraditionalMarket market)
    {
        var facilities = ToResponse(market.Facilities);
        return new TraditionalMarketResponse
        {
            MarketCode = market.MarketCode,
            CommunityScopeKey = TraditionalMarketCommunityScopes.Create(market.MarketCode),
            Name = market.Name,
            MarketType = market.MarketType,
            LotNumberAddress = market.LotNumberAddress,
            RoadAddress = market.RoadAddress,
            Province = market.Province,
            CityCounty = market.CityCounty,
            Facilities = facilities,
            AvailableFacilityCount = CountAvailableFacilities(facilities),
            IsActive = market.IsActive,
            SourceReferenceDate = market.SourceReferenceDate,
            LastSyncedAtUtc = market.LastSyncedAtUtc
        };
    }

    private static TraditionalMarketSyncResponse ToResponse(TraditionalMarketSyncRun run)
        => new()
        {
            RunId = run.Id,
            Status = run.Status,
            SourceDatasetKey = run.SourceDatasetKey,
            SourceReferenceDate = run.SourceReferenceDate,
            FetchedCount = run.FetchedCount,
            InsertedCount = run.InsertedCount,
            UpdatedCount = run.UpdatedCount,
            UnchangedCount = run.UnchangedCount,
            DeactivatedCount = run.DeactivatedCount,
            ErrorMessage = run.ErrorMessage,
            StartedAtUtc = run.StartedAtUtc,
            CompletedAtUtc = run.CompletedAtUtc
        };

    private static TraditionalMarketFacilities ToEntity(TraditionalMarketFacilityResponse value)
        => new()
        {
            HasArcade = value.HasArcade,
            HasElevatorOrEscalator = value.HasElevatorOrEscalator,
            HasCustomerSupportCenter = value.HasCustomerSupportCenter,
            HasSprinkler = value.HasSprinkler,
            HasFireDetector = value.HasFireDetector,
            HasChildrenPlayroom = value.HasChildrenPlayroom,
            HasCallCenter = value.HasCallCenter,
            HasCustomerLounge = value.HasCustomerLounge,
            HasNursingCenter = value.HasNursingCenter,
            HasLocker = value.HasLocker,
            HasBicycleStorage = value.HasBicycleStorage,
            HasSportsFacility = value.HasSportsFacility,
            HasLibrary = value.HasLibrary,
            HasShoppingCart = value.HasShoppingCart,
            HasForeignVisitorCenter = value.HasForeignVisitorCenter,
            HasCustomerPath = value.HasCustomerPath,
            HasBroadcastCenter = value.HasBroadcastCenter,
            HasCultureClassroom = value.HasCultureClassroom,
            HasSharedLogisticsWarehouse = value.HasSharedLogisticsWarehouse,
            HasDedicatedParking = value.HasDedicatedParking,
            HasTrainingRoom = value.HasTrainingRoom,
            HasMeetingRoom = value.HasMeetingRoom,
            HasAed = value.HasAed
        };

    private static TraditionalMarketFacilityResponse ToResponse(TraditionalMarketFacilities value)
        => new()
        {
            HasArcade = value.HasArcade,
            HasElevatorOrEscalator = value.HasElevatorOrEscalator,
            HasCustomerSupportCenter = value.HasCustomerSupportCenter,
            HasSprinkler = value.HasSprinkler,
            HasFireDetector = value.HasFireDetector,
            HasChildrenPlayroom = value.HasChildrenPlayroom,
            HasCallCenter = value.HasCallCenter,
            HasCustomerLounge = value.HasCustomerLounge,
            HasNursingCenter = value.HasNursingCenter,
            HasLocker = value.HasLocker,
            HasBicycleStorage = value.HasBicycleStorage,
            HasSportsFacility = value.HasSportsFacility,
            HasLibrary = value.HasLibrary,
            HasShoppingCart = value.HasShoppingCart,
            HasForeignVisitorCenter = value.HasForeignVisitorCenter,
            HasCustomerPath = value.HasCustomerPath,
            HasBroadcastCenter = value.HasBroadcastCenter,
            HasCultureClassroom = value.HasCultureClassroom,
            HasSharedLogisticsWarehouse = value.HasSharedLogisticsWarehouse,
            HasDedicatedParking = value.HasDedicatedParking,
            HasTrainingRoom = value.HasTrainingRoom,
            HasMeetingRoom = value.HasMeetingRoom,
            HasAed = value.HasAed
        };

    private static int CountAvailableFacilities(TraditionalMarketFacilityResponse value)
        => new bool?[]
        {
            value.HasArcade, value.HasElevatorOrEscalator, value.HasCustomerSupportCenter,
            value.HasSprinkler, value.HasFireDetector, value.HasChildrenPlayroom, value.HasCallCenter,
            value.HasCustomerLounge, value.HasNursingCenter, value.HasLocker, value.HasBicycleStorage,
            value.HasSportsFacility, value.HasLibrary, value.HasShoppingCart, value.HasForeignVisitorCenter,
            value.HasCustomerPath, value.HasBroadcastCenter, value.HasCultureClassroom,
            value.HasSharedLogisticsWarehouse, value.HasDedicatedParking, value.HasTrainingRoom,
            value.HasMeetingRoom, value.HasAed
        }.Count(x => x == true);

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
