using FluentResults;
using Hongdal.Application.CommandProcessing;
using ShipRequest = Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 화주운송의뢰현장지급처리CommandHandler : IRequestHandler<화주운송의뢰현장지급처리Command, Result<ShipRequest.화주운송의뢰응답>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 화주운송의뢰현장지급처리CommandHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<ShipRequest.화주운송의뢰응답>> Handle(화주운송의뢰현장지급처리Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return Result.Fail<ShipRequest.화주운송의뢰응답>("RequestId is required");
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

        entity.정산시점 = ShipRequest.정산시점.현장지급.ToString();
        entity.결제수단 = ShipRequest.결제수단.현금.ToString();
        entity.수납주체 = ShipRequest.수납주체.기사.ToString();
        entity.정산상태 = ShipRequest.운임정산상태.현장수금예정.ToString();
        entity.현장지급메모 = MergeMemo(entity.현장지급메모, request.현장지급메모);
        entity.배차상태 = 상태값.배차상태.매칭중;
        entity.UpdatedAt = DateTime.UtcNow;

        await EnsureDispatchQueueAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(화주운송의뢰매퍼.To응답(entity));
    }

    private async Task EnsureDispatchQueueAsync(홍달.도메인.화주.화주운송의뢰 entity, CancellationToken cancellationToken)
    {
        var existingQueue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == entity.의뢰Id, cancellationToken);
        if (existingQueue != null)
        {
            return;
        }

        _db.배차대기.Add(new 홍달.도메인.배차.배차대기
        {
            의뢰Id = entity.의뢰Id,
            화주Id = entity.화주Id,
            픽업_도로명주소 = entity.픽업_도로명주소,
            픽업_상세주소 = entity.픽업_상세주소,
            픽업_위도 = entity.픽업_위도,
            픽업_경도 = entity.픽업_경도,
            하차_도로명주소 = entity.하차_도로명주소,
            하차_상세주소 = entity.하차_상세주소,
            하차_위도 = entity.하차_위도,
            하차_경도 = entity.하차_경도,
            상태 = 상태값.배차대기상태.대기,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
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
