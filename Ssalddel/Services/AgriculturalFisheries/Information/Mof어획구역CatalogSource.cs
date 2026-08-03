using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IMof어획구역CatalogSource
{
    Task<Mof어획구역CatalogSnapshot> 수집Async(
        CancellationToken cancellationToken = default);
}

public interface I해양수산Map바다Tile조회UseCase
{
    Task<MarineFishingAreaOceanTileResponse> 조회Async(
        CancellationToken cancellationToken = default);
}

public sealed record Mof어획구역CatalogRecord(
    string Abbreviation,
    string EnglishName,
    string KoreanName,
    string SeaName);

public sealed record Mof어획구역CatalogSnapshot(
    DateTime CollectedAtUtc,
    string ContentSha256,
    IReadOnlyList<Mof어획구역CatalogRecord> Records);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Infrastructure,
    "해양수산부 어획구역 CSV를 CP949로 수집하고 출처 해시와 함께 단기 보관",
    ContractType = typeof(IMof어획구역CatalogSource),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    Boundary = "공식 원문 수집 실패를 표본 데이터로 숨기지 않으며 좌표나 어획량을 추정하지 않습니다.")]
public sealed class Mof어획구역CatalogSource(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IOptions<PublicDataOptions> options,
    TimeProvider timeProvider) : IMof어획구역CatalogSource
{
    private const string CacheKey = "public-data:mof:fishing-area-catalog:v1";
    private static readonly SemaphoreSlim CollectionLock = new(1, 1);

    public async Task<Mof어획구역CatalogSnapshot> 수집Async(
        CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue<Mof어획구역CatalogSnapshot>(CacheKey, out var cached)
            && cached is not null)
        {
            return cached;
        }

        await CollectionLock.WaitAsync(cancellationToken);
        try
        {
            if (memoryCache.TryGetValue<Mof어획구역CatalogSnapshot>(CacheKey, out cached)
                && cached is not null)
            {
                return cached;
            }

            var configured = options.Value.MofFishingAreas;
            using var response = await httpClient.GetAsync(
                configured.DownloadPath,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var records = Parse(bytes);
            if (records.Count == 0)
            {
                throw new InvalidDataException(
                    "해양수산부 어획구역 원문에서 유효한 행을 읽지 못했습니다.");
            }

            var snapshot = new Mof어획구역CatalogSnapshot(
                timeProvider.GetUtcNow().UtcDateTime,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
                records);
            memoryCache.Set(
                CacheKey,
                snapshot,
                TimeSpan.FromHours(Math.Clamp(configured.CacheHours, 1, 168)));
            return snapshot;
        }
        finally
        {
            CollectionLock.Release();
        }
    }

    internal static IReadOnlyList<Mof어획구역CatalogRecord> Parse(byte[] bytes)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var csv = Encoding.GetEncoding(949).GetString(bytes);
        using var reader = new StringReader(csv);
        var result = new List<Mof어획구역CatalogRecord>();
        var firstRow = true;
        while (reader.ReadLine() is { } line)
        {
            if (firstRow)
            {
                firstRow = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = ParseCsvLine(line);
            if (columns.Count < 4)
            {
                continue;
            }

            result.Add(new Mof어획구역CatalogRecord(
                columns[0].Trim(),
                columns[1].Trim(),
                columns[2].Trim(),
                columns[3].Trim()));
        }

        return result;
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var value = new StringBuilder();
        var quoted = false;
        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];
            if (current == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (current == ',' && !quoted)
            {
                result.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(current);
            }
        }

        result.Add(value.ToString());
        return result;
    }
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Application,
    "수집된 공식 어획구역을 바다별 개략 타일로 집계",
    ContractType = typeof(I해양수산Map바다Tile조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    Boundary = "공식 바다 분류의 건수만 집계하며 타일 좌표는 실제 어획구역 경계나 조업 위치가 아닙니다.")]
public sealed class 해양수산Map바다Tile조회UseCase(
    IMof어획구역CatalogSource source,
    IOptions<PublicDataOptions> options) : I해양수산Map바다Tile조회UseCase
{
    private static readonly IReadOnlyDictionary<string, OceanLayout> Layouts =
        new Dictionary<string, OceanLayout>(StringComparer.Ordinal)
        {
            ["북국해"] = new("arctic", "Arctic Ocean", 50m, 10m),
            ["북극해"] = new("arctic", "Arctic Ocean", 50m, 10m),
            ["베링해"] = new("bering", "Bering Sea", 82m, 19m),
            ["태평양"] = new("pacific", "Pacific Ocean", 78m, 49m),
            ["대서양"] = new("atlantic", "Atlantic Ocean", 42m, 43m),
            ["지중해"] = new("mediterranean", "Mediterranean Sea", 50m, 38m),
            ["인도양"] = new("indian", "Indian Ocean", 61m, 67m),
            ["남극수역"] = new("antarctic", "Antarctic Waters", 50m, 89m)
        };

    public async Task<MarineFishingAreaOceanTileResponse> 조회Async(
        CancellationToken cancellationToken = default)
    {
        var snapshot = await source.수집Async(cancellationToken);
        var tiles = snapshot.Records
            .Where(record => Layouts.ContainsKey(record.SeaName))
            .GroupBy(record => record.SeaName, StringComparer.Ordinal)
            .Select((group, index) =>
            {
                var layout = Layouts[group.Key];
                return new MarineFishingAreaOceanTileDto(
                    layout.TileKey,
                    group.Key,
                    layout.DisplayNameEn,
                    group.Count(),
                    layout.Left,
                    layout.Top,
                    index * 340,
                    group.Select(record => record.KoreanName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .Take(5)
                        .ToArray());
            })
            .OrderBy(tile => tile.AnchorTopPercent)
            .ThenBy(tile => tile.AnchorLeftPercent)
            .ToArray();
        var mapped = tiles.Sum(tile => tile.FishingAreaCount);
        var configured = options.Value.MofFishingAreas;

        return new MarineFishingAreaOceanTileResponse(
            "mof-fishing-area-catalog",
            "해양수산부 공동활용체계 어획구역",
            configured.SourceUrl,
            configured.DatasetVersion,
            snapshot.CollectedAtUtc,
            snapshot.ContentSha256,
            snapshot.Records.Count,
            mapped,
            snapshot.Records.Count - mapped,
            MarineFishingAreaGeometryBasisCodes.SchematicOceanCatalogLayout,
            [
                "원천 파일은 어획구역명과 바다 분류만 제공하며 좌표·경계·어획량·수온은 제공하지 않습니다.",
                "바다 타일의 위치와 움직임은 탐색용 개략 표현이며 실제 조업 위치나 데이터 변화가 아닙니다.",
                "실시간 어획량·수온은 승인된 별도 원천을 연결한 뒤 같은 출처·시각 경계로 추가해야 합니다."
            ],
            tiles);
    }

    private sealed record OceanLayout(
        string TileKey,
        string DisplayNameEn,
        decimal Left,
        decimal Top);
}
