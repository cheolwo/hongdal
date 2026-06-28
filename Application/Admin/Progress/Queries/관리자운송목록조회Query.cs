using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Progress;

public sealed record 관리자운송목록조회Query(string? 상태) : IRequest<IReadOnlyList<운송진행응답>>;
