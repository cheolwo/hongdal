using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송현재조회QueryHandler : IRequestHandler<운송현재조회Query, 기사운송요약응답?>
{
    private readonly HongdalContext _db;

    public 운송현재조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사운송요약응답?> Handle(운송현재조회Query request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .AsNoTracking()
            .Where(x => x.기사_운송자 == request.기사Id && x.상태 != "인수완료")
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var shipperRequest = await _db.화주운송의뢰
            .AsNoTracking()
            .Where(x => x.의뢰Id == entity.운송번호)
            .Select(x => new { x.결제수단, x.증빙방식, x.요청사항, x.정산메모 })
            .FirstOrDefaultAsync(cancellationToken);

        return new 기사운송요약응답
        {
            Id = entity.Id,
            운송번호 = entity.운송번호,
            상태 = entity.상태,
            출발지 = entity.출발지,
            도착지 = entity.도착지,
            기사_운송자 = entity.기사_운송자,
            출발_픽업 = entity.출발_픽업,
            도착 = entity.도착,
            운임 = entity.운임,
            결제방식 = shipperRequest?.결제수단 ?? string.Empty,
            인수증필요 = 기사운송증빙조건정책.인수증필요(shipperRequest?.증빙방식, shipperRequest?.결제수단),
            인수증서명필수 = 기사운송증빙조건정책.인수증서명필수(shipperRequest?.요청사항, shipperRequest?.정산메모),
            UpdatedAt = entity.UpdatedAt
        };
    }
}
