using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 기사창고화물인계조회Query(string 기사Id)
    : IRequest<CargoWarehouseHandoffResponse?>;
