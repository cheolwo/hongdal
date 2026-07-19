using 살뜰.도메인.운송;

namespace Ssalddel.Application.Admin.Management;

public sealed class 운임구성목록조회QueryHandler : IRequestHandler<운임구성목록조회Query, IReadOnlyList<운임구성>>
{
    private readonly SsalddelContext _db;

    public 운임구성목록조회QueryHandler(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<운임구성>> Handle(운임구성목록조회Query request, CancellationToken cancellationToken)
    {
        return await _db.운임구성.AsNoTracking().OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);
    }
}
