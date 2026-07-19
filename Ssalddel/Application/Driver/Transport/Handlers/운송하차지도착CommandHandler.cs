using FluentResults;
using Ssalddel.Contracts.Driver.Transport;

namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송하차지도착CommandHandler : IRequestHandler<운송하차지도착Command, Result<기사운송상태변경응답>>
{
    private readonly I기사운송상태변경CommandExecutor _executor;

    public 운송하차지도착CommandHandler(I기사운송상태변경CommandExecutor executor)
    {
        _executor = executor;
    }

    public Task<Result<기사운송상태변경응답>> Handle(운송하차지도착Command request, CancellationToken cancellationToken)
    {
        return _executor.실행Async(
            new 기사운송상태변경요청(
                request.기사Id,
                request.Id,
                request.참여자Id,
                request.실행역할,
                기사운송상태코드.하차지도착,
                nameof(운송하차지도착됨Event),
                context => new 운송하차지도착됨Event(
                    request.기사Id,
                    context.운송.Id,
                    context.이전상태,
                    context.운송.상태,
                    context.발생시각Utc,
                    context.TraceId)),
            cancellationToken);
    }
}
