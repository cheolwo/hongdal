using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.DeliveryZones;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Data;
using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.도메인.배달권;

namespace 살뜰.Services.DeliveryZones;

public interface I원장배달권투영Service
{
    Task<원장배달권연결Dto> 연결추적Async(
        원장배달권연결요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<원장배달권연결Dto>> 조회Async(
        string 원장유형코드,
        string 원장Id,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformDeliveryZoneLedger,
    SsalddelCodeLayer.Application,
    "업무 원장의 주소 또는 좌표를 플랫폼 공통 배달권으로 판정하고 멱등 투영을 추적한다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(I원장배달권투영Service),
    FlowOrder = 20,
    Boundary = "호출자가 업무 원장 변경과 함께 SaveChanges를 수행한다. 이 투영은 자동 참여나 자동 배차를 실행하지 않는다.")]
public sealed class 원장배달권투영Service : I원장배달권투영Service
{
    private const string 미정배달권키 = "unknown";
    private readonly SsalddelContext _db;

    public 원장배달권투영Service(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<원장배달권연결Dto> 연결추적Async(
        원장배달권연결요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var ledgerType = NormalizeRequired(request.원장유형코드, nameof(request.원장유형코드), 40);
        var ledgerId = NormalizeRequired(request.원장Id, nameof(request.원장Id), 120);
        var role = NormalizeRequired(request.역할코드, nameof(request.역할코드), 40);
        var basis = NormalizeRequired(request.생성근거, nameof(request.생성근거), 120);

        if (!원장배달권원장유형코드.지원여부(ledgerType))
        {
            throw new ArgumentException($"지원하지 않는 원장 유형입니다: {ledgerType}", nameof(request));
        }

        if (!원장배달권역할코드.지원여부(role))
        {
            throw new ArgumentException($"지원하지 않는 배달권 역할입니다: {role}", nameof(request));
        }

        if (request.기존연결우선여부)
        {
            var existing = _db.원장배달권투영.Local.FirstOrDefault(x =>
                               x.원장유형코드 == ledgerType
                               && x.원장Id == ledgerId
                               && x.역할코드 == role)
                           ?? await _db.원장배달권투영
                               .Include(x => x.배달권)
                               .SingleOrDefaultAsync(
                                   x => x.원장유형코드 == ledgerType
                                        && x.원장Id == ledgerId
                                        && x.역할코드 == role,
                                   cancellationToken);
            if (existing is not null)
            {
                if (existing.배달권 is null)
                {
                    await _db.Entry(existing)
                        .Reference(x => x.배달권)
                        .LoadAsync(cancellationToken);
                }

                var existingZone = existing.배달권
                                   ?? throw new InvalidOperationException(
                                       "원장 배달권 투영의 배달권 참조를 찾을 수 없습니다.");
                return ToDto(existing, existingZone);
            }
        }

        var result = ResolveDeliveryZone(request);
        var adjacentKeys = request.기존인접배송권키목록.Count > 0
            ? request.기존인접배송권키목록
            : 국내행정구역배달권Catalog.인접배달권키조회(result.배달권키);
        var zone = await GetOrTrackZoneAsync(result, adjacentKeys, cancellationToken);
        var projection = await GetOrTrackProjectionAsync(
            ledgerType,
            ledgerId,
            role,
            basis,
            zone,
            cancellationToken);

        return ToDto(projection, zone);
    }

    public async Task<IReadOnlyList<원장배달권연결Dto>> 조회Async(
        string 원장유형코드,
        string 원장Id,
        CancellationToken cancellationToken = default)
    {
        var ledgerType = NormalizeRequired(원장유형코드, nameof(원장유형코드), 40);
        var ledgerId = NormalizeRequired(원장Id, nameof(원장Id), 120);

        var items = await _db.원장배달권투영
            .AsNoTracking()
            .Include(x => x.배달권)
            .Where(x => x.원장유형코드 == ledgerType && x.원장Id == ledgerId)
            .OrderBy(x => x.역할코드)
            .ToListAsync(cancellationToken);
        return items.Select(x => ToDto(x, x.배달권)).ToArray();
    }

    private async Task<플랫폼배달권> GetOrTrackZoneAsync(
        배달권판정결과 result,
        IReadOnlyList<string> adjacentZoneKeys,
        CancellationToken cancellationToken)
    {
        var zone = _db.플랫폼배달권.Local.FirstOrDefault(x => x.배달권키 == result.배달권키)
                   ?? await _db.플랫폼배달권.SingleOrDefaultAsync(
                       x => x.배달권키 == result.배달권키,
                       cancellationToken);
        var adjacentKeys = adjacentZoneKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        var now = DateTime.UtcNow;

        if (zone is null)
        {
            zone = new 플랫폼배달권
            {
                배달권키 = result.배달권키,
                CreatedAtUtc = now
            };
            _db.플랫폼배달권.Add(zone);
        }

        zone.배달권명 = result.배달권명;
        zone.판정방식 = result.판정방식;
        zone.법정동코드 = result.법정동코드;
        zone.시도명 = result.시도명;
        zone.시군구명 = result.시군구명;
        zone.대표건물명 = result.대표건물명;
        zone.대표건물주소 = result.대표건물주소;
        zone.대표위도 = result.대표위도;
        zone.대표경도 = result.대표경도;
        zone.인접배달권키Json = JsonSerializer.Serialize(adjacentKeys);
        zone.활성 = !string.Equals(result.배달권키, 미정배달권키, StringComparison.Ordinal);
        zone.UpdatedAtUtc = now;
        return zone;
    }

    private async Task<원장배달권투영> GetOrTrackProjectionAsync(
        string ledgerType,
        string ledgerId,
        string role,
        string basis,
        플랫폼배달권 zone,
        CancellationToken cancellationToken)
    {
        var projection = _db.원장배달권투영.Local.FirstOrDefault(x =>
                             x.원장유형코드 == ledgerType
                             && x.원장Id == ledgerId
                             && x.역할코드 == role)
                         ?? await _db.원장배달권투영
                             .Include(x => x.배달권)
                             .SingleOrDefaultAsync(
                                 x => x.원장유형코드 == ledgerType
                                      && x.원장Id == ledgerId
                                      && x.역할코드 == role,
                                 cancellationToken);
        var now = DateTime.UtcNow;

        if (projection is null)
        {
            projection = new 원장배달권투영
            {
                원장유형코드 = ledgerType,
                원장Id = ledgerId,
                역할코드 = role,
                CreatedAtUtc = now
            };
            _db.원장배달권투영.Add(projection);
        }

        projection.배달권 = zone;
        projection.생성근거 = basis;
        projection.UpdatedAtUtc = now;
        return projection;
    }

    private static 배달권판정결과 ResolveDeliveryZone(원장배달권연결요청 request)
    {
        if (!string.IsNullOrWhiteSpace(request.기존배송권키))
        {
            var key = NormalizeRequired(request.기존배송권키, nameof(request.기존배송권키), 120);
            var name = string.IsNullOrWhiteSpace(request.기존배송권명)
                ? key
                : NormalizeRequired(request.기존배송권명, nameof(request.기존배송권명), 160);
            var method = string.IsNullOrWhiteSpace(request.기존배송권판정방식)
                ? "기존배송권연결"
                : NormalizeRequired(request.기존배송권판정방식, nameof(request.기존배송권판정방식), 40);
            return new 배달권판정결과(key, name, method);
        }

        return 국내화물배달권정책.판정(
            CreatePoint(request.위도, request.경도),
            request.도로명주소);
    }

    private static 원장배달권연결Dto ToDto(원장배달권투영 projection, 플랫폼배달권 zone)
        => new()
        {
            원장유형코드 = projection.원장유형코드,
            원장Id = projection.원장Id,
            역할코드 = projection.역할코드,
            생성근거 = projection.생성근거,
            UpdatedAtUtc = projection.UpdatedAtUtc,
            배달권 = new 플랫폼배달권Dto
            {
                배달권키 = zone.배달권키,
                배달권명 = zone.배달권명,
                판정방식 = zone.판정방식,
                법정동코드 = zone.법정동코드,
                시도명 = zone.시도명,
                시군구명 = zone.시군구명,
                대표위도 = zone.대표위도,
                대표경도 = zone.대표경도,
                인접배달권키목록 = DeserializeAdjacentKeys(zone.인접배달권키Json)
            }
        };

    private static IReadOnlyList<string> DeserializeAdjacentKeys(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(value) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        => latitude.HasValue && longitude.HasValue
            ? new 배차경로좌표(latitude.Value, longitude.Value)
            : null;

    private static string NormalizeRequired(string? value, string parameterName, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("필수 값입니다.", parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"최대 {maxLength}자까지 입력할 수 있습니다.", parameterName);
        }

        return normalized;
    }
}
