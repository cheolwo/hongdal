using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Application.CommandProcessing;

namespace Ssalddel.Application.Shipper.Request;

public sealed class 의뢰단건조회QueryHandler : IRequestHandler<의뢰단건조회Query, 화주운송의뢰응답?>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 의뢰단건조회QueryHandler(SsalddelContext db, ICurrentUserAccessor currentUserAccessor)
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

        var executionByRequestId = await 화주운송실행정보조회.조회Async(
            _db,
            [entity.의뢰Id],
            cancellationToken);
        executionByRequestId.TryGetValue(entity.의뢰Id, out var execution);

        return 화주운송의뢰매퍼.To응답(
            entity,
            execution?.운송원장,
            execution?.기사,
            execution?.최근위치);
    }
}
