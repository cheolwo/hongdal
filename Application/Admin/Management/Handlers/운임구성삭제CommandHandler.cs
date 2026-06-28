namespace Hongdal.Application.Admin.Management;

public sealed class 운임구성삭제CommandHandler : IRequestHandler<운임구성삭제Command, Unit>
{
    private readonly HongdalContext _db;

    public 운임구성삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(운임구성삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.운임구성.FindAsync([request.Id], cancellationToken);
        if (entity != null)
        {
            _db.운임구성.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
