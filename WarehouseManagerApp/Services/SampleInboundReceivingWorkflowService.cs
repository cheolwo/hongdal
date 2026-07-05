namespace WarehouseManagerApp.Services;

public sealed class SampleInboundReceivingWorkflowService : IInboundReceivingWorkflowService
{
    private static readonly IReadOnlyList<InboundExpectedProductDto> ExpectedProducts =
    [
        new("SKU:MILK-001", "우유 1L", "INB-20260705-001", "서울유업", 20, "냉장"),
        new("SKU:SALAD-SET", "샐러드 세트", "INB-20260705-001", "그린팜", 12, "냉장"),
        new("SKU:WATER-6P", "생수 6팩", "INB-20260705-002", "한강물류", 30, "상온"),
        new("ITEM:BOX-TAPE", "포장 테이프", "INB-20260705-003", "포장상사", 15, "상온")
    ];

    public Task<IReadOnlyList<InboundExpectedProductDto>> GetExpectedProductsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ExpectedProducts);
    }

    public Task<InboundExpectedProductDto?> FindExpectedProductAsync(string productBarcode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = NormalizeBarcode(productBarcode);
        var product = ExpectedProducts.FirstOrDefault(x =>
            string.Equals(NormalizeBarcode(x.Barcode), normalized, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(product);
    }

    public async Task<InboundReceivingConfirmationResult> ConfirmReceivedAsync(
        InboundReceivingConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var product = await FindExpectedProductAsync(request.ProductBarcode, cancellationToken)
            ?? throw new InvalidOperationException("입고 예정 상품을 찾지 못했습니다.");

        var quantityMatched = product.ExpectedQuantity == request.ReceivedQuantity;
        var status = quantityMatched ? "ReceivedConfirmed" : "ReceivedConfirmedWithQuantityDifference";
        var message = quantityMatched
            ? "입고 확인 상태로 변경되었습니다. 검수 작업으로 이동할 수 있습니다."
            : $"입고 확인 상태로 변경되었습니다. 예정 {product.ExpectedQuantity}개 / 실제 {request.ReceivedQuantity}개 차이를 검수에서 확인해야 합니다.";

        return new InboundReceivingConfirmationResult(
            product,
            request.ReceivedQuantity,
            quantityMatched,
            status,
            message);
    }

    private static string NormalizeBarcode(string value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}
