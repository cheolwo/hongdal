using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Progress;

public sealed class 관리자운송목록조회QueryHandler : IRequestHandler<관리자운송목록조회Query, IReadOnlyList<운송진행응답>>
{
    private readonly HongdalContext _db;

    public 관리자운송목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<운송진행응답>> Handle(관리자운송목록조회Query request, CancellationToken cancellationToken)
    {
        var query = _db.배송_운송.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.상태))
        {
            var status = request.상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new 운송진행응답
            {
                Id = x.Id,
                운송번호 = x.운송번호,
                상태 = x.상태,
                출발_픽업 = x.출발_픽업,
                도착 = x.도착,
                기사_운송자 = x.기사_운송자,
                출발지 = x.출발지,
                도착지 = x.도착지,
                운임 = x.운임,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
