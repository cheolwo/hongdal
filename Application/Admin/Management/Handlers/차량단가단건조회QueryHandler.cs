using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가단건조회QueryHandler : IRequestHandler<차량단가단건조회Query, 차량단가응답?>
{
    private readonly HongdalContext _db;

    public 차량단가단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<차량단가응답?> Handle(차량단가단건조회Query request, CancellationToken cancellationToken)
    {
        var entity = await _db.차량단가.FindAsync([request.Id], cancellationToken);
        return entity == null ? null : 차량추천관리매퍼.To응답(entity);
    }
}
