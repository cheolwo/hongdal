using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Shipper.Request;
using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Warehouse.Reconsignment.Commands;

public sealed record CreateReconsignmentOrderCommand(재고운송의뢰생성요청 Payload, string UserId)
    : IAppCommand<화주운송의뢰응답?>;
