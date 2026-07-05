namespace Hongdal.Application.Admin.Management;

public sealed class 운임구성삭제CommandHandler : IRequestHandler<운임구성삭제Command, FluentResults.Result<Unit>>
{
    private readonly HongdalContext _db;

    public 운임구성삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<FluentResults.Result<Unit>> Handle(운임구성삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.운임구성.FindAsync([request.Id], cancellationToken);
        if (entity is null)
        {
            return FluentResults.Result.Fail<Unit>("운임구성을 찾을 수 없습니다.");
        }

        _db.운임구성.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return FluentResults.Result.Ok(Unit.Value);
    }
}
