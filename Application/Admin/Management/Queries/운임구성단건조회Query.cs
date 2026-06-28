using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed record 운임구성단건조회Query(long Id) : IRequest<운임구성?>;
