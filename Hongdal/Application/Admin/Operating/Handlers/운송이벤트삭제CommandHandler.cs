namespace Hongdal.Application.Admin.Operating;

public sealed class 운송이벤트삭제CommandHandler : IRequestHandler<운송이벤트삭제Command, FluentResults.Result<Unit>>
{
    private readonly HongdalContext _db;

    public 운송이벤트삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<FluentResults.Result<Unit>> Handle(운송이벤트삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.운송이벤트.FindAsync([request.Id], cancellationToken);
        if (entity is null)
        {
            return FluentResults.Result.Fail<Unit>("운송이벤트를 찾을 수 없습니다.");
        }

        _db.운송이벤트.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return FluentResults.Result.Ok(Unit.Value);
    }
}
