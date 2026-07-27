using Ssalddel.Contracts.Common.DeliveryZones;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Engine;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.도메인.운송;

namespace 살뜰.Services.DeliveryZones;

public interface I운송원장배달권연결Service
{
    Task<운송원장배달권연결결과> 투영추적Async(
        운송원장 운송원장,
        CancellationToken cancellationToken = default);
}

public sealed record 운송원장배달권연결결과(
    배달권판정결과 픽업배달권,
    배달권판정결과 배송배달권);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformDeliveryZoneLedger,
    SsalddelCodeLayer.Application,
    "운송 원장의 픽업·배송 배달권과 원천 주문 원장의 배송·국내 인계 배달권을 함께 투영한다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(I운송원장배달권연결Service),
    FlowOrder = 25,
    Boundary = "원천 원장의 기존 배송권 연결을 우선하고 호출자가 SaveChanges를 수행한다. 기사 후보나 배차 상태는 변경하지 않는다.")]
public sealed class 운송원장배달권연결Service : I운송원장배달권연결Service
{
    private readonly I원장배달권투영Service _원장배달권투영Service;

    public 운송원장배달권연결Service(I원장배달권투영Service 원장배달권투영Service)
    {
        _원장배달권투영Service = 원장배달권투영Service;
    }

    public async Task<운송원장배달권연결결과> 투영추적Async(
        운송원장 운송원장,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(운송원장);
        ArgumentException.ThrowIfNullOrWhiteSpace(운송원장.의뢰Id);

        var pickup = 국내화물배달권정책.판정(
            CreatePoint(운송원장.픽업_위도, 운송원장.픽업_경도),
            운송원장.픽업_도로명주소);
        var dropoff = 국내화물배달권정책.판정(
            CreatePoint(운송원장.하차_위도, 운송원장.하차_경도),
            운송원장.하차_도로명주소);

        await _원장배달권투영Service.연결추적Async(
            CreateRequest(
                원장배달권원장유형코드.운송원장,
                운송원장.의뢰Id,
                원장배달권역할코드.픽업,
                운송원장.픽업_도로명주소,
                운송원장.픽업_위도,
                운송원장.픽업_경도,
                "운송원장 픽업 배달권"),
            cancellationToken);
        await _원장배달권투영Service.연결추적Async(
            CreateRequest(
                원장배달권원장유형코드.운송원장,
                운송원장.의뢰Id,
                원장배달권역할코드.배송,
                운송원장.하차_도로명주소,
                운송원장.하차_위도,
                운송원장.하차_경도,
                "운송원장 배송 배달권"),
            cancellationToken);

        var sourceLink = ToSourceLedger(운송원장.원본의뢰유형);
        if (sourceLink is not null && !string.IsNullOrWhiteSpace(운송원장.원본의뢰Id))
        {
            await _원장배달권투영Service.연결추적Async(
                CreateRequest(
                    sourceLink.Value.원장유형코드,
                    운송원장.원본의뢰Id,
                    sourceLink.Value.역할코드,
                    운송원장.하차_도로명주소,
                    운송원장.하차_위도,
                    운송원장.하차_경도,
                    $"운송원장 {운송원장.의뢰Id} 연계",
                    preserveExisting: true),
                cancellationToken);
        }

        return new 운송원장배달권연결결과(pickup, dropoff);
    }

    private static 원장배달권연결요청 CreateRequest(
        string ledgerType,
        string ledgerId,
        string role,
        string? address,
        decimal? latitude,
        decimal? longitude,
        string basis,
        bool preserveExisting = false)
        => new()
        {
            원장유형코드 = ledgerType,
            원장Id = ledgerId,
            역할코드 = role,
            도로명주소 = address,
            위도 = latitude,
            경도 = longitude,
            생성근거 = basis,
            기존연결우선여부 = preserveExisting
        };

    private static (string 원장유형코드, string 역할코드)? ToSourceLedger(string? sourceType)
    {
        if (운송의뢰배차원천유형.Is음식점주문(sourceType))
        {
            return (원장배달권원장유형코드.음식주문, 원장배달권역할코드.배송);
        }

        if (운송의뢰배차원천유형.Is살뜰마트음식주문(sourceType))
        {
            return (원장배달권원장유형코드.마트주문, 원장배달권역할코드.배송);
        }

        if (운송의뢰배차원천유형.IsAny(sourceType, 운송의뢰배차원천유형.공동주문국내운송))
        {
            return (원장배달권원장유형코드.같이주문, 원장배달권역할코드.배송);
        }

        if (운송의뢰배차원천유형.IsAny(
                sourceType,
                운송의뢰배차원천유형.수입화물운송,
                운송의뢰배차원천유형.Fcl연계운송,
                운송의뢰배차원천유형.Lcl연계운송))
        {
            return (원장배달권원장유형코드.같이수입, 원장배달권역할코드.국내인계);
        }

        return null;
    }

    private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        => latitude.HasValue && longitude.HasValue
            ? new 배차경로좌표(latitude.Value, longitude.Value)
            : null;
}
