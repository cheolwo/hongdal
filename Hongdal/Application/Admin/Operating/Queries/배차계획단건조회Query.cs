using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed record 배차계획단건조회Query(long Id) : IRequest<배차계획관리상세응답?>;
