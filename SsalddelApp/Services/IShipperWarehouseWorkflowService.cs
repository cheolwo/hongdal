using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SsalddelApp.Services;

public interface IShipperWarehouseWorkflowService : IWarehouseWorkspaceService
{
    Task<화주운송의뢰응답?> CreateReconsignmentAsync(재고운송의뢰생성요청 payload, CancellationToken cancellationToken = default);
}
