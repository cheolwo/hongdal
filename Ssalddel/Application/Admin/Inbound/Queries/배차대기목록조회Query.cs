using 살뜰.도메인.운송;

namespace Ssalddel.Application.Admin.Inbound;

public sealed record 배차대기목록조회Query() : IRequest<IReadOnlyList<운송원장>>;
