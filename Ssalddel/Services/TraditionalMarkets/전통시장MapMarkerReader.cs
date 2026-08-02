using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Infrastructure.Persistence.TraditionalMarkets;

namespace Ssalddel.Services.TraditionalMarkets;

public interface I전통시장MapMarkerReader
{
    Task<IReadOnlyList<TraditionalMarketMapMarkerResponse>> 공개Marker조회Async(
        CancellationToken cancellationToken = default);
}

public sealed class 전통시장MapMarkerReader(TraditionalMarketDbContext db) : I전통시장MapMarkerReader
{
    public async Task<IReadOnlyList<TraditionalMarketMapMarkerResponse>> 공개Marker조회Async(
        CancellationToken cancellationToken = default)
    {
        var rows = await (from hub in db.LogisticsHubs.AsNoTracking()
                          join market in db.Markets.AsNoTracking() on hub.MarketCode equals market.MarketCode
                          where market.IsActive
                                && (hub.Status == TraditionalMarketLogisticsHubStatuses.Pilot
                                    || hub.Status == TraditionalMarketLogisticsHubStatuses.Active)
                                && hub.HasOperatorConsent
                                && hub.SiteVerifiedAtUtc.HasValue
                                && hub.MapLatitude.HasValue
                                && hub.MapLongitude.HasValue
                                && hub.MapLocationVerifiedAtUtc.HasValue
                          orderby market.Province, market.CityCounty, market.Name
                          select new { Hub = hub, Market = market })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new TraditionalMarketMapMarkerResponse
            {
                MarketCode = row.Market.MarketCode,
                MarketName = row.Market.Name,
                CommunityScopeKey = TraditionalMarketCommunityScopes.Create(row.Market.MarketCode),
                HubReferenceKey = TraditionalMarketLogisticsHubReferences.Create(row.Market.MarketCode),
                Province = row.Market.Province,
                CityCounty = row.Market.CityCounty,
                Status = row.Hub.Status,
                ServiceRadiusKm = row.Hub.ServiceRadiusKm,
                DailyGroupPurchaseCapacity = row.Hub.DailyGroupPurchaseCapacity,
                SupportsResidentPickup = row.Hub.SupportsResidentPickup,
                SupportsLastMileDelivery = row.Hub.SupportsLastMileDelivery,
                SupportsRefrigeratedStorage = row.Hub.SupportsRefrigeratedStorage,
                SupportsFrozenStorage = row.Hub.SupportsFrozenStorage,
                MapAnchor = new TraditionalMarketMapAnchorResponse
                {
                    Latitude = row.Hub.MapLatitude!.Value,
                    Longitude = row.Hub.MapLongitude!.Value,
                    LocationPrecisionCode = row.Hub.MapLocationPrecisionCode,
                    SourceName = row.Hub.MapLocationSourceName,
                    SourceHref = row.Hub.MapLocationSourceHref,
                    VerifiedAtUtc = row.Hub.MapLocationVerifiedAtUtc!.Value
                }
            })
            .ToArray();
    }
}
