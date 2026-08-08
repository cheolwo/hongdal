namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송연계입고조회QueryHandler
    : IRequestHandler<운송연계입고조회Query, 운송연계입고Projection?>
{
    private readonly SsalddelContext db;

    public 운송연계입고조회QueryHandler(SsalddelContext db)
    {
        this.db = db;
    }

    public Task<운송연계입고Projection?> Handle(
        운송연계입고조회Query request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.운송의뢰Id);

        return db.입고요청
            .AsNoTracking()
            .Where(item => item.운송의뢰Id == request.운송의뢰Id)
            .OrderByDescending(item => item.UpdatedAt)
            .Select(item => new 운송연계입고Projection(
                item.Id,
                item.창고Id,
                item.상태,
                item.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
