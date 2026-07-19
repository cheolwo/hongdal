using Ssalddel.Contracts.Common.WarehouseScanning;

namespace Ssalddel.WebApp.Services;

public interface IInboundReceivingWorkflowService
{
    Task<IReadOnlyList<InboundExpectedProductDto>> GetExpectedProductsAsync(CancellationToken cancellationToken = default);

    Task<InboundExpectedProductDto?> FindExpectedProductAsync(string productBarcode, CancellationToken cancellationToken = default);

    Task<InboundReceivingConfirmationResult> RegisterUnplannedInboundAsync(
        UnplannedInboundRegistrationRequest request,
        CancellationToken cancellationToken = default);

    Task<InboundReceivingConfirmationResult> ConfirmReceivedAsync(
        InboundReceivingConfirmationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record InboundExpectedProductDto(
    string Barcode,
    string Name,
    string InboundRequestNo,
    string Supplier,
    int ExpectedQuantity,
    string StorageType,
    string ContractLinkStatus = "입고 예정",
    bool IsUnplannedInbound = false,
    string ExceptionReason = "");

public sealed record InboundReceivingConfirmationRequest(
    string ProductBarcode,
    string InboundBundleBarcode,
    int ReceivedQuantity);

public sealed record UnplannedInboundRegistrationRequest(
    string ProductBarcode,
    string InboundBundleBarcode,
    string ProductName,
    string Supplier,
    int ReceivedQuantity,
    string StorageType,
    string ContractLinkStatus,
    string ExceptionReason);

public sealed record InboundReceivingConfirmationResult(
    InboundExpectedProductDto Product,
    string InboundBundleBarcode,
    int ReceivedQuantity,
    bool QuantityMatched,
    string Status,
    string Message);

public sealed class SampleInboundReceivingWorkflowService : IInboundReceivingWorkflowService
{
    private readonly List<InboundExpectedProductDto> _expectedProducts =
    [
        new("SKU:MILK-001", "우유 1L", "INB-20260705-001", "서울유업", 20, "냉장"),
        new("SKU:SALAD-SET", "샐러드 세트", "INB-20260705-001", "그린팜", 12, "냉장"),
        new("SKU:WATER-6P", "생수 6팩", "INB-20260705-002", "한강물류", 30, "상온"),
        new("ITEM:BOX-TAPE", "포장 테이프", "INB-20260705-003", "포장상사", 15, "상온")
    ];

    public Task<IReadOnlyList<InboundExpectedProductDto>> GetExpectedProductsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<InboundExpectedProductDto>>(_expectedProducts.ToArray());
    }

    public Task<InboundExpectedProductDto?> FindExpectedProductAsync(string productBarcode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = NormalizeBarcode(productBarcode);
        var product = _expectedProducts.FirstOrDefault(x =>
            string.Equals(NormalizeBarcode(x.Barcode), normalized, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(product);
    }

    public Task<InboundReceivingConfirmationResult> RegisterUnplannedInboundAsync(
        UnplannedInboundRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.ProductBarcode))
        {
            throw new InvalidOperationException("상품 바코드를 입력해야 현장 입고를 등록할 수 있습니다.");
        }

        ValidateInboundBundleBarcode(request.InboundBundleBarcode);

        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            throw new InvalidOperationException("상품명을 입력해 주세요.");
        }

        if (request.ReceivedQuantity <= 0)
        {
            throw new InvalidOperationException("입고 수량은 1개 이상이어야 합니다.");
        }

        var normalized = NormalizeBarcode(request.ProductBarcode);
        var existing = _expectedProducts.FirstOrDefault(x =>
            string.Equals(NormalizeBarcode(x.Barcode), normalized, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            throw new InvalidOperationException("이미 입고 예정 목록에 있는 상품입니다. 예정 입고 조회 후 처리해 주세요.");
        }

        var product = new InboundExpectedProductDto(
            request.ProductBarcode.Trim(),
            request.ProductName.Trim(),
            $"UNPLANNED-{DateTimeOffset.Now:yyyyMMdd-HHmmss}",
            string.IsNullOrWhiteSpace(request.Supplier) ? "미확인 공급사" : request.Supplier.Trim(),
            request.ReceivedQuantity,
            string.IsNullOrWhiteSpace(request.StorageType) ? "미지정" : request.StorageType.Trim(),
            string.IsNullOrWhiteSpace(request.ContractLinkStatus) ? "계약 미연결" : request.ContractLinkStatus.Trim(),
            true,
            request.ExceptionReason.Trim());

        _expectedProducts.Add(product);

        return Task.FromResult(new InboundReceivingConfirmationResult(
            product,
            NormalizeBarcode(request.InboundBundleBarcode),
            request.ReceivedQuantity,
            true,
            "UnplannedInboundRegistered",
            "현장 입고로 임시 등록했습니다. 검수 단계에서 계약 연결, 정산 조건, 검수 사유를 보완해야 합니다."));
    }

    public async Task<InboundReceivingConfirmationResult> ConfirmReceivedAsync(
        InboundReceivingConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateInboundBundleBarcode(request.InboundBundleBarcode);

        var product = await FindExpectedProductAsync(request.ProductBarcode, cancellationToken)
            ?? throw new InvalidOperationException("입고 예정 상품을 찾지 못했습니다.");

        var quantityMatched = product.ExpectedQuantity == request.ReceivedQuantity;
        var status = product.IsUnplannedInbound
            ? "UnplannedInboundReceivedConfirmed"
            : quantityMatched ? "ReceivedConfirmed" : "ReceivedConfirmedWithQuantityDifference";

        var message = product.IsUnplannedInbound
            ? "현장 입고 확인 상태로 변경되었습니다. 검수에서 계약 연결 여부와 예외 사유를 함께 확인해야 합니다."
            : quantityMatched
                ? "입고 확인 상태로 변경되었습니다. 검수 작업으로 이동할 수 있습니다."
                : $"입고 확인 상태로 변경되었습니다. 예정 {product.ExpectedQuantity}개 / 실제 {request.ReceivedQuantity}개 차이를 검수에서 확인해야 합니다.";

        return new InboundReceivingConfirmationResult(
            product,
            NormalizeBarcode(request.InboundBundleBarcode),
            request.ReceivedQuantity,
            quantityMatched,
            status,
            message);
    }

    private static void ValidateInboundBundleBarcode(string inboundBundleBarcode)
    {
        if (string.IsNullOrWhiteSpace(inboundBundleBarcode))
        {
            throw new InvalidOperationException("입고 묶음 바코드를 스캔해야 입고 확인을 완료할 수 있습니다.");
        }

        var parsed = WarehouseBarcodeParser.Parse(inboundBundleBarcode);
        if (parsed.Kind != WarehouseBarcodeKindCode.Bundle)
        {
            throw new InvalidOperationException("입고 묶음 바코드는 BND: 또는 BUNDLE: 형식이어야 합니다.");
        }
    }

    private static string NormalizeBarcode(string value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}
