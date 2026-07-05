namespace WarehouseManagerApp.Services;

public interface IInboundReceivingWorkflowService
{
    Task<IReadOnlyList<InboundExpectedProductDto>> GetExpectedProductsAsync(CancellationToken cancellationToken = default);

    Task<InboundExpectedProductDto?> FindExpectedProductAsync(string productBarcode, CancellationToken cancellationToken = default);

    Task<InboundReceivingConfirmationResult> ConfirmReceivedAsync(
        InboundReceivingConfirmationRequest request,
        CancellationToken cancellationToken = default);
}
