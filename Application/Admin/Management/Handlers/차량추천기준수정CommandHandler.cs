using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량추천기준수정CommandHandler : IRequestHandler<차량추천기준수정Command, 차량추천기준응답?>
{
    private readonly HongdalContext _db;

    public 차량추천기준수정CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<차량추천기준응답?> Handle(차량추천기준수정Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.차량제원.FindAsync([request.차량코드], cancellationToken);
        if (entity == null)
        {
            return null;
        }

        entity.권장최대CBM = request.권장최대CBM.HasValue && request.권장최대CBM.Value > 0
            ? decimal.Round(request.권장최대CBM.Value, 3, MidpointRounding.AwayFromZero)
            : null;
        entity.추천우선순위 = request.추천우선순위;
        entity.추천사용여부 = request.추천사용여부;
        entity.운영권장중량Kg = request.운영권장중량Kg.HasValue && request.운영권장중량Kg.Value > 0
            ? request.운영권장중량Kg.Value
            : null;
        entity.팔레트적재개수 = request.팔레트적재개수.HasValue && request.팔레트적재개수.Value > 0
            ? request.팔레트적재개수.Value
            : null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return 차량추천관리매퍼.To응답(entity);
    }
}
