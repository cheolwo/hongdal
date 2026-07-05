using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;

namespace Hongdal.Ui.Common.Areas.App.Services;

public interface IWarehouseWorkspaceService
{
    Task<창고목록응답?> GetWarehousesAsync(CancellationToken cancellationToken = default);

    Task<창고요약응답?> CreateWarehouseAsync(창고저장요청 payload, CancellationToken cancellationToken = default);

    Task<입고요청목록응답?> GetInboundsAsync(CancellationToken cancellationToken = default);

    Task<입고요청항목응답?> CreateInboundAsync(입고요청저장요청 payload, CancellationToken cancellationToken = default);

    Task<입고상품목록응답?> CompleteInboundAsync(long inboundId, 입고완료요청 payload, CancellationToken cancellationToken = default);

    Task<재고목록응답?> GetInventoryAsync(CancellationToken cancellationToken = default);
}
