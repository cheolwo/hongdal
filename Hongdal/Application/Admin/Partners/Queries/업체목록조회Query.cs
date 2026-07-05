using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Partners;

public sealed record 업체목록조회Query(string? 상태) : IRequest<IReadOnlyList<업체관리응답>>;
