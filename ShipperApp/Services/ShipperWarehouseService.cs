using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Ui.Common.Areas.App.Services;
using ShipperApp.Services.Application;
using ShipperApp.Services.Warehouse.Reconsignment.Commands;

namespace ShipperApp.Services;

public sealed class ShipperWarehouseService : IShipperWarehouseWorkflowService
{
    private readonly InMemoryShipperStore _store;
    private readonly IAuthSession _authSession;
    private readonly IAppCommandHandler<CreateReconsignmentOrderCommand, 화주운송의뢰응답?> _createReconsignmentHandler;

    public ShipperWarehouseService(
        InMemoryShipperStore store,
        IAuthSession authSession,
        IAppCommandHandler<CreateReconsignmentOrderCommand, 화주운송의뢰응답?> createReconsignmentHandler)
    {
        _store = store;
        _authSession = authSession;
        _createReconsignmentHandler = createReconsignmentHandler;
    }

    public Task<창고목록응답?> GetWarehousesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<창고목록응답?>(new 창고목록응답 { Items = _store.GetWarehouses() });
    }

    public Task<창고요약응답?> CreateWarehouseAsync(창고저장요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _store.CreateWarehouse(payload, ResolveUserId());
        return Task.FromResult<창고요약응답?>(created);
    }

    public Task<입고요청목록응답?> GetInboundsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<입고요청목록응답?>(new 입고요청목록응답 { Items = _store.GetInbounds() });
    }

    public Task<입고요청항목응답?> CreateInboundAsync(입고요청저장요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _store.CreateInbound(payload, ResolveUserId());
        return Task.FromResult<입고요청항목응답?>(created);
    }

    public Task<입고상품목록응답?> CompleteInboundAsync(long inboundId, 입고완료요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var completed = _store.CompleteInbound(inboundId, payload, ResolveUserId());
        return Task.FromResult<입고상품목록응답?>(completed);
    }

    public Task<재고목록응답?> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<재고목록응답?>(new 재고목록응답 { Items = _store.GetInventory() });
    }

    public Task<화주운송의뢰응답?> CreateReconsignmentAsync(재고운송의뢰생성요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _createReconsignmentHandler.HandleAsync(new CreateReconsignmentOrderCommand(payload, ResolveUserId()), cancellationToken);
    }

    private string ResolveUserId()
    {
        return string.IsNullOrWhiteSpace(_authSession.UserId) ? "shipper-demo" : _authSession.UserId!;
    }
}
