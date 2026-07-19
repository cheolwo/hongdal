using Ssalddel.Contracts.Common.Exploration;

namespace Ssalddel.Application.Exploration;

public sealed record 탐색문의목록조회Query(string 대상UserId, string 대상역할) : IRequest<IReadOnlyList<탐색문의목록항목응답>>;
