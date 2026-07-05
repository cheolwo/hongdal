using Hongdal.Contracts.Admin.Management;
using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed class 운임구성생성CommandHandler : IRequestHandler<운임구성생성Command, 운임구성>
{
    private readonly HongdalContext _db;

    public 운임구성생성CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<운임구성> Handle(운임구성생성Command request, CancellationToken cancellationToken)
    {
        var entity = new 운임구성
        {
            의뢰Id = request.의뢰Id,
            기본운임 = request.기본운임,
            거리운임 = request.거리운임,
            할증 = request.할증,
            대기료 = request.대기료,
            수작업비 = request.수작업비,
            최종운임 = request.최종운임,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.운임구성.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
