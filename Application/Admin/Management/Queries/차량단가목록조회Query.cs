using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed record 차량단가목록조회Query() : IRequest<IReadOnlyList<차량단가>>;
