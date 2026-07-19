using 살뜰.도메인.운송;

namespace Ssalddel.Application.Admin.Management;

public sealed record 운임구성단건조회Query(long Id) : IRequest<운임구성?>;
