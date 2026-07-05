using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed record 의뢰단건조회Query(string RequestId) : IRequest<화주운송의뢰응답?>;
