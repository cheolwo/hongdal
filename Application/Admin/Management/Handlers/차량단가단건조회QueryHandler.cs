using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가단건조회QueryHandler : IRequestHandler<차량단가단건조회Query, 차량단가?>
{
    private readonly HongdalContext _db;

    public 차량단가단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<차량단가?> Handle(차량단가단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.차량단가.FindAsync([request.Id], cancellationToken);
    }
}
