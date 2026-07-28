using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Restaurants;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public sealed record CommunityNearbyRestaurantLookupResult(
    bool SourceAvailable,
    bool IsSimulationSource,
    IReadOnlyList<음식점요약응답> Items);

public interface ICommunityNearbyRestaurantDirectory
{
    Task<CommunityNearbyRestaurantLookupResult> FindAsync(
        decimal latitude,
        decimal longitude,
        decimal radiusKm,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed class MainServerCommunityNearbyRestaurantDirectory(
    SsalddelContext db) : ICommunityNearbyRestaurantDirectory
{
    public async Task<CommunityNearbyRestaurantLookupResult> FindAsync(
        decimal latitude,
        decimal longitude,
        decimal radiusKm,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var appliedRadius = Math.Clamp(radiusKm, 0.1m, 7m);
        var appliedLimit = Math.Clamp(limit, 1, 50);
        var radius = (double)appliedRadius;
        var latitudeDelta = (decimal)(radius / 110.574d);
        var longitudeDivisor = 111.320d * Math.Abs(Math.Cos((double)latitude * Math.PI / 180d));
        var longitudeDelta = (decimal)(radius / Math.Max(longitudeDivisor, 0.001d));
        var minimumLatitude = latitude - latitudeDelta;
        var maximumLatitude = latitude + latitudeDelta;
        var minimumLongitude = longitude - longitudeDelta;
        var maximumLongitude = longitude + longitudeDelta;

        var candidates = await db.음식점공개프로필
            .AsNoTracking()
            .Where(item =>
                item.공개여부
                && item.위도 >= minimumLatitude
                && item.위도 <= maximumLatitude
                && item.경도 >= minimumLongitude
                && item.경도 <= maximumLongitude)
            .Select(item => new RestaurantCandidate(
                item.Id,
                item.상호명,
                item.카테고리,
                item.공개주소,
                item.대표이미지Url,
                item.위도,
                item.경도,
                item.주문가능여부))
            .ToArrayAsync(cancellationToken);

        var withinRadius = candidates
            .Select(item => new
            {
                Item = item,
                DistanceKm = DistanceKm(latitude, longitude, item.Latitude, item.Longitude)
            })
            .Where(item => item.DistanceKm <= appliedRadius)
            .OrderBy(item => item.DistanceKm)
            .ThenBy(item => item.Item.Name, StringComparer.Ordinal)
            .Take(appliedLimit)
            .ToArray();
        var restaurantIds = withinRadius
            .Select(item => item.Item.Id)
            .Distinct()
            .ToList();
        var now = DateTime.UtcNow;
        var reviews = await db.음식점리뷰
            .AsNoTracking()
            .Where(item =>
                restaurantIds.Contains(item.음식점Id)
                && item.현재노출여부
                && (!item.게시종료일시Utc.HasValue || item.게시종료일시Utc > now))
            .Select(item => new { item.음식점Id, item.별점 })
            .ToArrayAsync(cancellationToken);
        var reviewStats = reviews
            .GroupBy(item => item.음식점Id)
            .ToDictionary(
                group => group.Key,
                group => new ReviewStats(
                    Math.Round(group.Average(item => (decimal)item.별점), 1),
                    group.Count()));
        var items = withinRadius
            .Select(item =>
            {
                var stats = reviewStats.GetValueOrDefault(item.Item.Id);
                return new 음식점요약응답
                {
                    Id = item.Item.Id,
                    상호명 = item.Item.Name,
                    카테고리 = item.Item.Category,
                    주소 = item.Item.PublicAddress,
                    대표이미지Url = item.Item.ImageUrl,
                    위도 = item.Item.Latitude,
                    경도 = item.Item.Longitude,
                    거리Km = item.DistanceKm,
                    평균평점 = stats?.AverageRating ?? 0m,
                    리뷰수 = stats?.ReviewCount ?? 0,
                    주문가능여부 = item.Item.IsOrderAvailable,
                    저평점주의필요 = stats is { ReviewCount: >= 3, AverageRating: < 3m }
                };
            })
            .ToArray();

        return new CommunityNearbyRestaurantLookupResult(
            SourceAvailable: true,
            IsSimulationSource: false,
            Items: items);
    }

    private static decimal DistanceKm(
        decimal latitude1,
        decimal longitude1,
        decimal latitude2,
        decimal longitude2)
    {
        const double earthRadiusKm = 6371.0088d;
        var lat1 = (double)latitude1 * Math.PI / 180d;
        var lat2 = (double)latitude2 * Math.PI / 180d;
        var deltaLatitude = ((double)latitude2 - (double)latitude1) * Math.PI / 180d;
        var deltaLongitude = ((double)longitude2 - (double)longitude1) * Math.PI / 180d;
        var haversine = Math.Pow(Math.Sin(deltaLatitude / 2d), 2d)
                        + Math.Cos(lat1) * Math.Cos(lat2)
                        * Math.Pow(Math.Sin(deltaLongitude / 2d), 2d);
        var normalized = Math.Clamp(haversine, 0d, 1d);
        var distance = earthRadiusKm * 2d * Math.Atan2(
            Math.Sqrt(normalized),
            Math.Sqrt(1d - normalized));
        return Math.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
    }

    private sealed record RestaurantCandidate(
        long Id,
        string Name,
        string Category,
        string PublicAddress,
        string? ImageUrl,
        decimal Latitude,
        decimal Longitude,
        bool IsOrderAvailable);

    private sealed record ReviewStats(decimal AverageRating, int ReviewCount);
}
