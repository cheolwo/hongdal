using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Contracts.Common.Warehouse;

namespace 홍달.Services.Warehouse;

public interface IWarehouseOperationService
{
    Task<창고목록응답> GetWarehousesAsync(CancellationToken cancellationToken);
    Task<창고요약응답> CreateWarehouseAsync(창고저장요청 request, CancellationToken cancellationToken);
    Task<창고사용자목록응답> GetWarehouseUsersAsync(long warehouseId, CancellationToken cancellationToken);
    Task<창고사용자항목응답> AddWarehouseUserAsync(long warehouseId, 창고사용자저장요청 request, CancellationToken cancellationToken);
    Task<입고요청목록응답> GetInboundsAsync(CancellationToken cancellationToken);
    Task<입고요청항목응답> CreateInboundAsync(입고요청저장요청 request, CancellationToken cancellationToken);
    Task<입고상품목록응답> CompleteInboundAsync(long inboundId, 입고완료요청 request, CancellationToken cancellationToken);
    Task<재고목록응답> GetInventoryAsync(CancellationToken cancellationToken);
    Task<화주운송의뢰응답> CreateReconsignmentRequestAsync(재고운송의뢰생성요청 request, CancellationToken cancellationToken);
}
