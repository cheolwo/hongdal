using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;
using ShipperApp.Models.Shipper;

namespace ShipperApp.Services;

public interface IShipperOperationsService
{
    Task<IReadOnlyList<ShipperRequestItem>> GetRequestsAsync(CancellationToken cancellationToken = default);

    Task<ShipperRequestItem?> GetRequestAsync(string requestId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공개화물요약응답>> GetPublicCargoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<창고요약응답>> GetWarehousesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<입고요청항목응답>> GetInboundsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<재고항목응답>> GetInventoryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetVehicleTypesAsync(CancellationToken cancellationToken = default);

    Task<decimal> EstimateFareAsync(string vehicleType, decimal distanceKm, CancellationToken cancellationToken = default);

    Task AddRequestAsync(ShipperRequestItem request, CancellationToken cancellationToken = default);
}
