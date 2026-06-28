using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed record 운송이벤트단건조회Query(long Id) : IRequest<운송이벤트로그응답?>;
