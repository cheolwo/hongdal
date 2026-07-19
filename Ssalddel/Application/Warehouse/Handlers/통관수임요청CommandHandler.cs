using FluentResults;
using MediatR;
using 살뜰.도메인.통관;

namespace Ssalddel.Application.Warehouse;

public sealed class 통관수임요청CommandHandler : IRequestHandler<통관수임요청Command, Result<Unit>>
{
    private readonly SsalddelContext _db;
    private readonly IPublisher _publisher;

    public 통관수임요청CommandHandler(SsalddelContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Result<Unit>> Handle(통관수임요청Command request, CancellationToken cancellationToken)
    {
        var 관세사 = await _db.관세사프로필
            .FirstOrDefaultAsync(x =>
                x.참여자Id == request.관세사참여자Id &&
                x.관리자승인여부 &&
                x.수임가능여부,
                cancellationToken);

        if (관세사 is null)
        {
            return Result.Fail<Unit>("수임 가능한 관세사 프로필이 없습니다.");
        }

        var 통관절차 = await _db.통관절차
            .FirstOrDefaultAsync(x => x.Id == request.통관절차Id, cancellationToken);

        if (통관절차 is null)
        {
            return Result.Fail<Unit>("통관절차를 찾을 수 없습니다.");
        }

        if (통관절차.상태 == 통관절차상태.완료 || 통관절차.상태 == 통관절차상태.반려)
        {
            return Result.Fail<Unit>("완료되었거나 반려된 통관절차에는 수임 요청할 수 없습니다.");
        }

        var 중복요청 = await _db.통관수임.AnyAsync(x =>
            x.통관절차Id == request.통관절차Id &&
            x.관세사참여자Id == request.관세사참여자Id &&
            (x.상태 == 통관수임상태.수임요청 || x.상태 == 통관수임상태.수임확정),
            cancellationToken);

        if (중복요청)
        {
            return Result.Fail<Unit>("이미 수임 요청된 통관절차입니다.");
        }

        var now = DateTime.UtcNow;
        _db.통관수임.Add(new 통관수임
        {
            통관절차Id = request.통관절차Id,
            관세사참여자Id = request.관세사참여자Id,
            상태 = 통관수임상태.수임요청,
            요청시각 = DateTimeOffset.UtcNow,
            메모 = request.메모,
            CreatedAt = now,
            UpdatedAt = now
        });

        if (통관절차.상태 == 통관절차상태.관세사검토대기 || 통관절차.상태 == 통관절차상태.준비필요)
        {
            통관절차.상태 = 통관절차상태.수임요청;
            통관절차.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new 통관수임요청됨Event(
                request.통관절차Id,
                request.관세사참여자Id,
                now,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
            cancellationToken);

        return Result.Ok(Unit.Value);
    }
}
