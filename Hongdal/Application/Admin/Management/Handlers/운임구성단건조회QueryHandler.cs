using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed class 운임구성단건조회QueryHandler : IRequestHandler<운임구성단건조회Query, 운임구성?>
{
    private readonly HongdalContext _db;

    public 운임구성단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<운임구성?> Handle(운임구성단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.운임구성.FindAsync([request.Id], cancellationToken);
    }
}
