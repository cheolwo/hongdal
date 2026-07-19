using Ssalddel.Contracts.Admin.Management;

namespace Ssalddel.Application.Admin.Partners;

public sealed record 화주목록조회Query() : IRequest<IReadOnlyList<화주관리응답>>;
