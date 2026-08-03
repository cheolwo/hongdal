using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.Geography;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed record 경기데이터드림가축사육MapSnapshot(
    DateTimeOffset CollectedAtUtc,
    IReadOnlyList<경기데이터드림가축사육지역집계> Items)
{
    public static 경기데이터드림가축사육MapSnapshot Empty { get; } =
        new(DateTimeOffset.MinValue, []);
}

public interface I경기데이터드림가축사육MapSnapshotStore
{
    경기데이터드림가축사육MapSnapshot Read();

    void Replace(경기데이터드림가축사육MapSnapshot snapshot);
}

public sealed class 경기데이터드림가축사육MapSnapshotStore
    : I경기데이터드림가축사육MapSnapshotStore
{
    private 경기데이터드림가축사육MapSnapshot _current =
        경기데이터드림가축사육MapSnapshot.Empty;

    public 경기데이터드림가축사육MapSnapshot Read()
        => Volatile.Read(ref _current);

    public void Replace(경기데이터드림가축사육MapSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Volatile.Write(ref _current, snapshot);
    }
}

public interface I경기데이터드림가축사육MapProjectionRefresher
{
    Task<bool> RefreshAsync(CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    "public-data.gyeonggi-data-dream.map-projection-refresh",
    SsalddelCodeLayer.Application,
    "경기데이터드림 가축사육업 원문을 비식별 시군·영업상태 snapshot으로 갱신",
    ContractType = typeof(I경기데이터드림가축사육MapProjectionRefresher),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    Boundary = "마커 조회 경로에서 외부 API를 호출하지 않도록 안전 집계만 메모리 snapshot에 교체")]
public sealed class 경기데이터드림가축사육MapProjectionRefresher(
    I경기데이터드림가축사육집계Client client,
    I경기데이터드림가축사육MapSnapshotStore store)
    : I경기데이터드림가축사육MapProjectionRefresher
{
    public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.QueryAsync(cancellationToken: cancellationToken);
        if (!response.Success)
        {
            return false;
        }

        store.Replace(new 경기데이터드림가축사육MapSnapshot(
            response.ObservedAt,
            response.Items
                .Where(item => !string.IsNullOrWhiteSpace(item.RegionCode))
                .ToArray()));
        return true;
    }
}

