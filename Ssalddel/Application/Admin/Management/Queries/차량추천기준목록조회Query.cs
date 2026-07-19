using Ssalddel.Contracts.Admin.Management;

namespace Ssalddel.Application.Admin.Management;

public sealed record 차량추천기준목록조회Query() : IRequest<IReadOnlyList<차량추천기준응답>>;
