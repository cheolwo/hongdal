namespace WarehouseManagerApp.Services;

public sealed record InboundExpectedProductDto(
    string Barcode,
    string Name,
    string InboundRequestNo,
    string Supplier,
    int ExpectedQuantity,
    string StorageType);

public sealed record InboundReceivingConfirmationRequest(
    string ProductBarcode,
    int ReceivedQuantity);

public sealed record InboundReceivingConfirmationResult(
    InboundExpectedProductDto Product,
    int ReceivedQuantity,
    bool QuantityMatched,
    string Status,
    string Message);
