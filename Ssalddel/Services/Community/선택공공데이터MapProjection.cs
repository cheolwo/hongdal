using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed record 국문관광정보MapItem(
    string ContentId,
    string ContentTypeId,
    string Title,
    double Latitude,
    double Longitude,
    DateTimeOffset? SourceUpdatedAtUtc);

public sealed record 국문관광정보MapSnapshot(
    DateTimeOffset CollectedAtUtc,
    int TotalCount,
    IReadOnlyList<국문관광정보MapItem> Items);

public sealed record 온라인가격MapSnapshot(
    DateTimeOffset CollectedAtUtc,
    int ItemCatalogCount);

public sealed record Kosis소비자물가MapSnapshot(
    DateTimeOffset CollectedAtUtc,
    string IndicatorId,
    string IndicatorName,
    string AreaName,
    string Period,
    string PeriodType,
    decimal Value,
    string Unit);

public sealed record 선택공공데이터MapSnapshot(
    국문관광정보MapSnapshot? Tourism,
    온라인가격MapSnapshot? OnlinePrice,
    Kosis소비자물가MapSnapshot? Kosis)
{
    public static 선택공공데이터MapSnapshot Empty { get; } = new(null, null, null);
}

public interface I선택공공데이터MapSnapshotStore
{
    선택공공데이터MapSnapshot Read();

    void Replace(선택공공데이터MapSnapshot snapshot);
}

public sealed class 선택공공데이터MapSnapshotStore : I선택공공데이터MapSnapshotStore
{
    private 선택공공데이터MapSnapshot _current = 선택공공데이터MapSnapshot.Empty;

    public 선택공공데이터MapSnapshot Read()
        => Volatile.Read(ref _current);

    public void Replace(선택공공데이터MapSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Volatile.Write(ref _current, snapshot);
    }
}

public sealed record 선택공공데이터MapRefreshResult(
    bool TourismRefreshed,
    bool OnlinePriceRefreshed,
    bool KosisRefreshed);

public interface I선택공공데이터MapProjectionRefresher
{
    Task<선택공공데이터MapRefreshResult> RefreshAsync(
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    "public-data.selected-sources.map-projection-refresh",
    SsalddelCodeLayer.Application,
    "관광 좌표·온라인가격 카탈로그·KOSIS 소비자물가를 출처별 snapshot으로 갱신",
    ContractType = typeof(I선택공공데이터MapProjectionRefresher),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    Boundary = "온라인 상품가격과 KOSIS 지수는 단위·시점을 분리하고 절감액·순위를 계산하지 않음")]
