namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가삭제CommandHandler : IRequestHandler<차량단가삭제Command, Unit>
{
    private readonly HongdalContext _db;

    public 차량단가삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(차량단가삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.차량단가.FindAsync([request.Id], cancellationToken);
        if (entity != null)
        {
            _db.차량단가.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
