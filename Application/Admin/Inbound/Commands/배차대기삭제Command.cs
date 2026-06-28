namespace Hongdal.Application.Admin.Inbound;

public sealed record 배차대기삭제Command(long Id) : IRequest<Unit>;
