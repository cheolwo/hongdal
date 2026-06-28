using FluentResults;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰수정CommandHandler : IRequestHandler<의뢰수정Command, Result<화주운송의뢰응답>>
{
    private static readonly string[] AllowedPaymentStatuses = 상태값.결제상태.허용값;

    private readonly HongdalContext _db;

    public 의뢰수정CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Result<화주운송의뢰응답>> Handle(의뢰수정Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.화주운송의뢰.FirstOrDefaultAsync(r => r.의뢰Id == request.RequestId, cancellationToken);
        if (entity == null)
        {
            return Result.Fail<화주운송의뢰응답>("의뢰를 찾을 수 없습니다.");
        }

        var updated = false;

        if (!string.IsNullOrWhiteSpace(request.화물종류))
        {
            entity.화물종류 = request.화물종류;
            if (request.화물설명 != null) entity.화물설명 = request.화물설명;
            if (request.화물수량.HasValue) entity.화물수량 = request.화물수량;
            if (request.화물중량Kg.HasValue) entity.화물중량Kg = request.화물중량Kg;
            if (request.화물부피Cbm.HasValue) entity.화물부피Cbm = request.화물부피Cbm;
            if (request.화물파손주의여부.HasValue) entity.화물파손주의여부 = request.화물파손주의여부.Value;
            if (request.화물온도조건 != null) entity.화물온도조건 = request.화물온도조건;
            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(request.픽업도로명주소) || request.픽업위도.HasValue || request.픽업경도.HasValue)
        {
            if (!string.IsNullOrWhiteSpace(request.픽업도로명주소)) entity.픽업_도로명주소 = request.픽업도로명주소;
            if (request.픽업상세주소 != null) entity.픽업_상세주소 = request.픽업상세주소;
            if (request.픽업위도.HasValue) entity.픽업_위도 = request.픽업위도;
            if (request.픽업경도.HasValue) entity.픽업_경도 = request.픽업경도;
            if (!string.IsNullOrWhiteSpace(request.픽업연락처이름)) entity.픽업_연락처_이름 = request.픽업연락처이름;
            if (!string.IsNullOrWhiteSpace(request.픽업연락처전화번호)) entity.픽업_연락처_전화번호 = request.픽업연락처전화번호;
            if (request.픽업시간창시작일시.HasValue && request.픽업시간창종료일시.HasValue)
            {
                if (request.픽업시간창시작일시 >= request.픽업시간창종료일시)
                {
                    return Result.Fail<화주운송의뢰응답>("pickup.window.startAt must be before endAt");
                }

                entity.픽업_시간창_시작일시 = request.픽업시간창시작일시.Value;
                entity.픽업_시간창_종료일시 = request.픽업시간창종료일시.Value;
            }

            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(request.하차도로명주소) || request.하차위도.HasValue || request.하차경도.HasValue)
        {
            if (!string.IsNullOrWhiteSpace(request.하차도로명주소)) entity.하차_도로명주소 = request.하차도로명주소;
            if (request.하차상세주소 != null) entity.하차_상세주소 = request.하차상세주소;
            if (request.하차위도.HasValue) entity.하차_위도 = request.하차위도;
            if (request.하차경도.HasValue) entity.하차_경도 = request.하차경도;
            if (!string.IsNullOrWhiteSpace(request.하차연락처이름)) entity.하차_연락처_이름 = request.하차연락처이름;
            if (!string.IsNullOrWhiteSpace(request.하차연락처전화번호)) entity.하차_연락처_전화번호 = request.하차연락처전화번호;
            if (request.하차시간창시작일시.HasValue && request.하차시간창종료일시.HasValue)
            {
                if (request.하차시간창시작일시 >= request.하차시간창종료일시)
                {
                    return Result.Fail<화주운송의뢰응답>("dropoff.window.startAt must be before endAt");
                }

                entity.하차_시간창_시작일시 = request.하차시간창시작일시.Value;
                entity.하차_시간창_종료일시 = request.하차시간창종료일시.Value;
            }

            updated = true;
        }

        if (request.서비스레벨 != null || request.요청사항 != null || request.대기료.HasValue || request.수작업비.HasValue || request.할증.HasValue)
        {
            if (request.서비스레벨 != null) entity.서비스레벨 = request.서비스레벨;
            if (request.요청사항 != null) entity.요청사항 = request.요청사항;
            if (request.대기료.HasValue) entity.대기료 = request.대기료;
            if (request.수작업비.HasValue) entity.수작업비 = request.수작업비;
            if (request.할증.HasValue) entity.할증 = request.할증;
            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(request.결제상태))
        {
            var paymentStatus = request.결제상태.Trim();
            if (!AllowedPaymentStatuses.Contains(paymentStatus))
            {
                return Result.Fail<화주운송의뢰응답>($"결제상태 must be one of: {string.Join(", ", AllowedPaymentStatuses)}");
            }

            entity.결제상태 = paymentStatus;
            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(request.운송방식))
        {
            entity.운송방식 = request.운송방식;
            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(request.상태))
        {
            entity.상태 = request.상태;
            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(request.배차상태))
        {
            entity.배차상태 = request.배차상태;
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
}
