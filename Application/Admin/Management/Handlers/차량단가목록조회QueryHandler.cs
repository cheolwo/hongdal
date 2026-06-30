using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가목록조회QueryHandler : IRequestHandler<차량단가목록조회Query, IReadOnlyList<차량단가응답>>
{
    private readonly HongdalContext _db;

    public 차량단가목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<차량단가응답>> Handle(차량단가목록조회Query request, CancellationToken cancellationToken)
    {
        var items = await _db.차량단가
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        return items.Select(차량추천관리매퍼.To응답).ToArray();
    }
}
