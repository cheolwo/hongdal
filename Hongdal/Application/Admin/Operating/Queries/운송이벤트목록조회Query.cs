using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed record 운송이벤트목록조회Query() : IRequest<IReadOnlyList<운송이벤트로그응답>>;