public sealed class 경기데이터드림가축사육MapProjectionRefreshService(
    IServiceScopeFactory scopeFactory,
    IOptions<PublicDataOptions> options,
    ILogger<경기데이터드림가축사육MapProjectionRefreshService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var projectionOptions = options.Value.GyeonggiDataDream;
        if (!projectionOptions.MapProjectionEnabled)
        {
            return;
        }

        var delay = TimeSpan.FromHours(Math.Clamp(
            projectionOptions.MapProjectionRefreshHours,
            1,
            168));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var refresher = scope.ServiceProvider
                    .GetRequiredService<I경기데이터드림가축사육MapProjectionRefresher>();
                var refreshed = await refresher.RefreshAsync(stoppingToken);
                if (!refreshed)
                {
                    logger.LogWarning(
                        "경기데이터드림 가축사육 지도 snapshot을 갱신하지 못해 이전 성공 자료를 유지합니다.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "경기데이터드림 가축사육 지도 snapshot 갱신 중 오류가 발생해 이전 성공 자료를 유지합니다.");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

public interface I경기데이터드림가축사육MapMarkerReader
{
    Task<IReadOnlyList<커뮤니티세계지도ObservationDto>> 공개Marker조회Async(
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    "public-data.gyeonggi-data-dream.map-projection-read",
    SsalddelCodeLayer.Application,
    "캐시된 가축사육업 시군 집계를 검증된 행정구역 대표점 마커로 투영",
    ContractType = typeof(I경기데이터드림가축사육MapMarkerReader),
    FlowOrder = 25,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "농장명·전화·주소·원문 좌표·식별번호를 읽거나 반환하지 않고 검증된 시군 기준점만 사용")]
public sealed class 경기데이터드림가축사육MapMarkerReader(
    I경기데이터드림가축사육MapSnapshotStore store,
    SsalddelContext geographyDb,
    TimeProvider timeProvider) : I경기데이터드림가축사육MapMarkerReader
{
    public const string DatasetKey = "LivestockBreeding";
    public const string SourceUrl =
        "https://data.gg.go.kr/portal/data/service/selectServicePage.do?infId=2XO0Z208BB6LSO00M897638968&infSeq=3";

    public async Task<IReadOnlyList<커뮤니티세계지도ObservationDto>> 공개Marker조회Async(
        CancellationToken cancellationToken = default)
    {
        var snapshot = store.Read();
        if (snapshot.Items.Count == 0)
        {
            return [];
        }

        var regionCodes = snapshot.Items
            .Select(item => item.RegionCode.Trim())
            .Where(code => code.Length > 0
                           && !string.Equals(
                               code,
                               경기데이터드림가축사육집계Client.DatasetScopeRegionCode,
                               StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var assignments = regionCodes.Count == 0
            ? []
            : await geographyDb.지역농수산Map행정구역CodeAssignments
                .AsNoTracking()
                .Where(item => item.SchemeCode == RegionalAgriculturalMapCodeSchemeCodes.KoreaMoisAdministrative
                               && regionCodes.Contains(item.ExternalCode))
                .Include(item => item.Region)
                .ThenInclude(region => region.Boundaries)
                .ToArrayAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        var verifiedRegionObservations = assignments
            .Where(IsVerifiedAssignment)
            .GroupBy(item => item.Region.Id)
            .Select(group => BuildObservation(group.First(), snapshot, now))
            .Where(item => item is not null)
            .Select(item => item!);
        var provinceObservation = BuildProvinceObservation(snapshot, now);
        return verifiedRegionObservations
            .Concat(provinceObservation is null ? [] : [provinceObservation])
            .OrderBy(item => item.Title, StringComparer.Ordinal)
            .ToArray();
    }

    private static 커뮤니티세계지도ObservationDto? BuildProvinceObservation(
        경기데이터드림가축사육MapSnapshot snapshot,
        DateTimeOffset now)
    {
        var rows = snapshot.Items
            .Where(item => string.Equals(
                item.RegionCode.Trim(),
                경기데이터드림가축사육집계Client.DatasetScopeRegionCode,
                StringComparison.Ordinal))
            .ToArray();
        var anchor = 지역문화행정구역대표점Catalog.All.First(item =>
            string.Equals(item.RegionKey, "kr-gyeonggi", StringComparison.Ordinal));
        return BuildObservation(
            anchor.RegionKey,
            anchor.FallbackRegionName ?? 경기데이터드림가축사육집계Client.DatasetScopeRegionName,
            anchor.Latitude,
            anchor.Longitude,
            rows,
            snapshot,
            now,
            $"경기데이터드림 · {지역문화행정구역대표점Catalog.SourceName}",
            "경기도 행정구역 대표점이며 실제 농장·축사·사업장 위치, 재고, 계약 또는 공급 가능성이 아닙니다.");
    }

    private static 커뮤니티세계지도ObservationDto? BuildObservation(
        지역농수산Map행정구역CodeAssignment assignment,
        경기데이터드림가축사육MapSnapshot snapshot,
        DateTimeOffset now)
    {
        var boundary = assignment.Region.Boundaries
            .Where(IsVerifiedBoundary)
            .OrderBy(item => item.SimplificationLevel)
            .ThenByDescending(item => item.VerifiedAtUtc)
            .FirstOrDefault();
        if (boundary is null)
        {
            return null;
        }

        var rows = snapshot.Items
            .Where(item => string.Equals(
                item.RegionCode.Trim(),
                assignment.ExternalCode,
                StringComparison.Ordinal))
            .ToArray();
        return BuildObservation(
            assignment.Region.PublicRegionKey,
            assignment.Region.DisplayNameKo,
            (double)boundary.AnchorLatitude,
            (double)boundary.AnchorLongitude,
            rows,
            snapshot,
            now,
            "경기데이터드림 · 행정안전부",
            "시군 행정구역 대표점이며 실제 농장·축사·사업장 위치, 재고, 계약 또는 공급 가능성이 아닙니다.");
    }

    private static 커뮤니티세계지도ObservationDto? BuildObservation(
        string regionKey,
        string regionName,
        double latitude,
        double longitude,
        IReadOnlyList<경기데이터드림가축사육지역집계> rows,
        경기데이터드림가축사육MapSnapshot snapshot,
        DateTimeOffset now,
        string sourceName,
        string boundaryNotice)
    {
        var total = rows.Sum(item => item.BusinessCount);
        if (total == 0)
        {
            return null;
        }

        var statusMetrics = rows
            .GroupBy(item => string.IsNullOrWhiteSpace(item.BusinessStatus)
                ? "상태 미제공"
                : item.BusinessStatus.Trim(), StringComparer.Ordinal)
            .Select(group => new 커뮤니티세계지도MetricDto(
                $"status:{group.Key}",
                group.Key,
                group.Sum(item => item.BusinessCount),
                "건"))
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.DisplayName, StringComparer.Ordinal)
            .ToArray();
        var metrics = new[]
            {
                new 커뮤니티세계지도MetricDto("total", "전체 인허가 원문", total, "건")
            }
            .Concat(statusMetrics)
            .ToArray();
        var statusSummary = string.Join(
            " · ",
            statusMetrics.Take(3).Select(metric => $"{metric.DisplayName} {metric.Value:N0}{metric.Unit}"));
        var freshness = snapshot.CollectedAtUtc == DateTimeOffset.MinValue
            ? 커뮤니티세계지도FreshnessCodes.Unknown
            : now - snapshot.CollectedAtUtc <= TimeSpan.FromDays(14)
                ? 커뮤니티세계지도FreshnessCodes.Fresh
                : 커뮤니티세계지도FreshnessCodes.Stale;

        return new 커뮤니티세계지도ObservationDto(
            $"gyeonggi-livestock:{regionKey}",
            CommunityPageRoutes.WorldMapDayWorkDataset,
            커뮤니티세계지도LayerCodes.GyeonggiLivestockPublicEvidence,
            RegionalAgriculturalMapCountryCodes.Korea,
            "대한민국",
            latitude,
            longitude,
            $"{regionName} 가축사육업 인허가 집계",
            $"경기데이터드림 원문 {total:N0}건을 공개 행정범위·영업상태로 집계했습니다. {statusSummary}. 농장 위치나 현재 공급 가능성을 뜻하지 않습니다.",
            sourceName,
            null,
            커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
            SourceUrl,
            SourceUrl,
            커뮤니티세계지도위치정밀도Codes.AdministrativeRegionRepresentative,
            SourceDatasetKey: DatasetKey,
            CollectedAtUtc: snapshot.CollectedAtUtc == DateTimeOffset.MinValue
                ? null
                : snapshot.CollectedAtUtc,
            UpdateCycle: "주간",
            FreshnessCode: freshness,
            BoundaryNotice: boundaryNotice,
            Metrics: metrics);
    }

    private static bool IsVerifiedAssignment(지역농수산Map행정구역CodeAssignment assignment)
        => assignment.VerifiedAtUtc != default
           && assignment.Region.CountryCode == RegionalAgriculturalMapCountryCodes.Korea;

    private static bool IsVerifiedBoundary(지역농수산Map행정구역Boundary boundary)
        => boundary.VerifiedAtUtc != default
           && boundary.SourceUrl.Length > 0
           && boundary.AnchorLatitude is >= -90m and <= 90m
           && boundary.AnchorLongitude is >= -180m and <= 180m;
}
