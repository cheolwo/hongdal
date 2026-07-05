using Hongdal.Contracts.Admin.Management;
using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed class 운임구성수정CommandHandler : IRequestHandler<운임구성수정Command, 운임구성?>
{
    private readonly HongdalContext _db;

    public 운임구성수정CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<운임구성?> Handle(운임구성수정Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.운임구성.FindAsync([request.Id], cancellationToken);
        if (entity == null)
        {
            return null;
        }

        entity.의뢰Id = request.의뢰Id;
        entity.기본운임 = request.기본운임;
        entity.거리운임 = request.거리운임;
        entity.할증 = request.할증;
        entity.대기료 = request.대기료;
        entity.수작업비 = request.수작업비;
        entity.최종운임 = request.최종운임;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
