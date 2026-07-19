using Ssalddel.Contracts.Common.Orderer;
using 살뜰.Services.External.Customs;

namespace Ssalddel.Services.Orderer;

public interface I공동구매해외선적통관동기화Service
{
    Task<공동구매해외선적통관동기화결과> SyncAsync(
        공동구매해외선적통관동기화요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class 공동구매해외선적통관동기화Service : I공동구매해외선적통관동기화Service
{
    private readonly I공동구매해외선적추적저장소 _store;
    private readonly I화물통관진행조회Service _customsTrackingService;

    public 공동구매해외선적통관동기화Service(
        I공동구매해외선적추적저장소 store,
        I화물통관진행조회Service customsTrackingService)
    {
        _store = store;
        _customsTrackingService = customsTrackingService;
    }

    public async Task<공동구매해외선적통관동기화결과> SyncAsync(
        공동구매해외선적통관동기화요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.문서관리번호))
        {
            throw new InvalidOperationException("documentManagementNumber is required.");
        }

        var shipment = await _store.GetBy문서관리번호Async(
            request.문서관리번호,
            cancellationToken);
        if (shipment is null)
        {
            return new 공동구매해외선적통관동기화결과
            {
                동기화됨 = false,
                메시지 = "공동주문 해외 선적 추적 원장을 찾을 수 없습니다.",
                조회시각Utc = DateTimeOffset.UtcNow
            };
        }

        var lookup = await _customsTrackingService.조회Async(
            new 화물통관진행조회Request
            {
                화물관리번호 = TrimToNull(request.통관화물관리번호),
                MasterBl = ResolveMasterBl(request, shipment),
                HouseBl = TrimToNull(request.하우스선하증권번호),
                BlYear = request.선하증권연도 ?? Resolve선하증권연도(shipment)
            },
            cancellationToken);

        if (!lookup.조회성공여부)
        {
            return new 공동구매해외선적통관동기화결과
            {
                동기화됨 = false,
                메시지 = lookup.오류메시지 ?? "관세청 화물통관 진행정보 조회에 실패했습니다.",
                조회시각Utc = lookup.조회시각,
                선적 = shipment
            };
        }

        var eventCode = 공동구매해외선적통관단계Mapper.To선적StatusCode(lookup.진행단계);
        var location = 공동구매해외선적통관단계Mapper.Resolve위치요약(lookup.장치장명);

        if (string.Equals(shipment.현재상태코드, eventCode, StringComparison.Ordinal)
            && string.Equals(shipment.현재위치요약, location, StringComparison.Ordinal))
        {
            return new 공동구매해외선적통관동기화결과
            {
                동기화됨 = true,
                메시지 = "관세청 조회 결과가 이미 원장 최신 상태와 같습니다.",
                통관단계명 = lookup.처리단계명 ?? string.Empty,
                Customs위치요약 = location,
                조회시각Utc = lookup.조회시각,
                선적 = shipment
            };
        }

        var updated = await _store.AppendEventAsync(
            shipment.문서관리번호,
            공동구매해외선적통관단계Mapper.To선적Event(lookup, request.주문자공개여부),
            updatedBy,
            cancellationToken);

        return new 공동구매해외선적통관동기화결과
        {
            동기화됨 = true,
            메시지 = "관세청 화물통관 진행정보를 공동구매 해외 선적 원장에 반영했습니다.",
            통관단계명 = lookup.처리단계명 ?? string.Empty,
            Customs위치요약 = location,
            조회시각Utc = lookup.조회시각,
            선적 = updated
        };
    }

    private static string? ResolveMasterBl(
        공동구매해외선적통관동기화요청 request,
        공동구매해외선적추적Dto shipment)
    {
        var explicitValue = TrimToNull(request.마스터선하증권번호);
        if (explicitValue is not null)
        {
            return explicitValue;
        }

        return string.Equals(shipment.운송문서유형, 공동구매선적문서유형코드.선하증권, StringComparison.Ordinal)
            ? TrimToNull(shipment.운송문서번호)
            : null;
    }

    private static int Resolve선하증권연도(공동구매해외선적추적Dto shipment)
        => (shipment.실제출발시각Utc
            ?? shipment.예상출발시각Utc
            ?? shipment.생성시각Utc).Year;

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
