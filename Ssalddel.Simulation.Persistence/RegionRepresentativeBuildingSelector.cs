using System.Security.Cryptography;
using System.Text;
using Ssalddel.Domain.PublicData.Korea;

namespace Ssalddel.Simulation.Persistence;

public static class 지역대표건축물Selector
{
    public static IReadOnlyList<대표건축물선정항목> SelectOnePerCategory(
        string regionStableId,
        IReadOnlyList<건축물대장표제부Record> buildings,
        IReadOnlyDictionary<Guid, string> categories)
    {
        if (string.IsNullOrWhiteSpace(regionStableId))
            throw new ArgumentException("지역 고유 식별자가 필요합니다.", nameof(regionStableId));
        if (buildings.Count == 0) return Array.Empty<대표건축물선정항목>();

        return buildings
            .GroupBy(building => categories.TryGetValue(building.Id, out var category)
                ? category
                : "unresolved", StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var representative = group.OrderByDescending(QualityScore)
                    .ThenBy(building => StableOrder(regionStableId, building.Id), StringComparer.Ordinal)
                    .First();
                return new 대표건축물선정항목(
                    representative,
                    "building-category:" + group.Key,
                    group.Count(),
                    1);
            })
            .ToArray();
    }

    private static int QualityScore(건축물대장표제부Record building)
    {
        var score = !string.IsNullOrWhiteSpace(building.BuildingName) ? 100 : 0;
        if (building.BuildingAreaSquareMeters.HasValue) score += 10;
        if (building.TotalFloorAreaSquareMeters.HasValue) score += 8;
        if (building.AboveGroundFloorCount.HasValue) score += 6;
        if (building.HeightMeters.HasValue) score += 4;
        if (!string.IsNullOrWhiteSpace(building.RoadAddress)) score += 2;
        return score;
    }

    private static string StableOrder(string regionStableId, Guid buildingId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            "representative:" + regionStableId + ":" + buildingId.ToString("N"))))
            .ToLowerInvariant();
}
