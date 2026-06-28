using Hongdal.Contracts.Admin.Management;
using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가수정CommandHandler : IRequestHandler<차량단가수정Command, 차량단가?>
{
    private readonly HongdalContext _db;

    public 차량단가수정CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<차량단가?> Handle(차량단가수정Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.차량단가.FindAsync([request.Id], cancellationToken);
        if (entity == null)
        {
            return null;
        }

        entity.차량종류 = request.차량종류;
        entity.기본운임 = request.기본운임;
        entity.Km당단가 = request.Km당단가;
        entity.야간할증 = request.야간할증;
        entity.우천할증 = request.우천할증;
        entity.최소운임 = request.최소운임;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
