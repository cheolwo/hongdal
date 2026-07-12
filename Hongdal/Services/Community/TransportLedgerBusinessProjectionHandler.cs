using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.도메인.운송;
using 홍달.도메인.화주;

namespace Hongdal.Services.Community;

public sealed class 운송원장업무투영Handler : I원장업무투영동기화Handler
{
    private readonly HongdalContext _db;
    private readonly ILogger<운송원장업무투영Handler> _logger;

    public 운송원장업무투영Handler(
        HongdalContext db,
        ILogger<운송원장업무투영Handler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public bool 처리대상인가(커뮤니티원장Dto 원장)
        => 운송원장업무투영Snapshot.처리대상인가(원장);

    public async Task 동기화Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default)
    {
        var snapshot = 운송원장업무투영Snapshot.생성(원장);
        if (snapshot is null)
        {
            return;
        }

        var transport = await _db.운송원장
            .FirstOrDefaultAsync(x => x.의뢰Id == snapshot.RequestId || x.운송번호 == snapshot.RequestId, cancellationToken);

        var isNew = transport is null;
        if (transport is null)
        {
            transport = new 운송원장
            {
                운송번호 = snapshot.RequestId,
                의뢰Id = snapshot.RequestId,
                화주Id = snapshot.ShipperId ?? string.Empty,
                원본의뢰유형 = snapshot.SourceType ?? CommunityLedgerTemplateKeys.CargoTransport,
                원본의뢰Id = snapshot.SourceId ?? snapshot.RequestId,
                커뮤니티원장Id = snapshot.LedgerId,
                커뮤니티원장템플릿Key = snapshot.LedgerTemplateKey,
                커뮤니티원장상태 = snapshot.LedgerState,
                커뮤니티원장동기화시각Utc = DateTime.UtcNow,
                상태 = snapshot.TransportState ?? 상태값.배차대기상태.대기,
                배차업무유형 = snapshot.DispatchBusinessType,
                배차큐단계 = snapshot.ResolveQueueStage(),
                배차노출상태 = snapshot.ResolveExposureState(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.운송원장.Add(transport);
        }

        var changed = isNew;
        changed |= ApplyTransportProjection(transport, snapshot, isNew);

        var shipperRequest = await _db.화주운송의뢰
            .FirstOrDefaultAsync(x => x.의뢰Id == snapshot.RequestId, cancellationToken);

        if (shipperRequest is not null)
        {
            changed |= ApplyShipperRequest(shipperRequest, snapshot);
        }
        else if (!isNew)
        {
            _logger.LogDebug(
                "커뮤니티 원장 업무 투영에서 화주운송의뢰를 찾지 못했습니다. 원장Id={원장Id}, 의뢰Id={의뢰Id}",
                원장.원장Id,
                snapshot.RequestId);
        }

        if (changed)
        {
            transport.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool ApplyTransportProjection(운송원장 entity, 운송원장업무투영Snapshot snapshot, bool isNew)
    {
        var changed = false;

        changed |= SetString(entity.화주Id, snapshot.ShipperId, value => entity.화주Id = value);
        changed |= SetString(entity.원본의뢰유형, snapshot.SourceType, value => entity.원본의뢰유형 = value);
        changed |= SetString(entity.원본의뢰Id, snapshot.SourceId, value => entity.원본의뢰Id = value);
        changed |= SetString(entity.커뮤니티원장Id, snapshot.LedgerId, value => entity.커뮤니티원장Id = value);
        changed |= SetString(entity.커뮤니티원장템플릿Key, snapshot.LedgerTemplateKey, value => entity.커뮤니티원장템플릿Key = value);
        changed |= SetString(entity.커뮤니티원장상태, snapshot.LedgerState, value => entity.커뮤니티원장상태 = value);
        changed |= SetString(entity.상태, snapshot.TransportState, value => entity.상태 = value);
        changed |= SetString(entity.확정기사Id, snapshot.DriverId, value => entity.확정기사Id = value);
        changed |= SetString(entity.기사_운송자, snapshot.DriverId, value => entity.기사_운송자 = value);
        changed |= SetString(entity.픽업_도로명주소, snapshot.PickupAddress, value => entity.픽업_도로명주소 = value);
        changed |= SetString(entity.픽업_상세주소, snapshot.PickupAddressDetail, value => entity.픽업_상세주소 = value);
        changed |= SetDecimal(entity.픽업_위도, snapshot.PickupLatitude, value => entity.픽업_위도 = value);
        changed |= SetDecimal(entity.픽업_경도, snapshot.PickupLongitude, value => entity.픽업_경도 = value);
        changed |= SetString(entity.하차_도로명주소, snapshot.DropoffAddress, value => entity.하차_도로명주소 = value);
        changed |= SetString(entity.하차_상세주소, snapshot.DropoffAddressDetail, value => entity.하차_상세주소 = value);
        changed |= SetDecimal(entity.하차_위도, snapshot.DropoffLatitude, value => entity.하차_위도 = value);
        changed |= SetDecimal(entity.하차_경도, snapshot.DropoffLongitude, value => entity.하차_경도 = value);
        changed |= SetString(entity.출발지, snapshot.PickupAddress, value => entity.출발지 = value);
        changed |= SetString(entity.도착지, snapshot.DropoffAddress, value => entity.도착지 = value);
        changed |= SetDecimal(entity.운임, snapshot.Fare, value => entity.운임 = value);
        if (isNew
            || string.IsNullOrWhiteSpace(entity.메모)
            || entity.메모.StartsWith("커뮤니티 원장 투영:", StringComparison.Ordinal))
        {
            changed |= SetString(entity.메모, snapshot.BuildMemo(), value => entity.메모 = value);
        }

        if (entity.배차업무유형 != snapshot.DispatchBusinessType)
        {
            entity.배차업무유형 = snapshot.DispatchBusinessType;
            changed = true;
        }

        entity.커뮤니티원장동기화시각Utc = DateTime.UtcNow;
        changed = true;

        if (isNew || snapshot.IsCompleted)
        {
            var queueStage = snapshot.ResolveQueueStage();
            var exposureState = snapshot.ResolveExposureState();
            if (entity.배차큐단계 != queueStage)
            {
                entity.배차큐단계 = queueStage;
                changed = true;
            }

            if (entity.배차노출상태 != exposureState)
            {
                entity.배차노출상태 = exposureState;
                changed = true;
            }
        }

        return changed;
    }

    private static bool ApplyShipperRequest(화주운송의뢰 entity, 운송원장업무투영Snapshot snapshot)
    {
        var changed = false;

        changed |= SetString(entity.화주Id, snapshot.ShipperId, value => entity.화주Id = value);
        changed |= SetString(entity.주문자UserId, snapshot.OrdererUserId, value => entity.주문자UserId = value);
        changed |= SetString(entity.화물종류, snapshot.CargoType, value => entity.화물종류 = value);
        changed |= SetString(entity.화물설명, snapshot.CargoDescription, value => entity.화물설명 = value);
        changed |= SetString(entity.픽업_도로명주소, snapshot.PickupAddress, value => entity.픽업_도로명주소 = value);
        changed |= SetString(entity.픽업_상세주소, snapshot.PickupAddressDetail, value => entity.픽업_상세주소 = value);
        changed |= SetDecimal(entity.픽업_위도, snapshot.PickupLatitude, value => entity.픽업_위도 = value);
        changed |= SetDecimal(entity.픽업_경도, snapshot.PickupLongitude, value => entity.픽업_경도 = value);
        changed |= SetString(entity.하차_도로명주소, snapshot.DropoffAddress, value => entity.하차_도로명주소 = value);
        changed |= SetString(entity.하차_상세주소, snapshot.DropoffAddressDetail, value => entity.하차_상세주소 = value);
        changed |= SetDecimal(entity.하차_위도, snapshot.DropoffLatitude, value => entity.하차_위도 = value);
        changed |= SetDecimal(entity.하차_경도, snapshot.DropoffLongitude, value => entity.하차_경도 = value);
        changed |= SetDecimal(entity.최종운임, snapshot.Fare, value => entity.최종운임 = value);

        var dispatchState = snapshot.ResolveShipperDispatchState();
        changed |= SetString(entity.배차상태, dispatchState, value => entity.배차상태 = value);

        if (changed)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return changed;
    }

    private static bool SetString(string? current, string? value, Action<string> setter)
    {
        var cleaned = 운송원장업무투영Snapshot.Clean(value);
        if (cleaned is null || string.Equals(current, cleaned, StringComparison.Ordinal))
        {
            return false;
        }

        setter(cleaned);
        return true;
    }

    private static bool SetDecimal(decimal? current, decimal? value, Action<decimal?> setter)
    {
        if (!value.HasValue || current == value)
        {
            return false;
        }

        setter(value);
        return true;
    }
}

public sealed class 운송원장업무투영Snapshot
{
    public string LedgerId { get; init; } = string.Empty;
    public string LedgerTemplateKey { get; init; } = string.Empty;
    public string LedgerState { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string? SourceType { get; init; }
    public string? SourceId { get; init; }
    public string? ShipperId { get; init; }
    public string? OrdererUserId { get; init; }
    public string? DriverId { get; init; }
    public string? TransportState { get; init; }
    public string? CargoType { get; init; }
    public string? CargoDescription { get; init; }
    public string? PickupAddress { get; init; }
    public string? PickupAddressDetail { get; init; }
    public decimal? PickupLatitude { get; init; }
    public decimal? PickupLongitude { get; init; }
    public string? DropoffAddress { get; init; }
    public string? DropoffAddressDetail { get; init; }
    public decimal? DropoffLatitude { get; init; }
    public decimal? DropoffLongitude { get; init; }
    public decimal? Fare { get; init; }

    public bool IsCompleted
        => string.Equals(LedgerState, 커뮤니티원장상태.완료, StringComparison.OrdinalIgnoreCase)
           || string.Equals(TransportState, 상태값.배차상태.인수완료, StringComparison.OrdinalIgnoreCase)
           || string.Equals(TransportState, 상태값.배차상태.하차완료, StringComparison.OrdinalIgnoreCase);

    public int DispatchBusinessType
        => string.Equals(LedgerTemplateKey, CommunityLedgerTemplateKeys.FoodDelivery, StringComparison.OrdinalIgnoreCase)
            ? 상태값.배차업무유형.음식배달
            : 상태값.배차업무유형.용달운송;

    public static bool 처리대상인가(커뮤니티원장Dto 원장)
    {
        if (string.Equals(원장.원장템플릿Key, CommunityLedgerTemplateKeys.CargoTransport, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (TryGet(원장.외부참조, "RdbTransportProjectionTable", "RdbTransportProjectionType", "화주운송의뢰Id", "운송번호") is not null)
        {
            return true;
        }

        return 원장.블록목록.Any(block =>
        {
            var entityHint = TryGet(block.Data, "업무엔티티");
            return ContainsAny(entityHint, "화주운송의뢰", "운송원장", "운송실행투영")
                   || ContainsAny(block.BlockId, "transport", "pickup", "dropoff")
                   || ContainsAny(block.Title, "운송 의뢰", "상차", "하차");
        });
    }

    public static 운송원장업무투영Snapshot? 생성(커뮤니티원장Dto 원장)
    {
        if (!처리대상인가(원장))
        {
            return null;
        }

        var requestBlock = FindBlock(원장, block =>
            ContainsAny(block.BlockId, "transport-request", "request")
            || ContainsAny(block.Title, "운송 의뢰", "의뢰")
            || ContainsAny(TryGet(block.Data, "업무엔티티"), "화주운송의뢰"));

        var pickupBlock = FindBlock(원장, block =>
            ContainsAny(block.BlockId, "pickup")
            || ContainsAny(block.Title, "상차", "픽업")
            || ContainsAny(TryGet(block.Data, "업무엔티티"), "상차"));

        var dropoffBlock = FindBlock(원장, block =>
            ContainsAny(block.BlockId, "dropoff")
            || ContainsAny(block.Title, "하차", "도착")
            || ContainsAny(TryGet(block.Data, "업무엔티티"), "하차"));

        var settlementBlock = FindBlock(원장, block =>
            ContainsAny(block.BlockId, "settlement", "fare", "payment")
            || ContainsAny(block.Title, "정산", "결제", "운임"));

        var requestId = FirstNonEmpty(
            TryGet(원장.외부참조, "화주운송의뢰Id", "의뢰Id", "RequestId", "requestId"),
            TryGet(원장.외부참조, "운송번호", "TransportNo", "transportNo"),
            TryGet(requestBlock?.Data, "의뢰Id", "RequestId", "requestId"),
            TryGet(requestBlock?.Data, "운송번호", "TransportNo", "transportNo"),
            ExtractTransportLedgerId(원장.원장Id),
            원장.원장Id);

        if (requestId is null)
        {
            return null;
        }

        return new 운송원장업무투영Snapshot
        {
            LedgerId = Clean(원장.원장Id) ?? requestId,
            LedgerTemplateKey = Clean(원장.원장템플릿Key) ?? CommunityLedgerTemplateKeys.CargoTransport,
            LedgerState = Clean(원장.상태) ?? 커뮤니티원장상태.초안,
            RequestId = requestId,
            SourceType = FirstNonEmpty(
                TryGet(원장.외부참조, "원천유형", "SourceType"),
                TryGet(requestBlock?.Data, "원천유형", "SourceType"),
                ResolveDefaultSourceType(원장)),
            SourceId = FirstNonEmpty(
                TryGet(원장.외부참조, "원천Id", "SourceId"),
                TryGet(requestBlock?.Data, "원천Id", "SourceId"),
                requestId),
            ShipperId = FirstNonEmpty(
                TryGet(원장.외부참조, "화주Id", "shipperId"),
                TryGet(requestBlock?.Data, "화주Id", "shipperId"),
                FindParticipantUserId(원장, "화주", "요청자")),
            OrdererUserId = FirstNonEmpty(
                TryGet(requestBlock?.Data, "주문자UserId", "OrdererUserId", "ordererUserId"),
                FindParticipantUserId(원장, "주문자")),
            DriverId = FirstNonEmpty(
                TryGet(원장.외부참조, "확정기사Id", "기사Id", "driverId"),
                TryGet(requestBlock?.Data, "확정기사Id", "기사Id", "driverId"),
                FindParticipantUserId(원장, "기사", "운반자", "배달자")),
            TransportState = ResolveTransportState(원장),
            CargoType = TryGet(requestBlock?.Data, "화물종류", "CargoType", "cargoType"),
            CargoDescription = FirstNonEmpty(
                TryGet(requestBlock?.Data, "화물설명", "CargoDescription", "cargoDescription"),
                원장.원함),
            PickupAddress = TryGet(pickupBlock?.Data, "주소", "도로명주소", "PickupAddress", "pickupAddress"),
            PickupAddressDetail = TryGet(pickupBlock?.Data, "상세주소", "AddressDetail", "pickupAddressDetail"),
            PickupLatitude = ParseDecimal(TryGet(pickupBlock?.Data, "위도", "Latitude", "lat", "pickupLatitude")),
            PickupLongitude = ParseDecimal(TryGet(pickupBlock?.Data, "경도", "Longitude", "lng", "pickupLongitude")),
            DropoffAddress = TryGet(dropoffBlock?.Data, "주소", "도로명주소", "DropoffAddress", "dropoffAddress"),
            DropoffAddressDetail = TryGet(dropoffBlock?.Data, "상세주소", "AddressDetail", "dropoffAddressDetail"),
            DropoffLatitude = ParseDecimal(TryGet(dropoffBlock?.Data, "위도", "Latitude", "lat", "dropoffLatitude")),
            DropoffLongitude = ParseDecimal(TryGet(dropoffBlock?.Data, "경도", "Longitude", "lng", "dropoffLongitude")),
            Fare = ParseDecimal(FirstNonEmpty(
                TryGet(settlementBlock?.Data, "최종운임", "운임", "Fare", "fare"),
                TryGet(settlementBlock?.Data, "결제예정금액", "EstimatedPaymentAmount")))
        };
    }

    public int ResolveQueueStage()
    {
        if (IsCompleted)
        {
            return 상태값.배차큐단계.종료;
        }

        if (string.Equals(TransportState, 상태값.배차대기상태.확정, StringComparison.Ordinal)
            || !string.IsNullOrWhiteSpace(DriverId)
            || IsInProgressTransportState(TransportState))
        {
            return 상태값.배차큐단계.확정;
        }

        return 상태값.배차큐단계.계획배차;
    }

    public int ResolveExposureState()
    {
        if (IsCompleted)
        {
            return 상태값.배차노출상태.종료;
        }

        if (ResolveQueueStage() == 상태값.배차큐단계.확정)
        {
            return 상태값.배차노출상태.확정;
        }

        return 상태값.배차노출상태.계획대기;
    }

    public string? ResolveShipperDispatchState()
    {
        if (string.IsNullOrWhiteSpace(TransportState))
        {
            return null;
        }

        return TransportState switch
        {
            상태값.배차대기상태.대기 => 상태값.배차상태.대기,
            상태값.배차대기상태.확정 => 상태값.배차상태.배차확정,
            "상차지도착" => 상태값.배차상태.상차중,
            "하차지도착" => 상태값.배차상태.운송중,
            상태값.배차상태.상차완료 => 상태값.배차상태.상차완료,
            상태값.배차상태.운송중 => 상태값.배차상태.운송중,
            상태값.배차상태.하차완료 => 상태값.배차상태.하차완료,
            상태값.배차상태.인수완료 => 상태값.배차상태.인수완료,
            _ => null
        };
    }

    public string BuildMemo()
        => $"커뮤니티 원장 투영: {LedgerId}";

    public static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static 커뮤니티원장블록Dto? FindBlock(커뮤니티원장Dto 원장, Func<커뮤니티원장블록Dto, bool> predicate)
        => 원장.블록목록.FirstOrDefault(predicate);

    private static string? ResolveDefaultSourceType(커뮤니티원장Dto 원장)
        => string.Equals(원장.원장템플릿Key, CommunityLedgerTemplateKeys.CargoTransport, StringComparison.OrdinalIgnoreCase)
            ? CommunityLedgerTemplateKeys.CargoTransport
            : 원장.원장템플릿Key;

    private static string? ResolveTransportState(커뮤니티원장Dto 원장)
    {
        var currentStage = Clean(원장.현재단계Key);
        if (currentStage is not null)
        {
            return currentStage;
        }

        if (string.Equals(원장.상태, 커뮤니티원장상태.완료, StringComparison.OrdinalIgnoreCase))
        {
            return 상태값.배차상태.인수완료;
        }

        if (string.Equals(원장.상태, 커뮤니티원장상태.진행중, StringComparison.OrdinalIgnoreCase))
        {
            return 상태값.배차대기상태.대기;
        }

        return null;
    }

    private static string? ExtractTransportLedgerId(string? ledgerId)
    {
        var cleaned = Clean(ledgerId);
        const string prefix = "transport:";
        return cleaned is not null && cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? cleaned[prefix.Length..]
            : null;
    }

    private static string? FindParticipantUserId(커뮤니티원장Dto 원장, params string[] roleHints)
        => 원장.참여자목록
            .FirstOrDefault(participant => roleHints.Any(hint =>
                ContainsAny(participant.RoleLabel, hint)
                || ContainsAny(participant.DisplayName, hint)))?
            .UserId;

    private static string? TryGet(IReadOnlyDictionary<string, string>? data, params string[] keys)
    {
        if (data is null || data.Count == 0)
        {
            return null;
        }

        foreach (var key in keys)
        {
            foreach (var pair in data)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return Clean(pair.Value);
                }
            }
        }

        return null;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.Select(Clean).FirstOrDefault(value => value is not null);

    private static bool ContainsAny(string? source, params string[] candidates)
    {
        var text = Clean(source);
        return text is not null && candidates.Any(candidate => text.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsInProgressTransportState(string? value)
        => value is 상태값.배차상태.상차중
            or 상태값.배차상태.상차완료
            or 상태값.배차상태.운송중
            or 상태값.배차상태.하차완료
            or "상차지도착"
            or "하차지도착";

    private static decimal? ParseDecimal(string? value)
    {
        var cleaned = Clean(value);
        if (cleaned is null)
        {
            return null;
        }

        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentValue)
            ? currentValue
            : null;
    }
}
