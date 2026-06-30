namespace Hongdal.Application.Admin.Management;

public sealed class 차량단가삭제CommandHandler : IRequestHandler<차량단가삭제Command, FluentResults.Result<Unit>>
{
    private readonly HongdalContext _db;

    public 차량단가삭제CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<FluentResults.Result<Unit>> Handle(차량단가삭제Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.차량단가.FindAsync([request.Id], cancellationToken);
        if (entity is null)
        {
            return FluentResults.Result.Fail<Unit>("차량단가를 찾을 수 없습니다.");
        }

        _db.차량단가.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return FluentResults.Result.Ok(Unit.Value);
    }
}
