using Hongdal.Contracts.Admin.Management;
using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가생성CommandHandler : IRequestHandler<차량단가생성Command, 차량단가응답>
{
    private readonly HongdalContext _db;

    public 차량단가생성CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<차량단가응답> Handle(차량단가생성Command request, CancellationToken cancellationToken)
    {
        var entity = new 차량단가
        {
            차량종류 = request.차량종류,
            기본운임 = request.기본운임,
            Km당단가 = request.Km당단가,
            야간할증 = request.야간할증,
            우천할증 = request.우천할증,
            최소운임 = request.최소운임,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.차량단가.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return 차량추천관리매퍼.To응답(entity);
    }
}
