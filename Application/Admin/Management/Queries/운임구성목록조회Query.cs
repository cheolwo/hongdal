using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed record 운임구성목록조회Query() : IRequest<IReadOnlyList<운임구성>>;
