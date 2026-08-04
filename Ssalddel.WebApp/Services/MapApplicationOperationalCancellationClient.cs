using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.WebApp.Services;

public sealed record MapApplicationOperationalCancellationResult(
    bool Cancelled,
    string Message);

public sealed class MapApplicationOperationalCancellationClient(
    ISsalddelJsonApiClient apiClient)
{
    public static bool Supports(string workCode)
        => string.Equals(workCode, 신청개인정보업무Codes.물류대행, StringComparison.Ordinal)
           || string.Equals(workCode, 신청개인정보업무Codes.개별주문, StringComparison.Ordinal);

    public async Task<MapApplicationOperationalCancellationResult> CancelAsync(
        string workCode,
        string operationalSourceId,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(workCode, 신청개인정보업무Codes.물류대행, StringComparison.Ordinal))
        {
            if (!long.TryParse(operationalSourceId, out var inboundId) || inboundId <= 0)
            {
                throw new InvalidOperationException("취소할 입고 요청 ID가 올바르지 않습니다.");
            }

            await apiClient.SendAsync(
                HttpMethod.Delete,
                $"api/v1/warehouse-operations/inbounds/{inboundId}",
                "입고 요청 취소",
                cancellationToken);
            return new(true, "입고 요청을 취소했습니다.");
        }

        if (string.Equals(workCode, 신청개인정보업무Codes.개별주문, StringComparison.Ordinal))
        {
            if (!Guid.TryParse(operationalSourceId, out var orderRequestId) || orderRequestId == Guid.Empty)
            {
                throw new InvalidOperationException("철회할 주문 요청 ID가 올바르지 않습니다.");
            }

            var response = await apiClient.SendAsync<마트주문요청철회요청, 마트주문요청응답>(
                HttpMethod.Post,
                $"api/v1/orderer/mart/order-requests/{orderRequestId:D}/withdrawal",
                new 마트주문요청철회요청(),
                "마트 주문 요청 철회",
                allowNotFound: false,
                cancellationToken);
            if (response is null
                || !string.Equals(response.상태코드, 마트주문요청상태코드.철회됨, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("마트 주문 요청 철회 상태를 확인하지 못했습니다.");
            }

            return new(true, "마트 주문 요청을 철회했습니다.");
        }

        if (string.Equals(workCode, 신청개인정보업무Codes.운송대행, StringComparison.Ordinal))
        {
            return new(false, "현재 운송 의뢰의 사용자용 DELETE는 상태 취소가 아니라 물리 삭제이므로 이 화면에서 실행하지 않습니다. 관리자 검토 후 안전한 취소·환불 절차를 이용해 주세요.");
        }

        throw new InvalidOperationException("지원하지 않는 지도 신청 업무입니다.");
    }
}
