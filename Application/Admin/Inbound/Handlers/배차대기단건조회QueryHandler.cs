namespace Hongdal.Application.Admin.Inbound;

public sealed class 배차대기단건조회QueryHandler : IRequestHandler<배차대기단건조회Query, 홍달.도메인.배차.배차대기?>
{
    private readonly HongdalContext _db;

    public 배차대기단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<홍달.도메인.배차.배차대기?> Handle(배차대기단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.배차대기.FindAsync([request.Id], cancellationToken);
    }
}
