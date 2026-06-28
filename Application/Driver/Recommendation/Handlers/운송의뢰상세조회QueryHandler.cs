namespace Hongdal.Application.Driver.Recommendation;

public sealed class 운송의뢰상세조회QueryHandler : IRequestHandler<운송의뢰상세조회Query, Hongdal.Contracts.Driver.Recommendation.기사운송의뢰상세응답>
{
    private readonly HongdalContext _db;
    private readonly I차량화물적합성Service _compatibilityService;

    public 운송의뢰상세조회QueryHandler(HongdalContext db, I차량화물적합성Service compatibilityService)
    {
        _db = db;
        _compatibilityService = compatibilityService;
    }

    public async Task<Hongdal.Contracts.Driver.Recommendation.기사운송의뢰상세응답> Handle(운송의뢰상세조회Query request, CancellationToken cancellationToken)
    {
        var dispatchRequest = await _db.화주운송의뢰.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("의뢰를 찾을 수 없습니다.");

        var queue = await _db.배차대기.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        var cargoRequirement = await _db.화물요구조건.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken);
        var vehicle = driver is null
            ? null
            : await _db.차량제원.AsNoTracking().FirstOrDefaultAsync(x => x.차량코드 == driver.차량 || x.차량명 == driver.차량, cancellationToken);
        var fit = _compatibilityService.판정(vehicle, dispatchRequest, cargoRequirement);

        return new Hongdal.Contracts.Driver.Recommendation.기사운송의뢰상세응답
        {
            의뢰Id = dispatchRequest.의뢰Id,
            화주Id = dispatchRequest.화주Id,
            화물종류 = dispatchRequest.화물종류,
            화물설명 = dispatchRequest.화물설명,
            픽업지 = dispatchRequest.픽업_도로명주소,
            픽업상세지 = dispatchRequest.픽업_상세주소,
            픽업위도 = dispatchRequest.픽업_위도,
            픽업경도 = dispatchRequest.픽업_경도,
            하차지 = dispatchRequest.하차_도로명주소,
            하차상세지 = dispatchRequest.하차_상세주소,
            하차위도 = dispatchRequest.하차_위도,
            하차경도 = dispatchRequest.하차_경도,
            결제상태 = dispatchRequest.결제상태,
            의뢰상태 = dispatchRequest.상태,
            배차상태 = dispatchRequest.배차상태,
            결제수단 = dispatchRequest.결제수단,
            결제예정금액 = dispatchRequest.결제예정금액,
            화물길이Mm = dispatchRequest.화물길이Mm,
            화물폭Mm = dispatchRequest.화물폭Mm,
            화물높이Mm = dispatchRequest.화물높이Mm,
            화물팔레트개수 = dispatchRequest.화물팔레트개수,
            차량적합여부 = fit.적합여부,
            부적합사유 = fit.부적합사유,
            경고 = fit.경고,
            배차대기상태 = queue?.상태,
            생성일시 = dispatchRequest.CreatedAt,
            수정일시 = dispatchRequest.UpdatedAt
        };
    }
}
