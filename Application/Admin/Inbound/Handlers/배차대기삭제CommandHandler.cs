namespace Hongdal.Application.Admin.Inbound;

public sealed class 배차대기삭제CommandHandler : IRequestHandler<배차대기삭제Command, Unit>
{
    private readonly HongdalContext _db;

    public 배차대기삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(배차대기삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배차대기.FindAsync([request.Id], cancellationToken);
        if (entity != null)
        {
            _db.배차대기.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
