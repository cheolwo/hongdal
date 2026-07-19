using 살뜰.도메인.운송;

namespace Ssalddel.Application.Admin.Inbound;

public sealed record 배차대기단건조회Query(long Id) : IRequest<운송원장?>;
