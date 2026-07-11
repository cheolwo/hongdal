using FluentResults;
using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Progress;

public sealed record 운송원장이벤트조회Query(
    string RequestId,
    DateTime? SinceUtc) : IRequest<Result<운송원장이벤트응답>>;
