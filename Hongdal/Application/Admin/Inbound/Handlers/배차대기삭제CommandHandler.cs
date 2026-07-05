namespace Hongdal.Application.Admin.Inbound;

public sealed class 배차대기삭제CommandHandler : IRequestHandler<배차대기삭제Command, FluentResults.Result<Unit>>
{
    private readonly HongdalContext _db;

    public 배차대기삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<FluentResults.Result<Unit>> Handle(배차대기삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배차대기.FindAsync([request.Id], cancellationToken);
        if (entity is null)
        {
            return FluentResults.Result.Fail<Unit>("배차대기 데이터를 찾을 수 없습니다.");
        }

        _db.배차대기.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return FluentResults.Result.Ok(Unit.Value);
    }
}
