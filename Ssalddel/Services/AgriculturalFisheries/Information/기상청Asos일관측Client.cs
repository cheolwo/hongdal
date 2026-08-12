using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record 기상청Asos공간Context(
    string TargetLocationStableId,
    decimal TargetLatitude,
    decimal TargetLongitude,
    decimal StationLatitude,
    decimal StationLongitude,
    string StationMetadataSourceHref);

public sealed record 기상청Asos일관측Query(
    DateOnly ObservationDate,
    string StationId,
    기상청Asos공간Context? SpatialContext = null);

public interface I기상청Asos일관측Client
{
    Task<기상청Asos일관측Snapshot> 조회Async(
        기상청Asos일관측Query query,
        CancellationToken cancellationToken = default);
}

public sealed class 기상청Asos일관측Client(
    HttpClient httpClient,
    IOptions<PublicDataOptions> options,
    TimeProvider timeProvider) : I기상청Asos일관측Client
{
    private const string DatasetKey = "kma-asos-daily";
    private const string SourceHref = "https://www.data.go.kr/data/15059093/openapi.do";
    private readonly PublicDataOptions _options = options.Value;

    public async Task<기상청Asos일관측Snapshot> 조회Async(
        기상청Asos일관측Query query,
        CancellationToken cancellationToken = default)
    {
        Validate(query);

        var serviceKey = _options.DataGoKrServiceKey;
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            throw new InvalidOperationException("KmaAsosApiKeyMissing");
        }

        var date = query.ObservationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var requestUri = string.Create(
            CultureInfo.InvariantCulture,
            $"{_options.KmaAsos.DailyPath}?serviceKey={Uri.EscapeDataString(serviceKey)}&pageNo=1&numOfRows=10&dataType=JSON&dataCd=ASOS&dateCd=DAY&startDt={date}&endDt={date}&stnIds={Uri.EscapeDataString(query.StationId)}");

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var payloadHash = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var responseElement = GetRequiredProperty(root, "response");
        var header = GetRequiredProperty(responseElement, "header");
        var resultCode = GetScalarText(GetRequiredProperty(header, "resultCode"));
        if (resultCode is not ("00" or "0000"))
        {
            throw new InvalidOperationException($"KmaAsosRemoteFailure:{resultCode}");
        }

        var body = GetRequiredProperty(responseElement, "body");
        var items = GetRequiredProperty(GetRequiredProperty(body, "items"), "item");
        var matches = EnumerateItems(items)
            .Where(item => IsRequestedObservation(item, query))
            .ToArray();

        if (matches.Length == 0)
        {
            throw new InvalidOperationException("KmaAsosObservationNotFound");
        }

        if (matches.Length > 1)
        {
            throw new InvalidOperationException("KmaAsosObservationAmbiguous");
        }

        return Map(matches[0], query, payloadHash, timeProvider.GetUtcNow());
    }

    private void Validate(기상청Asos일관측Query query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.StationId.Length != 3 || !query.StationId.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("KmaAsosStationIdInvalid", nameof(query));
        }

        var latestAvailableDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).AddDays(-1);
        if (query.ObservationDate > latestAvailableDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "KmaAsosObservationDateUnavailable");
        }

        if (query.SpatialContext is { } context)
        {
            if (string.IsNullOrWhiteSpace(context.TargetLocationStableId)
                || string.IsNullOrWhiteSpace(context.StationMetadataSourceHref)
                || context.TargetLatitude is < -90m or > 90m
                || context.StationLatitude is < -90m or > 90m
                || context.TargetLongitude is < -180m or > 180m
                || context.StationLongitude is < -180m or > 180m)
            {
                throw new ArgumentException("KmaAsosSpatialContextInvalid", nameof(query));
            }
        }
    }

    private static 기상청Asos일관측Snapshot Map(
        JsonElement item,
        기상청Asos일관측Query query,
        string payloadHash,
        DateTimeOffset retrievedAtUtc)
    {
        var values = new Dictionary<string, decimal?>
        {
            ["avgTa"] = GetNullableDecimal(item, "avgTa"),
            ["minTa"] = GetNullableDecimal(item, "minTa"),
            ["maxTa"] = GetNullableDecimal(item, "maxTa"),
            ["sumRn"] = GetNullableDecimal(item, "sumRn"),
            ["sumGsr"] = GetNullableDecimal(item, "sumGsr"),
            ["sumSsHr"] = GetNullableDecimal(item, "sumSsHr"),
            ["ssDur"] = GetNullableDecimal(item, "ssDur"),
            ["avgRhm"] = GetNullableDecimal(item, "avgRhm")
        };
        var missingFields = values
            .Where(pair => pair.Value is null)
            .Select(pair => pair.Key)
            .ToArray();
        var spatialContext = query.SpatialContext;
        var limitations = new List<string>
        {
            "ASOS 관측소 지점의 일 관측값이며 농장 필지 전체의 면 관측값이 아닙니다.",
            "예보가 아닌 과거 관측이며 통상 D-1까지 제공됩니다.",
            "결측은 0으로 대체하지 않습니다."
        };
        if (spatialContext is null)
        {
            limitations.Add("농장과 관측소의 좌표가 제공되지 않아 거리를 계산하지 않았습니다.");
        }
        else
        {
            limitations.Add($"관측소 좌표 근거: {spatialContext.StationMetadataSourceHref}");
        }

        return new 기상청Asos일관측Snapshot(
            $"weather-observation:kma.asos.{query.StationId}.{query.ObservationDate:yyyyMMdd}",
            1,
            기상관측SourceTypeCodes.PublicObservation,
            DatasetKey,
            query.ObservationDate,
            retrievedAtUtc,
            query.StationId,
            GetOptionalText(item, "stnNm") ?? query.StationId,
            기상관측공간정밀도Codes.StationObservation,
            spatialContext?.TargetLocationStableId,
            spatialContext is null ? null : CalculateDistanceKm(spatialContext),
            values["avgTa"],
            values["minTa"],
            values["maxTa"],
            values["sumRn"],
            values["sumGsr"],
            values["sumSsHr"],
            values["ssDur"],
            values["avgRhm"],
            new 기상청Asos관측단위("°C", "mm", "MJ/m²", "h", "%"),
            missingFields.Length == 0
                ? 기상관측품질Codes.Valid
                : 기상관측품질Codes.Incomplete,
            missingFields.Length == 0,
            missingFields,
            payloadHash,
            SourceHref,
            limitations);
    }

    private static decimal CalculateDistanceKm(기상청Asos공간Context context)
    {
        const double earthRadiusKm = 6371.0088;
        static double Radians(decimal degrees) => (double)degrees * Math.PI / 180d;

        var targetLatitude = Radians(context.TargetLatitude);
        var stationLatitude = Radians(context.StationLatitude);
        var latitudeDifference = stationLatitude - targetLatitude;
        var longitudeDifference = Radians(context.StationLongitude - context.TargetLongitude);
        var a = Math.Pow(Math.Sin(latitudeDifference / 2d), 2d)
            + Math.Cos(targetLatitude) * Math.Cos(stationLatitude)
            * Math.Pow(Math.Sin(longitudeDifference / 2d), 2d);
        var distance = 2d * earthRadiusKm * Math.Asin(Math.Min(1d, Math.Sqrt(a)));
        return decimal.Round((decimal)distance, 3, MidpointRounding.AwayFromZero);
    }

    private static bool IsRequestedObservation(JsonElement item, 기상청Asos일관측Query query)
    {
        var stationId = GetOptionalText(item, "stnId");
        var observationDate = GetOptionalText(item, "tm");
        var expectedCompact = query.ObservationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var expectedDashed = query.ObservationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return string.Equals(stationId, query.StationId, StringComparison.Ordinal)
            && (string.Equals(observationDate, expectedCompact, StringComparison.Ordinal)
                || string.Equals(observationDate, expectedDashed, StringComparison.Ordinal));
    }

    private static IEnumerable<JsonElement> EnumerateItems(JsonElement items)
    {
        return items.ValueKind switch
        {
            JsonValueKind.Array => items.EnumerateArray(),
            JsonValueKind.Object => [items],
            _ => []
        };
    }

    private static JsonElement GetRequiredProperty(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            throw new InvalidOperationException($"KmaAsosResponseFieldMissing:{name}");
        }

        return value;
    }

    private static string GetScalarText(JsonElement element)
        => element.ValueKind == JsonValueKind.String
            ? element.GetString() ?? string.Empty
            : element.GetRawText();

    private static string? GetOptionalText(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var text = GetScalarText(value).Trim();
        return text.Length == 0 ? null : text;
    }

    private static decimal? GetNullableDecimal(JsonElement element, string name)
    {
        var text = GetOptionalText(element, name);
        if (text is null)
        {
            return null;
        }

        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidOperationException($"KmaAsosResponseNumberInvalid:{name}");
        }

        return value;
    }
}
