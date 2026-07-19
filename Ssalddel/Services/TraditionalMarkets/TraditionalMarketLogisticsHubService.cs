using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Domain.TraditionalMarkets;
using Ssalddel.Infrastructure.Persistence.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;

namespace Ssalddel.Services.TraditionalMarkets;

public interface ITraditionalMarketLogisticsHubService
{
    Task<TraditionalMarketLogisticsHubListResponse> SearchAsync(
        TraditionalMarketLogisticsHubSearchRequest request,
        bool includeNonPublic,
        CancellationToken cancellationToken = default);

    Task<TraditionalMarketLogisticsHubResponse?> GetAsync(
        string marketCode,
        bool includeNonPublic,
        CancellationToken cancellationToken = default);

    Task<TraditionalMarketLogisticsHubResponse> UpsertAsync(
        string marketCode,
        TraditionalMarketLogisticsHubUpsertRequest request,
        string actorUserId,
        CancellationToken cancellationToken = default);

    Task<TraditionalMarketLogisticsHubResponse> ChangeStatusAsync(
        string marketCode,
        TraditionalMarketLogisticsHubStatusChangeRequest request,
        string actorUserId,
        CancellationToken cancellationToken = default);
}

public sealed class TraditionalMarketLogisticsHubConcurrencyException : Exception
{
    public TraditionalMarketLogisticsHubConcurrencyException(string message)
        : base(message)
    {
    }

    public TraditionalMarketLogisticsHubConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class TraditionalMarketLogisticsHubService : ITraditionalMarketLogisticsHubService
{
    private static readonly string[] PublicStatuses =
    [
        TraditionalMarketLogisticsHubStatuses.Pilot,
        TraditionalMarketLogisticsHubStatuses.Active
    ];

    private readonly TraditionalMarketDbContext _db;

    public TraditionalMarketLogisticsHubService(TraditionalMarketDbContext db)
    {
        _db = db;
    }

    public async Task<TraditionalMarketLogisticsHubListResponse> SearchAsync(
        TraditionalMarketLogisticsHubSearchRequest request,
        bool includeNonPublic,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = from hub in _db.LogisticsHubs.AsNoTracking()
                    join market in _db.Markets.AsNoTracking() on hub.MarketCode equals market.MarketCode
                    select new { Hub = hub, Market = market };

        if (!includeNonPublic)
        {
            query = query.Where(x => PublicStatuses.Contains(x.Hub.Status));
        }

        var requestedStatus = TraditionalMarketLogisticsHubStatuses.Normalize(request.Status ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (string.IsNullOrEmpty(requestedStatus))
            {
                throw new InvalidOperationException("지원하지 않는 물류 거점 상태입니다.");
            }

            query = query.Where(x => x.Hub.Status == requestedStatus);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                x.Market.Name.Contains(keyword)
                || x.Market.RoadAddress.Contains(keyword)
                || x.Market.LotNumberAddress.Contains(keyword)
                || x.Hub.OperatorOrganizationName.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(request.Province))
        {
            var province = request.Province.Trim();
            query = query.Where(x => x.Market.Province == province);
        }

        if (!string.IsNullOrWhiteSpace(request.CityCounty))
        {
            var cityCounty = request.CityCounty.Trim();
            query = query.Where(x => x.Market.CityCounty == cityCounty);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.Hub.Status == TraditionalMarketLogisticsHubStatuses.Active ? 0 : 1)
            .ThenBy(x => x.Market.Province)
            .ThenBy(x => x.Market.CityCounty)
            .ThenBy(x => x.Market.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new TraditionalMarketLogisticsHubListResponse
        {
            Items = rows.Select(x => ToResponse(x.Hub, x.Market)).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TraditionalMarketLogisticsHubResponse?> GetAsync(
        string marketCode,
        bool includeNonPublic,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeMarketCode(marketCode);
        var row = await (from hub in _db.LogisticsHubs.AsNoTracking()
                         join market in _db.Markets.AsNoTracking() on hub.MarketCode equals market.MarketCode
                         where hub.MarketCode == normalizedCode
                         select new { Hub = hub, Market = market })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null || (!includeNonPublic && !PublicStatuses.Contains(row.Hub.Status)))
        {
            return null;
        }

        return ToResponse(row.Hub, row.Market);
    }

    public async Task<TraditionalMarketLogisticsHubResponse> UpsertAsync(
        string marketCode,
        TraditionalMarketLogisticsHubUpsertRequest request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var normalizedCode = NormalizeMarketCode(marketCode);
        var market = await _db.Markets.FirstOrDefaultAsync(
            x => x.MarketCode == normalizedCode,
            cancellationToken);
        if (market is null)
        {
            throw new KeyNotFoundException("전통시장 기준정보를 찾을 수 없습니다.");
        }

        var hub = await _db.LogisticsHubs.FirstOrDefaultAsync(
            x => x.MarketCode == normalizedCode,
            cancellationToken);
        var now = DateTime.UtcNow;
        if (hub is null)
        {
            if (!market.IsActive)
            {
                throw new InvalidOperationException("비활성 전통시장은 새 물류 거점 후보로 등록할 수 없습니다.");
            }

            hub = new TraditionalMarketLogisticsHub
            {
                MarketCode = normalizedCode,
                Status = TraditionalMarketLogisticsHubStatuses.Candidate,
                CreatedAtUtc = now,
                StatusChangedAtUtc = now
            };
            _db.LogisticsHubs.Add(hub);
        }
        else
        {
            EnsureRevision(hub, request.ExpectedRevision);
        }

        var hadConsent = hub.HasOperatorConsent;
        var wasVerified = hub.SiteVerifiedAtUtc.HasValue;
        hub.OperatorOrganizationName = request.OperatorOrganizationName?.Trim() ?? string.Empty;
        hub.ServiceRadiusKm = request.ServiceRadiusKm;
        hub.DailyGroupPurchaseCapacity = request.DailyGroupPurchaseCapacity;
        hub.SupportsBulkReceiving = request.SupportsBulkReceiving;
        hub.SupportsSorting = request.SupportsSorting;
        hub.SupportsResidentPickup = request.SupportsResidentPickup;
        hub.SupportsLastMileDelivery = request.SupportsLastMileDelivery;
        hub.SupportsRefrigeratedStorage = request.SupportsRefrigeratedStorage;
        hub.SupportsFrozenStorage = request.SupportsFrozenStorage;
        hub.ReceivingWindow = request.ReceivingWindow?.Trim() ?? string.Empty;
        hub.PickupWindow = request.PickupWindow?.Trim() ?? string.Empty;
        hub.OperatingNotes = request.OperatingNotes?.Trim() ?? string.Empty;
        hub.HasOperatorConsent = request.HasOperatorConsent;
        hub.OperatorConsentedAtUtc = request.HasOperatorConsent
            ? hadConsent ? hub.OperatorConsentedAtUtc : now
            : null;
        hub.SiteVerifiedAtUtc = request.IsSiteVerified
            ? wasVerified ? hub.SiteVerifiedAtUtc : now
            : null;
        hub.SiteVerifiedByUserId = request.IsSiteVerified
            ? wasVerified ? hub.SiteVerifiedByUserId : NormalizeActor(actorUserId)
            : string.Empty;
        hub.UpdatedByUserId = NormalizeActor(actorUserId);
        hub.UpdatedAtUtc = now;
        hub.Revision++;

        if (TraditionalMarketLogisticsHubStatuses.Public.Contains(hub.Status))
        {
            var readinessError = TraditionalMarketLogisticsHubPolicy.GetReadinessError(hub);
            if (readinessError is not null)
            {
                throw new InvalidOperationException($"운영 중인 거점 정보를 해당 상태로 변경할 수 없습니다. {readinessError}");
            }
        }

        await SaveChangesAsync(cancellationToken);
        return ToResponse(hub, market);
    }

    public async Task<TraditionalMarketLogisticsHubResponse> ChangeStatusAsync(
        string marketCode,
        TraditionalMarketLogisticsHubStatusChangeRequest request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeMarketCode(marketCode);
        var targetStatus = TraditionalMarketLogisticsHubStatuses.Normalize(request.Status);
        if (string.IsNullOrEmpty(targetStatus))
        {
            throw new InvalidOperationException("지원하지 않는 물류 거점 상태입니다.");
        }

        if ((request.Reason?.Length ?? 0) > 500)
        {
            throw new InvalidOperationException("상태 변경 사유는 500자를 넘을 수 없습니다.");
        }

        var market = await _db.Markets.FirstOrDefaultAsync(
            x => x.MarketCode == normalizedCode,
            cancellationToken);
        var hub = await _db.LogisticsHubs.FirstOrDefaultAsync(
            x => x.MarketCode == normalizedCode,
            cancellationToken);
        if (market is null || hub is null)
        {
            throw new KeyNotFoundException("전통시장 물류 거점 후보를 찾을 수 없습니다.");
        }

        EnsureRevision(hub, request.ExpectedRevision);
        if (!TraditionalMarketLogisticsHubPolicy.CanTransition(hub.Status, targetStatus))
        {
            throw new InvalidOperationException($"{hub.Status} 상태에서 {targetStatus} 상태로 전환할 수 없습니다.");
        }

        if (TraditionalMarketLogisticsHubStatuses.Public.Contains(targetStatus))
        {
            var readinessError = TraditionalMarketLogisticsHubPolicy.GetReadinessError(hub);
            if (readinessError is not null)
            {
                throw new InvalidOperationException(readinessError);
            }
        }

        if ((targetStatus == TraditionalMarketLogisticsHubStatuses.Paused
             || targetStatus == TraditionalMarketLogisticsHubStatuses.Closed)
            && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException("중단 또는 종료 상태에는 변경 사유가 필요합니다.");
        }

        var now = DateTime.UtcNow;
        hub.Status = targetStatus;
        hub.StatusReason = request.Reason?.Trim() ?? string.Empty;
        hub.StatusChangedAtUtc = now;
        hub.UpdatedAtUtc = now;
        hub.UpdatedByUserId = NormalizeActor(actorUserId);
        hub.Revision++;
        await SaveChangesAsync(cancellationToken);
        return ToResponse(hub, market);
    }

    private static TraditionalMarketLogisticsHubResponse ToResponse(
        TraditionalMarketLogisticsHub hub,
        TraditionalMarket market)
        => new()
        {
            MarketCode = market.MarketCode,
            MarketName = market.Name,
            CommunityScopeKey = TraditionalMarketCommunityScopes.Create(market.MarketCode),
            HubReferenceKey = TraditionalMarketLogisticsHubReferences.Create(market.MarketCode),
            RoadAddress = market.RoadAddress,
            LotNumberAddress = market.LotNumberAddress,
            Province = market.Province,
            CityCounty = market.CityCounty,
            Status = hub.Status,
            OperatorOrganizationName = hub.OperatorOrganizationName,
            ServiceRadiusKm = hub.ServiceRadiusKm,
            DailyGroupPurchaseCapacity = hub.DailyGroupPurchaseCapacity,
            SupportsBulkReceiving = hub.SupportsBulkReceiving,
            SupportsSorting = hub.SupportsSorting,
            SupportsResidentPickup = hub.SupportsResidentPickup,
            SupportsLastMileDelivery = hub.SupportsLastMileDelivery,
            SupportsRefrigeratedStorage = hub.SupportsRefrigeratedStorage,
            SupportsFrozenStorage = hub.SupportsFrozenStorage,
            ReceivingWindow = hub.ReceivingWindow,
            PickupWindow = hub.PickupWindow,
            OperatingNotes = hub.OperatingNotes,
            HasOperatorConsent = hub.HasOperatorConsent,
            OperatorConsentedAtUtc = hub.OperatorConsentedAtUtc,
            SiteVerifiedAtUtc = hub.SiteVerifiedAtUtc,
            StatusReason = hub.StatusReason,
            Revision = hub.Revision,
            CreatedAtUtc = hub.CreatedAtUtc,
            UpdatedAtUtc = hub.UpdatedAtUtc,
            StatusChangedAtUtc = hub.StatusChangedAtUtc
        };

    private static void ValidateRequest(TraditionalMarketLogisticsHubUpsertRequest request)
    {
        if ((request.OperatorOrganizationName?.Length ?? 0) > 160)
        {
            throw new InvalidOperationException("운영주체명은 160자를 넘을 수 없습니다.");
        }

        if (request.ServiceRadiusKm is < 0 or > 100)
        {
            throw new InvalidOperationException("생활권 서비스 반경은 0km 이상 100km 이하여야 합니다.");
        }

        if (request.DailyGroupPurchaseCapacity is < 0 or > 100000)
        {
            throw new InvalidOperationException("일일 공동구매 처리 용량 범위가 올바르지 않습니다.");
        }

        if ((request.ReceivingWindow?.Length ?? 0) > 160
            || (request.PickupWindow?.Length ?? 0) > 160)
        {
            throw new InvalidOperationException("입고·수령 운영시간은 각각 160자를 넘을 수 없습니다.");
        }

        if ((request.OperatingNotes?.Length ?? 0) > 2000)
        {
            throw new InvalidOperationException("운영 메모는 2000자를 넘을 수 없습니다.");
        }
    }

    private static void EnsureRevision(TraditionalMarketLogisticsHub hub, long? expectedRevision)
    {
        if (expectedRevision.HasValue && hub.Revision != expectedRevision.Value)
        {
            throw new TraditionalMarketLogisticsHubConcurrencyException(
                $"물류 거점 정보가 이미 변경되었습니다. 현재 revision은 {hub.Revision}입니다.");
        }
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new TraditionalMarketLogisticsHubConcurrencyException(
                "물류 거점 정보가 다른 요청에서 먼저 변경되었습니다.",
                ex);
        }
    }

    private static string NormalizeMarketCode(string marketCode)
    {
        if (string.IsNullOrWhiteSpace(marketCode))
        {
            throw new InvalidOperationException("시장코드가 필요합니다.");
        }

        return marketCode.Trim().ToLowerInvariant();
    }

    private static string NormalizeActor(string actorUserId)
        => string.IsNullOrWhiteSpace(actorUserId) ? "system" : actorUserId.Trim();
}
