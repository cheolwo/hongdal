using Ssalddel.Contracts.Admin.Progress;

namespace Ssalddel.Application.Admin.Operating;

public sealed record 현재운행기사조회Query(현재운행기사조회요청 Request) : IRequest<IReadOnlyList<현재운행기사응답>>;
