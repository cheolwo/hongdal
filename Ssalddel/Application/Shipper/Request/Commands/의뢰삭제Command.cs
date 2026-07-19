using FluentResults;

namespace Ssalddel.Application.Shipper.Request;

public sealed record 의뢰삭제Command(string RequestId) : IRequest<Result<Unit>>;
