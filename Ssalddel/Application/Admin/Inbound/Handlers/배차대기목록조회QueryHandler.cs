namespace Ssalddel.Application.Admin.Inbound;

public sealed class 배차대기목록조회QueryHandler : IRequestHandler<배차대기목록조회Query, IReadOnlyList<살뜰.도메인.운송.운송원장>>
{
    private readonly SsalddelContext _db;

    public 배차대기목록조회QueryHandler(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<살뜰.도메인.운송.운송원장>> Handle(배차대기목록조회Query request, CancellationToken cancellationToken)
    {
        return await _db.운송원장
            .AsNoTracking()
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
