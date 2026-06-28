using FluentResults;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed record 배차수락Command(string 기사Id, string RequestId) : IRequest<Result<배차수락결과>>;

public sealed record 배차수락결과(string RequestId, string Message);
