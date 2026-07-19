using Ssalddel.Contracts.Admin.Progress;

namespace Ssalddel.Application.Admin.Operating;

public sealed record 배차계획단건조회Query(long Id) : IRequest<배차계획관리상세응답?>;
