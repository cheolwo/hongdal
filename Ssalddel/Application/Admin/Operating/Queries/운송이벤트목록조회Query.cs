using Ssalddel.Contracts.Admin.Progress;

namespace Ssalddel.Application.Admin.Operating;

public sealed record 운송이벤트목록조회Query() : IRequest<IReadOnlyList<운송이벤트로그응답>>;
