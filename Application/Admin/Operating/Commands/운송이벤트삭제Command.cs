namespace Hongdal.Application.Admin.Operating;

public sealed record 운송이벤트삭제Command(long Id) : IRequest<Unit>;
