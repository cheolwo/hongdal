using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송목록조회QueryHandler : IRequestHandler<운송목록조회Query, IReadOnlyList<기사운송요약응답>>
{
    private readonly HongdalContext _db;

    public 운송목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사운송요약응답>> Handle(운송목록조회Query request, CancellationToken cancellationToken)
    {
        var items = await _db.배송_운송
            .AsNoTracking()
            .Where(x => x.기사_운송자 == request.기사Id)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new 기사운송요약응답
            {
                Id = x.Id,
                운송번호 = x.운송번호,
                상태 = x.상태,
                출발지 = x.출발지,
                도착지 = x.도착지,
                기사_운송자 = x.기사_운송자,
                출발_픽업 = x.출발_픽업,
                도착 = x.도착,
                운임 = x.운임,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
