using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed record 현재운행기사조회Query(현재운행기사조회요청 Request) : IRequest<IReadOnlyList<현재운행기사응답>>;
