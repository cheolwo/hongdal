using FluentResults;

namespace Hongdal.Application.Shipper.Request;

public sealed record 의뢰삭제Command(string RequestId) : IRequest<Result<Unit>>;
