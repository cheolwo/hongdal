using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Progress;

public sealed record 관리자운송이벤트조회Query(string? RequestId) : IRequest<IReadOnlyList<운송이벤트로그응답>>;
