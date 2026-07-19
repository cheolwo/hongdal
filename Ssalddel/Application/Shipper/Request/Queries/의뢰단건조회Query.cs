using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Application.Shipper.Request;

public sealed record 의뢰단건조회Query(string RequestId) : IRequest<화주운송의뢰응답?>;
