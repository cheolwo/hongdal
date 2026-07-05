using FluentResults;
using Hongdal.Application.CommandProcessing;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰삭제CommandHandler : IRequestHandler<의뢰삭제Command, Result<Unit>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 의뢰삭제CommandHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<Unit>> Handle(의뢰삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == request.RequestId, cancellationToken);
        if (entity == null)
        {
            return Result.Fail<Unit>("의뢰를 찾을 수 없습니다.");
        }

        if (!주문자권한검사.IsServerAdmin(_currentUserAccessor)
            && !주문자권한검사.IsOwner(entity, _currentUserAccessor.UserId))
        {
            return Result.Fail<Unit>("의뢰를 찾을 수 없습니다.");
        }

        _db.화주운송의뢰.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(Unit.Value);
    }
}
