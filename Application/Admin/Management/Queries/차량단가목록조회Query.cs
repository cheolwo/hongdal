using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed record 차량단가목록조회Query() : IRequest<IReadOnlyList<차량단가응답>>;
