using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.Content;
using 살뜰.Data;

namespace Ssalddel.Services.Content;

public interface I지역문화이미지MapMarkerReader
{
    Task<IReadOnlyList<지역문화이미지MapMarker>> 공개Marker조회Async(
        CancellationToken cancellationToken = default);
}

public sealed record 지역문화이미지MapMarker(
    string RegionKey,
    string CountryCode,
    string CountryName,
    string SubdivisionCode,
    string RegionName,
    double Latitude,
    double Longitude,
    string CultureSummary,
    DateTimeOffset ImageUpdatedAtUtc);

public sealed class 지역문화이미지MapMarkerReader(
    SsalddelContext db) : I지역문화이미지MapMarkerReader
{
    private const string ImagePackId = "regional-culture-one-each-v1";

    public async Task<IReadOnlyList<지역문화이미지MapMarker>> 공개Marker조회Async(
        CancellationToken cancellationToken = default)
    {
        var prompts = await db.지역문화이미지Prompts
            .AsNoTracking()
            .Where(item => item.RegionKey != "kr-seoul")
            .Select(item => new
            {
                item.RegionKey,
                item.CountryCode,
                item.SubdivisionCode,
                item.RegionNameKo,
                item.CultureSummaryKo
            })
            .ToDictionaryAsync(item => item.RegionKey, StringComparer.Ordinal, cancellationToken);
        var images = await db.앱문맥이미지자산들
            .AsNoTracking()
            .Where(item => item.앱PackId == ImagePackId
                && item.활성화여부
                && item.품질상태 != 앱문맥이미지품질상태.제외)
            .Select(item => new
            {
                item.장면Key,
                item.제목,
                item.대체Text,
                item.수정시각
            })
            .ToDictionaryAsync(item => item.장면Key, StringComparer.Ordinal, cancellationToken);

        return 지역문화행정구역대표점Catalog.All
            .Select(anchor =>
            {
                var sceneKey = $"{anchor.RegionKey}--scene-01";
                if (!images.TryGetValue(sceneKey, out var image))
                {
                    return null;
                }

                prompts.TryGetValue(anchor.RegionKey, out var prompt);
                var countryCode = prompt?.CountryCode ?? CountryCode(anchor.RegionKey);
                var regionName = !string.IsNullOrWhiteSpace(prompt?.RegionNameKo)
                    ? prompt.RegionNameKo
                    : !string.IsNullOrWhiteSpace(anchor.FallbackRegionName)
                        ? anchor.FallbackRegionName
                    : !string.IsNullOrWhiteSpace(image.제목)
                        ? image.제목
                        : anchor.RegionKey;
                var cultureSummary = !string.IsNullOrWhiteSpace(prompt?.CultureSummaryKo)
                    ? prompt.CultureSummaryKo
                    : image.대체Text;

                return new 지역문화이미지MapMarker(
                    anchor.RegionKey,
                    countryCode,
                    CountryName(countryCode),
                    prompt?.SubdivisionCode ?? string.Empty,
                    regionName,
                    anchor.Latitude,
                    anchor.Longitude,
                    cultureSummary,
                    image.수정시각);
            })
            .OfType<지역문화이미지MapMarker>()
            .OrderBy(item => item.RegionKey, StringComparer.Ordinal)
            .ToArray();
    }

    private static string CountryCode(string regionKey)
        => regionKey[..2].ToUpperInvariant();

    private static string CountryName(string countryCode)
        => countryCode switch
        {
            "KR" => "대한민국",
            "US" => "미국",
            "CN" => "중국",
            _ => countryCode
        };
}