public sealed class 선택공공데이터MapProjectionRefresher(
    I국문관광정보공공데이터Client tourismClient,
    I온라인가격공공데이터Client onlinePriceClient,
    IKosis비교자료공공데이터Client kosisClient,
    I선택공공데이터MapSnapshotStore store,
    IOptions<PublicDataOptions> options)
    : I선택공공데이터MapProjectionRefresher
{
    public async Task<선택공공데이터MapRefreshResult> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        var projectionOptions = options.Value.SelectedPublicDataMap;
        var previous = store.Read();
        var next = previous;

        var tourism = projectionOptions.TourismEnabled
            ? await TryRefreshTourismAsync(projectionOptions, cancellationToken)
            : null;
        if (tourism is not null)
        {
            next = next with { Tourism = tourism };
        }

        var onlinePrice = projectionOptions.OnlinePriceEnabled
            ? await TryRefreshOnlinePriceAsync(cancellationToken)
            : null;
        if (onlinePrice is not null)
        {
            next = next with { OnlinePrice = onlinePrice };
        }

        var kosis = projectionOptions.KosisEnabled
            ? await TryRefreshKosisAsync(projectionOptions, cancellationToken)
            : null;
        if (kosis is not null)
        {
            next = next with { Kosis = kosis };
        }

        if (!ReferenceEquals(previous, next))
        {
            store.Replace(next);
        }

        return new 선택공공데이터MapRefreshResult(
            tourism is not null,
            onlinePrice is not null,
            kosis is not null);
    }

    private async Task<국문관광정보MapSnapshot?> TryRefreshTourismAsync(
        SelectedPublicDataMapOptions projectionOptions,
        CancellationToken cancellationToken)
    {
        try
        {
            var pageSize = Math.Clamp(projectionOptions.TourismMarkerLimit, 1, 200);
            var response = await tourismClient.QueryAsync(
                new 공공데이터포털업무ApiRequest
                {
                    ApiKey = "area-based",
                    Parameters = new Dictionary<string, string?>
                    {
                        ["pageNo"] = "1",
                        ["numOfRows"] = pageSize.ToString(CultureInfo.InvariantCulture),
                        ["MobileOS"] = "ETC",
                        ["MobileApp"] = "Ssalddel",
                        ["_type"] = "json",
                        ["arrange"] = "A"
                    }
                },
                cancellationToken);
            return ParseTourism(response, pageSize);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<온라인가격MapSnapshot?> TryRefreshOnlinePriceAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await onlinePriceClient.QueryAsync(
                new 공공데이터포털업무ApiRequest
                {
                    ApiKey = "item-list",
                    Parameters = new Dictionary<string, string?>
                    {
                        ["pageNo"] = "1",
                        ["numOfRows"] = "1",
                        ["type"] = "json"
                    }
                },
                cancellationToken);
            return ParseOnlinePrice(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Kosis소비자물가MapSnapshot?> TryRefreshKosisAsync(
        SelectedPublicDataMapOptions projectionOptions,
        CancellationToken cancellationToken)
    {
        try
        {
            var listResponse = await kosisClient.QueryAsync(
                new 공공데이터포털업무ApiRequest
                {
                    ApiKey = "indicator-by-name",
                    Parameters = new Dictionary<string, string?>
                    {
                        ["pageNo"] = "1",
                        ["numOfRows"] = "100",
                        ["STAT_JIPYO_NM"] = projectionOptions.KosisIndicatorSearchName,
                        ["format"] = "json"
                    }
                },
                cancellationToken);
            var detailResponse = await kosisClient.QueryAsync(
                new 공공데이터포털업무ApiRequest
                {
                    ApiKey = "indicator-detail-by-name",
                    Parameters = new Dictionary<string, string?>
                    {
                        ["pageNo"] = "1",
                        ["numOfRows"] = "100",
                        ["STAT_JIPYO_NM"] = projectionOptions.KosisIndicatorName,
                        ["RN"] = "1",
                        ["SRV_RN"] = Math.Clamp(
                                projectionOptions.KosisRecentPeriodCount,
                                1,
                                12)
                            .ToString(CultureInfo.InvariantCulture),
                        ["format"] = "json"
                    }
                },
                cancellationToken);
            return ParseKosis(
                listResponse,
                detailResponse,
                projectionOptions.KosisIndicatorName);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    internal static 국문관광정보MapSnapshot? ParseTourism(
        공공데이터포털업무ApiResponse response,
        int markerLimit)
    {
        if (!response.Success || string.IsNullOrWhiteSpace(response.Body))
        {
            return null;
        }

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        if (!TryGetBody(root, out var body)
            || !IsNormalResponse(root, "0000"))
        {
            return null;
        }

        var totalCount = ReadInt(body, "totalCount");
        var rows = ReadItemRows(body);
        var items = rows
            .Select(row =>
            {
                var latitude = ReadDouble(row, "mapy");
                var longitude = ReadDouble(row, "mapx");
                var contentId = ReadString(row, "contentid");
                var title = ReadString(row, "title");
                return latitude is >= -90 and <= 90
                       && longitude is >= -180 and <= 180
                       && contentId.Length > 0
                       && title.Length > 0
                    ? new 국문관광정보MapItem(
                        contentId,
                        ReadString(row, "contenttypeid"),
                        title,
                        latitude.Value,
                        longitude.Value,
                        ReadTourApiTimestamp(row, "modifiedtime"))
                    : null;
            })
            .Where(item => item is not null)
            .Select(item => item!)
            .GroupBy(item => item.ContentId, StringComparer.Ordinal)
            .Select(group => group.First())
            .Take(Math.Clamp(markerLimit, 1, 200))
            .ToArray();
        return items.Length == 0
            ? null
            : new 국문관광정보MapSnapshot(response.ObservedAt, totalCount, items);
    }

    internal static 온라인가격MapSnapshot? ParseOnlinePrice(
        공공데이터포털업무ApiResponse response)
    {
        if (!response.Success || string.IsNullOrWhiteSpace(response.Body))
        {
            return null;
        }

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        if (!TryGetBody(root, out var body) || !IsNormalResponse(root, "00"))
        {
            return null;
        }

        var totalCount = ReadInt(body, "totalCount");
        return totalCount > 0
            ? new 온라인가격MapSnapshot(response.ObservedAt, totalCount)
            : null;
    }

    internal static Kosis소비자물가MapSnapshot? ParseKosis(
        공공데이터포털업무ApiResponse listResponse,
        공공데이터포털업무ApiResponse detailResponse,
        string indicatorName)
    {
        if (!listResponse.Success
            || !detailResponse.Success
            || string.IsNullOrWhiteSpace(listResponse.Body)
            || string.IsNullOrWhiteSpace(detailResponse.Body))
        {
            return null;
        }

        using var listDocument = JsonDocument.Parse(listResponse.Body);
        using var detailDocument = JsonDocument.Parse(detailResponse.Body);
        if (!TryGetBody(listDocument.RootElement, out var listBody)
            || !TryGetBody(detailDocument.RootElement, out var detailBody)
            || !IsNormalResponse(listDocument.RootElement, "00")
            || !IsNormalResponse(detailDocument.RootElement, "00"))
        {
            return null;
        }

        var unitByIndicatorId = ReadItemRows(listBody)
            .Where(row => string.Equals(
                ReadString(row, "statJipyoNm"),
                indicatorName,
                StringComparison.Ordinal))
            .Select(row => new
            {
                Id = ReadString(row, "statJipyoId"),
                Unit = ReadString(row, "unit")
            })
            .Where(item => item.Id.Length > 0 && item.Unit.Length > 0)
            .GroupBy(item => item.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Unit, StringComparer.Ordinal);

        var item = ReadItemRows(detailBody)
            .Where(row => string.Equals(
                              ReadString(row, "statJipyoNm"),
                              indicatorName,
                              StringComparison.Ordinal)
                          && string.Equals(
                              ReadString(row, "itmNm"),
                              "전국",
                              StringComparison.Ordinal))
            .Select(row => new
            {
                Row = row,
                Id = ReadString(row, "statJipyoId"),
                Period = ReadString(row, "prdDe"),
                Value = ReadDecimal(row, "val")
            })
            .Where(candidate => candidate.Value is not null
                                && unitByIndicatorId.ContainsKey(candidate.Id))
            .OrderByDescending(candidate => candidate.Period, StringComparer.Ordinal)
            .FirstOrDefault();
        return item is null
            ? null
            : new Kosis소비자물가MapSnapshot(
                detailResponse.ObservedAt,
                item.Id,
                indicatorName,
                "전국",
                item.Period,
                ReadString(item.Row, "prdSe"),
                item.Value!.Value,
                unitByIndicatorId[item.Id]);
    }

    private static bool TryGetBody(JsonElement root, out JsonElement body)
    {
        if (root.TryGetProperty("response", out var response))
        {
            root = response;
        }

        return root.TryGetProperty("body", out body)
               && body.ValueKind == JsonValueKind.Object;
    }

    private static bool IsNormalResponse(JsonElement root, string expectedCode)
    {
        if (root.TryGetProperty("response", out var response))
        {
            root = response;
        }

        if (!root.TryGetProperty("header", out var header)
            || !header.TryGetProperty("resultCode", out var resultCode))
        {
            return true;
        }

        var code = resultCode.ValueKind == JsonValueKind.String
            ? resultCode.GetString()
            : resultCode.GetRawText();
        return string.Equals(code, expectedCode, StringComparison.Ordinal);
    }

    private static IReadOnlyList<JsonElement> ReadItemRows(JsonElement body)
    {
        if (!body.TryGetProperty("items", out var items))
        {
            return [];
        }

        if (items.ValueKind == JsonValueKind.Object
            && items.TryGetProperty("item", out var nestedItems))
        {
            items = nestedItems;
        }

        return items.ValueKind switch
        {
            JsonValueKind.Array => items.EnumerateArray().ToArray(),
            JsonValueKind.Object => [items],
            _ => []
        };
    }

    private static string ReadString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value)
            ? value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim() ?? string.Empty
                : value.GetRawText().Trim('"').Trim()
            : string.Empty;

    private static int ReadInt(JsonElement element, string propertyName)
        => int.TryParse(
            ReadString(element, propertyName),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : 0;

    private static double? ReadDouble(JsonElement element, string propertyName)
        => double.TryParse(
            ReadString(element, propertyName),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
        => decimal.TryParse(
            ReadString(element, propertyName),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;

    private static DateTimeOffset? ReadTourApiTimestamp(
        JsonElement element,
        string propertyName)
        => DateTime.TryParseExact(
            ReadString(element, propertyName),
            "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? new DateTimeOffset(
                    DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified),
                    TimeSpan.FromHours(9))
                .ToUniversalTime()
            : null;
}

public sealed class 선택공공데이터MapProjectionRefreshService(
    IServiceScopeFactory scopeFactory,
    IOptions<PublicDataOptions> options,
    ILogger<선택공공데이터MapProjectionRefreshService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var projectionOptions = options.Value.SelectedPublicDataMap;
        if (!projectionOptions.TourismEnabled
            && !projectionOptions.OnlinePriceEnabled
            && !projectionOptions.KosisEnabled)
        {
            return;
        }

        var delay = TimeSpan.FromHours(Math.Clamp(projectionOptions.RefreshHours, 1, 168));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var refresher = scope.ServiceProvider
                    .GetRequiredService<I선택공공데이터MapProjectionRefresher>();
                var result = await refresher.RefreshAsync(stoppingToken);
                if ((projectionOptions.TourismEnabled && !result.TourismRefreshed)
                    || (projectionOptions.OnlinePriceEnabled && !result.OnlinePriceRefreshed)
                    || (projectionOptions.KosisEnabled && !result.KosisRefreshed))
                {
                    logger.LogWarning(
                        "선택 공공데이터 지도 snapshot 일부를 갱신하지 못해 이전 성공 자료를 유지합니다. Tourism={Tourism}, OnlinePrice={OnlinePrice}, Kosis={Kosis}",
                        result.TourismRefreshed,
                        result.OnlinePriceRefreshed,
                        result.KosisRefreshed);
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
                    "선택 공공데이터 지도 snapshot 갱신 중 오류가 발생해 이전 성공 자료를 유지합니다.");
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

public interface I선택공공데이터MapMarkerReader
{
    Task<IReadOnlyList<커뮤니티세계지도ObservationDto>> 공개Marker조회Async(
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    "public-data.selected-sources.map-projection-read",
    SsalddelCodeLayer.Application,
    "캐시된 관광·온라인가격·KOSIS 자료를 공개 지도 레이어로 투영",
    ContractType = typeof(I선택공공데이터MapMarkerReader),
    FlowOrder = 25,
    Effects = SsalddelCodeEffect.None,
    Boundary = "관광 주소·연락처·이미지를 제외하고 온라인 상품가격과 KOSIS 지수를 서로 비교·순위화하지 않음")]
public sealed class 선택공공데이터MapMarkerReader(
    I선택공공데이터MapSnapshotStore store,
    TimeProvider timeProvider)
    : I선택공공데이터MapMarkerReader
{
    public const string TourismDatasetKey = "tourapi-korean-tourism";
    public const string OnlinePriceDatasetKey = "online-collected-prices";
    public const string KosisDatasetKey = "kosis-consumer-price-index";
    public const string TourismSourceUrl =
        "https://www.data.go.kr/data/15101578/openapi.do";
    public const string OnlinePriceSourceUrl =
        "https://www.data.go.kr/data/15080757/openapi.do";
    public const string KosisSourceUrl =
        "https://www.data.go.kr/data/15127763/openapi.do";

    public Task<IReadOnlyList<커뮤니티세계지도ObservationDto>> 공개Marker조회Async(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = store.Read();
        var now = timeProvider.GetUtcNow();
        var observations = new List<커뮤니티세계지도ObservationDto>();

        if (snapshot.Tourism is { } tourism)
        {
            observations.AddRange(tourism.Items.Select(item => new 커뮤니티세계지도ObservationDto(
                $"tourism:{item.ContentId}",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.TourismPublicEvidence,
                "KR",
                "대한민국",
                item.Latitude,
                item.Longitude,
                item.Title,
                $"한국관광공사 공개 관광정보입니다. 전체 {tourism.TotalCount:N0}건 중 지도 snapshot {tourism.Items.Count:N0}건만 표시하며 추천·대표 순위가 아닙니다.",
                "한국관광공사 TourAPI",
                item.SourceUpdatedAtUtc,
                커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                TourismSourceUrl,
                TourismSourceUrl,
                커뮤니티세계지도위치정밀도Codes.OfficialPoint,
                SourceDatasetKey: TourismDatasetKey,
                SourceUpdatedAtUtc: item.SourceUpdatedAtUtc,
                CollectedAtUtc: tourism.CollectedAtUtc,
                UpdateCycle: "실시간 API · snapshot 24시간",
                FreshnessCode: Freshness(tourism.CollectedAtUtc, now),
                BoundaryNotice: "공개 관광지 좌표이며 이용자·주민 위치, 상업 가용성 또는 지역문화 대표성을 뜻하지 않습니다.")));
        }

        if (snapshot.OnlinePrice is { } onlinePrice)
        {
            observations.Add(new 커뮤니티세계지도ObservationDto(
                "online-price:kr-catalog",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence,
                "KR",
                "대한민국",
                36.5,
                127.8,
                "온라인 수집가격 품목 카탈로그",
                $"웹 수집가격 품목코드 {onlinePrice.ItemCatalogCount:N0}건의 연결 범위입니다. 구조화된 판매단위가 없어 가격·절감액·순위는 지도에 표시하지 않습니다.",
                "국가데이터처 온라인가격정보",
                null,
                커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                OnlinePriceSourceUrl,
                OnlinePriceSourceUrl,
                커뮤니티세계지도위치정밀도Codes.CountryRepresentative,
                SourceDatasetKey: OnlinePriceDatasetKey,
                CollectedAtUtc: onlinePrice.CollectedAtUtc,
                UpdateCycle: "일일 웹 수집 · snapshot 24시간",
                FreshnessCode: Freshness(onlinePrice.CollectedAtUtc, now),
                BoundaryNotice: "대한민국 표시용 대표점이며 특정 판매처·재고·거래 가능성을 뜻하지 않습니다.",
                Metrics:
                [
                    new 커뮤니티세계지도MetricDto(
                        "item-catalog-count",
                        "품목코드",
                        onlinePrice.ItemCatalogCount,
                        "건")
                ]));
        }

        if (snapshot.Kosis is { } kosis)
        {
            observations.Add(new 커뮤니티세계지도ObservationDto(
                $"kosis-cpi:kr:{kosis.IndicatorId}",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext,
                "KR",
                "대한민국",
                36.5,
                127.8,
                $"KOSIS {kosis.IndicatorName}",
                $"{PeriodLabel(kosis.Period, kosis.PeriodType)} {kosis.AreaName} 지수 {kosis.Value:N2} ({kosis.Unit})입니다. 온라인 개별 상품가격과 단위가 달라 직접 가격차·절감액을 계산하지 않습니다.",
                "국가데이터처 KOSIS",
                null,
                커뮤니티세계지도EvidenceStatusCodes.OfficialSourceLinked,
                KosisSourceUrl,
                KosisSourceUrl,
                커뮤니티세계지도위치정밀도Codes.CountryRepresentative,
                SourceDatasetKey: KosisDatasetKey,
                CollectedAtUtc: kosis.CollectedAtUtc,
                UpdateCycle: "월 · snapshot 24시간",
                FreshnessCode: Freshness(kosis.CollectedAtUtc, now),
                BoundaryNotice: "전국 집계통계이며 개인·가구·커뮤니티 평가나 특정 지역 물가의 대리 지표가 아닙니다.",
                Metrics:
                [
                    new 커뮤니티세계지도MetricDto(
                        $"kosis:{kosis.IndicatorId}",
                        kosis.IndicatorName,
                        kosis.Value,
                        kosis.Unit)
                ]));
        }

        return Task.FromResult<IReadOnlyList<커뮤니티세계지도ObservationDto>>(
            observations.OrderBy(item => item.StableId, StringComparer.Ordinal).ToArray());
    }

    private static string Freshness(DateTimeOffset collectedAtUtc, DateTimeOffset now)
        => now - collectedAtUtc <= TimeSpan.FromHours(48)
            ? 커뮤니티세계지도FreshnessCodes.Fresh
            : 커뮤니티세계지도FreshnessCodes.Stale;

    private static string PeriodLabel(string period, string periodType)
        => string.Equals(periodType, "M", StringComparison.Ordinal)
           && DateOnly.TryParseExact(
               $"{period}01",
               "yyyyMMdd",
               CultureInfo.InvariantCulture,
               DateTimeStyles.None,
               out var month)
            ? $"{month:yyyy년 M월}"
            : period;
}
