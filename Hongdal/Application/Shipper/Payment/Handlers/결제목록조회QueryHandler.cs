using Hongdal.Contracts.Shipper.Payment;

namespace Hongdal.Application.Shipper.Payment;

public sealed class 결제목록조회QueryHandler : IRequestHandler<결제목록조회Query, IReadOnlyList<결제목록응답>>
{
    private readonly HongdalContext _db;

    public 결제목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<결제목록응답>> Handle(결제목록조회Query request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 200 ? 50 : request.PageSize;

        var query = _db.결제.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.결제상태))
        {
            var status = request.결제상태.Trim();
            query = query.Where(x => x.결제상태 == status);
        }

        if (!string.IsNullOrWhiteSpace(request.의뢰Id))
        {
            var requestId = request.의뢰Id.Trim();
            query = query.Where(x => x.의뢰Id == requestId);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return items.Select(결제매퍼.To목록응답).ToList();
    }
}
