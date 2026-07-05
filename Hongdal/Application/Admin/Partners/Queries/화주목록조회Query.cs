using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Partners;

public sealed record 화주목록조회Query() : IRequest<IReadOnlyList<화주관리응답>>;
