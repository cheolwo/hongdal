using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰단건조회QueryHandler : IRequestHandler<의뢰단건조회Query, 화주운송의뢰응답?>
{
    private readonly HongdalContext _db;

    public 의뢰단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<화주운송의뢰응답?> Handle(의뢰단건조회Query request, CancellationToken cancellationToken)
    {
        var entity = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.의뢰Id == request.RequestId, cancellationToken);

        return entity == null ? null : 화주운송의뢰매퍼.To응답(entity);
    }
}
