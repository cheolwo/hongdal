using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace WarehouseManagerApp.Services;

public sealed class HttpInboundReceivingWorkflowService(
    I입출고작업Service service) : IInboundReceivingWorkflowService
{
    public async Task<IReadOnlyList<InboundExpectedProductDto>> GetExpectedProductsAsync(
        CancellationToken cancellationToken = default)
        => (await service.입고목록조회Async(cancellationToken))
            .Where(item => item.상태 != 입고상태코드.취소)
            .Select(Map)
            .ToArray();

    public async Task<InboundExpectedProductDto?> FindExpectedProductAsync(
        string productBarcode,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(productBarcode);
        return (await GetExpectedProductsAsync(cancellationToken))
            .FirstOrDefault(item => Normalize(item.Barcode) == normalized);
    }

    public async Task<InboundReceivingConfirmationResult> RegisterUnplannedInboundAsync(
        UnplannedInboundRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var warehouse = (await service.창고목록조회Async(cancellationToken))
            .FirstOrDefault(item => item.IsActive)
            ?? throw new InvalidOperationException("현장 입고를 등록할 활성 창고가 없습니다.");

        var created = await service.현장입고요청생성Async(
            new 현장입고요청등록요청
            {
                클라이언트요청Id = Guid.NewGuid(),
                창고Id = warehouse.Id,
                상품바코드 = request.ProductBarcode,
                입고묶음바코드 = request.InboundBundleBarcode,
                상품명 = request.ProductName,
                공급처명 = request.Supplier,
                입고수량 = request.ReceivedQuantity,
                보관조건 = request.StorageType,
                현장입고사유 = request.ExceptionReason,
                임시입고안내확인 = true,
                안내버전 = 현장입고요청안내.현재버전
            },
            cancellationToken)
            ?? throw new InvalidOperationException("현장 입고 요청 저장 응답이 비어 있습니다.");

        return new InboundReceivingConfirmationResult(
            Map(created),
            created.입고묶음바코드,
            request.ReceivedQuantity,
            true,
            created.상태,
            "현장 입고 요청을 서버 원장에 저장하고 같은 요청을 다시 조회할 수 있게 했습니다.");
    }

    public async Task<InboundReceivingConfirmationResult> ConfirmReceivedAsync(
        InboundReceivingConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(request.ProductBarcode);
        var inbound = (await service.입고목록조회Async(cancellationToken))
            .FirstOrDefault(item => Normalize(item.예정SKU) == normalized)
            ?? throw new InvalidOperationException("입고 예정 서버 원장을 찾지 못했습니다.");

        await service.입고완료Async(
            inbound.Id,
            new 입고완료요청
            {
                Items =
                [
                    new 입고상품저장요청
                    {
                        상품명 = inbound.예정상품명,
                        SKU = inbound.예정SKU,
                        입고수량 = request.ReceivedQuantity,
                        보관위치 = inbound.보관조건
                    }
                ]
            },
            cancellationToken);

        var refreshed = await service.입고상세조회Async(inbound.Id, cancellationToken)
            ?? throw new InvalidOperationException("입고 완료 뒤 같은 서버 원장을 다시 조회하지 못했습니다.");
        var expected = refreshed.예정수량 ?? request.ReceivedQuantity;
        return new InboundReceivingConfirmationResult(
            Map(refreshed),
            request.InboundBundleBarcode,
            request.ReceivedQuantity,
            expected == request.ReceivedQuantity,
            refreshed.상태,
            "입고 완료 상태를 같은 서버 원장에서 다시 조회했습니다.");
    }

    private static InboundExpectedProductDto Map(입고요청항목응답 item)
        => new(
            item.예정SKU,
            item.예정상품명,
            item.Id.ToString(),
            item.공급처명,
            item.예정수량 ?? 0,
            item.보관조건,
            item.상태,
            item.입고흐름유형 == 입고흐름유형코드.현장임시입고,
            item.현장입고사유);

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}
