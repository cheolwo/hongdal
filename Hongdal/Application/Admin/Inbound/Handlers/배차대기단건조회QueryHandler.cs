namespace Hongdal.Application.Admin.Inbound;

public sealed class 배차대기단건조회QueryHandler : IRequestHandler<배차대기단건조회Query, 홍달.도메인.운송.운송원장?>
{
    private readonly HongdalContext _db;

    public 배차대기단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<홍달.도메인.운송.운송원장?> Handle(배차대기단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.운송원장.FindAsync([request.Id], cancellationToken);
    }
}
