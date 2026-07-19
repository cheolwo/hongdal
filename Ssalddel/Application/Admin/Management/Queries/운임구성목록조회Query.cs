using 살뜰.도메인.운송;

namespace Ssalddel.Application.Admin.Management;

public sealed record 운임구성목록조회Query() : IRequest<IReadOnlyList<운임구성>>;
