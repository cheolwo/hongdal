using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Partners;

public sealed class 업체목록조회QueryHandler : IRequestHandler<업체목록조회Query, IReadOnlyList<업체관리응답>>
{
    private readonly HongdalContext _db;

    public 업체목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<업체관리응답>> Handle(업체목록조회Query request, CancellationToken cancellationToken)
    {
        var query = _db.업체.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.상태))
        {
            var status = request.상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        return await query
            .OrderBy(x => x.업체명)
            .Select(x => new 업체관리응답
            {
                Id = x.Id,
                업체명 = x.업체명,
                상태 = x.상태,
                대표연락처 = x.대표_연락처,
                담당자 = x.담당자,
                이메일 = x.이메일,
                주소 = x.주소,
                정산결제조건 = x.정산_결제_조건,
                등록일 = x.등록일
            })
            .ToListAsync(cancellationToken);
    }
}
