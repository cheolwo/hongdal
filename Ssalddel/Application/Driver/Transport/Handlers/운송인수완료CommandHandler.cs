using FluentResults;
using Ssalddel.Contracts.Driver.Transport;

namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송인수완료CommandHandler : IRequestHandler<운송인수완료Command, Result<기사운송상태변경응답>>
{
    private readonly I기사운송상태변경CommandExecutor _executor;
    private readonly I운송증빙첨부JsonWriter _attachmentWriter;

    public 운송인수완료CommandHandler(
        I기사운송상태변경CommandExecutor executor,
        I운송증빙첨부JsonWriter attachmentWriter)
    {
        _executor = executor;
        _attachmentWriter = attachmentWriter;
    }

    public Task<Result<기사운송상태변경응답>> Handle(운송인수완료Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.하차사진ObjectName))
        {
            return Task.FromResult(Result.Fail<기사운송상태변경응답>("하차 완료 사진 업로드가 확인되어야 하차 완료 처리할 수 있습니다."));
        }

        var evidence = new 운송하차완료증빙(
            request.하차사진ObjectName.Trim(),
            request.하차사진Url?.Trim());

        return _executor.실행Async(
            new 기사운송상태변경요청(
                request.기사Id,
                request.Id,
                request.참여자Id,
                request.실행역할,
                기사운송상태코드.인수완료,
                nameof(운송인수완료됨Event),
                context => new 운송인수완료됨Event(
                    context.운송.Id,
                    context.운송.운송번호,
                    request.기사Id,
                    context.운송.출발지,
                    context.운송.도착지,
                    context.운송.상태,
                    context.발생시각Utc,
                    context.TraceId,
                    evidence),
                (entity, changedAt) => _attachmentWriter.추가(
                    entity,
                    new 운송증빙첨부(
                        "dropoff-complete-photo",
                        request.하차사진ObjectName,
                        request.하차사진Url,
                        request.기사Id,
                        changedAt,
                        new Dictionary<string, object?>()))),
            cancellationToken);
    }
}
