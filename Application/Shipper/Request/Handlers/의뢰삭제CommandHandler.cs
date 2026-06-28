using FluentResults;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰삭제CommandHandler : IRequestHandler<의뢰삭제Command, Result<Unit>>
{
    private readonly HongdalContext _db;

    public 의뢰삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Result<Unit>> Handle(의뢰삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == request.RequestId, cancellationToken);
        if (entity == null)
        {
            return Result.Fail<Unit>("의뢰를 찾을 수 없습니다.");
        }

        _db.화주운송의뢰.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(Unit.Value);
    }
}
