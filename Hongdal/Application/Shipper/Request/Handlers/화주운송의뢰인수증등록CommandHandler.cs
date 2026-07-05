using FluentResults;
using Hongdal.Application.CommandProcessing;
using ShipRequest = Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 화주운송의뢰인수증등록CommandHandler : IRequestHandler<화주운송의뢰인수증등록Command, Result<ShipRequest.화주운송의뢰응답>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 화주운송의뢰인수증등록CommandHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<ShipRequest.화주운송의뢰응답>> Handle(화주운송의뢰인수증등록Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return Result.Fail<ShipRequest.화주운송의뢰응답>("RequestId is required");
        }

        if (string.IsNullOrWhiteSpace(request.인수증번호))
        {
            return Result.Fail<ShipRequest.화주운송의뢰응답>("인수증번호 is required");
        }

        var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        if (entity == null)
        {
            return Result.Fail<ShipRequest.화주운송의뢰응답>("의뢰를 찾을 수 없습니다.");
        }

        if (!주문자권한검사.IsServerAdmin(_currentUserAccessor)
            && !주문자권한검사.IsOwner(entity, _currentUserAccessor.UserId))
        {
            return Result.Fail<ShipRequest.화주운송의뢰응답>("의뢰를 찾을 수 없습니다.");
        }

        if (entity.정산상태 != ShipRequest.운임정산상태.후불승인완료.ToString() &&
            entity.정산상태 != ShipRequest.운임정산상태.인수증대기.ToString())
        {
            return Result.Fail<ShipRequest.화주운송의뢰응답>("인수증 등록은 후불 승인 이후에만 가능합니다.");
        }

        entity.인수증번호 = request.인수증번호.Trim();
        entity.인수증등록일시 = DateTime.UtcNow;
        entity.정산상태 = ShipRequest.운임정산상태.인수증등록완료.ToString();
        entity.정산메모 = MergeMemo(entity.정산메모, request.등록메모);
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(화주운송의뢰매퍼.To응답(entity));
    }

    private static string MergeMemo(string? origin, string? memo)
    {
        if (string.IsNullOrWhiteSpace(memo))
        {
            return origin ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(origin))
        {
            return memo.Trim();
        }

        return $"{origin.Trim()} | {memo.Trim()}";
    }
}
