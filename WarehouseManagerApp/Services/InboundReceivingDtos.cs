namespace WarehouseManagerApp.Services;

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
