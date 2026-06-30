using FluentResults;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed record 배차거절Command(string 기사Id, string RequestId) : IRequest<Result<배차거절결과>>;

public sealed record 배차거절결과(string RequestId, string Message);
