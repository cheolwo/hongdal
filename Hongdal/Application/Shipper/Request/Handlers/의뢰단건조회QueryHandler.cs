using Hongdal.Contracts.Shipper.Request;
using Hongdal.Application.CommandProcessing;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰단건조회QueryHandler : IRequestHandler<의뢰단건조회Query, 화주운송의뢰응답?>
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 의뢰단건조회QueryHandler(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<화주운송의뢰응답?> Handle(의뢰단건조회Query request, CancellationToken cancellationToken)
    {
        var entity = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.의뢰Id == request.RequestId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        if (!주문자권한검사.IsServerAdmin(_currentUserAccessor)
            && !주문자권한검사.IsOwner(entity, _currentUserAccessor.UserId))
        {
            return null;
        }

        return 화주운송의뢰매퍼.To응답(entity);
    }
}
