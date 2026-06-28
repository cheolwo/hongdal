using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 의뢰목록조회QueryHandler : IRequestHandler<의뢰목록조회Query, IReadOnlyList<화주운송의뢰응답>>
{
    private readonly HongdalContext _db;

    public 의뢰목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<화주운송의뢰응답>> Handle(의뢰목록조회Query request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 200 ? 50 : request.PageSize;

        var query = _db.화주운송의뢰.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ShipperId))
        {
            query = query.Where(r => r.화주Id == request.ShipperId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(r => r.상태 == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
        {
            query = query.Where(r => r.결제상태 == request.PaymentStatus);
        }

        if (!string.IsNullOrWhiteSpace(request.DispatchStatus))
        {
            query = query.Where(r => r.배차상태 == request.DispatchStatus);
        }

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return items.Select(화주운송의뢰매퍼.To응답).ToList();
    }
}
