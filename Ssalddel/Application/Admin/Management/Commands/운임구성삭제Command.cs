using FluentResults;

namespace Ssalddel.Application.Admin.Management;

public sealed record 운임구성삭제Command(long Id) : IRequest<Result<Unit>>;
