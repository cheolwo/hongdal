using FluentResults;
using Hongdal.Application.CommandProcessing;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰수정CommandHandler : IRequestHandler<의뢰수정Command, Result<화주운송의뢰응답>>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 의뢰수정CommandHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<화주운송의뢰응답>> Handle(의뢰수정Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == request.RequestId, cancellationToken);
        if (entity == null)
        {
            return Result.Fail<화주운송의뢰응답>("의뢰를 찾을 수 없습니다.");
        }

        if (!주문자권한검사.IsServerAdmin(_currentUserAccessor)
            && !주문자권한검사.IsOwner(entity, _currentUserAccessor.UserId))
        {
            return Result.Fail<화주운송의뢰응답>("의뢰를 찾을 수 없습니다.");
        }

        var updated = false;

        if (request.화물정보 != null)
        {
            if (request.화물정보.화물종류 != null) entity.화물종류 = request.화물정보.화물종류;
            if (request.화물정보.화물설명 != null) entity.화물설명 = request.화물정보.화물설명;
            if (request.화물정보.화물수량.HasValue) entity.화물수량 = request.화물정보.화물수량;
            if (request.화물정보.화물중량Kg.HasValue) entity.화물중량Kg = request.화물정보.화물중량Kg;
            if (request.화물정보.화물부피Cbm.HasValue) entity.화물부피Cbm = request.화물정보.화물부피Cbm;
            if (request.화물정보.화물파손주의여부.HasValue) entity.화물파손주의여부 = request.화물정보.화물파손주의여부.Value;
            if (request.화물정보.화물온도조건 != null) entity.화물온도조건 = request.화물정보.화물온도조건;
            updated = true;
        }

        if (request.픽업지 != null)
        {
            if (request.픽업지.도로명주소 != null) entity.픽업_도로명주소 = request.픽업지.도로명주소;
            if (request.픽업지.상세주소 != null) entity.픽업_상세주소 = request.픽업지.상세주소;
            if (request.픽업지.위도.HasValue) entity.픽업_위도 = request.픽업지.위도;
            if (request.픽업지.경도.HasValue) entity.픽업_경도 = request.픽업지.경도;
            if (request.픽업지.연락처이름 != null) entity.픽업_연락처_이름 = request.픽업지.연락처이름;
            if (request.픽업지.연락처전화번호 != null) entity.픽업_연락처_전화번호 = request.픽업지.연락처전화번호;
            if (request.픽업지.시간창시작일시.HasValue && request.픽업지.시간창종료일시.HasValue)
            {
                if (request.픽업지.시간창시작일시 >= request.픽업지.시간창종료일시)
                {
                    return Result.Fail<화주운송의뢰응답>("pickup.window.startAt must be before endAt");
                }

                entity.픽업_시간창_시작일시 = request.픽업지.시간창시작일시.Value;
                entity.픽업_시간창_종료일시 = request.픽업지.시간창종료일시.Value;
            }

            updated = true;
        }

        if (request.하차지 != null)
        {
            if (request.하차지.도로명주소 != null) entity.하차_도로명주소 = request.하차지.도로명주소;
            if (request.하차지.상세주소 != null) entity.하차_상세주소 = request.하차지.상세주소;
            if (request.하차지.위도.HasValue) entity.하차_위도 = request.하차지.위도;
            if (request.하차지.경도.HasValue) entity.하차_경도 = request.하차지.경도;
            if (request.하차지.연락처이름 != null) entity.하차_연락처_이름 = request.하차지.연락처이름;
            if (request.하차지.연락처전화번호 != null) entity.하차_연락처_전화번호 = request.하차지.연락처전화번호;
            if (request.하차지.시간창시작일시.HasValue && request.하차지.시간창종료일시.HasValue)
            {
                if (request.하차지.시간창시작일시 >= request.하차지.시간창종료일시)
                {
                    return Result.Fail<화주운송의뢰응답>("dropoff.window.startAt must be before endAt");
                }

                entity.하차_시간창_시작일시 = request.하차지.시간창시작일시.Value;
                entity.하차_시간창_종료일시 = request.하차지.시간창종료일시.Value;
            }

            updated = true;
        }

        if (request.운송조건 != null)
        {
            if (request.운송조건.운송방식 != null) entity.운송방식 = request.운송조건.운송방식;
            if (request.운송조건.차량종류 != null) entity.차량종류 = request.운송조건.차량종류;
            if (request.운송조건.서비스레벨 != null) entity.서비스레벨 = request.운송조건.서비스레벨;
            updated = true;
        }

        if (request.정산조건 != null)
        {
            if (!string.IsNullOrWhiteSpace(request.정산조건.결제수단))
            {
                entity.결제수단 = request.정산조건.결제수단;
            }

            if (request.정산조건.정산조건 != null)
            {
                entity.정산시점 = request.정산조건.정산조건.정산시점.ToString();
                entity.증빙방식 = request.정산조건.정산조건.증빙방식.ToString();
                entity.수납주체 = request.정산조건.정산조건.수납주체.ToString();
                entity.정산메모 = request.정산조건.정산조건.정산메모 ?? string.Empty;
                entity.세금계산서필요 = request.정산조건.정산조건.세금계산서필요;
                entity.현금영수증필요 = request.정산조건.정산조건.현금영수증필요;
                entity.정산상태 = GetSettlementStatus(request.정산조건.정산조건.정산시점, request.정산조건.정산조건.증빙방식);
            }

            updated = true;
        }

        if (!updated)
        {
            return Result.Fail<화주운송의뢰응답>("수정할 필드를 하나 이상 제공해야 합니다.");
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await 화주운송의뢰매퍼.UpsertCargoRequirementAsync(_db, entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(화주운송의뢰매퍼.To응답(entity));
    }

    private static string GetSettlementStatus(정산시점 settlementTime, 증빙방식 evidenceMethod)
    {
        return settlementTime switch
        {
            정산시점.현장지급 => 운임정산상태.현장수금예정.ToString(),
            정산시점.운송완료후정산 when evidenceMethod == 증빙방식.인수증 => 운임정산상태.인수증대기.ToString(),
            정산시점.운송완료후정산 => 운임정산상태.청구대기.ToString(),
            정산시점.월말정산 => 운임정산상태.후불승인대기.ToString(),
            _ => 운임정산상태.결제대기.ToString()
        };
    }
}
