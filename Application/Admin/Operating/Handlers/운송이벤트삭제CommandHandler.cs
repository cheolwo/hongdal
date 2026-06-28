namespace Hongdal.Application.Admin.Operating;

public sealed class 운송이벤트삭제CommandHandler : IRequestHandler<운송이벤트삭제Command, Unit>
{
    private readonly HongdalContext _db;

    public 운송이벤트삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(운송이벤트삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.운송이벤트.FindAsync([request.Id], cancellationToken);
        if (entity != null)
        {
            _db.운송이벤트.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
