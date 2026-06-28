using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송현재조회QueryHandler : IRequestHandler<운송현재조회Query, 기사운송요약응답>
{
    private readonly HongdalContext _db;

    public 운송현재조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<기사운송요약응답> Handle(운송현재조회Query request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .AsNoTracking()
            .Where(x => x.기사_운송자 == request.기사Id && x.상태 != "인수완료")
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("진행 중인 운송을 찾을 수 없습니다.");
        }

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
            UpdatedAt = entity.UpdatedAt
        };
    }
}
