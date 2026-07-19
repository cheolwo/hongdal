using FluentResults;

namespace Ssalddel.Application.Admin.Management;

public sealed record 차량단가삭제Command(long Id) : IRequest<Result<Unit>>;
